-- DependsOn: ScriptHistory, Redblocks
create procedure if not exists `Redblocks.usp_SelectRedblockById`(
    p_RedblockId bigint)
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
        rb.DeletedOn
    from `Redblocks.Redblocks` rb
    where rb.RedblockId = p_RedblockId;
end;

