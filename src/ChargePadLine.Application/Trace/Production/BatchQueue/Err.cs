using ChargePadLine.Application;
using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Shared;

public record Err_物料批_Misc(string Code, string Message) : IErr;


public interface IErr_批次扣料 : IErr
{
    public record Err_批次扣料_扣除量非法(int Required) : IErr_批次扣料
    {
        public string Code => "扣除量非法";

        public string Message => $"扣除量非法：当前输入的扣除量求={Required}";
    }

    public record Err_批次扣料_超额(string BomItemCode, int Remaining, int Required) : IErr_批次扣料
    {
        public string Code => "超额扣料";

        public string Message => $"超额扣料：当前(BOM项={BomItemCode})剩余={Remaining}, 需求={Required}";
    }

    public record Err_批次扣料_指定批次错误(string BomItemCode, string InputBatchCode, string CurrentBatchCode) : IErr_批次扣料
    {
        public string Code => "指定批次扣料错误";

        public string Message => $"扣料错误：(BOM项={BomItemCode})队首批次={CurrentBatchCode}, 但是当前指定批次={InputBatchCode}";
    }
}


public interface IErr_批次上料 : IErr
{
    public record Err_批次上料_BomItem不存在(string BomItemCode): IErr_批次上料
    {
        public string Code => "上料批的BomItem不存在";

        public string Message => $"上料批的BomItem不存在: {nameof(BomItemCode)}={BomItemCode}";
    }

    public record Err_批次上料_批次重复(string BomItemCode, string BatchNum, int ExistingId) : IErr_批次上料
    {
        public string Code => "上料批次重复";

        public string Message => $"上料批次重复：当前队列({BomItemCode})中已有重复批次={BatchNum}项.既有队列项id={ExistingId}";
    }

    public record Err_批次上料_数量非法(int InputAmount) : IErr_批次上料
    {
        public string Code => "上料数量非法";

        public string Message => $"上料数量非法: (当前输入数量={InputAmount})";
    }
}
