using ChargePadLine.Entitys.Systems;

namespace ChargePadLine.WebApi.Controllers.util
{
    public class ApiResponse
    {
        public int Code { get; set; } = 200;
        public string Msg { get; set; } = "操作成功";
        public object Data { get; set; }  // 动态数据字段（可选）
    }
    public class ApiResponse<T> : ApiResponse
    {
        public new T Data { get; set; }  // 泛型数据字段
    }
    public class LoginResponse : ApiResponse
    {
        public string Token { get; set; }
        public SysUser User { get; set; }
        public List<string> Permissions { get; set; }
        public List<string> Roles { get; set; }
    }
}
