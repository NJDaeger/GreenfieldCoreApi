-- DependsOn: ScriptHistory, Codes
drop procedure if exists `BuildCodes.usp_UpdateCode`;
create procedure if not exists `BuildCodes.usp_UpdateCode`(
    p_CodeId bigint,
    p_ListOrder int,
    p_BuildCode varchar(4096))
begin
    update `BuildCodes.Codes`
    set
        ListOrder = p_ListOrder,
        BuildCode = p_BuildCode
    where CodeId = p_CodeId;
end;

