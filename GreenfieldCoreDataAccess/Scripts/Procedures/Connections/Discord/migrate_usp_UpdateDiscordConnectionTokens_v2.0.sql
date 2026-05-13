-- DependsOn: ScriptHistory, DiscordConnections
drop procedure if exists `Connections.Discord.usp_UpdateDiscordConnectionTokens`;
create procedure if not exists `Connections.Discord.usp_UpdateDiscordConnectionTokens`(
    p_DiscordConnectionId bigint,
    p_RefreshToken varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_AccessToken varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_TokenType varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_TokenExpiry datetime,
    p_Scope varchar(1024) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci
)
begin
    update `Connections.DiscordConnections`
    set RefreshToken = p_RefreshToken,
        AccessToken = p_AccessToken,
        TokenType = p_TokenType,
        TokenExpiry = p_TokenExpiry,
        Scope = p_Scope
    where DiscordConnectionId = p_DiscordConnectionId;
end;


