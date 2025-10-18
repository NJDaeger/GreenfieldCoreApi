-- DependsOn: ScriptHistory, Clients
create table if not exists ClientRoles (
    ClientRoleId bigint not null primary key unique auto_increment,
    ClientId char(36) not null,
    RoleName nvarchar(255) not null,
    CreatedOn datetime default current_timestamp not null,
    constraint UQ_ClientRoles_ClientId_RoleName unique (ClientId, RoleName),
    constraint FK_ClientRoles_Clients foreign key (ClientId) references Clients(ClientId) on delete cascade
)