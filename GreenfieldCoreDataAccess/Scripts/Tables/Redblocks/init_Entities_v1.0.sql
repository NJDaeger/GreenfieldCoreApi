-- DependsOn: ScriptHistory, Redblocks
create table if not exists `Redblocks.Entities` (
    RedblockId bigint not null,
    EntityGuid char(36) not null,
    constraint PK_Entities primary key (RedblockId, EntityGuid),
    constraint FK_Entities_Redblocks foreign key (RedblockId) references `Redblocks.Redblocks`(RedblockId) on delete cascade on update cascade
) character set utf8mb4 collate utf8mb4_unicode_ci;

