-- DependsOn: ScriptHistory, ImageLinks
create procedure if not exists `BuildApps.usp_UpdateImageLink`(
    p_ImageLinkId bigint,
    p_LinkType nvarchar(256),
    p_ImageLink nvarchar(2048))
begin
    update `BuildApps.ImageLinks` bali
    set
        bali.LinkType = p_LinkType,
        bali.ImageLink = p_ImageLink
    where bali.ImageLinkId = p_ImageLinkId;

    select
        bali.ImageLinkId,
        bali.ApplicationId,
        bali.LinkType,
        bali.ImageLink,
        bali.UpdatedOn,
        bali.CreatedOn
    from `BuildApps.ImageLinks` bali
    where bali.ImageLinkId = p_ImageLinkId;
end;
