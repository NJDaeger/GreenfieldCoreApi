-- DependsOn: ScriptHistory, Clients
drop procedure if exists `Clients.usp_VerifyClient`;
create procedure if not exists `Clients.usp_VerifyClient`(
    p_ClientId char(36),
    p_ClientSecretHash varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    p_Salt varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci)
begin
    select exists (select 1 from `Clients.Clients` c
        where c.ClientId = p_ClientId
        and c.ClientSecretHash = p_ClientSecretHash
        and c.Salt = p_Salt) as Verified;
end;

