using System;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Controls;
using QuanLiSoTietKiem.QuanLy.Views;

namespace QuanLiSoTietKiem
{
    public partial class MainMenuWindow : Window
    {
        public MainMenuWindow()
        {
            InitializeComponent();
            LoadContent("QuanLy");
        }

        private void MenuButton_Checked(object sender, RoutedEventArgs e)
        {
            var btn = sender as RadioButton;
            if (btn != null && btn.Tag is string tag)
                LoadContent(tag);
        }

        private void LoadContent(string tag)
        {
            // Tạo mới mỗi lần chuyển tab → luôn có dữ liệu mới nhất từ DB
            switch (tag)
            {
                case "QuanLy":
                    ContentArea.Content = new MainWindow();
                    break;
                case "PhieuGoi":
                    ContentArea.Content = new PhieuGoiWindow();
                    break;
                case "PhieuRut":
                    ContentArea.Content = new PhieuRutWindow();
                    break;
                case "TraCuu":
                    ContentArea.Content = new TraCuuSoTietKiem();
                    break;
                case "BaoCaoDoanhThu":
                    ContentArea.Content = new BaoCaoDoanhThuWindow();
                    break;
                case "BaoCaoMoDong":
                    ContentArea.Content = new BaoCaoMoDongSoWindow();
                    break;
                case "QuanLyQuyDinh":
                    ContentArea.Content = new QuanLyQuyDinhWindow();
                    break;
            }
        }

        private void Thoat_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}