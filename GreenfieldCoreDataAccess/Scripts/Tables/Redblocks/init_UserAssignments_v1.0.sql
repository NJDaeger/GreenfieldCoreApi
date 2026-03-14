-- DependsOn: ScriptHistory, Redblocks, Users
create table if not exists `Redblocks.UserAssignments` (
    UserAssignmentId bigint not null unique auto_increment primary key,
    RedblockId bigint not null,
    AssignedTo bigint not null,
    CreatedBy bigint not null,
    CreatedOn datetime default current_timestamp not null,
    constraint UQ_UserAssignments_RedblockId_UserId unique (RedblockId, UserId),
    constraint FK_UserAssignments_Redblocks foreign key (RedblockId) references `Redblocks.Redblocks`(RedblockId) on delete cascade on update cascade,
    constraint FK_UserAssignments_AssignedTo foreign key (AssignedTo) references `Users.Users`(UserId) on delete cascade,
    constraint FK_UserAssignments_CreatedBy foreign key (CreatedBy) references `Users.Users`(UserId)
) character set utf8mb4 collate utf8mb4_unicode_ci;

drop trigger if exists `Redblocks.trg_UsersBeforeDelete_SetUserAssignmentsCreatedBy`;
create trigger `Redblocks.trg_UsersBeforeDelete_SetUserAssignmentsCreatedBy`
before delete on `Users.Users`
for each row
update `Redblocks.UserAssignments` ua
set ua.CreatedBy = 1
where ua.CreatedBy = old.UserId;

