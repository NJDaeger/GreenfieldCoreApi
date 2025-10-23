-- DependsOn: ScriptHistory, Clients
create procedure if not exists usp_UpdateClientSecret(
    p_ClientId char(36),
    p_NewSecretHash varchar(256),
    p_NewSalt varchar(255))
begin
    update Clients
    set ClientSecretHash = p_NewSecretHash,
        Salt = p_NewSalt
    where ClientId = p_ClientId;
end;