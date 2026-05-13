-- DependsOn: ScriptHistory, Clients
drop procedure if exists `Clients.usp_UpdateClientName`;
create procedure if not exists `Clients.usp_UpdateClientName`(
    p_ClientId char(36),
    p_NewClientName varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci)
begin
    update `Clients.Clients`
    set ClientName = p_NewClientName
    where ClientId = p_ClientId;
end;

