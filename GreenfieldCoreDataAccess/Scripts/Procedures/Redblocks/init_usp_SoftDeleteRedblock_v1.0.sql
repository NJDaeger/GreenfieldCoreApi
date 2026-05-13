-- DependsOn: ScriptHistory, Projects, Redblocks, Statuses
create procedure if not exists `Redblocks.usp_SoftDeleteRedblock`(
    p_ProjectId bigint,
    p_KeyNumber bigint,
    p_DeletedBy bigint)
begin
    update `Redblocks.Redblocks` rb
    set
        rb.DeletedBy = p_DeletedBy,
        rb.DeletedOn = current_timestamp
    where rb.ProjectId = p_ProjectId
      and rb.KeyNumber = p_KeyNumber
      and rb.DeletedOn is null;

    if row_count() > 0 then
        select
            rb.RedblockId,
            rb.ProjectId,
            rb.KeyNumber,
            rb.Message,
            rs_ranked.Status,
            rb.X,
            rb.Y,
            rb.Z,
            rb.CreatedBy,
            rb.CreatedOn,
            rb.UpdatedBy,
            rb.UpdatedOn,
            rb.DeletedBy,
            rb.DeletedOn
        from `Redblocks.Redblocks` rb
                 inner join (
            select rs.*, ROW_NUMBER() over (partition by rs.RedblockId order by rs.CreatedOn desc) as StatusNumber
            from `Redblocks.Statuses` rs
        ) rs_ranked on rb.RedblockId = rs_ranked.RedblockId and rs_ranked.StatusNumber = 1
        where rb.ProjectId = p_ProjectId
          and rb.KeyNumber = p_KeyNumber;
    end if;
end;

