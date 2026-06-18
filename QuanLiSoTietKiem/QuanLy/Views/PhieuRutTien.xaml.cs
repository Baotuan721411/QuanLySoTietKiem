using System.Windows.Controls;
using QuanLiSoTietKiem.QuanLy.ViewModels;

namespace QuanLiSoTietKiem.QuanLy.Views
{
    public partial class PhieuRutWindow : UserControl
    {
        public PhieuRutWindow()
        {
            InitializeComponent();
            DataContext = new PhieuRutViewModel();
        }
    }
}