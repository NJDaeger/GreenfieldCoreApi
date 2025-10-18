-- DependsOn: ScriptHistory, Clients
create procedure if not exists usp_DeleteClient(
    p_ClientId char(36))
begin
    delete from Clients where ClientId = p_ClientId;
end;


