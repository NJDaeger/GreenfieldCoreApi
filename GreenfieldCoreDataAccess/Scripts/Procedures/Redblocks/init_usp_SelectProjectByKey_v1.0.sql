-- DependsOn: ScriptHistory, Projects
create procedure if not exists `Redblocks.usp_SelectProjectByKey`(
    p_ProjectKey varchar(6) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci)
begin
    select
        p.ProjectId,
        p.ProjectName,
        p.ProjectKey,
        p.LastUsedRedblockKeyNumber
    from `Redblocks.Projects` p
    where p.ProjectKey = p_ProjectKey;
end;

