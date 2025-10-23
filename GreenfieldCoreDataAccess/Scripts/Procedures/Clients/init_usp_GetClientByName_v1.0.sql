-- DependsOn: ScriptHistory, Clients
create procedure if not exists usp_GetClientByName(
    p_ClientName nvarchar(255))
begin
select c.ClientId, c.ClientName, c.Salt, c.CreatedOn
from Clients c
where c.ClientName = p_ClientName;
end;
