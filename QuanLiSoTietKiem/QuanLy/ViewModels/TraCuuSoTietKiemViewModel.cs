using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using QuanLiSoTietKiem.QuanLy.BLL;
using QuanLiSoTietKiem.QuanLy.DAL;
using QuanLiSoTietKiem.QuanLy.Helpers;
using QuanLiSoTietKiem.QuanLy.Models;

namespace QuanLiSoTietKiem.QuanLy.ViewModels
{
    /// <summary>
    /// ViewModel cho màn hình Tra cứu sổ tiết kiệm
    /// Bao gồm toàn bộ thuộc tính bộ lọc và dữ liệu mẫu
    /// </summary>
    public class TraCuuSoTietKiemViewModel : INotifyPropertyChanged
    {
        private readonly SoTietKiemBLL _bll = new SoTietKiemBLL();

        // ============================================================
        // CÁC THUỘC TÍNH BỘ LỌC (Binding với XAML)
        // ============================================================

        #region Filter properties – Row 1
        private string _filterMaSo;
        public string FilterMaSo
        {
            get => _filterMaSo;
            set { _filterMaSo = value; OnPropertyChanged(nameof(FilterMaSo)); }
        }

        private string _filterKhachHang;
        public string FilterKhachHang
        {
            get => _filterKhachHang;
            set { _filterKhachHang = value; OnPropertyChanged(nameof(FilterKhachHang)); }
        }

        private DateTime? _filterNgaySinh;
        public DateTime? FilterNgaySinh
        {
            get => _filterNgaySinh;
            set { _filterNgaySinh = value; OnPropertyChanged(nameof(FilterNgaySinh)); }
        }

        private string _filterCCCD;
        public string FilterCCCD
        {
            get => _filterCCCD;
            set { _filterCCCD = value; OnPropertyChanged(nameof(FilterCCCD)); }
        }

        private string _filterDiaChi;
        public string FilterDiaChi
        {
            get => _filterDiaChi;
            set { _filterDiaChi = value; OnPropertyChanged(nameof(FilterDiaChi)); }
        }

        private string _filterLoaiTietKiem;
        public string FilterLoaiTietKiem
        {
            get => _filterLoaiTietKiem;
            set { _filterLoaiTietKiem = value; OnPropertyChanged(nameof(FilterLoaiTietKiem)); }
        }

        private string _filterTrangThai;
        public string FilterTrangThai
        {
            get => _filterTrangThai;
            set { _filterTrangThai = value; OnPropertyChanged(nameof(FilterTrangThai)); }
        }

        #endregion

        #region Filter properties – Row 2 (Ngày mở sổ / Ngày cập nhật gần nhất)

        private DateTime? _filterNgayMoSoTu;
        public DateTime? FilterNgayMoSoTu
        {
            get => _filterNgayMoSoTu;
            set { _filterNgayMoSoTu = value; OnPropertyChanged(nameof(FilterNgayMoSoTu)); }
        }

        private DateTime? _filterNgayMoSoDen;
        public DateTime? FilterNgayMoSoDen
        {
            get => _filterNgayMoSoDen;
            set { _filterNgayMoSoDen = value; OnPropertyChanged(nameof(FilterNgayMoSoDen)); }
        }

        private DateTime? _filterNgayCapNhatTu;
        public DateTime? FilterNgayCapNhatTu
        {
            get => _filterNgayCapNhatTu;
            set { _filterNgayCapNhatTu = value; OnPropertyChanged(nameof(FilterNgayCapNhatTu)); }
        }

        private DateTime? _filterNgayCapNhatDen;
        public DateTime? FilterNgayCapNhatDen
        {
            get => _filterNgayCapNhatDen;
            set { _filterNgayCapNhatDen = value; OnPropertyChanged(nameof(FilterNgayCapNhatDen)); }
        }

        #endregion

        #region Filter properties – Row 3 (Số tiền / Số dư tối thiểu)

        private string _filterSoTienTu;
        public string FilterSoTienTu
        {
            get => _filterSoTienTu;
            set { _filterSoTienTu = value; OnPropertyChanged(nameof(FilterSoTienTu)); }
        }

        private string _filterSoTienDen;
        public string FilterSoTienDen
        {
            get => _filterSoTienDen;
            set { _filterSoTienDen = value; OnPropertyChanged(nameof(FilterSoTienDen)); }
        }

        private string _filterSoDuToiThieuTu;
        public string FilterSoDuToiThieuTu
        {
            get => _filterSoDuToiThieuTu;
            set { _filterSoDuToiThieuTu = value; OnPropertyChanged(nameof(FilterSoDuToiThieuTu)); }
        }

        private string _filterSoDuToiThieuDen;
        public string FilterSoDuToiThieuDen
        {
            get => _filterSoDuToiThieuDen;
            set { _filterSoDuToiThieuDen = value; OnPropertyChanged(nameof(FilterSoDuToiThieuDen)); }
        }

        #endregion

        #region Filter properties – Row 4 (Quy định / Lãi suất)

        private string _filterQuyDinhThoiGian;
        public string FilterQuyDinhThoiGian
        {
            get => _filterQuyDinhThoiGian;
            set { _filterQuyDinhThoiGian = value; OnPropertyChanged(nameof(FilterQuyDinhThoiGian)); }
        }

        private string _filterQuyDinhRutTien;
        public string FilterQuyDinhRutTien
        {
            get => _filterQuyDinhRutTien;
            set { _filterQuyDinhRutTien = value; OnPropertyChanged(nameof(FilterQuyDinhRutTien)); }
        }

        private string _filterLaiSuatTu;
        public string FilterLaiSuatTu
        {
            get => _filterLaiSuatTu;
            set { _filterLaiSuatTu = value; OnPropertyChanged(nameof(FilterLaiSuatTu)); }
        }

        private string _filterLaiSuatDen;
        public string FilterLaiSuatDen
        {
            get => _filterLaiSuatDen;
            set { _filterLaiSuatDen = value; OnPropertyChanged(nameof(FilterLaiSuatDen)); }
        }

        #endregion

        #region Filter properties – Row 5 (Mã phiếu gởi / rút)

        private string _filterMaPhieuGoi;
        public string FilterMaPhieuGoi
        {
            get => _filterMaPhieuGoi;
            set { _filterMaPhieuGoi = value; OnPropertyChanged(nameof(FilterMaPhieuGoi)); }
        }

        private string _filterMaPhieuRut;
        public string FilterMaPhieuRut
        {
            get => _filterMaPhieuRut;
            set { _filterMaPhieuRut = value; OnPropertyChanged(nameof(FilterMaPhieuRut)); }
        }

        #endregion

        #region Filter properties – Row 6 (Ngày gởi / rút)

        private DateTime? _filterNgayGoiTu;
        public DateTime? FilterNgayGoiTu
        {
            get => _filterNgayGoiTu;
            set { _filterNgayGoiTu = value; OnPropertyChanged(nameof(FilterNgayGoiTu)); }
        }

        private DateTime? _filterNgayGoiDen;
        public DateTime? FilterNgayGoiDen
        {
            get => _filterNgayGoiDen;
            set { _filterNgayGoiDen = value; OnPropertyChanged(nameof(FilterNgayGoiDen)); }
        }

        private DateTime? _filterNgayRutTu;
        public DateTime? FilterNgayRutTu
        {
            get => _filterNgayRutTu;
            set { _filterNgayRutTu = value; OnPropertyChanged(nameof(FilterNgayRutTu)); }
        }

        private DateTime? _filterNgayRutDen;
        public DateTime? FilterNgayRutDen
        {
            get => _filterNgayRutDen;
            set { _filterNgayRutDen = value; OnPropertyChanged(nameof(FilterNgayRutDen)); }
        }

        #endregion

        #region Filter properties – Row 7 (Số tiền gởi / rút)

        private string _filterSoTienGoiTu;
        public string FilterSoTienGoiTu
        {
            get => _filterSoTienGoiTu;
            set { _filterSoTienGoiTu = value; OnPropertyChanged(nameof(FilterSoTienGoiTu)); }
        }

        private string _filterSoTienGoiDen;
        public string FilterSoTienGoiDen
        {
            get => _filterSoTienGoiDen;
            set { _filterSoTienGoiDen = value; OnPropertyChanged(nameof(FilterSoTienGoiDen)); }
        }

        private string _filterSoTienRutTu;
        public string FilterSoTienRutTu
        {
            get => _filterSoTienRutTu;
            set { _filterSoTienRutTu = value; OnPropertyChanged(nameof(FilterSoTienRutTu)); }
        }

        private string _filterSoTienRutDen;
        public string FilterSoTienRutDen
        {
            get => _filterSoTienRutDen;
            set { _filterSoTienRutDen = value; OnPropertyChanged(nameof(FilterSoTienRutDen)); }
        }

        #endregion

        // ============================================================
        // DANH SÁCH DROPDOWN (ComboBox sources)
        // ============================================================

        public ObservableCollection<string> ListLoaiTietKiem { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListTrangThai { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListQuyDinhThoiGian { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListQuyDinhRutTien { get; } = new ObservableCollection<string>();

        // ============================================================
        // KẾT QUẢ TRA CỨU (DataGrid source)
        // ============================================================

        public ObservableCollection<SoTietKiemDisplayModel> ListSoResult { get; } = new ObservableCollection<SoTietKiemDisplayModel>();

        // ============================================================
        // COMMANDS
        // ============================================================

        public ICommand TimKiemCommand { get; }
        public ICommand TraCuuCommand { get; }
        public ICommand LamMoiCommand { get; }
        public ICommand ThoatCommand { get; }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public TraCuuSoTietKiemViewModel()
        {
            TimKiemCommand = new RelayCommand(_ => ExecuteTimKiem());
            TraCuuCommand = new RelayCommand(_ => ExecuteTimKiem());
            LamMoiCommand = new RelayCommand(_ => ExecuteLamMoi());
            ThoatCommand = new RelayCommand(o => (o as Window)?.Close());

            LoadDropdownData();
            LoadSoTietKiem(); // Nạp toàn bộ dữ liệu khi khởi động
        }

        // ============================================================
        // PRIVATE METHODS
        // ============================================================

        /// <summary>
        /// Build SearchFilter từ toàn bộ các ô nhập trên UI rồi gọi DB.
        /// Mọi điều kiện đều tùy chọn – bỏ trống = bỏ qua.
        /// </summary>
        private void ExecuteTimKiem()
        {
            var filter = new SearchFilter
            {
                // Row 1
                MaSo = FilterMaSo,
                TenKH = FilterKhachHang,
                NgaySinh = FilterNgaySinh,
                CCCD = FilterCCCD,
                DiaChi = FilterDiaChi,
                TenLoaiTietKiem = FilterLoaiTietKiem,
                TrangThai = FilterTrangThai,

                // Row 2
                NgayMoSoTu = FilterNgayMoSoTu,
                NgayMoSoDen = FilterNgayMoSoDen,
                NgayCapNhatTu = FilterNgayCapNhatTu,
                NgayCapNhatDen = FilterNgayCapNhatDen,

                // Row 3 – parse decimal (bỏ qua nếu không hợp lệ)
                SoTienTu = TryParseDecimal(FilterSoTienTu),
                SoTienDen = TryParseDecimal(FilterSoTienDen),
                SoDuToiThieuTu = TryParseDecimal(FilterSoDuToiThieuTu),
                SoDuToiThieuDen = TryParseDecimal(FilterSoDuToiThieuDen),

                // Row 4
                QuyDinhThoiGian = FilterQuyDinhThoiGian,
                QuyDinhRutTien = FilterQuyDinhRutTien,
                LaiSuatTu = TryParseDecimal(FilterLaiSuatTu),
                LaiSuatDen = TryParseDecimal(FilterLaiSuatDen),

                // Row 5
                MaPhieuGoi = FilterMaPhieuGoi,
                MaPhieuRut = FilterMaPhieuRut,

                // Row 6
                NgayGoiTu = FilterNgayGoiTu,
                NgayGoiDen = FilterNgayGoiDen,
                NgayRutTu = FilterNgayRutTu,
                NgayRutDen = FilterNgayRutDen,

                // Row 7
                SoTienGoiTu = TryParseDecimal(FilterSoTienGoiTu),
                SoTienGoiDen = TryParseDecimal(FilterSoTienGoiDen),
                SoTienRutTu = TryParseDecimal(FilterSoTienRutTu),
                SoTienRutDen = TryParseDecimal(FilterSoTienRutDen),
            };

            LoadSoTietKiem(filter);
        }

        /// <summary>Parse decimal từ chuỗi người dùng nhập; trả null nếu rỗng hoặc không hợp lệ.</summary>
        private static decimal? TryParseDecimal(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            // Chấp nhận cả dấu phẩy lẫn dấu chấm thập phân
            s = s.Replace(",", ".");
            return decimal.TryParse(s, System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out decimal v)
                   ? v : (decimal?)null;
        }

        /// <summary>Đặt lại toàn bộ bộ lọc về trạng thái ban đầu và load lại toàn bộ dữ liệu</summary>
        private void ExecuteLamMoi()
        {
            FilterMaSo = string.Empty;
            FilterKhachHang = string.Empty;
            FilterNgaySinh = null;
            FilterCCCD = string.Empty;
            FilterDiaChi = string.Empty;
            FilterLoaiTietKiem = ListLoaiTietKiem.Count > 0 ? ListLoaiTietKiem[0] : null;
            FilterTrangThai = ListTrangThai.Count > 0 ? ListTrangThai[0] : null;

            FilterNgayMoSoTu = null;
            FilterNgayMoSoDen = null;
            FilterNgayCapNhatTu = null;
            FilterNgayCapNhatDen = null;

            FilterSoTienTu = string.Empty;
            FilterSoTienDen = string.Empty;
            FilterSoDuToiThieuTu = string.Empty;
            FilterSoDuToiThieuDen = string.Empty;

            FilterQuyDinhThoiGian = ListQuyDinhThoiGian.Count > 0 ? ListQuyDinhThoiGian[0] : null;
            FilterQuyDinhRutTien = ListQuyDinhRutTien.Count > 0 ? ListQuyDinhRutTien[0] : null;
            FilterLaiSuatTu = string.Empty;
            FilterLaiSuatDen = string.Empty;

            FilterMaPhieuGoi = string.Empty;
            FilterMaPhieuRut = string.Empty;

            FilterNgayGoiTu = null;
            FilterNgayGoiDen = null;
            FilterNgayRutTu = null;
            FilterNgayRutDen = null;

            FilterSoTienGoiTu = string.Empty;
            FilterSoTienGoiDen = string.Empty;
            FilterSoTienRutTu = string.Empty;
            FilterSoTienRutDen = string.Empty;

            // Load lại toàn bộ dữ liệu từ CSDL (không có bộ lọc)
            LoadSoTietKiem();
        }

        /// <summary>Nạp dữ liệu cho các ComboBox bộ lọc từ CSDL thực</summary>
        private void LoadDropdownData()
        {
            var dsLoai = _bll.GetLoaiTietKiems();

            // ── Tên loại tiết kiệm ──────────────────────────────────
            ListLoaiTietKiem.Add("Tất cả");
            foreach (var loai in dsLoai)
                ListLoaiTietKiem.Add(loai.TenLoaiTietKiem);
            FilterLoaiTietKiem = "Tất cả";

            // ── Trạng thái: DB lưu 1 = Mở, 0 = Đóng ────────────────
            ListTrangThai.Add("Tất cả");
            ListTrangThai.Add("Mở");
            ListTrangThai.Add("Đóng");
            FilterTrangThai = "Tất cả";

            // ── Quy định thời gian rút tiền: lấy từ DB, ghép "ngày" ─
            ListQuyDinhThoiGian.Add("Tất cả");
            var daThem = new System.Collections.Generic.HashSet<int>();
            foreach (var loai in dsLoai)
            {
                if (daThem.Add(loai.ThoiGianRutTien))
                    ListQuyDinhThoiGian.Add($"{loai.ThoiGianRutTien} ngày");
            }
            FilterQuyDinhThoiGian = "Tất cả";

            // ── Quy định rút tiền: DB lưu 1 = Rút 1 phần, 0 = Rút toàn bộ ─
            ListQuyDinhRutTien.Add("Tất cả");
            ListQuyDinhRutTien.Add("Rút 1 phần");
            ListQuyDinhRutTien.Add("Rút toàn bộ");
            FilterQuyDinhRutTien = "Tất cả";
        }

        /// <summary>
        /// Nạp dữ liệu sổ tiết kiệm từ CSDL với bộ lọc nâng cao, convert các trường số thành chuỗi hiển thị.
        /// </summary>
        private void LoadSoTietKiem(SearchFilter filter = null)
        {
            ListSoResult.Clear();

            var dsLoai = _bll.GetLoaiTietKiems();
            var dsSo = filter != null
                       ? _bll.SearchSoTietKiemAdvanced(filter)
                       : _bll.SearchSoTietKiemAdvanced(new SearchFilter()); // không có filter = lấy tất cả

            int stt = 1;
            foreach (var so in dsSo)
            {
                // Tìm loại tiết kiệm tương ứng
                var loai = dsLoai.Find(l => l.MaLoaiTietKiem == so.MaLoaiTietKiem);

                ListSoResult.Add(new SoTietKiemDisplayModel
                {
                    STT = stt++,
                    MaSo = so.MaSo,
                    TenKH = so.TenKH,
                    NgaySinh = so.NgaySinh,
                    CCCD = so.CCCD,
                    DiaChi = so.DiaChi,
                    NgayMoSo = so.NgayMoSo,
                    NgayCapNhat = so.NgayCapNhatGanNhat == DateTime.MinValue
                                            ? (DateTime?)null
                                            : so.NgayCapNhatGanNhat,
                    SoTien = so.SoTien,
                    SoDuToiThieu = so.SoDuToiThieu,

                    // Tên loại tiết kiệm – lấy từ bảng loai_tiet_kiem
                    TenLoaiTietKiem = loai?.TenLoaiTietKiem ?? $"Mã {so.MaLoaiTietKiem}",

                    // Trạng thái: DB bool (1=true=Mở, 0=false=Đóng)
                    TrangThai = so.TrangThai ? "Mở" : "Đóng",

                    // Quy định thời gian rút tiền: ghép chữ "ngày"
                    QuyDinhThoiGianRut = loai != null ? $"{loai.ThoiGianRutTien} ngày" : "-",

                    // Quy định rút tiền: 1 = Rút 1 phần, 0 = Rút toàn bộ
                    QuyDinhRutTien = loai != null
                                            ? (loai.QuiDinhRutTien == 1 ? "Rút 1 phần" : "Rút toàn bộ")
                                            : "-",

                    // Lãi suất: format X.##% dùng InvariantCulture để nhất quán với ComboBox
                    LaiSuat = loai != null ? loai.LaiSuat.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "%" : "-",
                });
            }
        }

        // ============================================================
        // INotifyPropertyChanged
        // ============================================================

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}