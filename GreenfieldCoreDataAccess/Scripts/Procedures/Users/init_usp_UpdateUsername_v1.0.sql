-- DependsOn: ScriptHistory, Users
create procedure if not exists usp_UpdateUsername(
    p_MinecraftUuid char(36),
    p_NewUsername nvarchar(16))
begin
update Users
set MinecraftUsername = p_NewUsername
where MinecraftUuid = p_MinecraftUuid;
end;