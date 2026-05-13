-- DependsOn: ScriptHistory, Projects, Redblocks, Statuses
create procedure if not exists `Redblocks.usp_InsertStatus`(
    p_ProjectId bigint,
    p_KeyNumber bigint,
    p_Status varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_CreatedBy bigint)
begin
    declare v_RedblockId bigint;

    select rb.RedblockId
    into v_RedblockId
    from `Redblocks.Redblocks` rb
    where rb.ProjectId = p_ProjectId
      and rb.KeyNumber = p_KeyNumber
      and rb.DeletedOn is null;

    if v_RedblockId is not null then
        insert into `Redblocks.Statuses` (
            RedblockId,
            Status,
            CreatedBy)
        values (
            v_RedblockId,
            p_Status,
            p_CreatedBy);

        if row_count() > 0 then
            select
                s.StatusId,
                rb.ProjectId,
                rb.KeyNumber,
                s.RedblockId,
                s.Status,
                s.CreatedBy,
                s.CreatedOn
            from `Redblocks.Statuses` s
            join `Redblocks.Redblocks` rb on rb.RedblockId = s.RedblockId
            where s.StatusId = last_insert_id();
        end if;
    end if;
end;

