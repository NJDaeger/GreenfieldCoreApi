-- DependsOn: ScriptHistory, Users
create procedure if not exists usp_SelectUserByUserId(
    p_UserId bigint)
begin
    select u.UserId, u.MinecraftUuid, u.MinecraftUsername, u.CreatedOn
    from Users u
    where u.UserId = p_UserId;
end;