-- DependsOn: ScriptHistory, Applications
create procedure if not exists `BuildApps.usp_InsertApplication`(
    p_UserId bigint,
    p_UserAge int,
    p_UserNationality nvarchar(128),
    p_AdditionalBuildingInformation nvarchar(4096),
    p_WhyJoinGreenfield nvarchar(4096),
    p_AdditionalComments nvarchar(4096))
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
