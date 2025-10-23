-- DependsOn: ScriptHistory, ClientRoles
create procedure if not exists usp_ClearClientRoles(
    p_ClientId char(36))
begin
    delete from ClientRoles where ClientId = p_ClientId;
end;