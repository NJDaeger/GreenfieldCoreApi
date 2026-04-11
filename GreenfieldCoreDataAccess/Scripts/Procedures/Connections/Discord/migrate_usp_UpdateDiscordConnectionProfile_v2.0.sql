-- DependsOn: ScriptHistory, DiscordConnections
drop procedure if exists `Connections.Discord.usp_UpdateDiscordConnectionProfile`;
create procedure if not exists `Connections.Discord.usp_UpdateDiscordConnectionProfile`(
    p_DiscordConnectionId bigint,
    p_DiscordUsername varchar(256)
)
begin
    update `Connections.DiscordConnections`
    set DiscordUsername = p_DiscordUsername
    where DiscordConnectionId = p_DiscordConnectionId;
end;


