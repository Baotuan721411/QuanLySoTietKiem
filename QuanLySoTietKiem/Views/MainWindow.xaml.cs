using System.Windows;

namespace QuanLySoTietKiem.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage("vi-VN");
            InitializeComponent();
        }
    }
}
