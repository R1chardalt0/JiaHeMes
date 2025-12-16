namespace ChargePadLine.WebApi.util
{
    /// <summary>
    /// 响应
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class Resp<TData>
    {
        /// <summary>
        /// 标识请求是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 数据或者失败时的数据
        /// </summary>
        public TData? Data { get; set; }
        public ErrorInfo? ErrorInfo { get; set; }

        /// <summary>
        /// 错误键码，或者属性错误
        /// </summary>
        public string ErrorKey => this.ErrorInfo?.Code ?? "";
        /// <summary>
        /// 错误描述
        /// </summary>
        public string ErrorMessage => this.ErrorInfo?.Message ?? "";
    }
    public class Resp : Resp<object> { }
}
