-- DependsOn: ScriptHistory, PatreonConnections
drop procedure if exists `Connections.Patreon.usp_UpdatePatreonConnectionProfile`;
create procedure if not exists `Connections.Patreon.usp_UpdatePatreonConnectionProfile`(
    p_PatreonConnectionId bigint,
    p_FullName varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_Pledge decimal
)
begin
    update `Connections.PatreonConnections`
    set FullName = p_FullName,
        Pledge = p_Pledge
    where PatreonConnectionId = p_PatreonConnectionId;
end;


