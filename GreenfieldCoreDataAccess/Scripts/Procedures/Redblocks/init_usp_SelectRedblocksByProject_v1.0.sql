-- DependsOn: ScriptHistory, Projects, Redblocks, Statuses, UserAssignments, RoleAssignments
delimiter $$
create procedure if not exists `Redblocks.usp_SelectRedblocksByProject`(
    p_ProjectId bigint,
    -- StatusFilter format: 
    -- null 
    -- []:matchType where [] contains a comma separated list of statuses
    p_StatusFilter varchar(8192),
    -- DeletionFilter format:
    -- null
    -- []:matchType where [] contains a comma separated list of userIds
    p_DeletionFilter varchar(8192),
    -- UserAssignmentFilter format:
    -- null
    -- []:matchType where [] contains a comma separated list of userIds
    p_UserAssignmentFilter varchar(8192),
    -- RoleAssignmentFilter format:
    -- null
    -- []:matchType where [] contains a comma separated list of roles
    p_RoleAssignmentFilter varchar(8192),
    -- MessageFilter format:
    -- null
    -- []:matchType where [] contains the message to search for.
    p_MessageFilter varchar(2048)
    )
begin
    create temporary table TempRedblockIdList
    (
        RedblockId bigint not null primary key
    );
    
    set @StatusFilterSeparator = null;
    set @StatusFilterMatchType = null;
    create temporary table TempStatusFilterValues
    (
        Status nvarchar(32) not null
    );

    if (p_StatusFilter is not null) then
        # Format: ["status1","status2",...]:matchType
        # MatchType options: or, not
        set @StatusFilterSeparatorPosition = LENGTH(p_StatusFilter) - LOCATE(':', REVERSE(p_StatusFilter)) + 1;

        if (@StatusFilterSeparatorPosition > 1 and @StatusFilterSeparatorPosition <= LENGTH(p_StatusFilter)) then
            set @StatusFilterValueString = SUBSTRING(p_StatusFilter, 1, @StatusFilterSeparatorPosition - 1);
            set @StatusFilterMatchType = LOWER(TRIM(SUBSTRING(p_StatusFilter, @StatusFilterSeparatorPosition + 1, LENGTH(p_StatusFilter) - @StatusFilterSeparatorPosition + 1)));

            if (JSON_VALID(@StatusFilterValueString)) then
                insert into TempStatusFilterValues (Status)
                select jt.Status
                from json_table(@StatusFilterValueString, '$[*]' columns(Status nvarchar(32) path '$')) as jt;

                if (@StatusFilterMatchType = 'or') then
                    insert ignore into TempRedblockIdList (RedblockId)
                    select ls.RedblockId
                    from (
                        select ranked.RedblockId, ranked.Status
                        from (
                            select
                                s.RedblockId,
                                s.Status,
                                row_number() over (partition by s.RedblockId order by s.CreatedOn desc, s.StatusId desc) as rn
                            from `Redblocks.Statuses` s
                        ) ranked
                        where ranked.rn = 1
                    ) ls
                    join TempStatusFilterValues tsfv on tsfv.Status = ls.Status;
                elseif (@StatusFilterMatchType = 'not') then
                    insert ignore into TempRedblockIdList (RedblockId)
                    select ls.RedblockId
                    from (
                        select ranked.RedblockId, ranked.Status
                        from (
                            select
                                s.RedblockId,
                                s.Status,
                                row_number() over (partition by s.RedblockId order by s.CreatedOn desc, s.StatusId desc) as rn
                            from `Redblocks.Statuses` s
                        ) ranked
                        where ranked.rn = 1
                    ) ls
                    left join TempStatusFilterValues tsfv on tsfv.Status = ls.Status
                    where tsfv.Status is null;
                end if;
            end if;
        end if;
    end if;
    
end $$
delimiter ;

