using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.Recipes.Entities
{
    /// <summary>
    /// 物料编码
    /// </summary>
    /// <param name="Value"></param>
    [Owned]
    public record MaterialCode(string Value)
    {
        private MaterialCode() : this("") { }

        public static implicit operator MaterialCode(string code)
        {
            // todo: 校验规则
            return new MaterialCode(code);
        }
    }

    /// <summary>
    /// 产品编码
    /// </summary>
    /// <param name="Value"></param>
    [Owned]
    public record ProductCode(string Value)
    {
        private ProductCode() : this("") { }

        public static implicit operator ProductCode(string code)
        {
            // todo: 校验规则
            return new ProductCode(code);
        }
    }

    /// <summary>
    /// BOM编码
    /// </summary>
    /// <param name="Value"></param>
    [Owned]
    public record BomCode(string Value)
    {
        private BomCode() : this("") { }

        public static implicit operator BomCode(string code)
        {
            // todo: 校验规则
            return new BomCode(code);
        }
    }


    /// <summary>
    /// BOM项编码
    /// </summary>
    /// <param name="Value"></param>
    [Owned]
    public record BomItemCode(string Value)
    {
        private BomItemCode() : this("") { }

        public static implicit operator BomItemCode(string code)
        {
            // todo: 校验规则
            return new BomItemCode(code);
        }
    }

    /// <summary>
    /// 计量单位
    /// </summary>
    /// <param name="Value"></param>
    [Owned]
    public record MeasureUnit(string Value)
    {
        private MeasureUnit() : this("") { }

        public static implicit operator MeasureUnit(string value)
        {
            // todo: 校验规则
            return new MeasureUnit(value);
        }
    }
}
