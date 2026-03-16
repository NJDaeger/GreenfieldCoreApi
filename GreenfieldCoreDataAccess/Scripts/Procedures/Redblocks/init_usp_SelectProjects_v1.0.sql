-- DependsOn: ScriptHistory, Projects
create procedure if not exists `Redblocks.usp_SelectProjects`()
begin
    select
        p.ProjectId,
        p.ProjectName,
        p.ProjectKey,
        p.LastUsedRedblockKeyNumber
    from `Redblocks.Projects` p
    order by p.ProjectName;
end;

