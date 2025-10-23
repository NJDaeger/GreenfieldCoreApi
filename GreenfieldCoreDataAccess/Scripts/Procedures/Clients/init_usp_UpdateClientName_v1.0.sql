-- DependsOn: ScriptHistory, Clients
create procedure if not exists usp_UpdateClientName(
    p_ClientId char(36),
    p_NewClientName nvarchar(255))
begin
    update Clients
    set ClientName = p_NewClientName
    where ClientId = p_ClientId;
end;