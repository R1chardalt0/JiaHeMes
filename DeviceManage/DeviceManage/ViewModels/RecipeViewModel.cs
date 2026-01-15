using DeviceManage.Models;
using DeviceManage.Services.DeviceMagService;
using DeviceManage.Services.DeviceMagService.Impl;
using HandyControl.Controls;
using Microsoft.Extensions.Logging;
using Reactive.Bindings;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;
using Tag = DeviceManage.Models.Tag;

namespace DeviceManage.ViewModels
{
    /// <summary>
    /// 配方管理视图模型
    /// </summary>
    public class RecipeViewModel : ViewModelBase
    {
        private readonly IRecipeService _recipeSvc;
        private readonly IPlcDeviceService _plcDeviceService;
        private readonly ISwitchRecipeService _switchRecipeService;
        private readonly ILogger<RecipeViewModel> _logger;

        public ReactiveProperty<ObservableCollection<Recipe>> Recipes { get; }
        public ReactiveProperty<Recipe> SelectedRecipe { get; }
        public ReactiveProperty<Recipe> EditingRecipe { get; }
        public ReactiveProperty<bool> IsEditing { get; }
        public ReactiveProperty<bool> IsDialogOpen { get; }
        public ReactiveProperty<ObservableCollection<Tag>> RecipeTags { get; }
        public ReactiveProperty<bool> IsTagsListVisible { get; }
        public ReactiveProperty<string> TagsCountText { get; }


        public RecipeViewModel(IRecipeService recipeService, ILogger<RecipeViewModel> logger, IPlcDeviceService plcDeviceService, ISwitchRecipeService switchRecipeService)
        {
            _recipeSvc = recipeService;
            _logger = logger;

            Recipes = new ReactiveProperty<ObservableCollection<Recipe>>(new ObservableCollection<Recipe>());
            SelectedRecipe = new ReactiveProperty<Recipe>(new Recipe());
            EditingRecipe = new ReactiveProperty<Recipe>(new Recipe());
            IsEditing = new ReactiveProperty<bool>(false);
            IsDialogOpen = new ReactiveProperty<bool>(false);
            RecipeTags = new ReactiveProperty<ObservableCollection<Tag>>(new ObservableCollection<Tag>());
            IsTagsListVisible = new ReactiveProperty<bool>(false);
            TagsCountText = new ReactiveProperty<string>("");

            LoadRecipesCommand = new ReactiveCommand().WithSubscribe(async () => await LoadRecipesAsync());
            AddRecipeCommand = new ReactiveCommand().WithSubscribe(() => OpenAddDialog());
            UpdateRecipeCommand = new ReactiveCommand().WithSubscribe(async () => await SaveRecipeAsync());
            DeleteRecipeCommand = new ReactiveCommand<Recipe>().WithSubscribe(async r => await DeleteRecipeAsync(r));
            EditCommand = new ReactiveCommand<Recipe>().WithSubscribe(r => OpenEditDialog(r));
            CancelCommand = new ReactiveCommand().WithSubscribe(() => CloseDialog());
            ViewRecipeTagsCommand = new ReactiveCommand<Recipe>().WithSubscribe(async r => await ViewRecipeTagsAsync(r));
            CloseTagsListCommand = new ReactiveCommand().WithSubscribe(() => CloseTagsList());
            ToggleTagCommand = new ReactiveCommand<Tag>().WithSubscribe(async tag => await ToggleTagAsync(tag));
            BatchToggleCommand = new ReactiveCommand().WithSubscribe(async () => await BatchToggleAsync());


            Task.Run(async () => await LoadRecipesAsync());
            _plcDeviceService = plcDeviceService;
            _switchRecipeService = switchRecipeService;
        }

        #region Commands
        public ReactiveCommand LoadRecipesCommand { get; }
        public ReactiveCommand AddRecipeCommand { get; }
        public ReactiveCommand UpdateRecipeCommand { get; }
        public ReactiveCommand<Recipe> DeleteRecipeCommand { get; }
        public ReactiveCommand<Recipe> EditCommand { get; }
        public ReactiveCommand CancelCommand { get; }
        public ReactiveCommand<Recipe> OpenItemsDialogCommand { get; }
        public ReactiveCommand<Recipe> ViewRecipeTagsCommand { get; }
        public ReactiveCommand CloseTagsListCommand { get; }
        public ReactiveCommand<Tag> ToggleTagCommand { get; }
        public ReactiveCommand BatchToggleCommand { get; }
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

                // 如果当前配方的状态设置为 true（使用中），则将其他所有配方的状态设置为 false（停用）
                if (recipe.Status)
                {
                    var allRecipes = await _recipeSvc.GetAllRecipesAsync();
                    foreach (var existingRecipe in allRecipes)
                    {
                        if (existingRecipe.RecipeId != recipe.RecipeId && existingRecipe.Status)
                        {
                            existingRecipe.Status = false;
                            await _recipeSvc.UpdateRecipeAsync(existingRecipe);
                        }
                    }
                }

                if (recipe.RecipeId == 0)
                {
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
                if (recipe.Status)
                {
                    var allRecipes = await _recipeSvc.GetAllRecipesAsync();
                    var otherRecipes = allRecipes.Where(r => r.RecipeId != recipe.RecipeId).ToList();

                    if (otherRecipes.Any())
                    {
                        var firstAvailableRecipe = otherRecipes.First();
                        firstAvailableRecipe.Status = true;
                        await _recipeSvc.UpdateRecipeAsync(firstAvailableRecipe);
                    }
                }

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
                Status = recipe.Status,
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

        /// <summary>
        /// 查看配方对应的标签列表
        /// </summary>
        private async Task ViewRecipeTagsAsync(Recipe recipe)
        {
            if (recipe == null) return;

            try
            {
                var tags = await GetTagListByRecipeIdAsync(recipe.RecipeId);
                RecipeTags.Value = new ObservableCollection<Tag>(tags);

                // Update the tags count text
                TagsCountText.Value = $"共 {tags.Count} 个配方点位";

                // Show the tags list in the UI
                IsTagsListVisible.Value = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查看配方点位失败 (RecipeId: {recipe.RecipeId}): {ex.Message}");
                MessageBox.Show($"获取配方点位失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 根据配方ID获取标签列表
        /// </summary>
        public async Task<List<Tag>> GetTagListByRecipeIdAsync(int id)
        {
            try
            {
                return await _recipeSvc.GetTagListByRecipeIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"根据配方ID获取配方点位列表失败 (RecipeId: {id}): {ex.Message}");
                throw;
            }
        }

        private void CloseTagsList()
        {
            IsTagsListVisible.Value = false;
        }

        /// <summary>
        /// 切换配方状态
        /// </summary>
        private async Task ToggleTagAsync(Tag tag)
        {
            if (tag == null) return;

            try
            {
                var result = MessageBox.Show($"确定要切换配方点位: {tag.PlcTagName} 吗？", "确认切换", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                var res = await _switchRecipeService.SwitchRecipeTagAsync(tag);
                if (res)
                {
                    MessageBox.Show($"已切换配方点位: {tag.PlcTagName}", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"切换配方点位失败: {tag.PlcTagName}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"切换配方点位失败 (TagId: {tag?.Id}): {ex.Message}");
                MessageBox.Show($"切换配方点位失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 批量切换所有设备配方状态
        /// </summary>
        private async Task BatchToggleAsync()
        {
            try
            {
                var tags = RecipeTags.Value;
                if (tags == null || tags.Count == 0)
                {
                    MessageBox.Show("没有配方可切换", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 提示用户确认是否要批量切换
                var result = MessageBox.Show($"确定要一键切换 {tags.Count} 个配方点位吗？", "确认批量切换", MessageBoxButton.YesNo, MessageBoxImage.Question);

                // 如果用户选择"否"，则取消操作
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                // 批量切换所有标签的状态
                int successCount = 0;
                int failCount = 0;
                var failedTags = new System.Collections.Generic.List<string>();

                foreach (var tag in tags)
                {
                    try
                    {
                        var res = await _switchRecipeService.SwitchRecipeTagAsync(tag);
                        if (res)
                        {
                            successCount++;
                        }
                        else
                        {
                            failCount++;
                            failedTags.Add(tag.PlcTagName);
                        }
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        failedTags.Add($"{tag.PlcTagName} ({ex.Message})");
                        _logger.LogError(ex, $"切换配方点位失败 (TagId: {tag?.Id}): {ex.Message}");
                    }
                }

                // 显示批量操作结果
                if (failCount == 0)
                {
                    MessageBox.Show($"已成功切换 {successCount} 个配方点位", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (successCount == 0)
                {
                    MessageBox.Show($"切换配方点位全部失败，失败数量: {failCount}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    var failDetails = string.Join(", ", failedTags.Take(5)); // 只显示前5个失败的标签
                    if (failedTags.Count > 5)
                    {
                        failDetails += $" 等{failedTags.Count}个点位";
                    }
                    MessageBox.Show($"批量切换完成: 成功 {successCount} 个, 失败 {failCount} 个\n\n失败详情: {failDetails}", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"一键切换配方点位失败: {ex.Message}");
                MessageBox.Show($"一键切换配方点位失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
