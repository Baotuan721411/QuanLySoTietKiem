using QuanLiSoTietKiem.QuanLy.BLL;
using QuanLiSoTietKiem.QuanLy.Helpers;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace QuanLiSoTietKiem.QuanLy.ViewModels
{
    public class MainViewModels : INotifyPropertyChanged
    {
        private readonly CheckRequirements _bll = new CheckRequirements();
        public ObservableCollection<LoaiTietKiem> ListLoaiTK { get; set; }

        // Các thuộc tính Binding
        private string _tenKH, _cccd, _diaChi, _soTienStr, _maSo = "<Giá trị tự động>";
        public string TenKH { get => _tenKH; set { _tenKH = value; OnPropertyChanged("TenKH"); } }
        public string CCCD { get => _cccd; set { _cccd = value; OnPropertyChanged("CCCD"); } }
        public string DiaChi { get => _diaChi; set { _diaChi = value; OnPropertyChanged("DiaChi"); } }
        public string SoTienStr { get => _soTienStr; set { _soTienStr = value; OnPropertyChanged("SoTienStr"); } }
        public string MaSo { get => _maSo; set { _maSo = value; OnPropertyChanged("MaSo"); } }

        private DateTime? _ngaySinh;
        public DateTime? NgaySinh { get => _ngaySinh; set { _ngaySinh = value; OnPropertyChanged("NgaySinh"); } }

        private DateTime _ngayMoSo = DateTime.Now; // Mặc định là ngày hiện tại
        public DateTime NgayMoSo { get => _ngayMoSo; set { _ngayMoSo = value; OnPropertyChanged("NgayMoSo"); } }

        public int? SelectedLoaiId { get; set; }

        public ICommand TiepNhanCommand { get; }
        public ICommand ThoatCommand { get; }

        public MainViewModels()
        {
            ListLoaiTK = new ObservableCollection<LoaiTietKiem>(_bll.GetLoaiTietKiems());
            TiepNhanCommand = new RelayCommand(o => HandleSubmit());
            ThoatCommand = new RelayCommand(o => Application.Current.Shutdown());
        }

        private void HandleSubmit()
        {
            // KIỂM TRA TẤT CẢ THAM SỐ ĐẦU VÀO
            if (string.IsNullOrWhiteSpace(TenKH) ||
                string.IsNullOrWhiteSpace(CCCD) ||
                string.IsNullOrWhiteSpace(DiaChi) ||
                string.IsNullOrWhiteSpace(SoTienStr) ||
                NgaySinh == null ||
                SelectedLoaiId == null)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tất cả các trường thông tin!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(SoTienStr, out decimal st))
            {
                MessageBox.Show("Số tiền gửi không hợp lệ!"); return;
            }

            var so = new SoTietKiem
            {
                TenKH = TenKH,
                SoTien = st,
                NgaySinh = NgaySinh,
                CCCD = CCCD,
                DiaChi = DiaChi,
                MaLoaiTietKiem = SelectedLoaiId.Value,
                NgayMoSo = NgayMoSo // Sử dụng giá trị từ DatePicker
            };

            string res = _bll.ValidateAndSubmit(so);
            if (res == "SUCCESS")
            {
                MaSo = so.MaSo;
                MessageBox.Show("Tiếp nhận thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else MessageBox.Show(res, "Lỗi nghiệp vụ", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}