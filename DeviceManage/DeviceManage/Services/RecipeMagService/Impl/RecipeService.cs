using DeviceManage.DBContext;
using DeviceManage.DBContext.Repository;
using DeviceManage.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DeviceManage.Services.RecipeMagService.Impl
{
    /// <summary>
    /// 配方（Recipe）业务实现类
    /// 基本模式与 PlcDeviceService 相同：通过 IRepository<T> 直接操作 DbContext。
    /// </summary>
    public class RecipeService : IRecipeService
    {
        private readonly IRepository<Recipe> _recipeRepo;
        private readonly AppDbContext _db;
        private readonly ILogger<RecipeService> _logger;

        public RecipeService(IRepository<Recipe> recipeRepo,
                              AppDbContext db,
                              ILogger<RecipeService> logger)
        {
            _recipeRepo = recipeRepo;
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// 获取全部配方，包含导航属性 Items
        /// </summary>
        public async Task<List<Recipe>> GetAllRecipesAsync()
        {
            return await _db.Recipes.Include(r => r.Items).ToListAsync();
        }

        /// <summary>
        /// 根据主键获取配方（包含 Items）
        /// </summary>
        public async Task<Recipe?> GetRecipeByIdAsync(int id)
        {
            return await _db.Recipes.Include(r=>r.Items).FirstOrDefaultAsync(r=>r.RecipeId==id);
        }

        /// <summary>
        /// 新增配方
        /// </summary>
        public async Task<Recipe> AddRecipeAsync(Recipe recipe)
        {
            await _recipeRepo.InsertAsync(recipe);
            await _db.SaveChangesAsync();
            return recipe;
        }

        /// <summary>
        /// 更新配方（仅更新简单字段，明细 Items 交由 RecipeItemService 操作）
        /// </summary>
        public async Task<Recipe> UpdateRecipeAsync(Recipe recipe)
        {
            var existing = await _recipeRepo.GetAsync(r => r.RecipeId == recipe.RecipeId);
            if (existing == null) return recipe; // 理论上不应出现

            existing.RecipeName = recipe.RecipeName;
            existing.Remarks     = recipe.Remarks;
            existing.Version     = recipe.Version;

            _recipeRepo.Update(existing);
            await _db.SaveChangesAsync();
            return existing;
        }

        /// <summary>
        /// 删除配方（级联删除明细取决于数据库约束）
        /// </summary>
        public async Task DeleteRecipeAsync(int id)
        {
            var entity = await _recipeRepo.GetAsync(r => r.RecipeId == id);
            if (entity != null)
            {
                _recipeRepo.Delete(entity);
                await _db.SaveChangesAsync();
            }
        }
    }
}
