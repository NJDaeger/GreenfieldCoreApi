-- DependsOn: ScriptHistory
create table if not exists `BuildCodes.Codes` (
    CodeId bigint not null unique auto_increment primary key,
    ListOrder int not null,
    BuildCode varchar(4096) not null,
    CreatedOn datetime default current_timestamp not null
) character set utf8mb4 collate utf8mb4_unicode_ci;