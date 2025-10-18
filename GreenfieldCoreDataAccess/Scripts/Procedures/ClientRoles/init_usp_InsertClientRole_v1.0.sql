-- DependsOn: ScriptHistory, ClientRoles
create procedure if not exists usp_InsertClientRole(
    p_ClientId char(36),
    p_RoleName nvarchar(255))
begin
    if (select not exists (select 1 from ClientRoles cr where cr.ClientId = p_ClientId and cr.RoleName = p_RoleName)) then
        insert into ClientRoles (ClientId, RoleName)
        values (p_ClientId, p_RoleName);

        select cr.ClientRoleId, cr.ClientId, cr.RoleName, cr.CreatedOn from ClientRoles cr
        where cr.ClientId = p_ClientId and cr.RoleName = cr.RoleName;
    end if;
end;