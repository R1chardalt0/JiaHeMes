using DeviceManage.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceManage.Services.DeviceMagService
{
    /// <summary>
    /// 配方（Recipe）业务接口
    /// </summary>
    public interface IRecipeService
    {
        Task<List<Recipe>> GetAllRecipesAsync();
        Task<Recipe?> GetRecipeByIdAsync(int id);
        Task<Recipe> AddRecipeAsync(Recipe recipe);
        Task<Recipe> UpdateRecipeAsync(Recipe recipe);
        Task DeleteRecipeAsync(int id);
        Task<List<Tag>> GetTagListByRecipeIdAsync(int id);
    }
}

