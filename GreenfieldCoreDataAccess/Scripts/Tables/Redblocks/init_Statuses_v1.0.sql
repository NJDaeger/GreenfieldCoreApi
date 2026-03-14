-- DependsOn: ScriptHistory, Redblocks, Users
create table if not exists `Redblocks.Statuses` (
    StatusId bigint not null unique auto_increment primary key,
    RedblockId bigint not null,
    Status nvarchar(32) not null,
    CreatedBy bigint not null default 1,
    CreatedOn datetime default current_timestamp not null,
    constraint FK_Statuses_Redblocks foreign key (RedblockId) references `Redblocks.Redblocks`(RedblockId) on delete cascade on update cascade,
    constraint FK_Statuses_Users foreign key (CreatedBy) references `Users.Users`(UserId) on update cascade
) character set utf8mb4 collate utf8mb4_unicode_ci;

drop trigger if exists `Redblocks.trg_UsersBeforeDelete_SetStatusesCreatedBy`;
create trigger `Redblocks.trg_UsersBeforeDelete_SetStatusesCreatedBy`
before delete on `Users.Users`
for each row
update `Redblocks.Statuses` s
set s.CreatedBy = 1
where s.CreatedBy = old.UserId;

