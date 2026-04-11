-- DependsOn: ScriptHistory
create table if not exists `Users.Users` (
    UserId bigint auto_increment not null primary key unique,
    MinecraftUuid char(36) not null unique,
    MinecraftUsername varchar(16) not null,
    CreatedOn datetime default current_timestamp not null
) character set utf8mb4 collate utf8mb4_unicode_ci;

-- insert a system user into the Users table if it does not already exist
insert into `Users.Users` (MinecraftUuid, MinecraftUsername)
select '00000000-0000-0000-0000-000000000000', '##System##'
where not exists (
    select 1 from `Users.Users` u
    where u.MinecraftUuid = '00000000-0000-0000-0000-000000000000'
);