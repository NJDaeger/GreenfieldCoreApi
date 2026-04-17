-- DependsOn: ScriptHistory, Projects, Redblocks, Statuses
create procedure if not exists `Redblocks.usp_InsertRedblock`(
    p_ProjectId bigint,
    p_Message varchar(1024),
    p_X int,
    p_Y int,
    p_Z int,
    p_CreatedBy bigint)
begin
    declare v_NextKeyNumber bigint;
    declare v_RedblockId bigint;

    declare exit handler for sqlexception
    begin
        rollback;
        resignal;
    end;

    start transaction;

    update `Redblocks.Projects` p
    set p.LastUsedRedblockKeyNumber = p.LastUsedRedblockKeyNumber + 1
    where p.ProjectId = p_ProjectId;

    if row_count() = 0 then
        rollback;
    else
        select p.LastUsedRedblockKeyNumber
        into v_NextKeyNumber
        from `Redblocks.Projects` p
        where p.ProjectId = p_ProjectId;

        insert into `Redblocks.Redblocks` (
            ProjectId,
            KeyNumber,
            Message,
            X,
            Y,
            Z,
            CreatedBy)
        values (
            p_ProjectId,
            v_NextKeyNumber,
            p_Message,
            p_X,
            p_Y,
            p_Z,
            p_CreatedBy);

        if row_count() = 0 then
            rollback;
        else
            set v_RedblockId = last_insert_id();
            commit;

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
                rb.UpdatedBy,
                rb.UpdatedOn,
                rb.DeletedBy,
                rb.DeletedOn
            from `Redblocks.Redblocks` rb
            where rb.RedblockId = v_RedblockId;
        end if;
    end if;
end;

