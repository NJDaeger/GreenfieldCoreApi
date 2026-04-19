-- DependsOn: ScriptHistory, Redblocks, Entities
create procedure if not exists `Redblocks.usp_SelectRedblockEntities`(
    p_RedblockId bigint)
begin
    select
        e.EntityGuid
    from `Redblocks.Entities` e
    where e.RedblockId = p_RedblockId;
end;

