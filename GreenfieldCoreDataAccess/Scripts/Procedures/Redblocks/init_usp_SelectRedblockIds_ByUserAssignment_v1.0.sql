-- DependsOn: ScriptHistory, Redblocks, UserAssignments
create procedure if not exists `Redblocks.usp_SelectRedblockIds_ByUserAssignment`(
    p_ProjectId bigint,
    p_UserAssignmentFilter varchar(8192),
    p_UserAssignmentFilterMatchType varchar(16),
    p_AllowedRedblockIds varchar(8192))
begin
    create temporary table TempAllowedRedblockIds
    (
        RedblockId bigint not null primary key
    );

    create temporary table TempUserAssignmentFilterValues
    (
        UserId bigint not null primary key
    );

    if (p_AllowedRedblockIds is not null and JSON_VALID(p_AllowedRedblockIds)) then
        insert into TempAllowedRedblockIds (RedblockId)
        select jt.RedblockId
        from json_table(p_AllowedRedblockIds, '$[*]' columns(RedblockId bigint path '$')) as jt;
    end if;

    if (p_UserAssignmentFilter is not null and p_UserAssignmentFilterMatchType is not null and JSON_VALID(p_UserAssignmentFilter)) then
        insert into TempUserAssignmentFilterValues (UserId)
        select jt.UserId
        from json_table(p_UserAssignmentFilter, '$[*]' columns(UserId bigint path '$')) as jt;

        if (p_UserAssignmentFilterMatchType = 'or') then
            select distinct ua.RedblockId
            from `Redblocks.UserAssignments` ua
            join `Redblocks.Redblocks` rb on rb.RedblockId = ua.RedblockId
            where rb.ProjectId = p_ProjectId
              and ua.AssignedTo in (select UserId from TempUserAssignmentFilterValues)
              and (
                    p_AllowedRedblockIds is null
                    or exists(select 1 from TempAllowedRedblockIds ta where ta.RedblockId = ua.RedblockId)
                  );
        elseif (p_UserAssignmentFilterMatchType = 'and') then
            select ua.RedblockId
            from `Redblocks.UserAssignments` ua
            join `Redblocks.Redblocks` rb on rb.RedblockId = ua.RedblockId
            where rb.ProjectId = p_ProjectId
              and ua.AssignedTo in (select UserId from TempUserAssignmentFilterValues)
              and (
                    p_AllowedRedblockIds is null
                    or exists(select 1 from TempAllowedRedblockIds ta where ta.RedblockId = ua.RedblockId)
                  )
            group by ua.RedblockId
            having count(distinct ua.AssignedTo) = (select count(*) from TempUserAssignmentFilterValues);
        elseif (p_UserAssignmentFilterMatchType = 'not') then
            select rb.RedblockId
            from `Redblocks.Redblocks` rb
            where rb.ProjectId = p_ProjectId
              and (
                    p_AllowedRedblockIds is null
                    or exists(select 1 from TempAllowedRedblockIds ta where ta.RedblockId = rb.RedblockId)
                  )
              and not exists (
                    select 1
                    from `Redblocks.UserAssignments` ua
                    where ua.RedblockId = rb.RedblockId
                      and ua.AssignedTo in (select UserId from TempUserAssignmentFilterValues)
                  );
        else
            select null as RedblockId where 1 = 0;
        end if;
    else
        select null as RedblockId where 1 = 0;
    end if;

    drop temporary table if exists TempUserAssignmentFilterValues;
    drop temporary table if exists TempAllowedRedblockIds;
end;



