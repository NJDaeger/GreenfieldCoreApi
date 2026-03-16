-- DependsOn: ScriptHistory, Projects
create procedure if not exists `Redblocks.usp_InsertProject`(
    p_ProjectName nvarchar(64),
    p_ProjectKey varchar(6))
begin
    insert into `Redblocks.Projects` (
        ProjectName,
        ProjectKey)
    values (
        p_ProjectName,
        upper(p_ProjectKey));

    if row_count() > 0 then
        select
            p.ProjectId,
            p.ProjectName,
            p.ProjectKey,
            p.LastUsedRedblockKeyNumber
        from `Redblocks.Projects` p
        where p.ProjectId = last_insert_id();
    end if;
end;

