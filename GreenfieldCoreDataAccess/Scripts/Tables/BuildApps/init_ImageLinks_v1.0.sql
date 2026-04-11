-- DependsOn: ScriptHistory, Applications
create table if not exists `BuildApps.ImageLinks` (
    ImageLinkId bigint not null unique auto_increment primary key,
    ApplicationId bigint not null,
    LinkType varchar(256) not null,
    ImageLink varchar(2048) not null,
    UpdatedOn datetime default current_timestamp on update current_timestamp  null,
    CreatedOn datetime default current_timestamp not null,
    constraint FK_ImageLinks_Applications foreign key (ApplicationId) references `BuildApps.Applications`(ApplicationId) on delete cascade on update cascade
) character set utf8mb4 collate utf8mb4_unicode_ci;