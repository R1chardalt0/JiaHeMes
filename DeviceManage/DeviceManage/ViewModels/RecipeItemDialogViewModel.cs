using DeviceManage.DBContext;
using DeviceManage.Models;
using DeviceManage.Services.RecipeMagService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Reactive.Bindings;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManage.ViewModels
{
    /// <summary>
    /// 负责单个配方下配方项（RecipeItem）的增删改
    /// </summary>
    public class RecipeItemDialogViewModel : ViewModelBase
    {
        private readonly IRecipeItemService _itemSvc;
        private readonly AppDbContext _db;
        private readonly ILogger<RecipeItemDialogViewModel> _logger;

        public int RecipeId { get; set; }

        // 列表
        public ReactiveProperty<ObservableCollection<RecipeItem>> Items { get; }
        // 所有 Tag（下拉选择）
        public ReactiveProperty<ObservableCollection<Tag>> AllTags { get; }
        // 当前 TagKey 下拉
        public ReactiveProperty<ObservableCollection<string>> CurrentTagKeys { get; }

        public ReactiveProperty<RecipeItem> EditingItem { get; }
        public ReactiveProperty<bool> IsEditing { get; }
        public ReactiveProperty<bool> IsDialogOpen { get; }

        public RecipeItemDialogViewModel(IRecipeItemService itemSvc, AppDbContext db, ILogger<RecipeItemDialogViewModel> logger)
        {
            _itemSvc = itemSvc;
            _db = db;
            _logger = logger;

            Items = new ReactiveProperty<ObservableCollection<RecipeItem>>(new ObservableCollection<RecipeItem>());
            AllTags = new ReactiveProperty<ObservableCollection<Tag>>(new ObservableCollection<Tag>());
            CurrentTagKeys = new ReactiveProperty<ObservableCollection<string>>(new ObservableCollection<string>());

            EditingItem = new ReactiveProperty<RecipeItem>(new RecipeItem());
            IsEditing = new ReactiveProperty<bool>(false);
            IsDialogOpen = new ReactiveProperty<bool>(false);

            LoadCommand = new ReactiveCommand().WithSubscribe(async () => await LoadAsync());
            AddCommand = new ReactiveCommand().WithSubscribe(() => OpenAdd());
            SaveCommand = new ReactiveCommand().WithSubscribe(async () => await SaveAsync());
            DeleteCommand = new ReactiveCommand<RecipeItem>().WithSubscribe(async ri => await DeleteAsync(ri));
            TagChangedCommand = new ReactiveCommand<int>().WithSubscribe(tagId => RefreshTagKeys(tagId));
            CancelCommand = new ReactiveCommand().WithSubscribe(() => Close());
        }

        #region Commands
        public ReactiveCommand LoadCommand { get; }
        public ReactiveCommand AddCommand { get; }
        public ReactiveCommand SaveCommand { get; }
        public ReactiveCommand<RecipeItem> DeleteCommand { get; }
        public ReactiveCommand<int> TagChangedCommand { get; }
        public ReactiveCommand CancelCommand { get; }
        #endregion

        public async Task LoadAsync()
        {
            // tags
            var tags = await _db.Tags.ToListAsync();
            AllTags.Value = new ObservableCollection<Tag>(tags);

            // items
            var items = await _itemSvc.GetItemsByRecipeIdAsync(RecipeId);
            Items.Value = new ObservableCollection<RecipeItem>(items);
        }

        private void OpenAdd()
        {
            EditingItem.Value = new RecipeItem { RecipeId = RecipeId };
            IsEditing.Value = false;
            CurrentTagKeys.Value = new ObservableCollection<string>();
            IsDialogOpen.Value = true;
        }

        private async Task SaveAsync()
        {
            var item = EditingItem.Value;
            if (item.RecipeId == 0) item.RecipeId = RecipeId;
            if (item.Id == 0)
                await _itemSvc.AddItemAsync(item);
            else
                await _itemSvc.UpdateItemAsync(item);
            await LoadAsync();
            Close();
        }

        private async Task DeleteAsync(RecipeItem item)
        {
            if (item == null) return;
            await _itemSvc.DeleteItemAsync(item.Id);
            await LoadAsync();
        }

        private void RefreshTagKeys(int tagId)
        {
            var tag = AllTags.Value.FirstOrDefault(t => t.Id == tagId);
            if (tag != null && tag.TagDetailDataArray != null)
            {
                var keys = tag.TagDetailDataArray.Select(td => td.Address).Distinct().ToList();
                CurrentTagKeys.Value = new ObservableCollection<string>(keys);
            }
        }

        private void Close()
        {
            IsDialogOpen.Value = false;
            EditingItem.Value = new RecipeItem();
        }
    }
}

