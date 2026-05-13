-- DependsOn: ScriptHistory, Applications
drop procedure if exists `BuildApps.usp_InsertApplication`;
create procedure if not exists `BuildApps.usp_InsertApplication`(
    p_UserId bigint,
    p_UserAge int,
    p_UserNationality varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_AdditionalBuildingInformation varchar(4096) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_WhyJoinGreenfield varchar(4096) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_AdditionalComments varchar(4096) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci)
begin
    insert into `BuildApps.Applications` (
        UserId,
        UserAge,
        UserNationality,
        AdditionalBuildingInformation,
        WhyJoinGreenfield,
        AdditionalComments)
    values (
        p_UserId,
        p_UserAge,
        p_UserNationality,
        p_AdditionalBuildingInformation,
        p_WhyJoinGreenfield,
        p_AdditionalComments);

    select
        ba.ApplicationId,
        ba.UserId,
        ba.UserAge,
        ba.UserNationality,
        ba.AdditionalBuildingInformation,
        ba.WhyJoinGreenfield,
        ba.AdditionalComments,
        ba.CreatedOn
    from `BuildApps.Applications` ba
    where ba.ApplicationId = last_insert_id();
end;

