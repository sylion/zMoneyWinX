using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using zMoneyWinX.Model;

namespace zMoneyWinX.ViewModel
{
    public class ScheduledOperationsHelper : INotifyProperty
    {
        public ObservableCollection<GroupInfoList> Operations = new ObservableCollection<GroupInfoList>();
        
        private bool _ModePlanned { get; set; } = true;
        private bool _ModeProcessed { get; set; } = false;
        private bool _ModeDeleted { get; set; } = false;
        private bool _ModeAll { get; set; } = false;

        public bool ModePlanned { get { return _ModePlanned; } set { _ModePlanned = value; _ModeProcessed = !value; _ModeDeleted = !value; _ModeAll = !value; NotifyPropertyChanged(); initsource(); } }
        public bool ModeProcessed { get { return _ModeProcessed; } set { _ModeProcessed = value; _ModePlanned = !value; _ModeDeleted = !value; _ModeAll = !value; NotifyPropertyChanged(); initsource(); } }
        public bool ModeDeleted { get { return _ModeDeleted; } set { _ModeDeleted = value; _ModePlanned = !value; _ModeProcessed = !value; _ModeAll = !value; NotifyPropertyChanged(); initsource(); } }
        public bool ModeAll { get { return _ModeAll; } set { _ModeAll = value; _ModePlanned = !value; _ModeProcessed = !value; _ModeDeleted = !value; NotifyPropertyChanged(); initsource(); } }

        public Visibility isEmtyVisible { get { return Operations != null && Operations.Count > 0 ? Visibility.Collapsed : Visibility.Visible; } }

        public Guid Filter { get; private set; } = Guid.Empty;
        public void init(Guid Id)
        {
            Filter = Id;
            LatePayments = false;
            ModeAll = true;
            initsource();
        }

        public void init()
        {
            Filter = Guid.Empty;
            LatePayments = false;
            ModePlanned = true;
            initsource();
        }
        private bool LatePayments = false;
        public void init(bool LatePayments)
        {
            Filter = Guid.Empty;
            ModePlanned = true;
            this.LatePayments = LatePayments;
            initsource();
        }

        private void initsource()
        {
            Operations.Clear();
            if (Filter != Guid.Empty)
                foreach (var item in App.Provider.VMTransaction.getGroupedScheduledPayments(Filter, getState()))
                    Operations.Add(item);
            else if (LatePayments)
                foreach (var item in App.Provider.VMTransaction.getGroupedScheduledLatePayments())
                    Operations.Add(item);
            else
                foreach (var item in App.Provider.VMTransaction.getGroupedScheduledPayments(getState()))
                    Operations.Add(item);
            NotifyPropertyChanged();
        }

        private ReminderMarker.States getState()
        {
            if (_ModePlanned)
                return ReminderMarker.States.Planned;
            if (_ModeProcessed)
                return ReminderMarker.States.Processed;
            if (_ModeDeleted)
                return ReminderMarker.States.Deleted;

            return ReminderMarker.States.All;
        }
    }
}
