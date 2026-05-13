-- DependsOn: ScriptHistory, Clients
drop procedure if exists `Clients.usp_SelectClientByName`;
create procedure if not exists `Clients.usp_SelectClientByName`(
    p_ClientName varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci)
begin
select c.ClientId, c.ClientName, c.Salt, c.CreatedOn
from `Clients.Clients` c
where c.ClientName = p_ClientName;
end;

