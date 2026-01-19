namespace ChargePadLine.WebApi.util
{
    /// <summary>
    /// 分页的响应
    /// </summary>
    /// <typeparam name="IItem"></typeparam>
    public class PagedResp<IItem> : Resp<IList<IItem>>
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
