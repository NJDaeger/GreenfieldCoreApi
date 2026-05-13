-- DependsOn: ScriptHistory, DiscordConnections
drop procedure if exists `Connections.Discord.usp_InsertDiscordConnection`;
create procedure if not exists `Connections.Discord.usp_InsertDiscordConnection`(
    p_RefreshToken varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_AccessToken varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_TokenType varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_TokenExpiry datetime,
    p_Scope varchar(1024) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_DiscordSnowflake bigint unsigned,
    p_DiscordUsername varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci
)
begin
    insert ignore into `Connections.DiscordConnections` (
        RefreshToken,
        AccessToken,
        TokenType,
        TokenExpiry,
        Scope,
        DiscordSnowflake,
        DiscordUsername
    ) values (
        p_RefreshToken,
        p_AccessToken,
        p_TokenType,
        p_TokenExpiry,
        p_Scope,
        p_DiscordSnowflake,
        p_DiscordUsername
    );

    if row_count() > 0 then
        select dc.DiscordConnectionId, dc.RefreshToken, dc.AccessToken, dc.TokenType, dc.TokenExpiry, dc.Scope,
               dc.DiscordSnowflake, dc.DiscordUsername, dc.UpdatedOn, dc.CreatedOn
        from `Connections.DiscordConnections` dc
        where dc.DiscordSnowflake = p_DiscordSnowflake;
    end if;
end;


