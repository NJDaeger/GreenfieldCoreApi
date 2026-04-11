-- DependsOn: ScriptHistory, PatreonConnections
create procedure if not exists `Connections.Patreon.usp_UpdatePatreonConnectionTokens`(
    p_PatreonConnectionId bigint,
    p_RefreshToken nvarchar(512),
    p_AccessToken nvarchar(512),
    p_TokenType nvarchar(64),
    p_TokenExpiry datetime,
    p_Scope nvarchar(1024)
)
begin
    update `Connections.PatreonConnections`
    set RefreshToken = p_RefreshToken,
        AccessToken = p_AccessToken,
        TokenType = p_TokenType,
        TokenExpiry = p_TokenExpiry,
        Scope = p_Scope
    where PatreonConnectionId = p_PatreonConnectionId;
end;

