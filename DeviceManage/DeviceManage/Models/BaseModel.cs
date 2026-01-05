namespace DeviceManage.Models;

/// <summary>
/// 模型基类
/// </summary>
public abstract class BaseModel
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}

