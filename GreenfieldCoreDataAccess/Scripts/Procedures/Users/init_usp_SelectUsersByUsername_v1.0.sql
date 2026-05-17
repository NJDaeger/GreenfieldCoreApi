-- DependsOn: ScriptHistory, Users
create procedure if not exists `Users.usp_SelectUsersByUsername`(
    p_MinecraftUsername varchar(16) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci)
begin
select u.UserId, u.MinecraftUuid, u.MinecraftUsername, u.CreatedOn
from `Users.Users` u
where u.MinecraftUsername = p_MinecraftUsername;
end;

