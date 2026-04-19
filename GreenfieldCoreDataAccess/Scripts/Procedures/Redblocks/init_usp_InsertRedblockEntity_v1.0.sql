-- DependsOn: ScriptHistory, Projects, Redblocks, Entities
create procedure if not exists `Redblocks.usp_InsertRedblockEntity`(
    p_ProjectId bigint,
    p_KeyNumber bigint,
    p_EntityGuid char(36))
begin
    declare v_RedblockId bigint;

    select rb.RedblockId
    into v_RedblockId
    from `Redblocks.Redblocks` rb
    where rb.ProjectId = p_ProjectId
      and rb.KeyNumber = p_KeyNumber
      and rb.DeletedOn is null;

    if v_RedblockId is not null then
        insert ignore into `Redblocks.Entities` (
            RedblockId,
            EntityGuid)
        values (
            v_RedblockId,
            p_EntityGuid);

        select
            e.EntityGuid
        from `Redblocks.Entities` e
        where e.RedblockId = v_RedblockId
          and e.EntityGuid = p_EntityGuid
        limit 1;
    end if;
end;

