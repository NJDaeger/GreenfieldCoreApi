-- DependsOn: ScriptHistory, ImageLinks
drop procedure if exists `BuildApps.usp_InsertImageLink`;
create procedure if not exists `BuildApps.usp_InsertImageLink`(
    p_ApplicationId bigint,
    p_LinkType varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_ImageLink varchar(2048) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci)
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

