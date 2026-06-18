using System.Windows.Controls;
using QuanLiSoTietKiem.QuanLy.ViewModels;

namespace QuanLiSoTietKiem.QuanLy.Views
{
    public partial class QuanLyQuyDinhWindow : UserControl
    {
        public QuanLyQuyDinhWindow()
        {
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage("vi-VN");
            InitializeComponent();
            DataContext = new QuanLyQuyDinhViewModel();
        }

        // Khi người dùng click vào hàng trong DataGrid → đổ dữ liệu lên form
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is QuanLyQuyDinhViewModel vm)
                vm.LoadSelectedToForm();
        }
    }
}