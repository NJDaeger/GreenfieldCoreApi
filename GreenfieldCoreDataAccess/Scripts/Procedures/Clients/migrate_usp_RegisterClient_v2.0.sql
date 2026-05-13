-- DependsOn: ScriptHistory, Clients
drop procedure if exists `Clients.usp_RegisterClient`;
create procedure if not exists `Clients.usp_RegisterClient`(
    p_ClientId char(36),
    p_ClientName varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_ClientSecretHash varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_Salt varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci)
begin
insert into `Clients.Clients` (ClientId, ClientName, ClientSecretHash, Salt)
values (p_ClientId, p_ClientName, p_ClientSecretHash, p_Salt);
-- select the datetime the client was created
select c.CreatedOn from `Clients.Clients` c where c.ClientId = p_ClientId;
end;

