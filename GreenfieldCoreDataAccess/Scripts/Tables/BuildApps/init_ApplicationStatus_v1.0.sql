-- DependsOn: ScriptHistory, Applications
create table if not exists `BuildApps.ApplicationStatus` (
    ApplicationStatusId bigint not null unique auto_increment primary key,
    ApplicationId bigint not null,
    Status varchar(256) not null,
    StatusMessage varchar(4096) null,
    CreatedOn datetime default current_timestamp not null,
    constraint FK_ApplicationStatus_Applications foreign key (ApplicationId) references `BuildApps.Applications`(ApplicationId) on delete cascade on update cascade
) character set utf8mb4 collate utf8mb4_unicode_ci;