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
    /// <summary>
    /// Item dùng cho ComboBox "Quy định rút tiền".
    /// GiaTri: 0 = Rút toàn bộ, 1 = Rút một phần (đúng quy ước lưu CSDL).
    /// </summary>
    public class QuyDinhRutTienItem
    {
        public int GiaTri { get; set; }
        public string TenHienThi { get; set; }
    }

    public class QuanLyQuyDinhViewModel : INotifyPropertyChanged
    {
        private readonly QuanLyQuyDinhBLL _bll = new QuanLyQuyDinhBLL();

        // ─── Mã loại đang chọn (giữ ngầm để Cập nhật/Xóa, KHÔNG hiển thị trên UI) ───
        private int? _maLoaiDangChon;

        // ─── Danh sách hiển thị trên DataGrid ───────────────────────────
        public ObservableCollection<LoaiTietKiem> DanhSachLoaiTK { get; set; }

        // ─── Danh sách cho ComboBox Quy định rút tiền ───────────────────
        public ObservableCollection<QuyDinhRutTienItem> DanhSachQuyDinhRutTien { get; }
            = new ObservableCollection<QuyDinhRutTienItem>
            {
                new QuyDinhRutTienItem { GiaTri = 0, TenHienThi = "Rút toàn bộ" },
                new QuyDinhRutTienItem { GiaTri = 1, TenHienThi = "Rút một phần" }
            };

        // ─── Item đang chọn trên DataGrid ───────────────────────────────
        private LoaiTietKiem _selectedLoaiTK;
        public LoaiTietKiem SelectedLoaiTK
        {
            get => _selectedLoaiTK;
            set { _selectedLoaiTK = value; OnPropertyChanged(nameof(SelectedLoaiTK)); }
        }

        // ─── Các thuộc tính binding cho Form ────────────────────────────

        // Tên Loại tiết kiệm
        private string _tenLoaiChiTiet = string.Empty;
        public string TenLoaiChiTiet
        {
            get => _tenLoaiChiTiet;
            set { _tenLoaiChiTiet = value; OnPropertyChanged(nameof(TenLoaiChiTiet)); }
        }

        // Ngày áp dụng (của lịch sử lãi suất)
        private DateTime _ngayApDung = DateTime.Today;
        public DateTime NgayApDung
        {
            get => _ngayApDung;
            set { _ngayApDung = value; OnPropertyChanged(nameof(NgayApDung)); }
        }

        // Ngày kết thúc (của lịch sử lãi suất) — có thể để trống (đang áp dụng)
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

        // Thời gian gửi tối thiểu — đơn vị NGÀY
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

        // Quy định rút tiền: 0 = Rút toàn bộ, 1 = Rút một phần (binding với ComboBox.SelectedValue)
        private int _quiDinhRutTien;
        public int QuiDinhRutTien
        {
            get => _quiDinhRutTien;
            set { _quiDinhRutTien = value; OnPropertyChanged(nameof(QuiDinhRutTien)); }
        }

        // ─── Commands ────────────────────────────────────────────────────
        public ICommand ThemMoiCommand { get; }
        public ICommand CapNhatCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand LamMoiCommand { get; }
        public ICommand TimKiemTheoTenCommand { get; }

        // ─── Constructor ─────────────────────────────────────────────────
        public QuanLyQuyDinhViewModel()
        {
            DanhSachLoaiTK = new ObservableCollection<LoaiTietKiem>(_bll.GetAll());

            ThemMoiCommand = new RelayCommand(o => HandleThemMoi());
            CapNhatCommand = new RelayCommand(o => HandleCapNhat());
            XoaCommand = new RelayCommand(o => HandleXoa());
            LamMoiCommand = new RelayCommand(o => ResetForm());
            TimKiemTheoTenCommand = new RelayCommand(o => HandleTimKiemTheoTen());
        }

        // ─── Đổ dữ liệu từ hàng được chọn lên Form ──────────────────────
        public void LoadSelectedToForm()
        {
            if (SelectedLoaiTK == null) return;

            _maLoaiDangChon = SelectedLoaiTK.MaLoaiTietKiem;
            TenLoaiChiTiet = SelectedLoaiTK.TenLoaiTietKiem;
            TienGoiToiThieuStr = SelectedLoaiTK.TienGoiToiThieu.ToString("N0");
            ThoiGianGuiToiThieuStr = SelectedLoaiTK.ThoiGianRutTien.ToString();
            LaiSuatStr = SelectedLoaiTK.LaiSuat.ToString("N2");
            QuiDinhRutTien = SelectedLoaiTK.QuiDinhRutTien;
            NgayApDung = DateTime.Today;   // Ngày áp dụng mới khi sửa lãi suất
            NgayKetThuc = null;
        }

        // ─── Xây dựng đối tượng từ Form ──────────────────────────────────
        private LoaiTietKiem BuildFromForm()
        {
            decimal.TryParse(TienGoiToiThieuStr.Replace(",", "").Replace(".", ""), out decimal tienGui);
            int.TryParse(ThoiGianGuiToiThieuStr, out int thoiGian);
            decimal.TryParse(LaiSuatStr.Replace(",", "."), System.Globalization.NumberStyles.Any,
                             System.Globalization.CultureInfo.InvariantCulture, out decimal laiSuat);

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

        // ─── Validate form cơ bản ─────────────────────────────────────────
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

        // ─── THÊM MỚI ────────────────────────────────────────────────────
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
            else
            {
                MessageBox.Show(res, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ─── CẬP NHẬT ────────────────────────────────────────────────────
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
            else
            {
                MessageBox.Show(res, "Lỗi cập nhật", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ─── XÓA ─────────────────────────────────────────────────────────
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
            else
            {
                MessageBox.Show(res, "Lỗi xóa", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ─── TÌM KIẾM THEO TÊN — đổ thông tin lên form nếu tìm thấy ────
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

            // Đổ thông tin lên form
            _maLoaiDangChon = ketQua.MaLoaiTietKiem;
            TenLoaiChiTiet = ketQua.TenLoaiTietKiem;
            TienGoiToiThieuStr = ketQua.TienGoiToiThieu.ToString("N0");
            ThoiGianGuiToiThieuStr = ketQua.ThoiGianRutTien.ToString();
            LaiSuatStr = ketQua.LaiSuat.ToString("N2");
            QuiDinhRutTien = ketQua.QuiDinhRutTien;
            NgayApDung = DateTime.Today;
            NgayKetThuc = null;

            // Đồng thời highlight hàng tương ứng trên DataGrid
            foreach (var item in DanhSachLoaiTK)
            {
                if (item.MaLoaiTietKiem == ketQua.MaLoaiTietKiem)
                {
                    SelectedLoaiTK = item;
                    break;
                }
            }
        }

        // ─── LÀM MỚI form và load lại danh sách ─────────────────────────
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

        // ─── Load lại toàn bộ danh sách ──────────────────────────────────
        private void RefreshDanhSach()
        {
            DanhSachLoaiTK.Clear();
            foreach (var item in _bll.GetAll())
                DanhSachLoaiTK.Add(item);
        }

        // ─── INotifyPropertyChanged ───────────────────────────────────────
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}