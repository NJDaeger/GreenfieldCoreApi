-- DependsOn: ScriptHistory, Redblocks
create procedure if not exists `Redblocks.usp_SelectRedblockById`(
    p_RedblockId bigint)
begin
    select
        rb.RedblockId,
        rb.ProjectId,
        rb.KeyNumber,
        rb.Message,
        (select rs.Status from `Redblocks.Statuses` rs where rs.RedblockId = rb.RedblockId order by rs.CreatedOn desc limit 1) as Status,
        rb.X,
        rb.Y,
        rb.Z,
        rb.CreatedBy,
        rb.CreatedOn,
        rb.UpdatedBy,
        rb.UpdatedOn,
        rb.DeletedBy,
        rb.DeletedOn
    from `Redblocks.Redblocks` rb
    where rb.RedblockId = p_RedblockId;
end;

