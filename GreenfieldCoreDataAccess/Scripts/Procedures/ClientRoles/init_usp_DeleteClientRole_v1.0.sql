-- DependsOn: ScriptHistory, ClientRoles
create procedure if not exists usp_DeleteClientRole(
    p_ClientId char(36),
    p_RoleName nvarchar(255))
begin
    delete from ClientRoles
    where ClientId = p_ClientId
    and RoleName = p_RoleName;
end;