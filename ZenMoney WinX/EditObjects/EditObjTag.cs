using System;

namespace zMoneyWinX.Model
{
    public class TagEdit : Tag
    {
        public TagEdit(Tag item)
        {
            Id = item.Id;
            Title = item.Title;
            ShowIncome = item.ShowIncome;
            ShowOutcome = item.ShowOutcome;
            Parent = item.Parent;
            Icon = item.Icon;
            BudgetIncome = item.BudgetIncome;
            BudgetOutcome = item.BudgetOutcome;
            Required = item.Required;
            ColorStr = item.ColorStr;
        }

        public string editTitle
        {
            get
            {
                return Title;
            }
            set
            {
                Title = value;
                NotifyPropertyChanged();
            }
        }
        public bool editShowIncome
        {
            get
            {
                return ShowIncome;
            }
            set
            {
                ShowIncome = value;
                if (!value)
                    BudgetIncome = false;
                NotifyPropertyChanged();
            }
        }
        public bool editShowOutcome
        {
            get
            {
                return ShowOutcome;
            }
            set
            {
                ShowOutcome = value;
                if (!value)
                    BudgetOutcome = false;
                NotifyPropertyChanged();
            }
        }
        public bool editBudgetIncome
        {
            get
            {
                return BudgetIncome;
            }
            set
            {
                if (ShowIncome)
                    BudgetIncome = value;
                else
                    BudgetIncome = false;
                NotifyPropertyChanged();
            }
        }
        public bool editBudgetOutcome
        {
            get
            {
                return BudgetOutcome;
            }
            set
            {
                if (ShowOutcome)
                    BudgetOutcome = value;
                else
                    BudgetOutcome = false;
                BudgetOutcome = value;
                NotifyPropertyChanged();
            }
        }
        public Guid? editParent
        {
            get
            {
                return Parent;
            }
            set
            {
                Parent = value;
                NotifyPropertyChanged();
            }
        }

        public string editIcon
        {
            get
            {
                return Icon;
            }
            set
            {
                Icon = value;
                NotifyPropertyChanged();
            }
        }

        public bool? editRequired
        {
            get
            {
                return Required;
            }
            set
            {
                Required = value;
                NotifyPropertyChanged();
            }
        }
    }
}