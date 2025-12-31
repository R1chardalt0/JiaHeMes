using System.Windows.Controls;

namespace DeviceManage.Views
{
    /// <summary>
    /// 配方项对话框
    /// </summary>
    public partial class RecipeItemDialog : UserControl
    {
        public RecipeItemDialog()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is DeviceManage.ViewModels.RecipeItemDialogViewModel vm && sender is ComboBox cb && cb.SelectedValue is int tagId)
            {
                vm.TagChangedCommand.Execute(tagId);
            }
        }
    }
}
