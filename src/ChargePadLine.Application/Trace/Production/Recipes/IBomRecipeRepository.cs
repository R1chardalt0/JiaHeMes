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
        Task<BomRecipe?> FindAsync(int bomRecipeId);
        Task<BomRecipeItem?> FindBomItemByCodeWithNoTrackAsync(string bomItemCode);
        public Task<PaginatedList<BomRecipe>> PaginateAsync(int page, int size);
        
        /// <summary>
        /// 添加新的 BOM 配方记录
        /// </summary>
        /// <param name="entity">要添加的 BOM 配方实体</param>
        /// <param name="saveChanges">是否立即保存更改</param>
        /// <returns>添加后的实体</returns>
        Task<BomRecipe> AddAsync(BomRecipe entity, bool saveChanges);
        
        /// <summary>
        /// 保存更改到数据库
        /// </summary>
        /// <returns>受影响的行数</returns>
        Task<int> SaveChangesAsync();
    }
}
