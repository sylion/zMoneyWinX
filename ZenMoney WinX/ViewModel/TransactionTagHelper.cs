using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using zMoneyWinX.Model;

namespace zMoneyWinX.ViewModel
{
    public class TransactionTagHelper
    {
        private List<Tag> Tags { get; set; }
        private ObservableCollection<Tag> _IncomeTags { get; set; }
        private ObservableCollection<Tag> _OutcomeTags { get; set; }
        public ObservableCollection<Tag> IncomeTags { get { return _IncomeTags; } set { _IncomeTags = value; } }
        public ObservableCollection<Tag> OutcomeTags { get { return _OutcomeTags; } set { _OutcomeTags = value; } }
        public ObservableCollection<Tag> IncomeTagsChild { get; set; } = new ObservableCollection<Tag>();
        public ObservableCollection<Tag> OutcomeTagsChild { get; set; } = new ObservableCollection<Tag>();
        public TransactionTagHelper(List<Guid> CheckedTags)
        {
            Tags = TagViewModel.Tags;

            if (CheckedTags != null && CheckedTags.Count > 0)
                foreach (Guid x in CheckedTags)
                    Tags.FirstOrDefault(i => i.Id == x).isChecked = true;

            IncomeTags = new ObservableCollection<Tag>(Tags.Where(i => i.ShowIncome && i.Parent == null));
            OutcomeTags = new ObservableCollection<Tag>(Tags.Where(i => i.ShowOutcome && i.Parent == null));

            foreach (Tag x in Tags.Where(i => i.isChecked && i.ShowIncome && (i.Parent != null && i.Parent != Guid.Empty)))
            {
                int idx = IncomeTags.IndexOf(IncomeTags.FirstOrDefault(i => i.Id == x.Parent));
                IncomeTags.RemoveAt(idx);
                IncomeTags.Insert(idx, x);
            }

            foreach (Tag x in Tags.Where(i => i.isChecked && i.ShowOutcome && (i.Parent != null && i.Parent != Guid.Empty)))
            {
                int idx = OutcomeTags.IndexOf(OutcomeTags.FirstOrDefault(i => i.Id == x.Parent));
                OutcomeTags.RemoveAt(idx);
                OutcomeTags.Insert(idx, x);
            }
        }

        private Guid LastIncomeClicked;
        public void IncomeClicked(Tag item)
        {
            if (IncomeTagsChild.Count > 0)
            {
                IncomeTagsChild.Clear();
                if (item.Id == LastIncomeClicked)
                {
                    item.isChecked = true;
                    return;
                }
            }
            LastIncomeClicked = item.Id;

            if (item.isChecked)
            {
                item.isChecked = false;
                if (item.Parent != null && item.Parent != Guid.Empty)
                {
                    int idx = IncomeTags.IndexOf(item);
                    IncomeTags.RemoveAt(idx);
                    IncomeTags.Insert(idx, Tags.FirstOrDefault(i => i.Id == item.Parent));
                }
                return;
            }

            IEnumerable<Tag> Child = Tags.Where(i => i.Parent == item.Id && i.ShowIncome);
            if (Child != null && Child.Count() > 0)
            {
                IncomeTagsChild.Add(item);
                foreach (Tag x in Child)
                    IncomeTagsChild.Add(x);
            }
            else
                item.isChecked = true;
        }

        public void IncomeChildClicked(Tag item)
        {
            LastIncomeClicked = Guid.Empty;
            if (item.Parent == null || item.Parent == Guid.Empty)
            {
                item.isChecked = true;
            }
            else
            {
                int idx = IncomeTags.IndexOf(IncomeTags.FirstOrDefault(i => i.Id == item.Parent));
                IncomeTags.RemoveAt(idx);
                IncomeTags.Insert(idx, item);
                item.isChecked = true;
            }
            IncomeTagsChild.Clear();
        }

        private Guid LastOutcomeClicked;
        public void OutcomeClicked(Tag item)
        {
            if (OutcomeTagsChild.Count > 0)
            {
                OutcomeTagsChild.Clear();
                if (item.Id == LastOutcomeClicked)
                {
                    item.isChecked = true;
                    return;
                }
            }
            LastOutcomeClicked = item.Id;

            if (item.isChecked)
            {
                item.isChecked = false;
                if (item.Parent != null && item.Parent != Guid.Empty)
                {
                    int idx = OutcomeTags.IndexOf(item);
                    OutcomeTags.RemoveAt(idx);
                    OutcomeTags.Insert(idx, Tags.FirstOrDefault(i => i.Id == item.Parent));
                }
                return;
            }

            IEnumerable<Tag> Child = Tags.Where(i => i.Parent == item.Id && i.ShowOutcome);
            if (Child != null && Child.Count() > 0)
            {
                OutcomeTagsChild.Add(item);
                foreach (Tag x in Child)
                    OutcomeTagsChild.Add(x);
            }
            else
                item.isChecked = true;
        }

        public void OutcomeChildClicked(Tag item)
        {
            LastOutcomeClicked = Guid.Empty;
            if (item.Parent == null || item.Parent == Guid.Empty)
            {
                item.isChecked = true;
            }
            else
            {
                int idx = OutcomeTags.IndexOf(OutcomeTags.FirstOrDefault(i => i.Id == item.Parent));
                OutcomeTags.RemoveAt(idx);
                OutcomeTags.Insert(idx, item);
                item.isChecked = true;
            }
            OutcomeTagsChild.Clear();
        }

        public List<Guid> GetTransTags(TransactionType TransType)
        {
            if (TransType == TransactionType.Income)
                return IncomeTags.Where(i => i.isChecked).Select(i => i.Id).ToList();
            else if (TransType == TransactionType.Outcome)
                return OutcomeTags.Where(i => i.isChecked).Select(i => i.Id).ToList();
            return new List<Guid>();
        }
    }
}
