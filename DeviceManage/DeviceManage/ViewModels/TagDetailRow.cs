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
        public ReactiveProperty<string?> Unit { get; } = new ReactiveProperty<string?>();
        public ReactiveProperty<string> Remarks { get; } = new ReactiveProperty<string>();

        public TagDetailRow()
        {
            TagName.Value = string.Empty;
            Address.Value = string.Empty;
            DataType.Value = Models.DataType.Float;
            Unit.Value = null;
            Remarks.Value = string.Empty;
        }

        public TagDetailRow(TagDetail detail) : this()
        {
            TagName.Value = detail.TagName;
            Address.Value = detail.Address;
            DataType.Value = detail.DataType;
            Unit.Value = detail.Unit;
            Remarks.Value = detail.Remarks;
        }

        public TagDetail ToEntity() => new()
        {
            TagName = TagName.Value,
            Address = Address.Value,
            DataType = DataType.Value,
            Unit = Unit.Value,
            Remarks = Remarks.Value ?? string.Empty
        };
    }
}
