-- DependsOn: ScriptHistory, Projects, Redblocks, Statuses, UserAssignments, RoleAssignments
create procedure if not exists `Redblocks.usp_SelectRedblocksByProject`(
    p_ProjectId bigint,
    -- StatusFilter format: 
    -- null 
    -- []:matchType where [] contains a comma separated list of statuses
    p_StatusFilter varchar(8192),
    p_StatusFilterMatchType varchar(16),
    -- DeletionFilter format:
    -- null
    -- []:matchType where [] contains a comma separated list of userIds
    p_DeletionFilter varchar(8192),
    p_DeletionFilterMatchType varchar(16),
    -- UserAssignmentFilter format:
    -- null
    -- []:matchType where [] contains a comma separated list of userIds
    p_UserAssignmentFilter varchar(8192),
    p_UserAssignmentFilterMatchType varchar(16),    
    -- RoleAssignmentFilter format:
    -- null
    -- []:matchType where [] contains a comma separated list of roles
    p_RoleAssignmentFilter varchar(8192),
    p_RoleAssignmentFilterMatchType varchar(16),    
    -- MessageFilter format:
    -- null
    -- []:matchType where [] contains the message to search for.
    p_MessageFilter varchar(2048),
    p_MessageFilterMatchType varchar(16)
    )
begin
    create temporary table TempRedblockIdList
    (
        RedblockId bigint not null primary key
    );
    
    create temporary table TempStatusFilterValues
    (
        Status nvarchar(32) not null primary key
    );

    create temporary table TempUserAssignmentFilterValues
    (
        UserId bigint not null primary key
    );

    create temporary table TempUserAssignmentMatchedRedblockIds
    (
        RedblockId bigint not null primary key
    );

    # Status filter processing
    if (p_StatusFilter is not null and p_StatusFilterMatchType is not null and JSON_VALID(p_StatusFilter)) then
        # Format: ["status1","status2",...]
        # MatchType options: or, not
        insert into TempStatusFilterValues (Status)
        select jt.Status
        from json_table(@p_StatusFilter, '$[*]' columns(Status nvarchar(32) path '$')) as jt;

        if (p_StatusFilterMatchType = 'or') then
            insert into TempRedblockIdList (RedblockId)
            with statusSelect as (
                select *, ROW_NUMBER() over (partition by rs.RedblockId order by rs.CreatedOn desc) as Numb from `Redblocks.Statuses` rs
            ) select RedblockId from statusSelect where Numb = 1 and rs.Status in (select Status from TempStatusFilterValues);
        elseif (p_StatusFilterMatchType = 'not') then
            insert into TempRedblockIdList (RedblockId)
            with statusSelect as (
                select *, ROW_NUMBER() over (partition by rs.RedblockId order by rs.CreatedOn desc) as Numb from `Redblocks.Statuses` rs
            ) select RedblockId from statusSelect where Numb = 1 and rs.Status not in (select Status from TempStatusFilterValues);
        end if;
    end if;

    # User assignment filter processing
    if (p_UserAssignmentFilter is not null and p_UserAssignmentFilterMatchType is not null and JSON_VALID(p_UserAssignmentFilter)) then
        # Format: [1,2,3,...]
        # MatchType options: or, and, not
        insert into TempUserAssignmentFilterValues (UserId)
        select jt.UserId
        from json_table(p_UserAssignmentFilter, '$[*]' columns(UserId bigint path '$')) as jt;

        if (p_UserAssignmentFilterMatchType = 'or') then
            insert into TempUserAssignmentMatchedRedblockIds (RedblockId)
            select distinct ua.RedblockId
            from `Redblocks.UserAssignments` ua
            join `Redblocks.Redblocks` rb on rb.RedblockId = ua.RedblockId
            where rb.ProjectId = p_ProjectId
              and ua.AssignedTo in (select UserId from TempUserAssignmentFilterValues)
              and (
                    not exists(select 1 from TempRedblockIdList)
                    or exists(select 1 from TempRedblockIdList tr where tr.RedblockId = ua.RedblockId)
                  );
        elseif (p_UserAssignmentFilterMatchType = 'and') then
            insert into TempUserAssignmentMatchedRedblockIds (RedblockId)
            select ua.RedblockId
            from `Redblocks.UserAssignments` ua
            join `Redblocks.Redblocks` rb on rb.RedblockId = ua.RedblockId
            where rb.ProjectId = p_ProjectId
              and ua.AssignedTo in (select UserId from TempUserAssignmentFilterValues)
              and (
                    not exists(select 1 from TempRedblockIdList)
                    or exists(select 1 from TempRedblockIdList tr where tr.RedblockId = ua.RedblockId)
                  )
            group by ua.RedblockId
            having count(distinct ua.AssignedTo) = (select count(*) from TempUserAssignmentFilterValues);
        elseif (p_UserAssignmentFilterMatchType = 'not') then
            insert into TempUserAssignmentMatchedRedblockIds (RedblockId)
            select rb.RedblockId
            from `Redblocks.Redblocks` rb
            where rb.ProjectId = p_ProjectId
              and (
                    not exists(select 1 from TempRedblockIdList)
                    or exists(select 1 from TempRedblockIdList tr where tr.RedblockId = rb.RedblockId)
                  )
              and not exists (
                    select 1
                    from `Redblocks.UserAssignments` ua
                    where ua.RedblockId = rb.RedblockId
                      and ua.AssignedTo in (select UserId from TempUserAssignmentFilterValues)
                  );
        end if;

        if exists(select 1 from TempRedblockIdList) then
            delete from TempRedblockIdList
            where RedblockId not in (select RedblockId from TempUserAssignmentMatchedRedblockIds);
        else
            insert into TempRedblockIdList (RedblockId)
            select RedblockId from TempUserAssignmentMatchedRedblockIds;
        end if;
    end if;
    

    
end;

