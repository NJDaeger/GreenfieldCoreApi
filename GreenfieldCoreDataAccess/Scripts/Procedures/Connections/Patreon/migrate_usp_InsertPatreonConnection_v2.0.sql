-- DependsOn: ScriptHistory, PatreonConnections
drop procedure if exists `Connections.Patreon.usp_InsertPatreonConnection`;
create procedure if not exists `Connections.Patreon.usp_InsertPatreonConnection`(
    p_RefreshToken varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_AccessToken varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_TokenType varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_TokenExpiry datetime,
    p_Scope varchar(1024) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_PatreonId bigint,
    p_FullName varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_Pledge decimal
)
begin
    insert ignore into `Connections.PatreonConnections` (
        RefreshToken,
        AccessToken,
        TokenType,
        TokenExpiry,
        Scope,
        PatreonId,
        FullName,
        Pledge
    ) values (
        p_RefreshToken,
        p_AccessToken,
        p_TokenType,
        p_TokenExpiry,
        p_Scope,
        p_PatreonId,
        p_FullName,
        p_Pledge
    );

    if row_count() > 0 then
        select pc.PatreonConnectionId, pc.RefreshToken, pc.AccessToken, pc.TokenType, pc.TokenExpiry, pc.Scope,
               pc.PatreonId, pc.FullName, pc.Pledge, pc.UpdatedOn, pc.CreatedOn
        from `Connections.PatreonConnections` pc
        where pc.PatreonId = p_PatreonId;
    end if;
end;


