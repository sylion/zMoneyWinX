namespace zMoneyWinX.Model
{
    public class MerchantEdit : Merchant
    {
        public MerchantEdit(Merchant item)
        {
            Id = item.Id;
            Title = item.Title;
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
    }
}
