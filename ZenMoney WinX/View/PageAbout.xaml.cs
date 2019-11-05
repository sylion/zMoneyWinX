using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;

namespace zMoneyWinX.View
{
    public sealed partial class PageAbout : Page
    {
        public PageAbout()
        {
            this.InitializeComponent();
            Version.Text = GetAppVersion();
        }

        private static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}
