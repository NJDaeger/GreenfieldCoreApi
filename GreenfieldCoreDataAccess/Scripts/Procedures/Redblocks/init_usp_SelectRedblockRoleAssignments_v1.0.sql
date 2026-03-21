-- DependsOn: ScriptHistory, Redblocks, RoleAssignments
create procedure if not exists `Redblocks.usp_SelectRedblockRoleAssignments`(
    p_RedblockId bigint)
begin
    select
        ra.RoleAssignmentId,
        ra.RedblockId,
        ra.RoleName,
        ra.CreatedBy,
        ra.CreatedOn
    from `Redblocks.RoleAssignments` ra
    where ra.RedblockId = p_RedblockId
    order by ra.CreatedOn desc, ra.RoleAssignmentId desc;
end;

