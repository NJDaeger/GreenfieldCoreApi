-- DependsOn: ScriptHistory, ApplicationStatus
create procedure if not exists `BuildApps.usp_InsertApplicationStatus`(
    p_ApplicationId bigint,
    p_Status varchar(256),
    p_StatusMessage varchar(2048))
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
