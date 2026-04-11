-- DependsOn: ScriptHistory, ClientRoles
drop procedure if exists `Clients.usp_DeleteClientRole`;
create procedure if not exists `Clients.usp_DeleteClientRole`(
    p_ClientId char(36),
    p_RoleName varchar(255))
begin
    delete from `Clients.ClientRoles`
    where ClientId = p_ClientId
    and RoleName = p_RoleName;
end;

