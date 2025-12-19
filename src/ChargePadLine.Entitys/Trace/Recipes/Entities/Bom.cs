using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.Recipes.Entities
{
    public enum BomRecipeStatus
    {
        Draft = 0,
        Committed = 1,
        Approved = 2,
        Deleted = 999,
    }

    public class BomRecipe
    {
        public int Id { get; set; }

        /// <summary>
        /// BOM名称，相同意味着适用
        /// </summary>
        public string BomName { get; set; } = "";

        /// <summary>
        /// BOM编码，唯一编号
        /// </summary>
        public BomCode BomCode { get; set; } = null!;

        public ProductCode ProductCode { get; set; } = null!;

        public string Description { get; set; } = "";

        public IList<BomRecipeItem> Items { get; set; } = new List<BomRecipeItem>();

        /// <summary>
        /// BOM配方状态
        /// </summary>
        public BomRecipeStatus Status { get; set; }

        public static BomRecipe MakeNew(BomCode bomCode, string bomName, ProductCode prodCode, string description)
        {
            return new BomRecipe()
            {
                BomCode = bomCode,
                ProductCode = prodCode,
                BomName = bomName,
                Description = description,
                Items = new List<BomRecipeItem>(),
                Status = BomRecipeStatus.Draft,
            };
        }
    }


    public class BomRecipeItem
    {
        public int Id { get; set; }
        /// <summary>
        /// BOM子项的编号。该编号在父BOM范围内唯一
        /// </summary>
        public BomItemCode BomItemCode { get; set; } = "";

        #region 
        public int BomId { get; set; }
        public BomRecipe? Bom { get; set; }
        #endregion

        #region 物料信息
        /// <summary>
        /// 物料编码
        /// </summary>
        public MaterialCode MaterialCode { get; set; } = null!;
        /// <summary>
        /// 缓存的物料名称
        /// </summary>
        public string MaterialName = "";
        /// <summary>
        /// 缓存的计量单位
        /// </summary>
        public MeasureUnit MeasureUnit { get; set; } = null!;
        #endregion

        public decimal Quota { get; set; }

        public string Description = "";

        public bool IsDeleted { get; set; }
    }


    public static class BomRecipeExtensions
    {
        public static void SetMaterial(this BomRecipeItem item, Material material)
        {
            item.MaterialCode = material.MaterialCode.Value;
            item.MeasureUnit = material.MeasureUnit.Value;
            item.MaterialName = material.Name;
        }

        public static BomRecipeItem AddItem(this BomRecipe bom, BomItemCode itemCode, Material material, decimal amount, string? description = null)
        {
            var item = new BomRecipeItem()
            {
                BomItemCode = itemCode,

                BomId = bom.Id,
                Bom = bom,
                Quota = amount,
                Description = description ?? "",
                IsDeleted = false,
            };
            item.SetMaterial(material);

            if (bom.Items == null)
            {
                bom.Items = new List<BomRecipeItem>();
            }
            bom.Items.Add(item);
            return item;
        }
    }
}
