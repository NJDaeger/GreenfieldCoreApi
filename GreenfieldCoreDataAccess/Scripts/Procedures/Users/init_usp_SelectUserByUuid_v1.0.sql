-- DependsOn: ScriptHistory, Users
create procedure if not exists usp_SelectUserByUuid(
    u_MinecraftUuid char(36))
begin
select u.UserId, u.MinecraftUuid, u.MinecraftUsername, u.CreatedOn
from Users u
where u.MinecraftUuid = u_MinecraftUuid;
end;