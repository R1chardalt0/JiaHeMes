using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using DeviceManage.ViewModels;

namespace DeviceManage.Views
{
    public partial class TagView : UserControl
    {
        private TagDetailRow _draggedItem;
        private Point _dragStartPoint;

        public TagView()
        {
            InitializeComponent();
            TagDetailDataGrid.PreviewMouseLeftButtonDown += TagDetailDataGrid_PreviewMouseLeftButtonDown;
            TagDetailDataGrid.PreviewMouseMove += TagDetailDataGrid_PreviewMouseMove;
            TagDetailDataGrid.Drop += TagDetailDataGrid_Drop;
            TagDetailDataGrid.DragOver += TagDetailDataGrid_DragOver;
            TagDetailDataGrid.AllowDrop = true;
        }

        private void TagDetailDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            var hit = VisualTreeHelper.HitTest(dataGrid, e.GetPosition(dataGrid));
            if (hit == null || hit.VisualHit == null) return;

            var row = FindParent<DataGridRow>(hit.VisualHit);
            
            if (row != null && row.Item is TagDetailRow item)
            {
                _draggedItem = item;
                _dragStartPoint = e.GetPosition(dataGrid);
            }
        }

        private void TagDetailDataGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _draggedItem == null) return;

            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            var currentPoint = e.GetPosition(dataGrid);
            var diff = currentPoint - _dragStartPoint;

            if (System.Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                System.Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                DragDrop.DoDragDrop(dataGrid, _draggedItem, DragDropEffects.Move);
                _draggedItem = null;
            }
        }

        private void TagDetailDataGrid_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void TagDetailDataGrid_Drop(object sender, DragEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null || _draggedItem == null) return;

            var hit = VisualTreeHelper.HitTest(dataGrid, e.GetPosition(dataGrid));
            if (hit == null || hit.VisualHit == null) return;

            var row = FindParent<DataGridRow>(hit.VisualHit);
            
            if (row != null && row.Item is TagDetailRow targetItem)
            {
                var itemsSource = dataGrid.ItemsSource as ObservableCollection<TagDetailRow>;
                if (itemsSource == null) return;

                var draggedIndex = itemsSource.IndexOf(_draggedItem);
                var targetIndex = itemsSource.IndexOf(targetItem);

                if (draggedIndex >= 0 && targetIndex >= 0 && draggedIndex != targetIndex)
                {
                    // 在可视（过滤后）集合中调整顺序
                    itemsSource.RemoveAt(draggedIndex);
                    itemsSource.Insert(targetIndex, _draggedItem);

                    // 同步调整到原始集合，确保保存时顺序正确
                    if (DataContext is TagViewModel vm && vm.TagDetailRows?.Value is ObservableCollection<TagDetailRow> allRows)
                    {
                        int oldIndexAll = allRows.IndexOf(_draggedItem);
                        int newIndexAll = allRows.IndexOf(targetItem);
                        if (oldIndexAll >= 0 && newIndexAll >= 0 && oldIndexAll != newIndexAll)
                        {
                            allRows.RemoveAt(oldIndexAll);
                            allRows.Insert(newIndexAll, _draggedItem);
                        }
                    }
                }
            }

            _draggedItem = null;
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is T) return parent as T;
            return FindParent<T>(parent);
        }

        private void DataTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 当数据类型改变时，强制刷新 DataGrid 以更新 Value 列的显示
            if (sender is ComboBox comboBox && comboBox.DataContext is TagDetailRow row)
            {
                // 找到同一行的 Value 列单元格并强制刷新其绑定
                var dataGridRow = FindParent<DataGridRow>(comboBox);
                if (dataGridRow != null)
                {
                    // 遍历该行的所有单元格，找到 Value 列的单元格
                    for (int i = 0; i < TagDetailDataGrid.Columns.Count; i++)
                    {
                        var column = TagDetailDataGrid.Columns[i];
                        if (column is DataGridTemplateColumn templateColumn && templateColumn.Header?.ToString() == "值")
                        {
                            var cell = GetCell(dataGridRow, i);
                            if (cell != null)
                            {
                                // 强制刷新该单元格的内容
                                cell.UpdateLayout();
                            }
                        }
                    }
                    
                    // 强制刷新整个 DataGrid
                    TagDetailDataGrid.UpdateLayout();
                }
            }
        }

        private DataGridCell GetCell(DataGridRow row, int columnIndex)
        {
            if (row == null) return null;
            
            var presenter = FindVisualChild<DataGridCellsPresenter>(row);
            if (presenter == null) return null;
            
            return presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex) as DataGridCell;
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                {
                    return result;
                }
                
                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            
            return null;
        }
    }
}

