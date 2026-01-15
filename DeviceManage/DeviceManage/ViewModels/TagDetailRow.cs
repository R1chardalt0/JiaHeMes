using DeviceManage.Models;
using Reactive.Bindings;

namespace DeviceManage.ViewModels
{
    /// <summary>
    /// TagDetail 的可编辑行模型（用于 DataGrid 编辑）
    /// </summary>
    public class TagDetailRow
    {
        // 这里用默认构造函数 new ReactiveProperty<T>()，避免 ReactiveProperty 的重载歧义
        public ReactiveProperty<string> TagName { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> Address { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<DataType> DataType { get; } = new ReactiveProperty<DataType>();
        public ReactiveProperty<string> Value { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string?> Unit { get; } = new ReactiveProperty<string?>();
        public ReactiveProperty<string> Remarks { get; } = new ReactiveProperty<string>();
        
        // 判断是否为 Bool 类型的计算属性
        public ReactiveProperty<bool> IsBoolType { get; } = new ReactiveProperty<bool>(false);
        
        // 用于跟踪之前的数据类型，以便检测从 Bool 切换到其他类型
        private DataType? _previousDataType = null;

        public TagDetailRow()
        {
            TagName.Value = string.Empty;
            Address.Value = string.Empty;
            DataType.Value = Models.DataType.Float;
            Value.Value = string.Empty;
            Unit.Value = null;
            Remarks.Value = string.Empty;
            
            // 初始化 IsBoolType
            IsBoolType.Value = DataType.Value == Models.DataType.Bool;
            _previousDataType = DataType.Value;
            
            // 订阅数据类型变化，当切换到 Bool 时自动调整值，从 Bool 切换到其他类型时清空值
            DataType.Subscribe(dt =>
            {
                // 检查是否从 Bool 切换到其他类型
                if (_previousDataType == Models.DataType.Bool && dt != Models.DataType.Bool)
                {
                    // 从 Bool 切换到其他类型，清空值
                    Value.Value = string.Empty;
                }
                
                // 更新 IsBoolType 属性
                IsBoolType.Value = dt == Models.DataType.Bool;
                
                if (dt == Models.DataType.Bool)
                {
                    // 如果当前值不是 "true" 或 "false"，则设置为 "true"
                    var currentValue = Value.Value?.Trim().ToLower();
                    if (currentValue != "true" && currentValue != "false")
                    {
                        Value.Value = "true";
                    }
                }
                
                // 更新之前的数据类型
                _previousDataType = dt;
            });
        }

        public TagDetailRow(TagDetail detail) : this()
        {
            TagName.Value = detail.TagName;
            Address.Value = detail.Address;
            // 先设置之前的数据类型，避免触发清空逻辑
            _previousDataType = detail.DataType;
            DataType.Value = detail.DataType;
            Value.Value = detail.Value;
            Unit.Value = detail.Unit;
            Remarks.Value = detail.Remarks;
        }

        public TagDetail ToEntity() => new()
        {
            TagName = TagName.Value,
            Address = Address.Value,
            DataType = DataType.Value,
            Value = Value.Value ?? string.Empty,
            Unit = Unit.Value,
            Remarks = Remarks.Value ?? string.Empty
        };
    }
}
