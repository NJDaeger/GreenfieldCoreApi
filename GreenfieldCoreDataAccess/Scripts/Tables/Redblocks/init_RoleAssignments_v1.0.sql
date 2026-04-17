-- DependsOn: ScriptHistory, Redblocks, Users
create table if not exists `Redblocks.RoleAssignments` (
    RoleAssignmentId bigint not null unique auto_increment primary key,
    RedblockId bigint not null,
    RoleName varchar(32) not null,
    CreatedBy bigint not null,
    CreatedOn datetime default current_timestamp not null,
    constraint UQ_RoleAssignments_RedblockId_RoleName unique (RedblockId, RoleName),
    constraint FK_RoleAssignments_Redblocks foreign key (RedblockId) references `Redblocks.Redblocks`(RedblockId) on delete cascade,
    constraint FK_RoleAssignments_CreatedBy foreign key (CreatedBy) references `Users.Users`(UserId)
) character set utf8mb4 collate utf8mb4_unicode_ci;

drop trigger if exists `Redblocks.trg_UsersBeforeDelete_SetRoleAssignmentsCreatedBy`;
create trigger `Redblocks.trg_UsersBeforeDelete_SetRoleAssignmentsCreatedBy`
before delete on `Users.Users`
for each row
update `Redblocks.RoleAssignments` ra
set ra.CreatedBy = 1
where ra.CreatedBy = old.UserId;

