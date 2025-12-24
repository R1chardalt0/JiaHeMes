using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.Recipes.Entities
{
    /// <summary>
    /// 物料信息
    /// </summary>
    [Table("mes_recipes_material")]
    public class Material
    {
        public int Id { get; set; }

        public MaterialCode MaterialCode { get; set; } = "";

        public string Name { get; set; } = null!;

        public MeasureUnit MeasureUnit { get; set; } = "";

        public string Description { get; set; } = "";

        public bool IsDeleted { get; set; }
    }

    public class MaterialEntityTypeConfiguration : IEntityTypeConfiguration<Material>
    {
        public void Configure(EntityTypeBuilder<Material> builder)
        {
            builder.HasIndex(x => x.Name);

            for (var i = 1; i <= 10; i++)
            {
                builder.HasData(new Material
                {
                    Id = i,
                    IsDeleted = false,
                    Name = $"测试物料{i:D2}",
                    Description = "测试物料",
                    MaterialCode = null!,
                    MeasureUnit = null!,
                });
            }

            builder.OwnsOne(x => x.MaterialCode, nb =>
            {
                nb.HasIndex(mcode => mcode.Value).IsUnique();
                for (var i = 1; i <= 10; i++)
                {
                    nb.HasData(new { Id = i, MaterialId = i, Value = $"TST_0000_00{i:D2}", });
                }
            });

            builder.OwnsOne(x => x.MeasureUnit, nb =>
            {
                for (var i = 1; i <= 10; i++)
                {
                    nb.HasData(new { Id = i, MaterialId = i, Value = $"个", });
                }
            });
        }

    }
}
