-- DependsOn: ScriptHistory, ClientRoles
create procedure if not exists `Clients.usp_DeleteClientRole`(
    p_ClientId char(36),
    p_RoleName nvarchar(255))
begin
    delete from `Clients.ClientRoles`
    where ClientId = p_ClientId
    and RoleName = p_RoleName;
end;