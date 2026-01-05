using System.Windows;
using System.Windows.Controls;
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
    }
}

