-- DependsOn: ScriptHistory, Projects, Redblocks, UserAssignments
create procedure if not exists `Redblocks.usp_InsertUserAssignment`(
    p_ProjectId bigint,
    p_KeyNumber bigint,
    p_AssignedTo bigint,
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
        insert ignore into `Redblocks.UserAssignments` (
            RedblockId,
            AssignedTo,
            CreatedBy)
        values (
            v_RedblockId,
            p_AssignedTo,
            p_CreatedBy);

        select
            ua.UserAssignmentId,
            ua.RedblockId,
            ua.AssignedTo,
            ua.CreatedBy,
            ua.CreatedOn
        from `Redblocks.UserAssignments` ua
        where ua.RedblockId = v_RedblockId
          and ua.AssignedTo = p_AssignedTo
        order by ua.UserAssignmentId desc
        limit 1;
    end if;
end;

