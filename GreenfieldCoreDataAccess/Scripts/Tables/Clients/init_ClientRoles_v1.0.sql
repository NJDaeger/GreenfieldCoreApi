-- DependsOn: ScriptHistory, Clients
create table if not exists `Clients.ClientRoles` (
    ClientRoleId bigint not null primary key unique auto_increment,
    ClientId char(36) not null,
    RoleName varchar(255) not null,
    CreatedOn datetime default current_timestamp not null,
    constraint UQ_ClientRoles_ClientId_RoleName unique (ClientId, RoleName),
    constraint FK_ClientRoles_Clients foreign key (ClientId) references `Clients.Clients`(ClientId) on delete cascade
) character set utf8mb4 collate utf8mb4_unicode_ci;