using SqlSugar;

namespace Api.Framework;

public abstract class EntityBase
{

    [SugarColumn(IsOnlyIgnoreUpdate = true)]
    public virtual long CreateAt { get; set; }

    [SugarColumn(IsOnlyIgnoreInsert = true)]
    public virtual long? UpdateAt { get; set; }

    [SugarColumn(IsOnlyIgnoreInsert = true)]
    public virtual long? DeleteAt { get; set; }
}