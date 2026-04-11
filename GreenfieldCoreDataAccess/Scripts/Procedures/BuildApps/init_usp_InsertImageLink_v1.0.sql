-- DependsOn: ScriptHistory, ImageLinks
create procedure if not exists `BuildApps.usp_InsertImageLink`(
    p_ApplicationId bigint,
    p_LinkType nvarchar(256),
    p_ImageLink nvarchar(2048))
begin
    insert into `BuildApps.ImageLinks` (
        ApplicationId,
        LinkType,
        ImageLink)
    values (
        p_ApplicationId,
        p_LinkType,
        p_ImageLink);

    select 
        bali.ImageLinkId,
        bali.ApplicationId,
        bali.LinkType,
        bali.ImageLink,
        bali.UpdatedOn,
        bali.CreatedOn
    from `BuildApps.ImageLinks` bali
    where bali.ImageLinkId = last_insert_id();
end;
