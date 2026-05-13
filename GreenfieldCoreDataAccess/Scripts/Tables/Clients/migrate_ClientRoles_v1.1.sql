-- DependsOn: ScriptHistory, Clients
alter table `Clients.ClientRoles`
    drop foreign key `FK_ClientRoles_Clients`;

alter table `Clients.ClientRoles`
    convert to character set utf8mb4 collate utf8mb4_unicode_ci;

alter table `Clients.ClientRoles`
    add constraint `FK_ClientRoles_Clients`
        foreign key (`ClientId`) references `Clients.Clients`(`ClientId`) on delete cascade;

