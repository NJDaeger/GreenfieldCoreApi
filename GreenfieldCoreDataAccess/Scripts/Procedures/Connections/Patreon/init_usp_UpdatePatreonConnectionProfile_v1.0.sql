-- DependsOn: ScriptHistory, PatreonConnections
create procedure if not exists `Connections.Patreon.usp_UpdatePatreonConnectionProfile`(
    p_PatreonConnectionId bigint,
    p_FullName nvarchar(256),
    p_Pledge decimal
)
begin
    update `Connections.PatreonConnections`
    set FullName = p_FullName,
        Pledge = p_Pledge
    where PatreonConnectionId = p_PatreonConnectionId;
end;

