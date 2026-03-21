using System.Net;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.BuildApps;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services;

public class BuilderApplicationService(IUnitOfWork uow, ILogger<IBuilderApplicationService> logger, ICacheService<long, BuilderApplication> buildAppCache) : IBuilderApplicationService
{
    public async Task<Result<long>> SubmitApplication(long userId,
        int age,
        string? nationality,
        List<(string Link, string ImageType)> images,
        string? additionalBuildingInformation,
        string whyJoinGreenfield,
        string? additionalComments)
    {
        var builderRepo = uow.Repository<IApplicationRepository>();
        
        uow.BeginTransaction();
        try
        {
            var applicationInsertResult = await builderRepo.InsertApplication(
                userId,
                age,
                nationality ?? string.Empty,
                additionalBuildingInformation,
                whyJoinGreenfield,
                additionalComments);

            if (!applicationInsertResult.IsSuccessful)
                return Result<long>.Failure(applicationInsertResult.ErrorMessage ?? "Failed to insert application.", applicationInsertResult.StatusCode);

            var application = applicationInsertResult.GetNonNullOrThrow();
            var applicationId = application.ApplicationId;
            var linkedImages = new List<ApplicationImageLinkEntity>();

            var anyFailedImageInserts = false;
            if (images.Count > 0)
            {
                foreach (var image in images)
                {
                    var imageResult = await builderRepo.InsertImage(applicationId, image.ImageType, image.Link);
                    if (imageResult.IsSuccessful) linkedImages.Add(imageResult.GetNonNullOrThrow());
                    else
                    {
                        anyFailedImageInserts = true;
                        logger.LogWarning("Failed to insert image for builder application {ApplicationId} (link: {Link}). {Error}", applicationId, image.Link, imageResult.ErrorMessage);
                    }
                }
            }
            
            if (anyFailedImageInserts)
                return Result<long>.Failure("One or more images failed to be linked to the application.", HttpStatusCode.PartialContent);
            
            uow.CompleteAndCommit();
            
            var mapped = MapApplicationEntityToModel(userId, application, [], linkedImages);
            buildAppCache.SetValue(applicationId, mapped);
            
            return Result<long>.Success(applicationId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure while submitting builder application for user {UserId}.", userId);
            return Result<long>.Failure("An unexpected error occurred while submitting the application.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<BuildAppStatus>> AddApplicationStatus(long applicationId, string status, string? statusMessage)
    {
        var builderRepo = uow.Repository<IApplicationRepository>();
        uow.BeginTransaction();

        var statusResult = await builderRepo.InsertStatus(applicationId, status, statusMessage);
        if (!statusResult.TryGetDataNonNull(out var newStatus))
        {
            logger.LogWarning("Failed to insert builder application status {Status} for application {ApplicationId}. {Error}", status, applicationId, statusResult.ErrorMessage);
            return Result<BuildAppStatus>.Failure(statusResult.ErrorMessage ?? "Failed to insert application status.", statusResult.StatusCode);
        }

        uow.CompleteAndCommit();

        if (buildAppCache.TryGetValue(applicationId, out var cachedApplication))
        {
            var refreshedStatusesResult = await builderRepo.SelectApplicationStatuses(applicationId);
            if (refreshedStatusesResult.IsSuccessful)
            {
                cachedApplication.BuildAppStatuses = refreshedStatusesResult
                    .GetNonNullOrThrow()
                    .Select(s => new BuildAppStatus(s.Status, s.StatusMessage, s.CreatedOn))
                    .ToList();
            }
            else
            {
                logger.LogWarning("Failed to refresh cache statuses for builder application {ApplicationId}. {Error}", applicationId, refreshedStatusesResult.ErrorMessage);
                cachedApplication.BuildAppStatuses.Add(new BuildAppStatus(status, statusMessage, DateTime.UtcNow));
            }

            buildAppCache.SetValue(applicationId, cachedApplication);
        }
        else
        {
            var cacheRefreshResult = await GetApplicationInternal(applicationId, true, null, builderRepo);
            if (!cacheRefreshResult.IsSuccessful)
                logger.LogWarning("Failed to cache builder application {ApplicationId} after status update. {Error}", applicationId, cacheRefreshResult.ErrorMessage);
        }

        return Result<BuildAppStatus>.Success(BuildAppStatus.FromModel(newStatus));
    }

    public async Task<Result> UpdateApplicationImage(long imageLinkId, string newImageLink, string newImageType)
    {
        var builderRepo = uow.Repository<IApplicationRepository>();
        uow.BeginTransaction();
        
        var updateResult = await builderRepo.UpdateImage(imageLinkId, newImageLink, newImageType);
        if (!updateResult.IsSuccessful)
            return Result.Failure(updateResult.ErrorMessage ?? "Failed to update application image.", updateResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        // Invalidate cache for the application that owns this image.
        var imageEntity = updateResult.GetNonNullOrThrow();
        if (buildAppCache.TryGetValue(imageEntity.ApplicationId, out var cachedApplication)) 
            buildAppCache.RemoveValue(imageEntity.ApplicationId);
        
        return Result.Success();
    }

    public Task<Result<BuilderApplication>> GetApplicationById(long applicationId) =>
        GetApplicationInternal(applicationId);

    public async Task<Result<List<ApplicationLatestStatus>>> GetApplicationsFromUser(long userId)
    {
        var builderRepo = uow.Repository<IApplicationRepository>();
        var latestResult = await builderRepo.SelectApplicationsWithLatestStatusByUser(userId);
        if (!latestResult.IsSuccessful)
            return Result<List<ApplicationLatestStatus>>.Failure(latestResult.ErrorMessage ?? "Failed to retrieve user applications with latest status.", latestResult.StatusCode);

        var rows = latestResult.GetNonNullOrThrow().ToList();
        var results = rows
            .Select(r => new ApplicationLatestStatus
            {
                ApplicationId = r.ApplicationId,
                LatestStatus = r.Status is not null && r.CreatedOn is not null
                    ? new BuildAppStatus(r.Status, r.StatusMessage, r.CreatedOn.Value)
                    : null
            })
            .ToList();

        return Result<List<ApplicationLatestStatus>>.Success(results);
    }

    private async Task<Result<BuilderApplication>> GetApplicationInternal(long applicationId, bool bypassCache = false, ApplicationEntity? existingEntity = null, IApplicationRepository? repository = null)
    {
        if (!bypassCache && buildAppCache.TryGetValue(applicationId, out var cachedApplication))
            return Result<BuilderApplication>.Success(cachedApplication);
        
        var builderRepo = repository ?? uow.Repository<IApplicationRepository>();
        ApplicationEntity applicationEntity;
        
        if (existingEntity is not null) applicationEntity = existingEntity;
        else
        {
            var applicationResult = await builderRepo.SelectApplication(applicationId);

            if (!applicationResult.IsSuccessful)
                return Result<BuilderApplication>.Failure(applicationResult.ErrorMessage ?? "Failed to retrieve application.", applicationResult.StatusCode);
            
            applicationEntity = applicationResult.GetNonNullOrThrow();
        }
        
        var statusesResult = await builderRepo.SelectApplicationStatuses(applicationId);
        if (!statusesResult.IsSuccessful)
            return Result<BuilderApplication>.Failure(statusesResult.ErrorMessage ?? "Failed to retrieve application statuses.", statusesResult.StatusCode);
        
        var imagesResult = await builderRepo.SelectApplicationImages(applicationId);
        if (!imagesResult.IsSuccessful)
            return Result<BuilderApplication>.Failure(imagesResult.ErrorMessage ?? "Failed to retrieve application images.", imagesResult.StatusCode);
        
        var images = imagesResult.GetNonNullOrThrow().ToList();
        var statuses = statusesResult.GetNonNullOrThrow().ToList();
        
        var applicationModel = MapApplicationEntityToModel(applicationEntity.UserId, applicationEntity, statuses, images);
        buildAppCache.SetValue(applicationId, applicationModel);
        return Result<BuilderApplication>.Success(applicationModel);
    }

    private BuilderApplication MapApplicationEntityToModel(long userId, 
        ApplicationEntity entity,
        List<ApplicationStatusEntity> statuses,
        List<ApplicationImageLinkEntity> images)
    {
        return new BuilderApplication
        {
            ApplicationId = entity.ApplicationId,
            UserId = userId,
            Age = entity.UserAge,
            Nationality = entity.UserNationality,
            AdditionalBuildingInformation = entity.AdditionalBuildingInformation,
            WhyJoinGreenfield = entity.WhyJoinGreenfield,
            AdditionalComments = entity.AdditionalComments,
            CreatedOn = entity.CreatedOn,
            BuildAppStatuses = statuses
                .Select(s => new BuildAppStatus(s.Status, s.StatusMessage, s.CreatedOn))
                .ToList(),
            Images = images
                .Select(img => new BuildAppImage(img.ImageLinkId, img.ImageLink, img.LinkType, img.CreatedOn))
                .ToList()
        };
    }
}
