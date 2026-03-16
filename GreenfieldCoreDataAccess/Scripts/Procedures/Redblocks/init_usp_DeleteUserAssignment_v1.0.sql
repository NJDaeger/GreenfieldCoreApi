-- DependsOn: ScriptHistory, Projects, Redblocks, UserAssignments
create procedure if not exists `Redblocks.usp_DeleteUserAssignment`(
    p_ProjectId bigint,
    p_KeyNumber bigint,
    p_AssignedTo bigint)
begin
    delete ua
    from `Redblocks.UserAssignments` ua
    join `Redblocks.Redblocks` rb on rb.RedblockId = ua.RedblockId
    where rb.ProjectId = p_ProjectId
      and rb.KeyNumber = p_KeyNumber
      and ua.AssignedTo = p_AssignedTo;
end;

