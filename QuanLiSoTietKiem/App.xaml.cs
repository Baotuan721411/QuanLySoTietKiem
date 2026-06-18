using System.Windows;

namespace QuanLiSoTietKiem
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var menu = new MainMenuWindow();
            menu.Show();
        }
    }
}