using System.Windows.Controls;
using QuanLiSoTietKiem.QuanLy.ViewModels;

namespace QuanLiSoTietKiem.QuanLy.Views
{
    public partial class TraCuuSoTietKiem : UserControl
    {
        public TraCuuSoTietKiem()
        {
            InitializeComponent();
            DataContext = new TraCuuSoTietKiemViewModel();
        }
    }
}