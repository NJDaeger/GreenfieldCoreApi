-- DependsOn: ScriptHistory, ClientRoles
create procedure if not exists usp_InsertClientRole(
    p_ClientId char(36),
    p_RoleName nvarchar(255))
begin
    insert into ClientRoles (ClientId, RoleName)
    values (p_ClientId, p_RoleName);
end;