-- DependsOn: ScriptHistory, DiscordConnections
create procedure if not exists `Connections.Discord.usp_UpdateDiscordConnectionTokens`(
    p_DiscordConnectionId bigint,
    p_RefreshToken varchar(512),
    p_AccessToken varchar(512),
    p_TokenType varchar(64),
    p_TokenExpiry datetime,
    p_Scope varchar(1024)
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

