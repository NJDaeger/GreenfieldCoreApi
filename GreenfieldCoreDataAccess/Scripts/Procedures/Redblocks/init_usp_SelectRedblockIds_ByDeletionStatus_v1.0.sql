-- DependsOn: ScriptHistory, Redblocks
create procedure if not exists `Redblocks.usp_SelectRedblockIds_ByDeletionStatus`(
    p_ProjectId bigint,
    p_DeletionFilter varchar(8192),
    p_DeletionFilterMatchType varchar(16),
    p_AllowedRedblockIds varchar(8192))
begin
    create temporary table TempAllowedRedblockIds
    (
        RedblockId bigint not null primary key
    );

    create temporary table TempDeletionFilterValues
    (
        UserId bigint not null primary key
    );

    if (p_AllowedRedblockIds is not null and JSON_VALID(p_AllowedRedblockIds)) then
        insert into TempAllowedRedblockIds (RedblockId)
        select jt.RedblockId
        from json_table(p_AllowedRedblockIds, '$[*]' columns(RedblockId bigint path '$')) as jt;
    end if;

    if (p_DeletionFilter is not null and p_DeletionFilterMatchType is not null and JSON_VALID(p_DeletionFilter)) then
        insert into TempDeletionFilterValues (UserId)
        select jt.UserId
        from json_table(p_DeletionFilter, '$[*]' columns(UserId bigint path '$')) as jt;

        if (p_DeletionFilterMatchType = 'or') then
            select rb.RedblockId
            from `Redblocks.Redblocks` rb
            where rb.ProjectId = p_ProjectId
              and rb.DeletedBy in (select UserId from TempDeletionFilterValues)
              and (
                    p_AllowedRedblockIds is null
                    or exists(select 1 from TempAllowedRedblockIds ta where ta.RedblockId = rb.RedblockId)
                  );
        elseif (p_DeletionFilterMatchType = 'and') then
            select rb.RedblockId
            from `Redblocks.Redblocks` rb
            where rb.ProjectId = p_ProjectId
              and rb.DeletedBy in (select UserId from TempDeletionFilterValues)
              and (
                    p_AllowedRedblockIds is null
                    or exists(select 1 from TempAllowedRedblockIds ta where ta.RedblockId = rb.RedblockId)
                  );
        elseif (p_DeletionFilterMatchType = 'not') then
            select rb.RedblockId
            from `Redblocks.Redblocks` rb
            where rb.ProjectId = p_ProjectId
              and (
                    p_AllowedRedblockIds is null
                    or exists(select 1 from TempAllowedRedblockIds ta where ta.RedblockId = rb.RedblockId)
                  )
              and (rb.DeletedBy is null or rb.DeletedBy not in (select UserId from TempDeletionFilterValues));
        else
            select null as RedblockId where 1 = 0;
        end if;
    else
        select null as RedblockId where 1 = 0;
    end if;

    drop temporary table if exists TempDeletionFilterValues;
    drop temporary table if exists TempAllowedRedblockIds;
end;



