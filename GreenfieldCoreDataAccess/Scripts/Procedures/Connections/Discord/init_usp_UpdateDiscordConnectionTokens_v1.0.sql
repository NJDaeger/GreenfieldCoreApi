-- DependsOn: ScriptHistory, DiscordConnections
create procedure if not exists `Connections.Discord.usp_UpdateDiscordConnectionTokens`(
    p_DiscordConnectionId bigint,
    p_RefreshToken nvarchar(512),
    p_AccessToken nvarchar(512),
    p_TokenType nvarchar(64),
    p_TokenExpiry datetime,
    p_Scope nvarchar(1024)
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

