using DeviceManage.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceManage.Services.RecipeMagService
{
    public interface IRecipeItemService
    {
        Task<List<RecipeItem>> GetItemsByRecipeIdAsync(int recipeId);
        Task<RecipeItem?> GetItemByIdAsync(int id);
        Task<RecipeItem> AddItemAsync(RecipeItem item);
        Task<RecipeItem> UpdateItemAsync(RecipeItem item);
        Task DeleteItemAsync(int id);
    }
}

