using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using zMoneyWinX.Model;
using zMoneyWinX.Client;

namespace zMoneyWinX.ViewModel
{
    public class TagViewModel
    {
        private ObservableCollection<GroupInfoList> _groupedTags = null;
        public static List<Tag> Tags => Tag.Tags;

        private static ObservableCollection<GroupInfoList> _GetGroupedTags()
        {
            ObservableCollection<GroupInfoList> groups = new ObservableCollection<GroupInfoList>();
            GroupInfoList singletons = new GroupInfoList("");            
            foreach (Tag t in Tags.Where(i => i.Parent == null && !i.isDeleted).OrderBy(i => i.Title))
            {
                IOrderedEnumerable<Tag> children = Tags.Where(i => i.Parent == t.Id && !i.isDeleted).OrderBy(i => i.Title);
                if(children.Count() > 0)
                {
                    GroupInfoList info = new GroupInfoList(t.Title);
                    info.Add(t);
                    foreach (Tag x in children)
                        info.Add(x);
                    groups.Add(info);
                }
                else
                {
                    singletons.Add(t);
                }
            }
            if (singletons.Count > 0)
                groups.Add(singletons);
            return groups;
        }

        public ObservableCollection<GroupInfoList> GroupedTags
        {
            get
            {
                if (_groupedTags == null)
                    _groupedTags = _GetGroupedTags();
                return _groupedTags;
            }
            set
            {
                _groupedTags = value;
            }
        }
        public void Insert(Tag Item)
        {
            DBObject.Insert(Item);
            Refresh();
        }
        public void Insert(List<Tag> Items)
        {
            DBObject.Insert(Items);
            Refresh();
        }
        public void Delete(Tag Item)
        {
            CollectionDelete(Item.Id);
            Item.isDeleted = true;
            Item.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
            DBObject.Insert(Item);
        }
        public void Delete(Guid ID)
        {
            CollectionDelete(ID);
            DBObject.Delete(ID, typeof(Tag));
        }
        public async void Update(Tag Item)
        {
            DBObject.Insert(Item);
            Refresh();
            await App.Provider.VMTransaction.Refresh();
        }
        private void CollectionDelete(Guid ID)
        {
            foreach (GroupInfoList x in GroupedTags)
            {
                object res = x.SingleOrDefault(i => ((Tag)i).Id == ID);
                if (res != null)
                {
                    x.Remove(res);
                    if (x.Count <= 0)
                        GroupedTags.Remove(x);
                    break;
                }
            }
        }
        private void Refresh()
        {
            GroupedTags.Clear();
            foreach (GroupInfoList x in _GetGroupedTags())
                GroupedTags.Add(x);
        }
    }
}
