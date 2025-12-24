using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Application.Trace.Production.Recipes
{
    public interface IBomRecipeRepository 
    {
        Task<BomRecipeItem?> FindBomItemByCodeWithNoTrackAsync(string bomItemCode);
        public Task<PaginatedList<BomRecipe>> PaginateAsync(int page, int size);
    }
}
