using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.Recipes.Entities
{
    public class Material
    {
        public int Id { get; set; }

        public MaterialCode MaterialCode { get; set; } = "";

        public string Name { get; set; } = null!;

        public MeasureUnit MeasureUnit { get; set; } = "";

        public string Description { get; set; } = "";

        public bool IsDeleted { get; set; }
    }
}
