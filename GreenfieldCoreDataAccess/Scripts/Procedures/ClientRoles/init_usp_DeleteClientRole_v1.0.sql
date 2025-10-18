-- DependsOn: ScriptHistory, ClientRoles
create procedure if not exists usp_DeleteClientRole(
    p_ClientId char(36),
    p_RoleName nvarchar(255))
begin
    create temporary table if not exists TempDeletedClientRole (
        ClientRoleId bigint,
        ClientId char(36),
        RoleName nvarchar(255),
        CreatedOn datetime
    );
    insert into TempDeletedClientRole (ClientRoleId, ClientId, RoleName, CreatedOn)
    select cr.ClientRoleId, cr.ClientId, cr.RoleName, cr.CreatedOn
    from ClientRoles cr
    where cr.ClientId = p_ClientId
    and cr.RoleName = p_RoleName;
    
    delete from ClientRoles
    where ClientRoleId = v_ClientRoleId;
    
    if row_count() > 0 then
        select tcr.* from TempDeletedClientRole tcr limit 1;
    end if;
    
    drop temporary table if exists TempDeletedClientRole;
end;