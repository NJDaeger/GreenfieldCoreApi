-- DependsOn: ScriptHistory, ApplicationStatus
drop procedure if exists `BuildApps.usp_InsertApplicationStatus`;
create procedure if not exists `BuildApps.usp_InsertApplicationStatus`(
    p_ApplicationId bigint,
    p_Status varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_StatusMessage varchar(2048) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci)
begin
    insert into `BuildApps.ApplicationStatus` (
        ApplicationId,
        Status,
        StatusMessage)
    values (
        p_ApplicationId,
        p_Status,
        p_StatusMessage);

    select 
        bas.ApplicationStatusId,
        bas.ApplicationId,
        bas.Status,
        bas.StatusMessage,
        bas.CreatedOn
    from `BuildApps.ApplicationStatus` bas
    where bas.ApplicationStatusId = last_insert_id();
end;

