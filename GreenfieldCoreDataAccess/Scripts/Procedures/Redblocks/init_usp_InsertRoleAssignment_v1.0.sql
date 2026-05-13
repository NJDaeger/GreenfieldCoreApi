-- DependsOn: ScriptHistory, Projects, Redblocks, RoleAssignments
create procedure if not exists `Redblocks.usp_InsertRoleAssignment`(
    p_ProjectId bigint,
    p_KeyNumber bigint,
    p_RoleName varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_CreatedBy bigint)
begin
    declare v_RedblockId bigint;

    select rb.RedblockId
    into v_RedblockId
    from `Redblocks.Redblocks` rb
    where rb.ProjectId = p_ProjectId
      and rb.KeyNumber = p_KeyNumber
      and rb.DeletedOn is null;

    if v_RedblockId is not null then
        insert ignore into `Redblocks.RoleAssignments` (
            RedblockId,
            RoleName,
            CreatedBy)
        values (
            v_RedblockId,
            p_RoleName,
            p_CreatedBy);

        select
            ra.RoleAssignmentId,
            rb.ProjectId,
            rb.KeyNumber,
            ra.RedblockId,
            ra.RoleName,
            ra.CreatedBy,
            ra.CreatedOn
        from `Redblocks.RoleAssignments` ra
        join `Redblocks.Redblocks` rb on rb.RedblockId = ra.RedblockId
        where ra.RedblockId = v_RedblockId
          and ra.RoleName = p_RoleName
        order by ra.RoleAssignmentId desc
        limit 1;
    end if;
end;

