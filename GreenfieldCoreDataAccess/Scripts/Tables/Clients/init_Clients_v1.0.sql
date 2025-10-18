-- DependsOn: ScriptHistory
create table if not exists Clients (
    ClientId char(36) not null primary key unique,
    ClientName nvarchar(255) not null unique,
    ClientSecretHash nvarchar(255) not null,
    Salt nvarchar(255) not null,
    CreatedOn datetime default current_timestamp not null
)
