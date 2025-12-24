using ChargePadLine.Entitys.Trace.Production;

namespace ChargePadLine.Application.Trace.Production.BatchQueue;

public record 扣料描述(int Id, string BomItemCode, int Offset, string BatchNum, int Amout);

public static class 扣料描述列表扩展
{
    public static IReadOnlyList<SKU> ExpandSKUs(this IReadOnlyList<扣料描述> list)
    {
        return list
            .SelectMany(i => Enumerable.Range(i.Offset, i.Amout), (description, idx) => description.FormatSKU(idx))
            .ToList();
    }

    public static SKU FormatSKU(this 扣料描述 description, int offset)
    {
        return $"{description.BatchNum}-{offset + 1}";
    }
}
