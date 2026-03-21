-- DependsOn: ScriptHistory, Redblocks, Statuses
create procedure if not exists `Redblocks.usp_SelectRedblockStatuses`(
    p_RedblockId bigint)
begin
    select
        s.StatusId,
        s.RedblockId,
        s.Status,
        s.CreatedBy,
        s.CreatedOn
    from `Redblocks.Statuses` s
    where s.RedblockId = p_RedblockId
    order by s.CreatedOn desc, s.StatusId desc;
end;

