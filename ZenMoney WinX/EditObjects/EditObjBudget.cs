namespace zMoneyWinX.Model
{
    class BudgetEdit : Budget
    {
        public TransactionType Type { get; private set; }
        public BudgetEdit(Budget item, TransactionType type)
        {
            Type = type;
            if (Type == TransactionType.Income)
                EditSum = item.Income;
            else
                EditSum = item.Outcome;
        }

        public double EditSum
        {
            get
            {
                if (Type == TransactionType.Income)
                    return Income;
                else
                    return Outcome;
            }
            set
            {
                if (Type == TransactionType.Income)
                    Income = value;
                else
                    Outcome = value;
                NotifyPropertyChanged();
            }
        }
    }
}
