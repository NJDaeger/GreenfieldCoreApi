-- DependsOn: ScriptHistory
create table if not exists `Clients.Clients` (
    ClientId char(36) not null primary key unique,
    ClientName varchar(255) not null unique,
    ClientSecretHash varchar(255) not null,
    Salt varchar(255) not null,
    CreatedOn datetime default current_timestamp not null
) character set utf8mb4 collate utf8mb4_unicode_ci;
