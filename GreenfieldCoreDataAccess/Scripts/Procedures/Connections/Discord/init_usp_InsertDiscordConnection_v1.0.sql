-- DependsOn: ScriptHistory, DiscordConnections
create procedure if not exists `Connections.Discord.usp_InsertDiscordConnection`(
    p_RefreshToken nvarchar(512),
    p_AccessToken nvarchar(512),
    p_TokenType nvarchar(64),
    p_TokenExpiry datetime,
    p_Scope nvarchar(1024),
    p_DiscordSnowflake bigint unsigned,
    p_DiscordUsername nvarchar(256)
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

