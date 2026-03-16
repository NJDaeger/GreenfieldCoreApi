-- DependsOn: ScriptHistory, Projects, Redblocks, Statuses
create procedure if not exists `Redblocks.usp_SelectRedblockByKey`(
    p_ProjectId bigint,
    p_KeyNumber bigint)
begin
    select
        rb.RedblockId,
        rb.ProjectId,
        rb.KeyNumber,
        rb.Message,
        rb.X,
        rb.Y,
        rb.Z,
        rb.CreatedBy,
        rb.CreatedOn,
        rb.DeletedBy,
        rb.DeletedOn,
        ls.StatusId as LatestStatusId,
        ls.Status as LatestStatus,
        ls.CreatedBy as LatestStatusCreatedBy,
        ls.CreatedOn as LatestStatusCreatedOn
    from `Redblocks.Redblocks` rb
    left join (
        select
            ranked.StatusId,
            ranked.RedblockId,
            ranked.Status,
            ranked.CreatedBy,
            ranked.CreatedOn
        from (
            select
                s.*,
                row_number() over (partition by s.RedblockId order by s.CreatedOn desc, s.StatusId desc) as rn
            from `Redblocks.Statuses` s
        ) ranked
        where ranked.rn = 1
    ) ls on ls.RedblockId = rb.RedblockId
    where rb.ProjectId = p_ProjectId
      and rb.KeyNumber = p_KeyNumber;
end;

