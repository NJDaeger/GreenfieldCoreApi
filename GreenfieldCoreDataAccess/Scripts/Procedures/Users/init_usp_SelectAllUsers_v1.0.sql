-- DependsOn: ScriptHistory, Users
create procedure if not exists `Users.usp_SelectAllUsers`()
begin
    select u.UserId, u.MinecraftUuid, u.MinecraftUsername, u.CreatedOn
    from `Users.Users` u;
end;