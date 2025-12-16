using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace
{
    public enum ParameterDataType
    {
        NUMBER,   // 数值型，如 25.5, 100
        TEXT,     // 文本型，如 "运行中"
        FORMULA,  // 公式型，如 "=A1+B1"（根据业务需要）
        BOOLEAN   // 布尔型，如 true/false
    }

    public class Iotdata
    {
        /// <summary>
        /// 参数名称，例如：Temperature, Humidity
        /// </summary>
        [Key]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 参数类型
        /// </summary>
        public ParameterDataType Type { get; set; }

        /// <summary>
        /// 参数值（统一用字符串表示，服务端解析）
        /// </summary>
        public string Value { get; set; } = string.Empty;

        // 可选：添加单位
        public string? Unit { get; set; }
    }
}
