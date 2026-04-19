-- DependsOn: ScriptHistory, Projects, Redblocks, Entities
create procedure if not exists `Redblocks.usp_DeleteRedblockEntities`(
    p_ProjectId bigint,
    p_KeyNumber bigint)
begin
    delete e
    from `Redblocks.Entities` e
    join `Redblocks.Redblocks` rb on rb.RedblockId = e.RedblockId
    where rb.ProjectId = p_ProjectId
      and rb.KeyNumber = p_KeyNumber;
end;

