-- DependsOn: ScriptHistory, Clients
create procedure if not exists `Clients.usp_RegisterClient`(
    p_ClientId char(36),
    p_ClientName nvarchar(255),
    p_ClientSecretHash nvarchar(255),
    p_Salt nvarchar(255))
begin
insert into `Clients.Clients` (ClientId, ClientName, ClientSecretHash, Salt)
values (p_ClientId, p_ClientName, p_ClientSecretHash, p_Salt);
-- select the datetime the client was created
select c.CreatedOn from `Clients.Clients` c where c.ClientId = p_ClientId;
end;
