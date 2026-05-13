-- DependsOn: ScriptHistory, ImageLinks
drop procedure if exists `BuildApps.usp_UpdateImageLink`;
create procedure if not exists `BuildApps.usp_UpdateImageLink`(
    p_ImageLinkId bigint,
    p_LinkType varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_ImageLink varchar(2048) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci)
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

