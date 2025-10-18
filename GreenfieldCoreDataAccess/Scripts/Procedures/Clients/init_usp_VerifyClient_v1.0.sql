-- DependsOn: ScriptHistory, Clients
create procedure if not exists usp_VerifyClient(
    p_ClientId char(36),
    p_ClientSecretHash nvarchar(255),
    p_Salt nvarchar(255))
begin
    select exists (select 1 from Clients c
        where c.ClientId = p_ClientId
        and c.ClientSecretHash = p_ClientSecretHash
        and c.Salt = p_Salt) as Verified;
end;


