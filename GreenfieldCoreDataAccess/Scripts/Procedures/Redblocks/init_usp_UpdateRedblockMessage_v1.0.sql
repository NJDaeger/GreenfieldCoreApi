-- DependsOn: ScriptHistory, Projects, Redblocks, Statuses
create procedure if not exists `Redblocks.usp_UpdateRedblockMessage`(
    p_ProjectId bigint,
    p_KeyNumber bigint,
    p_Message nvarchar(1024))
begin
    update `Redblocks.Redblocks` rb
    set rb.Message = p_Message
    where rb.ProjectId = p_ProjectId
      and rb.KeyNumber = p_KeyNumber
      and rb.DeletedOn is null;

    if row_count() > 0 then
        call `Redblocks.usp_SelectRedblockByKey`(p_ProjectId, p_KeyNumber);
    end if;
end;

