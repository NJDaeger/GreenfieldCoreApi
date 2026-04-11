-- DependsOn: ScriptHistory, Users
drop procedure if exists `Users.usp_UpdateUsername`;
create procedure if not exists `Users.usp_UpdateUsername`(
    p_MinecraftUuid char(36),
    p_NewUsername varchar(16))
begin
update `Users.Users`
set MinecraftUsername = p_NewUsername
where MinecraftUuid = p_MinecraftUuid;
end;

