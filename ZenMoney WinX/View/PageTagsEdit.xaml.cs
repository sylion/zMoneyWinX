using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using zMoneyWinX.Model;
using zMoneyWinX.ViewModel;

namespace zMoneyWinX.View
{
    public sealed partial class PageTagsEdit : Page
    {
        Tag item;
        TagEdit editItem;

        private bool Create = false;

        public PageTagsEdit(Tag _item = null)
        {
            this.InitializeComponent();
            if (_item != null)
            {
                item = _item;
                CmdDelete.Visibility = Visibility.Visible;
            }
            else
            {
                item = new Tag();
                Create = true;
            }
        }

        public bool CanGoBack()
        {
            if (IconPick.Visibility == Visibility.Visible || ColorPick.Visibility == Visibility.Visible)
            {
                IconPick.Visibility = Visibility.Collapsed;
                ColorPick.Visibility = Visibility.Collapsed;
                Root.Visibility = Visibility.Visible;
                return false;
            }
            else
                return true;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProgress.Visibility = Visibility.Visible;
            await Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
           {
               editItem = new TagEdit(item);
               var TagSource = TagViewModel.Tags.Where(i => i.Id != item.Id && !i.isDeleted && i.Parent == null).OrderBy(i => i.Title).ToList();
               TagSource.Insert(0, Model.Tag.Empty);
               ParentTag.ItemsSource = TagSource;
               Root.DataContext = editItem;
               ListIcon.ItemsSource = IconSet.getIconSet();
               ListColor.ItemsSource = Model.Extensions.TagColors;
           });
            LoadProgress.Visibility = Visibility.Collapsed;
        }

        private async void CmdClick(object sender, RoutedEventArgs e)
        {
            switch (((AppBarButton)sender).Tag.ToString())
            {
                case "Save":
                    save();
                    return;
                case "Delete":
                    await delete();
                    return;
                case "Cancel":
                    Signals.CloseMeInvoke();
                    return;
                default:
                    break;
            }
        }

        private async Task delete()
        {
            if (await Model.Extensions.ReallyDelete(Model.Extensions.ItemType.Tag))
            {
                LoadProgress.Visibility = Visibility.Visible;
                App.Provider.VMTag.Delete(item);
                Signals.CloseMeInvoke();
                LoadProgress.Visibility = Visibility.Collapsed;
            }
        }

        private void save()
        {
            LoadProgress.Visibility = Visibility.Visible;
            if (!string.IsNullOrWhiteSpace(editItem.Title))
            {
                item.update(editItem, Create);

                if (Create)
                    App.Provider.VMTag.Insert(item);
                else
                    App.Provider.VMTag.Update(item);

                Signals.CloseMeInvoke();
            }
            LoadProgress.Visibility = Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Tag.ToString())
            {
                case "Icon":
                    IconPick.Visibility = Visibility.Visible;
                    ColorPick.Visibility = Visibility.Collapsed;
                    Root.Visibility = Visibility.Collapsed;
                    break;
                case "Color":
                    ColorPick.Visibility = Visibility.Visible;
                    IconPick.Visibility = Visibility.Collapsed;
                    Root.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }
        private void ListIcon_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!string.IsNullOrEmpty((string)e.ClickedItem) && (string)e.ClickedItem != "0000_none")
                editItem.editIcon = (string)e.ClickedItem;
            else
                editItem.editIcon = "";

            IconPick.Visibility = Visibility.Collapsed;
            Root.Visibility = Visibility.Visible;
        }

        private void ListColor_ItemClick(object sender, ItemClickEventArgs e)
        {
            editItem.TagColor = (SolidColorBrush)e.ClickedItem;

            ColorPick.Visibility = Visibility.Collapsed;
            IconPick.Visibility = Visibility.Collapsed;
            Root.Visibility = Visibility.Visible;
        }
    }
}
