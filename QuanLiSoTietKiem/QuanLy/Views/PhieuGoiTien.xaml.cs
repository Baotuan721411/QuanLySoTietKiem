using System.Windows.Controls;
using QuanLiSoTietKiem.QuanLy.ViewModels;

namespace QuanLiSoTietKiem.QuanLy.Views
{
    public partial class PhieuGoiWindow : UserControl
    {
        public PhieuGoiWindow()
        {
            InitializeComponent();
            DataContext = new PhieuGoiViewModel();
        }
    }
}