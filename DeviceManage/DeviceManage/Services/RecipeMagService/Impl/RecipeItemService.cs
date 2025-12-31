using DeviceManage.DBContext;
using DeviceManage.DBContext.Repository;
using DeviceManage.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DeviceManage.Services.RecipeMagService.Impl
{
    /// <summary>
    /// 配方项（RecipeItem）业务实现类
    /// 负责一个配方下的 Tag 目标值增删改查。
    /// </summary>
    public class RecipeItemService : IRecipeItemService
    {
        private readonly IRepository<RecipeItem> _repo;
        private readonly AppDbContext _db;
        private readonly ILogger<RecipeItemService> _logger;

        public RecipeItemService(IRepository<RecipeItem> repo,
                                  AppDbContext db,
                                  ILogger<RecipeItemService> logger)
        {
            _repo   = repo;
            _db     = db;
            _logger = logger;
        }

        /// <summary>
        /// 获取某配方的全部配方项（包含 Tag 导航属性）
        /// </summary>
        public async Task<List<RecipeItem>> GetItemsByRecipeIdAsync(int recipeId)
        {
            return await _db.RecipeItems.Include(r=>r.Tag).Where(r=>r.RecipeId==recipeId).ToListAsync();
        }

        /// <summary>
        /// 根据主键获取配方项
        /// </summary>
        public async Task<RecipeItem?> GetItemByIdAsync(int id)
        {
            return await _repo.GetAsync(r => r.Id == id);
        }

        /// <summary>
        /// 新增配方项
        /// </summary>
        public async Task<RecipeItem> AddItemAsync(RecipeItem item)
        {
            await _repo.InsertAsync(item);
            await _db.SaveChangesAsync();
            return item;
        }

        /// <summary>
        /// 更新配方项（Tag、TagKey、TargetValue）
        /// </summary>
        public async Task<RecipeItem> UpdateItemAsync(RecipeItem item)
        {
            var exist = await _repo.GetAsync(r => r.Id == item.Id);
            if (exist == null) return item;

            exist.TagId       = item.TagId;
            exist.TagKey      = item.TagKey;
            exist.TargetValue = item.TargetValue;

            _repo.Update(exist);
            await _db.SaveChangesAsync();
            return exist;
        }

        /// <summary>
        /// 删除配方项
        /// </summary>
        public async Task DeleteItemAsync(int id)
        {
            var entity = await _repo.GetAsync(r => r.Id == id);
            if (entity != null)
            {
                _repo.Delete(entity);
                await _db.SaveChangesAsync();
            }
        }
    }
}
