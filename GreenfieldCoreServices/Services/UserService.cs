using GreenfieldCoreServices.Models;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.Interfaces;
using static GreenfieldCoreServices.Models.Result<GreenfieldCoreServices.Models.Users.User?>;

namespace GreenfieldCoreServices.Services;

public class UserService : IUserService
{
    public Task<Result<User?>> CreateUser(Guid minecraftUuid, string username)
    {
        return Task.FromResult(Ok(new User
        {
            UserId = 1,
            MinecraftUuid = minecraftUuid,
            Username = username,
            DisplayName = "displayName"
        }));
    }

    public Task<Result<User?>> GetUser(Guid minecraftUuid)
    {
        return Task.FromResult(Ok(new User
        {
            UserId = 1,
            MinecraftUuid = minecraftUuid,
            Username = "TestUser",
            DisplayName = "Test User"
        }));
    }

    public Task<Result<IEnumerable<User>>> FindUser(string username)
    {
        var users = new List<User>
        {
            new User
            {
                UserId = 1,
                MinecraftUuid = Guid.NewGuid(),
                Username = username,
                DisplayName = "Test User 1"
            },
            new User
            {
                UserId = 2,
                MinecraftUuid = Guid.NewGuid(),
                Username = username,
                DisplayName = "Test User 2"
            }
        };

        return Task.FromResult(Result<IEnumerable<User>>.Ok(users));
    }
}