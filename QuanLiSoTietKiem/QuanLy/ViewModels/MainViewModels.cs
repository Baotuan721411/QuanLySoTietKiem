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

        // --- Các thuộc tính Binding ---
        private string _tenKH, _cccd, _diaChi, _soTienStr, _maSo = "<Giá trị tự động>";
        public string TenKH { get => _tenKH; set { _tenKH = value; OnPropertyChanged("TenKH"); } }
        public string CCCD { get => _cccd; set { _cccd = value; OnPropertyChanged("CCCD"); } }
        public string DiaChi { get => _diaChi; set { _diaChi = value; OnPropertyChanged("DiaChi"); } }
        public string SoTienStr { get => _soTienStr; set { _soTienStr = value; OnPropertyChanged("SoTienStr"); } }
        public string MaSo { get => _maSo; set { _maSo = value; OnPropertyChanged("MaSo"); } }

        private DateTime? _ngaySinh;
        public DateTime? NgaySinh { get => _ngaySinh; set { _ngaySinh = value; OnPropertyChanged("NgaySinh"); } }

        private DateTime _ngayMoSo = DateTime.Now;
        public DateTime NgayMoSo { get => _ngayMoSo; set { _ngayMoSo = value; OnPropertyChanged("NgayMoSo"); } }

        private int? _selectedLoaiId;
        public int? SelectedLoaiId { get => _selectedLoaiId; set { _selectedLoaiId = value; OnPropertyChanged("SelectedLoaiId"); } }

        // --- Khai báo các Command (Sửa lỗi CS0103) ---
        public ICommand TiepNhanCommand { get; }
        public ICommand TaoMoiCommand { get; }
        public ICommand TimKiemCommand { get; }
        public ICommand CapNhatCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand ThoatCommand { get; }

        public MainViewModels()
        {
            ListLoaiTK = new ObservableCollection<LoaiTietKiem>(_bll.GetLoaiTietKiems());

            // Gán logic cho Command
            TiepNhanCommand = new RelayCommand(o => HandleSubmit());
            TaoMoiCommand = new RelayCommand(o => ResetForm());
            TimKiemCommand = new RelayCommand(o => HandleSearch());
            CapNhatCommand = new RelayCommand(o => HandleUpdate());
            XoaCommand = new RelayCommand(o => HandleDelete());
            ThoatCommand = new RelayCommand(o => Application.Current.Shutdown());
        }

        private void ResetForm()
        {
            MaSo = "<Giá trị tự động>";
            TenKH = string.Empty;
            CCCD = string.Empty;
            DiaChi = string.Empty;
            SoTienStr = string.Empty;
            NgaySinh = null;
            NgayMoSo = DateTime.Now;
            SelectedLoaiId = null;
        }

        private void HandleSubmit()
        {
            if (string.IsNullOrWhiteSpace(TenKH) || string.IsNullOrWhiteSpace(CCCD) ||
                string.IsNullOrWhiteSpace(DiaChi) || string.IsNullOrWhiteSpace(SoTienStr) ||
                NgaySinh == null || SelectedLoaiId == null)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tất cả các trường thông tin!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal.TryParse(SoTienStr, out decimal st);
            var so = new SoTietKiem
            {
                TenKH = TenKH,
                SoTien = st,
                NgaySinh = NgaySinh,
                CCCD = CCCD,
                DiaChi = DiaChi,
                MaLoaiTietKiem = SelectedLoaiId.Value,
                NgayMoSo = NgayMoSo
            };

            string res = _bll.ValidateAndSubmit(so);
            if (res == "SUCCESS")
            {
                MaSo = so.MaSo;
                MessageBox.Show("Tiếp nhận thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else MessageBox.Show(res, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void HandleSearch()
        {
            // Tìm theo Tên hoặc Mã hiện có trong ô nhập
            string keyword = !string.IsNullOrWhiteSpace(TenKH) ? TenKH : (MaSo != "<Giá trị tự động>" ? MaSo : "");

            if (string.IsNullOrEmpty(keyword))
            {
                MessageBox.Show("Vui lòng nhập Tên hoặc Mã sổ để tìm kiếm!");
                return;
            }

            var list = _bll.SearchSoTietKiem(keyword);
            if (list.Count > 0)
            {
                var s = list[0];
                MaSo = s.MaSo;
                TenKH = s.TenKH;
                SoTienStr = s.SoTien.ToString();
                NgaySinh = s.NgaySinh;
                NgayMoSo = s.NgayMoSo;
                CCCD = s.CCCD;
                DiaChi = s.DiaChi;
                SelectedLoaiId = s.MaLoaiTietKiem;
            }
            else MessageBox.Show("Không tìm thấy sổ!");
        }

        private void HandleUpdate()
        {
            if (MaSo == "<Giá trị tự động>")
            {
                MessageBox.Show("Vui lòng tìm sổ trước khi cập nhật!");
                return;
            }

            decimal.TryParse(SoTienStr, out decimal st);
            var so = new SoTietKiem
            {
                MaSo = MaSo,
                TenKH = TenKH,
                SoTien = st,
                NgaySinh = NgaySinh,
                CCCD = CCCD,
                DiaChi = DiaChi,
                MaLoaiTietKiem = SelectedLoaiId ?? 0,
                NgayMoSo = NgayMoSo
            };

            string res = _bll.ValidateAndUpdate(so);
            if (res == "SUCCESS")
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(res, "Lỗi cập nhật", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void HandleDelete()
        {
            if (MaSo == "<Giá trị tự động>") return;

            if (MessageBox.Show($"Bạn có chắc muốn xóa sổ {MaSo}?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (_bll.DeleteSoTietKiem(MaSo))
                {
                    MessageBox.Show("Đã xóa thành công!");
                    ResetForm();
                }
                else
                {
                    MessageBox.Show("Xóa thất bại!");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}