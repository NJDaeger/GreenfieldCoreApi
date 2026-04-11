-- DependsOn: ScriptHistory, Clients
create procedure if not exists `Clients.usp_SelectClientByName`(
    p_ClientName nvarchar(255))
begin
select c.ClientId, c.ClientName, c.Salt, c.CreatedOn
from `Clients.Clients` c
where c.ClientName = p_ClientName;
end;
