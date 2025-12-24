using System.Collections.Generic;
using System.Linq;
using ChargePadLine.Application;
using ChargePadLine.Application.Trace.Production.BatchQueue;
using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.Production.BatchQueue;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Shared;
using Microsoft.FSharp.Core;


public record CmdArg_物料批_上料(BomItemCode BomItemCode, string BatchCode, int Amount, int Priority);

public record CmdArg_物料批_扣料(BomItemCode BomItemCode, IList<BatchMaterialQueueItem> Candicates, int RequiredAmount);

public record CmdArg_物料批_调整优先级(BatchMaterialQueueItem QueueItem, int Priority);



public static class MaterialBatchQueueCmds
{

    public static FSharpResult<BatchMaterialQueueItem, IErr_批次上料> 上料(CmdArg_物料批_上料 cmdArg)
    {
        var queueItem = new BatchMaterialQueueItem()
        { 
            BomItemCode = cmdArg.BomItemCode.Value,
            CreatedAt = DateTimeOffset.UtcNow,
            TotalAmount = cmdArg.Amount,
            RemainingAmount = cmdArg.Amount,
            BatchCode = cmdArg.BatchCode,
            Priority = cmdArg.Priority,
            IsDeleted = false,
            DeletedAt = default,
        };
        return queueItem.ToOkResult<BatchMaterialQueueItem, IErr_批次上料>();
    }

    /// <summary>
    /// 如果成功，则扣减剩余数量，并返回扣料描述；如果不成功，不会扣料
    /// </summary>
    /// <param name="procingItem"></param>
    /// <param name="requiredAmount"></param>
    /// <returns></returns>
    public static FSharpResult<扣料描述, IErr_批次扣料> 单扣料(BatchMaterialQueueItem procingItem, int requiredAmount)
    {
        var remaining = procingItem.RemainingAmount;
        if (remaining < requiredAmount)
        {
            return new IErr_批次扣料.Err_批次扣料_超额(procingItem.BomItemCode, remaining, requiredAmount)
                .ToErrResult<扣料描述, IErr_批次扣料>();
        }

        procingItem.RemainingAmount = remaining - requiredAmount;
        // 如果全部扣完，标记删除
        if (procingItem.RemainingAmount == 0)
        {
            procingItem.IsDeleted = true;
            procingItem.DeletedAt = DateTimeOffset.UtcNow;
        }

        // 这是此批次里的第几个？
        var offset = procingItem.TotalAmount - remaining;
        var description = new 扣料描述(procingItem.Id, procingItem.BomItemCode, offset, procingItem.BatchCode, requiredAmount);
        return description.ToOkResult<扣料描述, IErr_批次扣料>();
    }


    public static FSharpResult<List<扣料描述>, IErr_批次扣料> 扣料(CmdArg_物料批_扣料 cmdArg)
    {
        var 初始总余量 = cmdArg.Candicates.Sum(i => i.RemainingAmount);

        // 如果初始总余量不足，则无需后续尝试
        if (初始总余量 < cmdArg.RequiredAmount)
        {
            var err = new IErr_批次扣料.Err_批次扣料_超额(
                cmdArg.BomItemCode.Value,
                初始总余量,
                cmdArg.RequiredAmount);
            return err.ToErrResult<List<扣料描述>, IErr_批次扣料>();
        }

        var 仍需扣除量 = cmdArg.RequiredAmount;
        List<扣料描述> descriptions = new();
        foreach (var candicate in cmdArg.Candicates)
        {
            if (仍需扣除量 == 0)
            {
                break;
            }

            // 大部分情况下，可以直接扣料，所以这里先直接乐观尝试单扣
            var res = 单扣料(candicate, 仍需扣除量);
            if (res.IsOk)
            {
                descriptions.Add(res.ResultValue);
                return res.Select(i => descriptions);
            }

            // 如果条件不满足单扣，则需要分次扣除
            var current = 单扣料(candicate, candicate.RemainingAmount);
            if (current.IsError)
            {
                throw new Exception($"扣料失败：{res.ErrorValue?.Message}");
            }
            var ok = current.ResultValue;
            descriptions.Add(ok);
            仍需扣除量 -= ok.Amout;
        }

        if (仍需扣除量 != 0)  // 说明不够扣
        {
            return new IErr_批次扣料.Err_批次扣料_超额(
                    cmdArg.BomItemCode.Value,
                    初始总余量, 
                    cmdArg.RequiredAmount
                )
                .ToErrResult<List<扣料描述>, IErr_批次扣料>();
        }
        else 
        {
            return descriptions.ToOkResult<List<扣料描述>, IErr_批次扣料>();
        }
    }


    public static FSharpResult<BatchMaterialQueueItem, IErr> 调整优先级(CmdArg_物料批_调整优先级 cmdArg)
    {
        var item = cmdArg.QueueItem;
        item.Priority = cmdArg.Priority;
        return item.ToOkResult<BatchMaterialQueueItem, IErr>();
    }
}
