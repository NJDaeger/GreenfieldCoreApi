-- DependsOn: ScriptHistory, Projects, Users
create table if not exists `Redblocks.Redblocks` (
    RedblockId bigint not null unique auto_increment primary key,
    ProjectId bigint not null,
    KeyNumber bigint not null,
    Message varchar(1024) not null,
    X int not null,
    Y int not null,
    Z int not null,
    CreatedBy bigint not null,
    CreatedOn datetime default current_timestamp not null,
    UpdatedBy bigint null,
    UpdatedOn datetime null,
    DeletedBy bigint null,
    DeletedOn datetime null,
    constraint UQ_Redblocks_ProjectId_KeyNumber unique (ProjectId, KeyNumber),
    constraint FK_Redblocks_Projects foreign key (ProjectId) references `Redblocks.Projects`(ProjectId) on delete cascade,
    constraint FK_Redblocks_CreatedBy foreign key (CreatedBy) references `Users.Users`(UserId),
    constraint FK_Redblocks_DeletedBy foreign key (DeletedBy) references `Users.Users`(UserId)
) character set utf8mb4 collate utf8mb4_unicode_ci;

create index if not exists IX_Redblocks_ProjectId_XYZ on `Redblocks.Redblocks` (ProjectId, X, Y, Z);

create trigger `Redblocks.trg_UsersBeforeDelete_SetRedblocksCreatedBy`
    before delete on `Users.Users`
    for each row
    update `Redblocks.Redblocks` rb
    set rb.CreatedBy = 1
    where rb.CreatedBy = old.UserId;

create trigger `Redblocks.trg_UsersBeforeDelete_SetRedblocksDeletedBy`
    before delete on `Users.Users`
    for each row
    update `Redblocks.Redblocks` rb
    set rb.DeletedBy = 1
    where rb.DeletedBy = old.UserId;

create trigger `Redblocks.trg_UsersBeforeDelete_SetRedblocksUpdatedBy`
    before delete on `Users.Users`
    for each row
    update `Redblocks.Redblocks` rb
    set rb.UpdatedBy = 1
    where rb.UpdatedBy = old.UserId;

