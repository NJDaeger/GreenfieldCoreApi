using System.Net;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.BuildCodes;
using GreenfieldCoreServices.Services.Interfaces;

namespace GreenfieldCoreServices.Services;

public class CodeService(IUnitOfWork uow, ICacheService<long, BuildCode> cache) : ICodeService
{
    public async Task<Result<IEnumerable<BuildCode>>> GetAllBuildCodes()
    {
        var repo = uow.Repository<ICodeRepository>();
        var buildCodes = (await repo.SelectCodes()).GetNonNullOrThrow();
        
        var result = buildCodes.Select(BuildCode.FromModel).ToList();
        
        foreach (var buildCode in result) 
            cache.SetValue(buildCode.CodeId, buildCode);
        
        return Result<IEnumerable<BuildCode>>.Success(result);
    }

    public async Task<Result<BuildCode>> GetBuildCodeById(long buildCodeId)
    {
        if (cache.TryGetValue(buildCodeId, out var cachedBuildCode))
            return Result<BuildCode>.Success(cachedBuildCode);
        
        var repo = uow.Repository<ICodeRepository>();
        var buildCodeEntity = (await repo.SelectCode(buildCodeId)).GetOrThrow();
        if (buildCodeEntity is null) return Result<BuildCode>.Failure($"Build code id {buildCodeId} not found.", HttpStatusCode.NotFound);
        
        var buildCode = BuildCode.FromModel(buildCodeEntity);
        cache.SetValue(buildCode.CodeId, buildCode);
        return Result<BuildCode>.Success(buildCode);
    }

    public async Task<Result<BuildCode>> DeleteBuildCode(long buildCodeId)
    {
        var foundBuildCodeResult = await GetBuildCodeById(buildCodeId);
        if (!foundBuildCodeResult.IsSuccessful) return foundBuildCodeResult;
        var foundBuildCode = foundBuildCodeResult.GetNonNullOrThrow(nullDataMessage: "GetBuildCodeById returned null unexpectedly despite being successful.");
        
        var repo = uow.Repository<ICodeRepository>();
        
        uow.BeginTransaction();
        var deleteResult = await repo.DeleteCode(buildCodeId);
        
        if (!deleteResult.IsSuccessful) return Result<BuildCode>.Failure($"Build code id {buildCodeId} could not be deleted.");
        uow.CompleteAndCommit();
        
        cache.RemoveValue(buildCodeId);
        return Result<BuildCode>.Success(foundBuildCode);
    }

    public async Task<Result<BuildCode>> CreateBuildCode(int listOrder, string buildCode)
    {
        if (string.IsNullOrWhiteSpace(buildCode))
            return Result<BuildCode>.Failure("A valid build code must be provided.");
        
        var repo = uow.Repository<ICodeRepository>();
        
        uow.BeginTransaction();
        //there isn't really a reason the BuildCode couldn't be created aside from a DB issue
        var created = (await repo.InsertCode(listOrder, buildCode)).GetNonNullOrThrow(nullDataMessage: "The build code could not be created.");
        uow.CompleteAndCommit();
        
        var buildCodeModel = BuildCode.FromModel(created);
        cache.SetValue(buildCodeModel.CodeId, buildCodeModel);
        return Result<BuildCode>.Success(buildCodeModel);
    }

    public async Task<Result<BuildCode>> UpdateBuildCode(long buildCodeId, int? listOrder = null,
        string? buildCode = null)
    {
        if (listOrder is null && buildCode is null)
            return Result<BuildCode>.Failure("At least one of listOrder or buildCode must be provided for update.");
        
        var foundBuildCodeResult = await GetBuildCodeById(buildCodeId);
        if (!foundBuildCodeResult.IsSuccessful) return foundBuildCodeResult;
        var foundBuildCode = foundBuildCodeResult.GetNonNullOrThrow(nullDataMessage: "GetBuildCodeById returned null unexpectedly despite being successful.");
        
        var repo = uow.Repository<ICodeRepository>();
        
        if (buildCode is not null && string.IsNullOrWhiteSpace(buildCode))
            return Result<BuildCode>.Failure("A valid build code must be provided for update.");
        
        uow.BeginTransaction();
        var updateResult = (await repo.UpdateCode(
            buildCodeId,
            listOrder ?? foundBuildCode.ListOrder,
            buildCode ?? foundBuildCode.Code));
        
        if (!updateResult.IsSuccessful) return Result<BuildCode>.Failure($"Build code id {buildCodeId} could not be updated.");
        uow.CompleteAndCommit();
        
        foundBuildCode.ListOrder = listOrder ?? foundBuildCode.ListOrder;
        foundBuildCode.Code = buildCode ?? foundBuildCode.Code;
        cache.SetValue(foundBuildCode.CodeId, foundBuildCode);
        return Result<BuildCode>.Success(foundBuildCode);
    }
}