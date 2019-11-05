using System.Collections.ObjectModel;

namespace zMoneyWinX.Model
{
    public class GroupInfoList : ObservableCollection<object>
    {
        public GroupInfoList()
        {
        }
        public GroupInfoList(object key)
        {
            Key = key;
        }

        public object Key { get; set; }
    }
}