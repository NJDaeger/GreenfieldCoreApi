-- DependsOn: ScriptHistory, Redblocks, RoleAssignments
create procedure if not exists `Redblocks.usp_SelectRedblockIds_ByRoleAssignment`(
    p_ProjectId bigint,
    p_RoleAssignmentFilter varchar(8192),
    p_RoleAssignmentFilterMatchType varchar(16),
    p_AllowedRedblockIds varchar(8192))
begin
    create temporary table TempAllowedRedblockIds
    (
        RedblockId bigint not null primary key
    );

    create temporary table TempRoleAssignmentFilterValues
    (
        RoleName nvarchar(32) not null primary key
    );

    if (p_AllowedRedblockIds is not null and JSON_VALID(p_AllowedRedblockIds)) then
        insert into TempAllowedRedblockIds (RedblockId)
        select jt.RedblockId
        from json_table(p_AllowedRedblockIds, '$[*]' columns(RedblockId bigint path '$')) as jt;
    end if;

    if (p_RoleAssignmentFilter is not null and p_RoleAssignmentFilterMatchType is not null and JSON_VALID(p_RoleAssignmentFilter)) then
        insert into TempRoleAssignmentFilterValues (RoleName)
        select jt.RoleName
        from json_table(p_RoleAssignmentFilter, '$[*]' columns(RoleName nvarchar(32) path '$')) as jt;

        if (p_RoleAssignmentFilterMatchType = 'or') then
            select distinct ra.RedblockId
            from `Redblocks.RoleAssignments` ra
            join `Redblocks.Redblocks` rb on rb.RedblockId = ra.RedblockId
            where rb.ProjectId = p_ProjectId
              and ra.RoleName in (select RoleName from TempRoleAssignmentFilterValues)
              and (
                    p_AllowedRedblockIds is null
                    or exists(select 1 from TempAllowedRedblockIds ta where ta.RedblockId = ra.RedblockId)
                  );
        elseif (p_RoleAssignmentFilterMatchType = 'and') then
            select ra.RedblockId
            from `Redblocks.RoleAssignments` ra
            join `Redblocks.Redblocks` rb on rb.RedblockId = ra.RedblockId
            where rb.ProjectId = p_ProjectId
              and ra.RoleName in (select RoleName from TempRoleAssignmentFilterValues)
              and (
                    p_AllowedRedblockIds is null
                    or exists(select 1 from TempAllowedRedblockIds ta where ta.RedblockId = ra.RedblockId)
                  )
            group by ra.RedblockId
            having count(distinct ra.RoleName) = (select count(*) from TempRoleAssignmentFilterValues);
        elseif (p_RoleAssignmentFilterMatchType = 'not') then
            select rb.RedblockId
            from `Redblocks.Redblocks` rb
            where rb.ProjectId = p_ProjectId
              and (
                    p_AllowedRedblockIds is null
                    or exists(select 1 from TempAllowedRedblockIds ta where ta.RedblockId = rb.RedblockId)
                  )
              and not exists (
                    select 1
                    from `Redblocks.RoleAssignments` ra
                    where ra.RedblockId = rb.RedblockId
                      and ra.RoleName in (select RoleName from TempRoleAssignmentFilterValues)
                  );
        else
            select null as RedblockId where 1 = 0;
        end if;
    else
        select null as RedblockId where 1 = 0;
    end if;

    drop temporary table if exists TempRoleAssignmentFilterValues;
    drop temporary table if exists TempAllowedRedblockIds;
end;



