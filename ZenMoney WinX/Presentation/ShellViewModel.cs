﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace zMoneyWinX.Presentation
{
    public class ShellViewModel : NotifyPropertyChanged
    {
        private ObservableCollection<MenuItem> menuItems = new ObservableCollection<MenuItem>();
        private MenuItem selectedMenuItem;
        private bool isSplitViewPaneOpen;

        public ShellViewModel()
        {
            this.ToggleSplitViewPaneCommand = new Command(() => this.IsSplitViewPaneOpen = !this.IsSplitViewPaneOpen);
        }

        public ICommand ToggleSplitViewPaneCommand { get; private set; }

        public bool IsSplitViewPaneOpen
        {
            get { return this.isSplitViewPaneOpen; }
            set { Set(ref this.isSplitViewPaneOpen, value); }
        }

        public MenuItem SelectedMenuItem
        {
            get { return this.selectedMenuItem; }
            set
            {
                if (Set(ref this.selectedMenuItem, value))
                {
                    OnPropertyChanged("SelectedPageType");

                    // auto-close split view pane
                    if (zMoneyWinX.App.AutoClose())
                        this.IsSplitViewPaneOpen = false;
                }
            }
        }

        public Type SelectedPageType
        {
            get
            {
                if (this.selectedMenuItem != null)
                {
                    return this.selectedMenuItem.PageType;
                }
                return null;
            }
            set
            {
                // select associated menu item
                this.SelectedMenuItem = this.menuItems.FirstOrDefault(m => m.PageType == value);
            }
        }

        public ObservableCollection<MenuItem> MenuItems
        {
            get { return this.menuItems; }
        }
    }
}
