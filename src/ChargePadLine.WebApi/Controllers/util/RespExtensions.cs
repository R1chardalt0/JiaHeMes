using ChargePadLine.Service;
using Microsoft.FSharp.Core;
namespace ChargePadLine.WebApi.util
{
  public static class RespExtensions
  {
    public static Resp MakeSuccess()
    {
      var resp = new Resp()
      {
        Success = true,
        Data = { },
      };
      return resp;
    }

    public static Resp<TData> ToResp<TData>(this FSharpResult<TData, ErrorInfo> res)
    {
      if (res.IsOk)
      {
        return MakeSuccess(res.ResultValue);
      }
      else
      {
        var err = res.ErrorValue;
        return MakeFail<TData>(err.Code, err.Message);
      }
    }

    public static Resp<TData> FromResult<TData>(FSharpResult<TData, ErrorInfo> res)
    {
      if (res.IsOk)
      {
        return MakeSuccess(res.ResultValue);
      }
      else
      {
        var err = res.ErrorValue;
        return MakeFail<TData>(err.Code, err.Message);
      }
    }


    public static Resp<TData> MakeSuccess<TData>(TData data)
    {
      var resp = new Resp<TData>()
      {
        Success = true,
        Data = data,
      };
      return resp;
    }


    public static PagedResp<TItem> MakePagedSuccess<TItem>(PaginatedList<TItem> data)
    {
      if (data == null)
      {
        return MakePagedEmpty<TItem>();
      }

      var resp = new PagedResp<TItem>()
      {
        Success = true,
        Data = data.ToList(), // 确保转换为List
        Total = data.TotalCounts,
        Page = data.PageIndex,
        PageSize = data.PageSize,
      };
      return resp;
    }

    public static PagedResp<TItem> MakePagedEmpty<TItem>()
    {
      var resp = new PagedResp<TItem>()
      {
        Success = true,
        Data = new List<TItem>(),
        Total = 0,
        Page = 1,
        PageSize = 10, // 默认页面大小
      };
      return resp;
    }

    public static Resp MakeFail(string errCode, string errMsg)
    {
      var resp = new Resp()
      {
        Success = false,
        ErrorInfo = new ErrorInfo()
        {
          Code = errCode,
          Message = errMsg,
          ErrorFields = new List<ErrorField>(),
        },
      };
      return resp;
    }


    public static Resp<TData> MakeFail<TData>(string errCode, string errMsg)
    {
      var resp = new Resp<TData>()
      {
        Success = false,
        ErrorInfo = new ErrorInfo()
        {
          Code = errCode,
          Message = errMsg,
          ErrorFields = new List<ErrorField>(),
        },
      };
      return resp;
    }

    public static Resp<TData> MakeFail<TData>(string errCode, string errMsg, IList<ErrorField> errFields)
    {
      var resp = new Resp<TData>()
      {
        Success = false,
        ErrorInfo = new ErrorInfo()
        {
          Code = errCode,
          Message = errMsg,
          ErrorFields = errFields
        },
      };
      return resp;
    }

    public static Resp<TData> AddFieldError<TData>(this Resp<TData> resp, string fieldName, params string[] fieldErrors)
    {
      if (resp.ErrorInfo == null)
      {
        resp.ErrorInfo = new ErrorInfo();
      }
      if (resp.ErrorInfo.ErrorFields == null)
      {
        resp.ErrorInfo.ErrorFields = new List<ErrorField>();
      }
      resp.ErrorInfo.ErrorFields.Add(new ErrorField() { Name = fieldName, Errors = fieldErrors });
      return resp;
    }

    /// <summary>
    /// 创建字段错误
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="fieldName"></param>
    /// <param name="fieldError"></param>
    /// <returns></returns>
    public static Resp<TData> MakeFieldFail<TData>(string fieldName, string fieldError) =>
        MakeFail<TData>("400", $"{fieldName}: {fieldError}").AddFieldError(fieldName, fieldError);
  }
}
