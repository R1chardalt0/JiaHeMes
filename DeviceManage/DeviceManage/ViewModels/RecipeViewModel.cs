using DeviceManage.Models;
using Reactive.Bindings;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HandyControl.Controls;
using Microsoft.Extensions.Logging;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;
using DeviceManage.Services.DeviceMagService;

namespace DeviceManage.ViewModels
{
    /// <summary>
    /// 配方管理视图模型
    /// </summary>
    public class RecipeViewModel : ViewModelBase
    {
        private readonly IRecipeService _recipeSvc;
        private readonly ILogger<RecipeViewModel> _logger;

        public ReactiveProperty<ObservableCollection<Recipe>> Recipes { get; }
        public ReactiveProperty<Recipe> SelectedRecipe { get; }
        public ReactiveProperty<Recipe> EditingRecipe { get; }
        public ReactiveProperty<bool> IsEditing { get; }
        public ReactiveProperty<bool> IsDialogOpen { get; }

    
        public RecipeViewModel(IRecipeService recipeService,
                              
                                ILogger<RecipeViewModel> logger)
        {
            _recipeSvc = recipeService;

            _logger = logger;

            Recipes = new ReactiveProperty<ObservableCollection<Recipe>>(new ObservableCollection<Recipe>());
            SelectedRecipe = new ReactiveProperty<Recipe>(new Recipe());
            EditingRecipe = new ReactiveProperty<Recipe>(new Recipe());
            IsEditing = new ReactiveProperty<bool>(false);
            IsDialogOpen = new ReactiveProperty<bool>(false);

            LoadRecipesCommand = new ReactiveCommand().WithSubscribe(async () => await LoadRecipesAsync());
            AddRecipeCommand = new ReactiveCommand().WithSubscribe(() => OpenAddDialog());
            UpdateRecipeCommand = new ReactiveCommand().WithSubscribe(async () => await SaveRecipeAsync());
            DeleteRecipeCommand = new ReactiveCommand<Recipe>().WithSubscribe(async r => await DeleteRecipeAsync(r));
            EditCommand = new ReactiveCommand<Recipe>().WithSubscribe(r => OpenEditDialog(r));
            CancelCommand = new ReactiveCommand().WithSubscribe(() => CloseDialog());
         

            Task.Run(async () => await LoadRecipesAsync());
        }

        #region Commands
        public ReactiveCommand LoadRecipesCommand { get; }
        public ReactiveCommand AddRecipeCommand { get; }
        public ReactiveCommand UpdateRecipeCommand { get; }
        public ReactiveCommand<Recipe> DeleteRecipeCommand { get; }
        public ReactiveCommand<Recipe> EditCommand { get; }
        public ReactiveCommand CancelCommand { get; }
        public ReactiveCommand<Recipe> OpenItemsDialogCommand { get; }
        #endregion

        private async Task LoadRecipesAsync()
        {
            try
            {
                var list = await _recipeSvc.GetAllRecipesAsync();
                Recipes.Value = new ObservableCollection<Recipe>(list);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "加载配方失败");
            }
        }

        private async Task SaveRecipeAsync()
        {
            try
            {
                var recipe = EditingRecipe.Value;
                if (string.IsNullOrWhiteSpace(recipe.RecipeName))
                {
                    MessageBox.Show("配方名称不能为空");
                    return;
                }

                if (recipe.RecipeId == 0)
                {
                    recipe.Version = 1;
                    await _recipeSvc.AddRecipeAsync(recipe);
                }
                else
                {
                    await _recipeSvc.UpdateRecipeAsync(recipe);
                }

                await LoadRecipesAsync();
                CloseDialog();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "保存配方失败");
            }
        }

        private async Task DeleteRecipeAsync(Recipe recipe)
        {
            if (recipe == null) return;
            var result = MessageBox.Show($"确认删除配方 '{recipe.RecipeName}' ?", "确认", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                await _recipeSvc.DeleteRecipeAsync(recipe.RecipeId);
                await LoadRecipesAsync();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "删除配方失败");
            }
        }

        private void OpenAddDialog()
        {
            EditingRecipe.Value = new Recipe();
            IsEditing.Value = false;
            IsDialogOpen.Value = true;
        }

        private void OpenEditDialog(Recipe recipe)
        {
            if (recipe == null) return;
            EditingRecipe.Value = new Recipe
            {
                RecipeId = recipe.RecipeId,
                RecipeName = recipe.RecipeName,
                Remarks = recipe.Remarks,
                Version = recipe.Version
            };
            IsEditing.Value = true;
            IsDialogOpen.Value = true;
        }

        private void CloseDialog()
        {
            IsDialogOpen.Value = false;
            EditingRecipe.Value = new Recipe();
        }
    }
}
