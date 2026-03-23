-- DependsOn: ScriptHistory, Projects
create procedure if not exists `Redblocks.usp_SelectProjectByKey`(
    p_ProjectKey varchar(6))
begin
    select
        p.ProjectId,
        p.ProjectName,
        p.ProjectKey,
        p.LastUsedRedblockKeyNumber
    from `Redblocks.Projects` p
    where p.ProjectKey = p_ProjectKey;
end;

