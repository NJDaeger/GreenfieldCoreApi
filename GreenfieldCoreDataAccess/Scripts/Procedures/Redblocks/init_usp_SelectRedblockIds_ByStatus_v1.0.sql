-- DependsOn: ScriptHistory, Redblocks, Statuses
create procedure if not exists `Redblocks.usp_SelectRedblockIds_ByStatus`(
    p_ProjectId bigint,
    p_StatusFilter varchar(8192),
    p_StatusFilterMatchType varchar(16),
    p_AllowedRedblockIds varchar(8192))
begin
    if (
        p_StatusFilter is not null
        and p_StatusFilterMatchType is not null
        and JSON_VALID(p_StatusFilter)
        and (p_StatusFilterMatchType = 'or' or p_StatusFilterMatchType = 'not')
    ) then
        with
            StatusFilterValues as (
                select jt.Status
                from json_table(p_StatusFilter, '$[*]' columns(Status nvarchar(32) path '$')) as jt
            ),
            AllowedRedblockIds as (
                select jt.RedblockId
                from json_table(
                    case
                        when p_AllowedRedblockIds is not null and JSON_VALID(p_AllowedRedblockIds) then p_AllowedRedblockIds
                        else json_array()
                    end,
                    '$[*]' columns(RedblockId bigint path '$')
                ) as jt
            ),
            CandidateRedblocks as (
                select rb.RedblockId
                from `Redblocks.Redblocks` rb
                where rb.ProjectId = p_ProjectId
                  and (
                        p_AllowedRedblockIds is null
                        or exists(
                            select 1
                            from AllowedRedblockIds ari
                            where ari.RedblockId = rb.RedblockId
                        )
                      )
            ),
            LatestStatus as (
                select
                    rs.RedblockId,
                    rs.Status,
                    row_number() over (partition by rs.RedblockId order by rs.CreatedOn desc, rs.StatusId desc) as RowNum
                from `Redblocks.Statuses` rs
                join CandidateRedblocks cr on cr.RedblockId = rs.RedblockId
            )
        select ls.RedblockId
        from LatestStatus ls
        where ls.RowNum = 1
          and (
                (p_StatusFilterMatchType = 'or' and exists(select 1 from StatusFilterValues sfv where sfv.Status = ls.Status))
                or (p_StatusFilterMatchType = 'not' and not exists(select 1 from StatusFilterValues sfv where sfv.Status = ls.Status))
              );
    else
        select null as RedblockId where 1 = 0;
    end if;
end;



