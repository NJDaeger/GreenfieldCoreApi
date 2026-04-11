-- DependsOn: ScriptHistory, Codes
create procedure if not exists `BuildCodes.usp_InsertCode`(
    p_ListOrder int,
    p_BuildCode nvarchar(4096))
begin
    insert into `BuildCodes.Codes` (
        ListOrder,
        BuildCode)
    values (
        p_ListOrder,
        p_BuildCode);
    
    if row_count() > 0 then
        select
            bc.CodeId,
            bc.ListOrder,
            bc.BuildCode,
            bc.CreatedOn
        from `BuildCodes.Codes` bc
        where bc.CodeId = last_insert_id();
    end if;
end;