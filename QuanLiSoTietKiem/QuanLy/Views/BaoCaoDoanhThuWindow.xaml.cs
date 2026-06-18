using System.Windows.Controls;
using QuanLiSoTietKiem.QuanLy.ViewModels;

namespace QuanLiSoTietKiem.QuanLy.Views
{
    public partial class BaoCaoDoanhThuWindow : UserControl
    {
        public BaoCaoDoanhThuWindow()
        {
            InitializeComponent();
            DataContext = new BaoCaoDoanhThuViewModel();
        }
    }
}