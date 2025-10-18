-- DependsOn: ScriptHistory, Clients
create procedure if not exists usp_GetClientById(
    p_ClientId char(36))
begin
    select c.ClientId, c.ClientName, c.Salt, c.CreatedOn
    from Clients c
    where c.ClientId = p_ClientId;
end;


