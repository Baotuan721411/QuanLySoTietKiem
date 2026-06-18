using System.Windows.Controls;
using QuanLiSoTietKiem.QuanLy.ViewModels;

namespace QuanLiSoTietKiem.QuanLy.Views
{
    public partial class BaoCaoMoDongSoWindow : UserControl
    {
        public BaoCaoMoDongSoWindow()
        {
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage("vi-VN");
            InitializeComponent();
            DataContext = new BaoCaoMoDongSoViewModel();
        }
    }
}