-- DependsOn: ScriptHistory
create table if not exists `Redblocks.Projects` (
    ProjectId bigint not null unique auto_increment primary key,
    ProjectName varchar(64) not null,
    ProjectKey varchar(6) not null,
    LastUsedRedblockKeyNumber bigint not null default 0,
    constraint UQ_Projects_ProjectName unique (ProjectName),
    constraint UQ_Projects_ProjectKey unique (ProjectKey)
) character set utf8mb4 collate utf8mb4_unicode_ci;

