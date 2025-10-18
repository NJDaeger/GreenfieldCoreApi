-- DependsOn: ScriptHistory, ClientRoles
create procedure if not exists usp_SelectClientRoles(
    p_ClientId char(36))
begin
    select cr.ClientRoleId, cr.ClientId, cr.RoleName, cr.CreatedOn
    from ClientRoles cr
    where cr.ClientId = p_ClientId;
end;