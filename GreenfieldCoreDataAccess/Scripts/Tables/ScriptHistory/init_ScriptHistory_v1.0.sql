create table if not exists ScriptHistory (
    IsInit bit not null,
    AppliesTo nvarchar(255) not null,
    Major int not null,
    Minor int not null,
    AppliedAt datetime not null default CURRENT_TIMESTAMP,
    primary key (IsInit, AppliesTo, Major, Minor)
);

create procedure if not exists usp_RecordScriptExecution(
    p_IsInit bit,
    p_AppliesTo nvarchar(255),
    p_Major int,
    p_Minor int
)
begin
    insert into ScriptHistory (IsInit, AppliesTo, Major, Minor)
    values (p_IsInit, p_AppliesTo, p_Major, p_Minor);
end;

create procedure if not exists usp_ShouldScriptBeApplied(
    p_IsInit bit,
    p_AppliesTo nvarchar(255),
    p_Major int,
    p_Minor int
)
begin
    # Lets say the following data is in the Clients table.
    # 1	Clients	1	0	2025-10-11 17:23:40
    # 0	Clients	1	1	2025-10-11 17:23:51
    # 0	Clients	1	2	2025-10-11 17:23:55
    # 0	Clients	1	3	2025-10-11 17:23:57
    
    -- Lets say we want to check if we can run the following data.      
    -- true, Clients, 2, 0 -> False, because there are other script runs for Clients
    -- false, Clients, 1, 3 -> False, because this script has already ran
    -- false, Clients, 2, 0 -> True, because this script has not ran yet and because Clients exists.
    -- false, AnotherTable, 1, 1 -> False, because no init has been processed for this table.
    -- true, AnotherTable, 1, 0 -> True, because there have been no other inits for this table. (then the script runs right after this check)
    -- false, AnotherTable, 1, 1 -> True, because this script has not ran yet and because AnotherTable exists.

    select case
               -- If this is an init script and there are no scripts applied for the same AppliesTo, then apply it
               when p_IsInit = 1 then (select count(*) from ScriptHistory sh where sh.AppliesTo = p_AppliesTo) = 0
               -- We are not an init script, so check if there are scripts applied for the same AppliesTo
               when (select count(*) from ScriptHistory sh where sh.AppliesTo = p_AppliesTo) > 0 then
                   -- There are scripts applied for the same AppliesTo, so check the versioning
                   case
                       when (select max(sh.Major) from ScriptHistory sh where sh.AppliesTo = p_AppliesTo) < p_Major then 1
                       when (select max(sh.Major) from ScriptHistory sh where sh.AppliesTo = p_AppliesTo) = p_Major
                           and (select max(sh.Minor) from ScriptHistory sh where sh.AppliesTo = p_AppliesTo and sh.Major = p_Major) < p_Minor then 1
                       else 0
                       end
               else 0
               end as ShouldApply;
end;