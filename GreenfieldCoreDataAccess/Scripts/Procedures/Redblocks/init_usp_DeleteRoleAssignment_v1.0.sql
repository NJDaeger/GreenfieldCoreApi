-- DependsOn: ScriptHistory, Projects, Redblocks, RoleAssignments
create procedure if not exists `Redblocks.usp_DeleteRoleAssignment`(
    p_ProjectId bigint,
    p_KeyNumber bigint,
    p_RoleName varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci)
begin
    delete ra
    from `Redblocks.RoleAssignments` ra
    join `Redblocks.Redblocks` rb on rb.RedblockId = ra.RedblockId
    where rb.ProjectId = p_ProjectId
      and rb.KeyNumber = p_KeyNumber
      and ra.RoleName = p_RoleName;
end;

