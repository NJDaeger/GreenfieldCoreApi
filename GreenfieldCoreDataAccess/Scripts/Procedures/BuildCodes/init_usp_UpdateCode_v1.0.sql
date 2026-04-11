-- DependsOn: ScriptHistory, Codes
create procedure if not exists `BuildCodes.usp_UpdateCode`(
    p_CodeId bigint,
    p_ListOrder int,
    p_BuildCode nvarchar(4096))
begin
    update `BuildCodes.Codes`
    set
        ListOrder = p_ListOrder,
        BuildCode = p_BuildCode
    where CodeId = p_CodeId;
end;