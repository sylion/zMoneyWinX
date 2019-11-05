﻿using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using zMoneyWinX.Model;

namespace zMoneyWinX.View
{
    public sealed partial class PageTags : Page
    {
        private static bool loaded = false;
        public PageTags()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!loaded)
                await init();
            loaded = true;
        }

        private async Task init()
        {
            Signals.invokeSyncStarted();
            await Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Tags.Source = App.Provider.VMTag.GroupedTags;
            });
            Signals.invokeSyncEnded();
        }

        private async void TagList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (Tag)e.ClickedItem;
            await Task.Delay(250);
            Signals.TagBeginEditInvoke(item);
        }
    }
}
