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
    public class QuyDinhRutTienItem
    {
        public int GiaTri { get; set; }
        public string TenHienThi { get; set; }
    }

    public class QuanLyQuyDinhViewModel : INotifyPropertyChanged
    {
        private readonly QuanLyQuyDinhBLL _bll = new QuanLyQuyDinhBLL();

        private int? _maLoaiDangChon;

        public ObservableCollection<LoaiTietKiem> DanhSachLoaiTK { get; set; }

        public ObservableCollection<QuyDinhRutTienItem> DanhSachQuyDinhRutTien { get; }
            = new ObservableCollection<QuyDinhRutTienItem>
            {
                new QuyDinhRutTienItem { GiaTri = 0, TenHienThi = "Rút toàn bộ"  },
                new QuyDinhRutTienItem { GiaTri = 1, TenHienThi = "Rút một phần" }
            };

        private LoaiTietKiem _selectedLoaiTK;
        public LoaiTietKiem SelectedLoaiTK
        {
            get => _selectedLoaiTK;
            set { _selectedLoaiTK = value; OnPropertyChanged(nameof(SelectedLoaiTK)); }
        }

        // ── Binding Form ──────────────────────────────────────────────────────

        private string _tenLoaiChiTiet = string.Empty;
        public string TenLoaiChiTiet
        {
            get => _tenLoaiChiTiet;
            set { _tenLoaiChiTiet = value; OnPropertyChanged(nameof(TenLoaiChiTiet)); }
        }

        private DateTime _ngayApDung = DateTime.Today;
        public DateTime NgayApDung
        {
            get => _ngayApDung;
            set { _ngayApDung = value; OnPropertyChanged(nameof(NgayApDung)); }
        }

        private DateTime? _ngayKetThuc;
        public DateTime? NgayKetThuc
        {
            get => _ngayKetThuc;
            set { _ngayKetThuc = value; OnPropertyChanged(nameof(NgayKetThuc)); }
        }

        private string _tienGoiToiThieuStr = string.Empty;
        public string TienGoiToiThieuStr
        {
            get => _tienGoiToiThieuStr;
            set { _tienGoiToiThieuStr = value; OnPropertyChanged(nameof(TienGoiToiThieuStr)); }
        }

        private string _thoiGianGuiToiThieuStr = string.Empty;
        public string ThoiGianGuiToiThieuStr
        {
            get => _thoiGianGuiToiThieuStr;
            set { _thoiGianGuiToiThieuStr = value; OnPropertyChanged(nameof(ThoiGianGuiToiThieuStr)); }
        }

        private string _laiSuatStr = string.Empty;
        public string LaiSuatStr
        {
            get => _laiSuatStr;
            set { _laiSuatStr = value; OnPropertyChanged(nameof(LaiSuatStr)); }
        }

        private int _quiDinhRutTien;
        public int QuiDinhRutTien
        {
            get => _quiDinhRutTien;
            set { _quiDinhRutTien = value; OnPropertyChanged(nameof(QuiDinhRutTien)); }
        }

        // ── Commands ──────────────────────────────────────────────────────────
        public ICommand ThemMoiCommand { get; }
        public ICommand CapNhatCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand LamMoiCommand { get; }
        public ICommand TimKiemTheoTenCommand { get; }

        public QuanLyQuyDinhViewModel()
        {
            DanhSachLoaiTK = new ObservableCollection<LoaiTietKiem>(_bll.GetAll());

            ThemMoiCommand = new RelayCommand(o => HandleThemMoi());
            CapNhatCommand = new RelayCommand(o => HandleCapNhat());
            XoaCommand = new RelayCommand(o => HandleXoa());
            LamMoiCommand = new RelayCommand(o => ResetForm());
            TimKiemTheoTenCommand = new RelayCommand(o => HandleTimKiemTheoTen());
        }

        // ── Đổ dữ liệu từ hàng được chọn lên Form ────────────────────────────
        /// <summary>
        /// Gọi khi người dùng click một hàng trên DataGrid.
        /// Lấy thêm NgayApDung và NgayKetThuc chính xác từ lich_su_lai_suat
        /// thay vì dùng DateTime.Today mặc định.
        /// </summary>
        public void LoadSelectedToForm()
        {
            if (SelectedLoaiTK == null) return;

            _maLoaiDangChon = SelectedLoaiTK.MaLoaiTietKiem;
            TenLoaiChiTiet = SelectedLoaiTK.TenLoaiTietKiem;
            TienGoiToiThieuStr = SelectedLoaiTK.TienGoiToiThieu.ToString("N0");
            ThoiGianGuiToiThieuStr = SelectedLoaiTK.ThoiGianRutTien.ToString();
            QuiDinhRutTien = SelectedLoaiTK.QuiDinhRutTien;

            // Lấy giai đoạn lãi suất đang có hiệu lực từ DB
            var lichSuHienTai = _bll.LayLichSuHienTai(SelectedLoaiTK.MaLoaiTietKiem);
            if (lichSuHienTai != null)
            {
                LaiSuatStr = lichSuHienTai.LaiSuatCuaKyHan.ToString("N2");
                NgayApDung = lichSuHienTai.NgayApDung;
                NgayKetThuc = lichSuHienTai.NgayKetThuc;  // null nếu đang áp dụng
            }
            else
            {
                // Chưa có lịch sử lãi suất → để trống, người dùng tự nhập
                LaiSuatStr = string.Empty;
                NgayApDung = DateTime.Today;
                NgayKetThuc = null;
            }
        }

        // ── Xây dựng đối tượng từ Form ────────────────────────────────────────
        private LoaiTietKiem BuildFromForm()
        {
            decimal.TryParse(TienGoiToiThieuStr.Replace(",", "").Replace(".", ""),
                             out decimal tienGui);
            int.TryParse(ThoiGianGuiToiThieuStr, out int thoiGian);
            decimal.TryParse(LaiSuatStr.Replace(",", "."),
                             System.Globalization.NumberStyles.Any,
                             System.Globalization.CultureInfo.InvariantCulture,
                             out decimal laiSuat);

            return new LoaiTietKiem
            {
                MaLoaiTietKiem = _maLoaiDangChon ?? 0,
                TenLoaiTietKiem = TenLoaiChiTiet,
                TienGoiToiThieu = tienGui,
                ThoiGianRutTien = thoiGian,
                LaiSuat = laiSuat,
                QuiDinhRutTien = QuiDinhRutTien
            };
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(TenLoaiChiTiet))
            {
                MessageBox.Show("Vui lòng nhập Tên Loại tiết kiệm!", "Cảnh báo",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(TienGoiToiThieuStr))
            {
                MessageBox.Show("Vui lòng nhập Tiền gửi tối thiểu!", "Cảnh báo",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(ThoiGianGuiToiThieuStr) ||
                !int.TryParse(ThoiGianGuiToiThieuStr, out _))
            {
                MessageBox.Show("Thời gian gửi tối thiểu phải là số nguyên (ngày)!", "Cảnh báo",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(LaiSuatStr))
            {
                MessageBox.Show("Vui lòng nhập Lãi suất!", "Cảnh báo",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (NgayKetThuc.HasValue && NgayKetThuc.Value.Date < NgayApDung.Date)
            {
                MessageBox.Show("Ngày kết thúc không được nhỏ hơn Ngày áp dụng!", "Cảnh báo",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void HandleThemMoi()
        {
            if (!ValidateForm()) return;
            var loai = BuildFromForm();
            string res = _bll.ThemLoai(loai, NgayApDung, NgayKetThuc);
            if (res == "SUCCESS")
            {
                MessageBox.Show("Thêm loại tiết kiệm thành công!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
            }
            else MessageBox.Show(res, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void HandleCapNhat()
        {
            if (_maLoaiDangChon == null || _maLoaiDangChon == 0)
            {
                MessageBox.Show("Vui lòng chọn một loại tiết kiệm từ danh sách trước khi cập nhật!",
                                "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidateForm()) return;
            var loai = BuildFromForm();
            string res = _bll.CapNhatLoai(loai, NgayApDung, NgayKetThuc);
            if (res == "SUCCESS")
            {
                MessageBox.Show("Cập nhật loại tiết kiệm thành công!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
            }
            else MessageBox.Show(res, "Lỗi cập nhật", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void HandleXoa()
        {
            if (_maLoaiDangChon == null || _maLoaiDangChon == 0)
            {
                MessageBox.Show("Vui lòng chọn một loại tiết kiệm từ danh sách trước khi xóa!",
                                "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn xóa loại tiết kiệm \"{TenLoaiChiTiet}\"?\n" +
                "Lưu ý: Không thể xóa nếu còn sổ tiết kiệm đang dùng loại này!",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            string res = _bll.XoaLoai(_maLoaiDangChon.Value);
            if (res == "SUCCESS")
            {
                MessageBox.Show("Xóa loại tiết kiệm thành công!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
            }
            else MessageBox.Show(res, "Lỗi xóa", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void HandleTimKiemTheoTen()
        {
            if (string.IsNullOrWhiteSpace(TenLoaiChiTiet))
            {
                MessageBox.Show("Vui lòng nhập Tên Loại tiết kiệm cần tìm!", "Cảnh báo",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ketQua = _bll.TimKiemTheoTen(TenLoaiChiTiet.Trim());
            if (ketQua == null)
            {
                MessageBox.Show($"Không tìm thấy loại tiết kiệm có tên \"{TenLoaiChiTiet}\"!",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _maLoaiDangChon = ketQua.MaLoaiTietKiem;
            TenLoaiChiTiet = ketQua.TenLoaiTietKiem;
            TienGoiToiThieuStr = ketQua.TienGoiToiThieu.ToString("N0");
            ThoiGianGuiToiThieuStr = ketQua.ThoiGianRutTien.ToString();
            QuiDinhRutTien = ketQua.QuiDinhRutTien;

            // Lấy NgayApDung, NgayKetThuc, LaiSuat chính xác từ DB
            var lichSuHienTai = _bll.LayLichSuHienTai(ketQua.MaLoaiTietKiem);
            if (lichSuHienTai != null)
            {
                LaiSuatStr = lichSuHienTai.LaiSuatCuaKyHan.ToString("N2");
                NgayApDung = lichSuHienTai.NgayApDung;
                NgayKetThuc = lichSuHienTai.NgayKetThuc;
            }
            else
            {
                LaiSuatStr = string.Empty;
                NgayApDung = DateTime.Today;
                NgayKetThuc = null;
            }

            // Highlight hàng tương ứng trên DataGrid
            foreach (var item in DanhSachLoaiTK)
            {
                if (item.MaLoaiTietKiem == ketQua.MaLoaiTietKiem)
                {
                    SelectedLoaiTK = item;
                    break;
                }
            }
        }

        private void ResetForm()
        {
            _maLoaiDangChon = null;
            TenLoaiChiTiet = string.Empty;
            TienGoiToiThieuStr = string.Empty;
            ThoiGianGuiToiThieuStr = string.Empty;
            LaiSuatStr = string.Empty;
            QuiDinhRutTien = 0;
            NgayApDung = DateTime.Today;
            NgayKetThuc = null;
            SelectedLoaiTK = null;
            RefreshDanhSach();
        }

        private void RefreshDanhSach()
        {
            DanhSachLoaiTK.Clear();
            foreach (var item in _bll.GetAll())
                DanhSachLoaiTK.Add(item);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}