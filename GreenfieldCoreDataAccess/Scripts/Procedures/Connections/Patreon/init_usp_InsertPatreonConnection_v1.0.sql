-- DependsOn: ScriptHistory, PatreonConnections
create procedure if not exists `Connections.Patreon.usp_InsertPatreonConnection`(
    p_RefreshToken nvarchar(512),
    p_AccessToken nvarchar(512),
    p_TokenType nvarchar(64),
    p_TokenExpiry datetime,
    p_Scope nvarchar(1024),
    p_PatreonId bigint,
    p_FullName nvarchar(256),
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

