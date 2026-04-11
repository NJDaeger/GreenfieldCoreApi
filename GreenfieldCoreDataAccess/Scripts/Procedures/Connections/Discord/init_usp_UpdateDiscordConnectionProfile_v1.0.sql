-- DependsOn: ScriptHistory, DiscordConnections
create procedure if not exists `Connections.Discord.usp_UpdateDiscordConnectionProfile`(
    p_DiscordConnectionId bigint,
    p_DiscordUsername nvarchar(256)
)
begin
    update `Connections.DiscordConnections`
    set DiscordUsername = p_DiscordUsername
    where DiscordConnectionId = p_DiscordConnectionId;
end;

