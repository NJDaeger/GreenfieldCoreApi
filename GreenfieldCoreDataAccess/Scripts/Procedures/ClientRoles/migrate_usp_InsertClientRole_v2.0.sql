-- DependsOn: ScriptHistory, ClientRoles
drop procedure if exists `Clients.usp_InsertClientRole`;
create procedure if not exists `Clients.usp_InsertClientRole`(
    p_ClientId char(36),
    p_RoleName varchar(255))
begin
    insert into `Clients.ClientRoles` (ClientId, RoleName)
    values (p_ClientId, p_RoleName);
end;

