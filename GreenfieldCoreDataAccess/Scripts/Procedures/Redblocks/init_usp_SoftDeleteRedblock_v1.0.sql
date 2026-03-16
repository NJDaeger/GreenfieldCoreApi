-- DependsOn: ScriptHistory, Projects, Redblocks, Statuses
create procedure if not exists `Redblocks.usp_SoftDeleteRedblock`(
    p_ProjectId bigint,
    p_KeyNumber bigint,
    p_DeletedBy bigint)
begin
    update `Redblocks.Redblocks` rb
    set
        rb.DeletedBy = p_DeletedBy,
        rb.DeletedOn = current_timestamp
    where rb.ProjectId = p_ProjectId
      and rb.KeyNumber = p_KeyNumber
      and rb.DeletedOn is null;

    if row_count() > 0 then
        call `Redblocks.usp_SelectRedblockByKey`(p_ProjectId, p_KeyNumber);
    end if;
end;

