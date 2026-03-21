-- DependsOn: ScriptHistory, Redblocks, UserAssignments
create procedure if not exists `Redblocks.usp_SelectRedblockUserAssignments`(
    p_RedblockId bigint)
begin
    select
        ua.UserAssignmentId,
        ua.RedblockId,
        ua.AssignedTo,
        ua.CreatedBy,
        ua.CreatedOn
    from `Redblocks.UserAssignments` ua
    where ua.RedblockId = p_RedblockId
    order by ua.CreatedOn desc, ua.UserAssignmentId desc;
end;

