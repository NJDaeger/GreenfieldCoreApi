-- DependsOn: ScriptHistory, Clients
create procedure if not exists usp_GetAllClients()
begin
    select c.ClientId, c.ClientName, c.Salt, c.CreatedOn
    from Clients c;
end;

