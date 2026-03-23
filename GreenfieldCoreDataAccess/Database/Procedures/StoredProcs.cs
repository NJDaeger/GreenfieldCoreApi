using System.Data;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.ScriptManager;

namespace GreenfieldCoreDataAccess.Database.Procedures;

/// <summary>
/// All stored procedures in the database.
/// </summary>
public static class StoredProcs
{

    public static class ScriptManager
    {
        /// <summary>
        /// Determines if a script should be applied.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<bool, Script> ShouldScriptBeApplied = new("`ScriptManager.usp_ShouldScriptBeApplied`", (script, parms) =>
        {
            parms.Add("p_IsInit", script.IsInit, DbType.Boolean);
            parms.Add("p_AppliesTo", script.AppliesTo, DbType.String, size: 255);
            parms.Add("p_Major", script.Major, DbType.Int32);
            parms.Add("p_Minor", script.Minor, DbType.Int32);
            return parms;
        });
        
        /// <summary>
        /// Records the execution of a script.
        /// </summary>
        public static readonly ParameterizedProcedure<Script> RecordScriptExecution = new("`ScriptManager.usp_RecordScriptExecution`", (script, parms) =>
        {
            parms.Add("p_IsInit", script.IsInit, DbType.Boolean);
            parms.Add("p_AppliesTo", script.AppliesTo, DbType.String, size: 255);
            parms.Add("p_Major", script.Major, DbType.Int32);
            parms.Add("p_Minor", script.Minor, DbType.Int32);
            return parms;
        });
        
    }
    
    /// <summary>
    /// Procedures related to user management.
    /// </summary>
    public static class Users
    {
        #region Top Level User Procedures
        
        /// <summary>
        /// Inserts a new user into the database.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<UserEntity, (Guid minecraftGuid, string minecraftUsername)> InsertUser = new("`Users.usp_InsertUser`", (args, parms) =>
        {
            parms.Add("p_MinecraftUuid", args.minecraftGuid, DbType.Guid);
            parms.Add("p_MinecraftUsername", args.minecraftUsername, DbType.String, size: 16);
            return parms;
        });
        
        /// <summary>
        /// Selects a user by their user ID.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<UserEntity, long> SelectUserByUserId = new("`Users.usp_SelectUserByUserId`", (userId, parms) =>
        {
            parms.Add("p_UserId", userId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Selects a user by their Minecraft UUID.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<UserEntity, Guid> SelectUserByUuid = new("`Users.usp_SelectUserByUuid`", (uuid, parms) =>
        {
            parms.Add("p_MinecraftUuid", uuid, DbType.Guid);
            return parms;
        });
        
        /// <summary>
        /// Updates a user's Minecraft username.
        /// </summary>
        public static readonly ParameterizedProcedure<(Guid minecraftUuid, string newUsername)> UpdateUsername = new("`Users.usp_UpdateUsername`", (args, parms) =>
        {
            parms.Add("p_MinecraftUuid", args.minecraftUuid, DbType.Guid);
            parms.Add("p_NewUsername", args.newUsername, DbType.String, size: 16);
            return parms;
        });
        
        #endregion

        #region Discord Connection Procedures
        
        /// <summary>
        /// Inserts a new Discord connection for a user.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<UserDiscordConnectionEntity, (long userId, long discordConnectionId)> InsertUserDiscordConnection = new("`Users.usp_InsertUserDiscordConnection`", (args, parms) =>
        {
            parms.Add("p_UserId", args.userId, DbType.Int64);
            parms.Add("p_DiscordConnectionId", args.discordConnectionId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Deletes a Discord connection for a user.
        /// </summary>
        public static readonly ParameterizedProcedure<(long userId, long discordConnectionId)> DeleteUserDiscordConnection = new("`Users.usp_DeleteUserDiscordConnection`", (args, parms) =>
        {
            parms.Add("p_UserId", args.userId, DbType.Int64);
            parms.Add("p_DiscordConnectionId", args.discordConnectionId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Selects all Discord connections for a user.
        /// </summary>
        public static readonly ParameterizedQueryProcedure<UserDiscordConnectionEntity, long> SelectDiscordConnectionsByUserId = new("`Users.usp_SelectDiscordConnectionsByUserId`", (userId, parms) =>
        {
            parms.Add("p_UserId", userId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Selects all users by Discord connection ID.
        /// </summary>
        public static readonly ParameterizedQueryProcedure<UserByDiscordConnectionEntity, long> SelectUsersByDiscordConnectionId = new("`Users.usp_SelectUsersByDiscordConnectionId`", (discordConnectionId, parms) =>
        {
            parms.Add("p_DiscordConnectionId", discordConnectionId, DbType.Int64);
            return parms;
        });

        #endregion

        #region Patreon Connection Procedures

        /// <summary>
        /// Inserts a new Patreon connection for a user.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<UserPatreonConnectionEntity, (long userId, long patreonConnectionId)> InsertUserPatreonConnection = new("`Users.usp_InsertUserPatreonConnection`", (args, parms) =>
        {
            parms.Add("p_UserId", args.userId, DbType.Int64);
            parms.Add("p_PatreonConnectionId", args.patreonConnectionId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Deletes a Patreon connection for a user.
        /// </summary>
        public static readonly ParameterizedProcedure<(long userId, long patreonConnectionId)> DeleteUserPatreonConnection = new("`Users.usp_DeleteUserPatreonConnection`", (args, parms) =>
        {
            parms.Add("p_UserId", args.userId, DbType.Int64);
            parms.Add("p_PatreonConnectionId", args.patreonConnectionId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Selects all Patreon connections for a user.
        /// </summary>
        public static readonly ParameterizedQueryProcedure<UserPatreonConnectionEntity, long> SelectPatreonConnectionsByUserId = new("`Users.usp_SelectPatreonConnectionsByUserId`", (userId, parms) =>
        {
            parms.Add("p_UserId", userId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Selects all users by Patreon connection ID.
        /// </summary>
        public static readonly ParameterizedQueryProcedure<UserByPatreonConnectionEntity, long> SelectUsersByPatreonConnectionId = new("`Users.usp_SelectUsersByPatreonConnectionId`", (patreonConnectionId, parms) =>
        {
            parms.Add("p_PatreonConnectionId", patreonConnectionId, DbType.Int64);
            return parms;
        });
        
        #endregion
    }

    /// <summary>
    /// Procedures related to external connections.
    /// </summary>
    public static class Connections
    {
        /// <summary>
        /// Procedures related to Discord connections.
        /// </summary>
        public static class Discord
        {
            /// <summary>
            /// Inserts a new Discord connection.
            /// </summary>
            public static readonly ParameterizedQuerySingleProcedure<DiscordConnectionEntity?, (string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope, ulong discordSnowflake, string discordUsername)> InsertDiscordConnection = new("`Connections.Discord.usp_InsertDiscordConnection`", (args, parms) =>
            {
                parms.Add("p_RefreshToken", args.refreshToken, DbType.String, size: 512);
                parms.Add("p_AccessToken", args.accessToken, DbType.String, size: 512);
                parms.Add("p_TokenType", args.tokenType, DbType.String, size: 64);
                parms.Add("p_TokenExpiry", args.tokenExpiry, DbType.DateTime);
                parms.Add("p_Scope", args.scope, DbType.String, size: 1024);
                parms.Add("p_DiscordSnowflake", args.discordSnowflake, DbType.UInt64);
                parms.Add("p_DiscordUsername", args.discordUsername, DbType.String, size: 256);
                return parms;
            });

            /// <summary>
            /// Deletes a Discord connection for a user.
            /// </summary>
            public static readonly ParameterizedProcedure<long> DeleteDiscordConnection = new("`Connections.Discord.usp_DeleteDiscordConnection`", (discordConnectionId, parms) =>
            {
                parms.Add("p_DiscordConnectionId", discordConnectionId, DbType.Int64);
                return parms;
            });

            /// <summary>
            /// Selects all Discord connections.
            /// </summary>
            public static readonly QueryProcedure<DiscordConnectionEntity> SelectAllDiscordConnections = new("`Connections.Discord.usp_SelectAllDiscordConnections`");

            /// <summary>
            /// Selects a Discord connection by its ID.
            /// </summary>
            public static readonly ParameterizedQuerySingleProcedure<DiscordConnectionEntity?, long> SelectDiscordConnection = new("`Connections.Discord.usp_SelectDiscordConnection`", (discordConnectionId, parms) =>
            {
                parms.Add("p_DiscordConnectionId", discordConnectionId, DbType.Int64);
                return parms;
            });

            /// <summary>
            /// Selects a Discord connection by its Discord snowflake.
            /// </summary>
            public static readonly ParameterizedQuerySingleProcedure<DiscordConnectionEntity?, ulong> SelectDiscordConnectionBySnowflake = new("`Connections.Discord.usp_SelectDiscordConnectionBySnowflake`", (discordSnowflake, parms) =>
            {
                parms.Add("p_DiscordSnowflake", discordSnowflake, DbType.UInt64);
                return parms;
            });
            
            /// <summary>
            /// Updates a Discord connection's profile information.
            /// </summary>
            public static readonly ParameterizedProcedure<(long discordConnectionId, string discordUsername)> UpdateDiscordConnectionProfile = new("`Connections.Discord.usp_UpdateDiscordConnectionProfile`", (args, parms) =>
            {
                parms.Add("p_DiscordConnectionId", args.discordConnectionId, DbType.Int64);
                parms.Add("p_DiscordUsername", args.discordUsername, DbType.String, size: 256);
                return parms;
            });

            /// <summary>
            /// Updates a Discord connection's tokens.
            /// </summary>
            public static readonly ParameterizedProcedure<(long discordConnectionId, string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope)> UpdateDiscordConnectionTokens = new("`Connections.Discord.usp_UpdateDiscordConnectionTokens`", (args, parms) =>
            {
                parms.Add("p_DiscordConnectionId", args.discordConnectionId, DbType.Int64);
                parms.Add("p_RefreshToken", args.refreshToken, DbType.String, size: 512);
                parms.Add("p_AccessToken", args.accessToken, DbType.String, size: 512);
                parms.Add("p_TokenType", args.tokenType, DbType.String, size: 64);
                parms.Add("p_TokenExpiry", args.tokenExpiry, DbType.DateTime);
                parms.Add("p_Scope", args.scope, DbType.String, size: 1024);
                return parms;
            });
        }

        /// <summary>
        /// Procedures related to Patreon connections.
        /// </summary>
        public static class Patreon
        {
            /// <summary>
            /// Inserts a new Patreon connection.
            /// </summary>
            public static readonly ParameterizedQuerySingleProcedure<PatreonConnectionEntity?, (string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope, long patreonId, string fullName, decimal? pledge)> InsertPatreonConnection = new("`Connections.Patreon.usp_InsertPatreonConnection`", (args, parms) =>
            {
                parms.Add("p_RefreshToken", args.refreshToken, DbType.String, size: 512);
                parms.Add("p_AccessToken", args.accessToken, DbType.String, size: 512);
                parms.Add("p_TokenType", args.tokenType, DbType.String, size: 64);
                parms.Add("p_TokenExpiry", args.tokenExpiry, DbType.DateTime);
                parms.Add("p_Scope", args.scope, DbType.String, size: 1024);
                parms.Add("p_PatreonId", args.patreonId, DbType.Int64);
                parms.Add("p_FullName", args.fullName, DbType.String, size: 256);
                parms.Add("p_Pledge", args.pledge, DbType.Decimal);
                return parms;
            });

            /// <summary>
            /// Deletes a Patreon connection.
            /// </summary>
            public static readonly ParameterizedProcedure<long> DeletePatreonConnection = new("`Connections.Patreon.usp_DeletePatreonConnection`", (patreonConnectionId, parms) =>
            {
                parms.Add("p_PatreonConnectionId", patreonConnectionId, DbType.Int64);
                return parms;
            });

            /// <summary>
            /// Selects all Patreon connections.
            /// </summary>
            public static readonly QueryProcedure<PatreonConnectionEntity> SelectAllPatreonConnections = new("`Connections.Patreon.usp_SelectAllPatreonConnections`");

            /// <summary>
            /// Selects a Patreon connection by its ID.
            /// </summary>
            public static readonly ParameterizedQuerySingleProcedure<PatreonConnectionEntity?, long> SelectPatreonConnection = new("`Connections.Patreon.usp_SelectPatreonConnection`", (patreonConnectionId, parms) =>
            {
                parms.Add("p_PatreonConnectionId", patreonConnectionId, DbType.Int64);
                return parms;
            });

            /// <summary>
            /// Selects a Patreon connection by its Patreon ID.
            /// </summary>
            public static readonly ParameterizedQuerySingleProcedure<PatreonConnectionEntity?, long> SelectPatreonConnectionByPatreonId = new("`Connections.Patreon.usp_SelectPatreonConnectionByPatreonId`", (patreonId, parms) =>
            {
                parms.Add("p_PatreonId", patreonId, DbType.Int64);
                return parms;
            });
   
            /// <summary>
            /// Updates a Patreon connection's profile information.
            /// </summary>
            public static readonly ParameterizedProcedure<(long patreonConnectionId, string fullName, decimal? pledge)> UpdatePatreonConnectionProfile = new("`Connections.Patreon.usp_UpdatePatreonConnectionProfile`", (args, parms) =>
            {
                parms.Add("p_PatreonConnectionId", args.patreonConnectionId, DbType.Int64);
                parms.Add("p_FullName", args.fullName, DbType.String, size: 256);
                parms.Add("p_Pledge", args.pledge, DbType.Decimal);
                return parms;
            });

            /// <summary>
            /// Updates a Patreon connection's tokens.
            /// </summary>
            public static readonly ParameterizedProcedure<(long patreonConnectionId, string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope)> UpdatePatreonConnectionTokens = new("`Connections.Patreon.usp_UpdatePatreonConnectionTokens`", (args, parms) =>
            {
                parms.Add("p_PatreonConnectionId", args.patreonConnectionId, DbType.Int64);
                parms.Add("p_RefreshToken", args.refreshToken, DbType.String, size: 512);
                parms.Add("p_AccessToken", args.accessToken, DbType.String, size: 512);
                parms.Add("p_TokenType", args.tokenType, DbType.String, size: 64);
                parms.Add("p_TokenExpiry", args.tokenExpiry, DbType.DateTime);
                parms.Add("p_Scope", args.scope, DbType.String, size: 1024);
                return parms;
            });
        }
    }

    /// <summary>
    /// Procedures related to client application management.
    /// </summary>
    public static class Clients
    {

        #region Client Management Procedures
        
        /// <summary>
        /// Registers a new client application.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<DateTime, (Guid clientId, string clientName, string clientSecretHash, string salt)> RegisterClient = new("`Clients.usp_RegisterClient`", (args, parms) =>
        {
            parms.Add("p_ClientId", args.clientId, DbType.Guid);
            parms.Add("p_ClientName", args.clientName, DbType.String, size: 255);
            parms.Add("p_ClientSecretHash", args.clientSecretHash, DbType.String, size: 255);
            parms.Add("p_Salt", args.salt, DbType.String, size: 255);
            return parms;
        });
        
        /// <summary>
        /// Deletes a client application.
        /// </summary>
        public static readonly ParameterizedProcedure<Guid> DeleteClient = new("`Clients.usp_DeleteClient`", (clientId, parms) =>
        {
            parms.Add("p_ClientId", clientId, DbType.Guid);
            return parms;
        });
        
        /// <summary>
        /// Selects all client applications.
        /// </summary>
        public static readonly QueryProcedure<ClientEntity> SelectAllClients = new("`Clients.usp_SelectAllClients`");
        
        /// <summary>
        /// Selects a client application by its ID.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<ClientEntity?, Guid> SelectClientById = new("`Clients.usp_SelectClientById`", (clientId, parms) =>
        {
            parms.Add("p_ClientId", clientId, DbType.Guid);
            return parms;
        });
        
        /// <summary>
        /// Selects a client application by its name.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<ClientEntity?, string> SelectClientByName = new("`Clients.usp_SelectClientByName`", (clientName, parms) =>
        {
            parms.Add("p_ClientName", clientName, DbType.String, size: 255);
            return parms;
        });
        
        /// <summary>
        /// Updates a client's name.
        /// </summary>
        public static readonly ParameterizedProcedure<(Guid clientId, string newClientName)> UpdateClientName = new("`Clients.usp_UpdateClientName`", (args, parms) =>
        {
            parms.Add("p_ClientId", args.clientId, DbType.Guid);
            parms.Add("p_NewClientName", args.newClientName, DbType.String, size: 255);
            return parms;
        });
        
        /// <summary>
        /// Updates a client's secret hash and salt.
        /// </summary>
        public static readonly ParameterizedProcedure<(Guid clientId, string newClientSecretHash, string newSalt)> UpdateClientSecret = new("`Clients.usp_UpdateClientSecret`", (args, parms) =>
        {
            parms.Add("p_ClientId", args.clientId, DbType.Guid);
            parms.Add("p_NewSecretHash", args.newClientSecretHash, DbType.String, size: 255);
            parms.Add("p_NewSalt", args.newSalt, DbType.String, size: 255);
            return parms;
        });
        
        /// <summary>
        /// Verifies client credentials.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<bool, (Guid clientId, string secretHash, string salt)> VerifyClientCredentials = new("`Clients.usp_VerifyClient`", (args, parms) =>
        {
            parms.Add("p_ClientId", args.clientId, DbType.Guid);
            parms.Add("p_ClientSecretHash", args.secretHash, DbType.String, size: 255);
            parms.Add("p_Salt", args.salt, DbType.String, size: 255);
            return parms;
        });
        
        #endregion

        #region Client Role Procedures
        
        /// <summary>
        /// Clears all roles for a client.
        /// </summary>
        public static readonly ParameterizedProcedure<Guid> ClearClientRoles = new("`Clients.usp_ClearClientRoles`", (clientId, parms) =>
        {
            parms.Add("p_ClientId", clientId, DbType.Guid);
            return parms;
        });
        
        /// <summary>
        /// Deletes a role from a client.
        /// </summary>
        public static readonly ParameterizedProcedure<(Guid clientId, string roleName)> DeleteClientRole = new("`Clients.usp_DeleteClientRole`", (args, parms) =>
        {
            parms.Add("p_ClientId", args.clientId, DbType.Guid);
            parms.Add("p_RoleName", args.roleName, DbType.String, size: 255);
            return parms;
        });
        
        /// <summary>
        /// Inserts a role for a client.
        /// </summary>
        public static readonly ParameterizedProcedure<(Guid clientId, string roleName)> InsertClientRole = new("`Clients.usp_InsertClientRole`", (args, parms) =>
        {
            parms.Add("p_ClientId", args.clientId, DbType.Guid);
            parms.Add("p_RoleName", args.roleName, DbType.String, size: 255);
            return parms;
        });
        
        /// <summary>
        /// Selects all roles for a client.
        /// </summary>
        public static readonly ParameterizedQueryProcedure<ClientRoleEntity, Guid> SelectClientRoles = new("`Clients.usp_SelectClientRoles`", (clientId, parms) =>
        {
            parms.Add("p_ClientId", clientId, DbType.Guid);
            return parms;
        });

        #endregion
        
    }

    /// <summary>
    /// Procedures related to build code management.
    /// </summary>
    public static class BuildCodes
    {
        /// <summary>
        /// Inserts a new build code.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<BuildCodeEntity?, (int listOrder, string buildCode)> InsertCode = new("`BuildCodes.usp_InsertCode`", (args, parms) =>
        {
            parms.Add("p_ListOrder", args.listOrder, DbType.Int32);
            parms.Add("p_BuildCode", args.buildCode, DbType.String, size: 4096);
            return parms;
        });

        /// <summary>
        /// Selects a build code by its ID.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<BuildCodeEntity?, long> SelectCode = new("`BuildCodes.usp_SelectCode`", (buildCodeId, parms) =>
        {
            parms.Add("p_CodeId", buildCodeId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Selects all build codes.
        /// </summary>
        public static readonly QueryProcedure<BuildCodeEntity> SelectCodes = new("`BuildCodes.usp_SelectCodes`");

        /// <summary>
        /// Deletes a build code by its ID.
        /// </summary>
        public static readonly ParameterizedProcedure<long> DeleteCode = new("`BuildCodes.usp_DeleteCode`", (buildCodeId, parms) =>
        {
            parms.Add("p_CodeId", buildCodeId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Updates a build code.
        /// </summary>
        public static readonly ParameterizedProcedure<(long buildCodeId, int listOrder, string buildCode)> UpdateCode = new("`BuildCodes.usp_UpdateCode`", (args, parms) =>
        {
            parms.Add("p_CodeId", args.buildCodeId, DbType.Int64);
            parms.Add("p_ListOrder", args.listOrder, DbType.Int32);
            parms.Add("p_BuildCode", args.buildCode, DbType.String, size: 4096);
            return parms;
        });
    }

    /// <summary>
    /// Procedures related to builder applications.
    /// </summary>
    public static class BuildApps
    {
        /// <summary>
        /// Inserts a new builder application.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<ApplicationEntity, (long userId, int userAge, string? userNationality, string? additionalBuildingInformation, string whyJoinGreenfield, string? additionalComments)> InsertApplication = new("`BuildApps.usp_InsertApplication`", (args, parms) =>
        {
            parms.Add("p_UserId", args.userId, DbType.Int64);
            parms.Add("p_UserAge", args.userAge, DbType.Int32);
            parms.Add("p_UserNationality", args.userNationality, DbType.String, size: 128);
            parms.Add("p_AdditionalBuildingInformation", args.additionalBuildingInformation, DbType.String, size: 4096);
            parms.Add("p_WhyJoinGreenfield", args.whyJoinGreenfield, DbType.String, size: 4096);
            parms.Add("p_AdditionalComments", args.additionalComments, DbType.String, size: 4096);
            return parms;
        });

        /// <summary>
        /// Inserts a new application status.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<ApplicationStatusEntity, (long applicationId, string status, string? statusMessage)> InsertApplicationStatus = new("`BuildApps.usp_InsertApplicationStatus`", (args, parms) =>
        {
            parms.Add("p_ApplicationId", args.applicationId, DbType.Int64);
            parms.Add("p_Status", args.status, DbType.String, size: 256);
            parms.Add("p_StatusMessage", args.statusMessage, DbType.String, size: 2048);
            return parms;
        });

        /// <summary>
        /// Inserts a new image link.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<ApplicationImageLinkEntity, (long applicationId, string linkType, string imageLink)> InsertImageLink = new("`BuildApps.usp_InsertImageLink`", (args, parms) =>
        {
            parms.Add("p_ApplicationId", args.applicationId, DbType.Int64);
            parms.Add("p_LinkType", args.linkType, DbType.String, size: 256);
            parms.Add("p_ImageLink", args.imageLink, DbType.String, size: 2048);
            return parms;
        });

        public static readonly ParameterizedQuerySingleProcedure<ApplicationImageLinkEntity, (long imageLinkId, string newImageLink, string newLinkType)> UpdateImageLink = new("`BuildApps.usp_UpdateImageLink`", (args, parms) =>
        {
            parms.Add("p_ImageLinkId", args.imageLinkId, DbType.Int64);
            parms.Add("p_ImageLink", args.newImageLink, DbType.String, size: 2048);
            parms.Add("p_LinkType", args.newLinkType, DbType.String, size: 256);
            return parms;
        });
        
        /// <summary>
        /// Selects a builder application by its ID.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<ApplicationEntity, long> SelectApplicationById = new("`BuildApps.usp_SelectApplicationById`", (applicationId, parms) =>
        {
            parms.Add("p_ApplicationId", applicationId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Selects image links for a builder application.
        /// </summary>
        public static readonly ParameterizedQueryProcedure<ApplicationImageLinkEntity, long> SelectApplicationImages = new("`BuildApps.usp_SelectApplicationImages`", (applicationId, parms) =>
        {
            parms.Add("p_ApplicationId", applicationId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Selects builder applications by user ID.
        /// </summary>
        public static readonly ParameterizedQueryProcedure<ApplicationEntity, long> SelectApplicationsByUser = new("`BuildApps.usp_SelectApplicationsByUser`", (userId, parms) =>
        {
            parms.Add("p_UserId", userId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Selects application statuses for a builder application.
        /// </summary>
        public static readonly ParameterizedQueryProcedure<ApplicationStatusEntity, long> SelectApplicationStatuses = new("`BuildApps.usp_SelectApplicationStatuses`", (applicationId, parms) =>
        {
            parms.Add("p_ApplicationId", applicationId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Selects applications with their latest status by user ID.
        /// </summary>
        public static readonly ParameterizedQueryProcedure<LatestApplicationStatusEntity, long> SelectApplicationsWithLatestStatusByUser = new("`BuildApps.usp_SelectApplicationsWithLatestStatusByUser`", (userId, parms) =>
        {
            parms.Add("p_UserId", userId, DbType.Int64);
            return parms;
        });
    }

    /// <summary>
    /// Procedures related to redblock management.
    /// </summary>
    public static class Redblocks
    {
        /// <summary>
        /// Inserts a new redblock project.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<RedblockProjectEntity?, (string projectName, string projectKey)> InsertProject = new("`Redblocks.usp_InsertProject`", (args, parms) =>
        {
            parms.Add("p_ProjectName", args.projectName, DbType.String, size: 64);
            parms.Add("p_ProjectKey", args.projectKey, DbType.String, size: 6);
            return parms;
        });
        
        /// <summary>
        /// Updates a redblock project's name.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<RedblockProjectEntity?, (long projectId, string projectName)> UpdateProject = new("`Redblocks.usp_UpdateProject`", (args, parms) =>
        {
            parms.Add("p_ProjectId", args.projectId, DbType.Int64);
            parms.Add("p_ProjectName", args.projectName, DbType.String, size: 64);
            return parms;
        });

        /// <summary>
        /// Selects all redblock projects.
        /// </summary>
        public static readonly QueryProcedure<RedblockProjectEntity> SelectProjects = new("`Redblocks.usp_SelectProjects`");

        /// <summary>
        /// Selects a redblock project by its ID.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<RedblockProjectEntity?, long> SelectProjectById = new("`Redblocks.usp_SelectProjectById`", (projectId, parms) =>
        {
            parms.Add("p_ProjectId", projectId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Selects a redblock project by its key.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<RedblockProjectEntity?, string> SelectProjectByKey = new("`Redblocks.usp_SelectProjectByKey`", (projectKey, parms) =>
        {
            parms.Add("p_ProjectKey", projectKey, DbType.String, size: 6);
            return parms;
        });
        
        /// <summary>
        /// Inserts a new redblock.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<RedblockEntity?, (long projectId, string message, int x, int y, int z, long createdBy)> InsertRedblock = new("`Redblocks.usp_InsertRedblock`", (args, parms) =>
        {
            parms.Add("p_ProjectId", args.projectId, DbType.Int64);
            parms.Add("p_Message", args.message, DbType.String, size: 1024);
            parms.Add("p_X", args.x, DbType.Int32);
            parms.Add("p_Y", args.y, DbType.Int32);
            parms.Add("p_Z", args.z, DbType.Int32);
            parms.Add("p_CreatedBy", args.createdBy, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Selects a redblock by project and key number.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<RedblockEntity?, (long projectId, long keyNumber)> SelectRedblockByKey = new("`Redblocks.usp_SelectRedblockByKey`", (args, parms) =>
        {
            parms.Add("p_ProjectId", args.projectId, DbType.Int64);
            parms.Add("p_KeyNumber", args.keyNumber, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Selects a redblock by its internal ID.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<RedblockEntity?, long> SelectRedblockById = new("`Redblocks.usp_SelectRedblockById`", (redblockId, parms) =>
        {
            parms.Add("p_RedblockId", redblockId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Selects redblocks in a project with optional filters.
        /// </summary>
        public static readonly ParameterizedQueryProcedure<RedblockEntity, (long projectId, string? statusFilter, string? deletionFilter, string? userAssignmentFilter, string? roleAssignmentFilter, string? messageFilter)> SelectRedblocksByProject = new("`Redblocks.usp_SelectRedblocksByProject`", (args, parms) =>
        {
            parms.Add("p_ProjectId", args.projectId, DbType.Int64);
            parms.Add("p_StatusFilter", args.statusFilter, DbType.String, size: 8192);
            parms.Add("p_DeletionFilter", args.deletionFilter, DbType.String, size: 8192);
            parms.Add("p_UserAssignmentFilter", args.userAssignmentFilter, DbType.String, size: 8192);
            parms.Add("p_RoleAssignmentFilter", args.roleAssignmentFilter, DbType.String, size: 8192);
            parms.Add("p_MessageFilter", args.messageFilter, DbType.String, size: 2048);
            return parms;
        });

        /// <summary>
        /// Selects status rows for a redblock by internal ID.
        /// </summary>
        public static readonly ParameterizedQueryProcedure<RedblockStatusEntity, long> SelectRedblockStatuses = new("`Redblocks.usp_SelectRedblockStatuses`", (redblockId, parms) =>
        {
            parms.Add("p_RedblockId", redblockId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Selects user assignments for a redblock by internal ID.
        /// </summary>
        public static readonly ParameterizedQueryProcedure<RedblockUserAssignmentEntity, long> SelectRedblockUserAssignments = new("`Redblocks.usp_SelectRedblockUserAssignments`", (redblockId, parms) =>
        {
            parms.Add("p_RedblockId", redblockId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Selects role assignments for a redblock by internal ID.
        /// </summary>
        public static readonly ParameterizedQueryProcedure<RedblockRoleAssignmentEntity, long> SelectRedblockRoleAssignments = new("`Redblocks.usp_SelectRedblockRoleAssignments`", (redblockId, parms) =>
        {
            parms.Add("p_RedblockId", redblockId, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Updates a redblock message.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<RedblockEntity?, (long projectId, long keyNumber, string message, long updatedBy)> UpdateRedblockMessage = new("`Redblocks.usp_UpdateRedblockMessage`", (args, parms) =>
        {
            parms.Add("p_ProjectId", args.projectId, DbType.Int64);
            parms.Add("p_KeyNumber", args.keyNumber, DbType.Int64);
            parms.Add("p_Message", args.message, DbType.String, size: 1024);
            parms.Add("p_UpdatedBy", args.updatedBy, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Soft deletes a redblock.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<RedblockEntity?, (long projectId, long keyNumber, long deletedBy)> SoftDeleteRedblock = new("`Redblocks.usp_SoftDeleteRedblock`", (args, parms) =>
        {
            parms.Add("p_ProjectId", args.projectId, DbType.Int64);
            parms.Add("p_KeyNumber", args.keyNumber, DbType.Int64);
            parms.Add("p_DeletedBy", args.deletedBy, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Inserts a status for a redblock.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<RedblockStatusEntity?, (long projectId, long keyNumber, string status, long createdBy)> InsertStatus = new("`Redblocks.usp_InsertStatus`", (args, parms) =>
        {
            parms.Add("p_ProjectId", args.projectId, DbType.Int64);
            parms.Add("p_KeyNumber", args.keyNumber, DbType.Int64);
            parms.Add("p_Status", args.status, DbType.String, size: 32);
            parms.Add("p_CreatedBy", args.createdBy, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Inserts a user assignment for a redblock.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<RedblockUserAssignmentEntity?, (long projectId, long keyNumber, long assignedTo, long createdBy)> InsertUserAssignment = new("`Redblocks.usp_InsertUserAssignment`", (args, parms) =>
        {
            parms.Add("p_ProjectId", args.projectId, DbType.Int64);
            parms.Add("p_KeyNumber", args.keyNumber, DbType.Int64);
            parms.Add("p_AssignedTo", args.assignedTo, DbType.Int64);
            parms.Add("p_CreatedBy", args.createdBy, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Deletes a user assignment from a redblock.
        /// </summary>
        public static readonly ParameterizedProcedure<(long projectId, long keyNumber, long assignedTo)> DeleteUserAssignment = new("`Redblocks.usp_DeleteUserAssignment`", (args, parms) =>
        {
            parms.Add("p_ProjectId", args.projectId, DbType.Int64);
            parms.Add("p_KeyNumber", args.keyNumber, DbType.Int64);
            parms.Add("p_AssignedTo", args.assignedTo, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Inserts a role assignment for a redblock.
        /// </summary>
        public static readonly ParameterizedQuerySingleProcedure<RedblockRoleAssignmentEntity?, (long projectId, long keyNumber, string roleName, long createdBy)> InsertRoleAssignment = new("`Redblocks.usp_InsertRoleAssignment`", (args, parms) =>
        {
            parms.Add("p_ProjectId", args.projectId, DbType.Int64);
            parms.Add("p_KeyNumber", args.keyNumber, DbType.Int64);
            parms.Add("p_RoleName", args.roleName, DbType.String, size: 32);
            parms.Add("p_CreatedBy", args.createdBy, DbType.Int64);
            return parms;
        });

        /// <summary>
        /// Deletes a role assignment from a redblock.
        /// </summary>
        public static readonly ParameterizedProcedure<(long projectId, long keyNumber, string roleName)> DeleteRoleAssignment = new("`Redblocks.usp_DeleteRoleAssignment`", (args, parms) =>
        {
            parms.Add("p_ProjectId", args.projectId, DbType.Int64);
            parms.Add("p_KeyNumber", args.keyNumber, DbType.Int64);
            parms.Add("p_RoleName", args.roleName, DbType.String, size: 32);
            return parms;
        });
    }
}