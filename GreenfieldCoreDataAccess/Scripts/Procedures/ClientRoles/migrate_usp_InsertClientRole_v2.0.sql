-- DependsOn: ScriptHistory, ClientRoles
drop procedure if exists `Clients.usp_InsertClientRole`;
create procedure if not exists `Clients.usp_InsertClientRole`(
    p_ClientId char(36),
    p_RoleName varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci)
begin
    insert into `Clients.ClientRoles` (ClientId, RoleName)
    values (p_ClientId, p_RoleName);
end;

