-- DependsOn: ScriptHistory, Projects
create procedure if not exists `Redblocks.usp_UpdateProject`(
    p_ProjectId bigint,
    p_ProjectName nvarchar(64))
begin
    update `Redblocks.Projects`
    set ProjectName = p_ProjectName
    where ProjectId = p_ProjectId;

    if row_count() > 0 then
        select
            p.ProjectId,
            p.ProjectName,
            p.ProjectKey,
            p.LastUsedRedblockKeyNumber
        from `Redblocks.Projects` p
        where p.ProjectId = p_ProjectId;
    end if;
end;

