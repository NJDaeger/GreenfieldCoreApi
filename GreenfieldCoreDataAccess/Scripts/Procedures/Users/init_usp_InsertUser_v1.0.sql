-- DependsOn: ScriptHistory, Users
create procedure if not exists `Users.usp_InsertUser`(
    p_MinecraftUuid char(36),
    p_MinecraftUsername nvarchar(16))
begin
    -- Attempt insert; ignore if UUID already exists
    insert ignore into `Users.Users` (MinecraftUuid, MinecraftUsername)
    values (p_MinecraftUuid, p_MinecraftUsername);

    -- If an insert happened, return the created user row
    if row_count() > 0 then
        select u.UserId, u.MinecraftUuid, u.MinecraftUsername, u.CreatedOn
        from `Users.Users` u
        where u.MinecraftUuid = p_MinecraftUuid;
    end if;
end;