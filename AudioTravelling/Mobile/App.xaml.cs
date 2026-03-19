using Microsoft.Extensions.DependencyInjection;

namespace Mobile
{
    public partial class App : Application
    {
        [Obsolete]
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell(); // Dòng này cực kỳ quan trọng để gọi AppShell lên
        }
    }
}