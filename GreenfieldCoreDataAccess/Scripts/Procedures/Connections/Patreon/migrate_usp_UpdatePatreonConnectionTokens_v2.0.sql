-- DependsOn: ScriptHistory, PatreonConnections
drop procedure if exists `Connections.Patreon.usp_UpdatePatreonConnectionTokens`;
create procedure if not exists `Connections.Patreon.usp_UpdatePatreonConnectionTokens`(
    p_PatreonConnectionId bigint,
    p_RefreshToken varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_AccessToken varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_TokenType varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_TokenExpiry datetime,
    p_Scope varchar(1024) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci
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


