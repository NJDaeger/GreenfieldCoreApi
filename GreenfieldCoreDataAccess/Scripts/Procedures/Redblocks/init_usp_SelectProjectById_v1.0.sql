-- DependsOn: ScriptHistory, Projects
create procedure if not exists `Redblocks.usp_SelectProjectById`(
    p_ProjectId bigint)
begin
    select
        p.ProjectId,
        p.ProjectName,
        p.ProjectKey,
        p.LastUsedRedblockKeyNumber
    from `Redblocks.Projects` p
    where p.ProjectId = p_ProjectId;
end;

