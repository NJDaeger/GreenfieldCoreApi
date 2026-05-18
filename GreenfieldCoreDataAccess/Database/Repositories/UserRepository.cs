using System.Data.Common;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Procedures;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class UserRepository(IUnitOfWork uow, ILogger<IUserRepository> logger) : BaseRepository(uow), IUserRepository
{
    
    public async Task<Result<UserEntity>> SelectUserByUserId(long userId)
    {
        try {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Users.SelectUserByUserId, userId, Transaction);
            return Result<UserEntity>.Success(result);
        } catch (DbException ex) {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<UserEntity>.Failure($"Failed to get user by ID: {ex.Message}");
        }
    }

    public async Task<Result<UserEntity>> SelectUserByUuid(Guid minecraftUuid)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Users.SelectUserByUuid, minecraftUuid, Transaction);
            return Result<UserEntity>.Success(result);
        } catch (DbException ex) {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<UserEntity>.Failure($"Failed to get user by UUID: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserEntity>>> SelectAllUsers()
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Users.SelectAllUsers, Transaction);
            return Result<IEnumerable<UserEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<UserEntity>>.Failure($"Failed to get all users: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserEntity>>> SelectUsersByUsername(string minecraftUsername)
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Users.SelectUsersByUsername, minecraftUsername, Transaction);
            return Result<IEnumerable<UserEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<UserEntity>>.Failure($"Failed to get users by username: {ex.Message}");
        }
    }

    public async Task<Result<UserEntity>> CreateUser(Guid minecraftUuid, string minecraftUsername)
    {
        try {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Users.InsertUser, (minecraftUuid, minecraftUsername), Transaction);
            return Result<UserEntity>.Success(result);
        } catch (DbException ex) {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<UserEntity>.Failure($"Failed to create user: {ex.Message}");
        }
    }

    public async Task<Result> UpdateUsername(Guid minecraftUuid, string newMinecraftUsername)
    {
        try {
            var rows = await Connection.ExecuteProcedure(StoredProcs.Users.UpdateUsername, (minecraftUuid, newMinecraftUsername), Transaction);
            return rows > 0 ? Result.Success() : Result.Failure("No rows were updated.");
        } catch (DbException ex) {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to update username: {ex.Message}");
        }
    }
}