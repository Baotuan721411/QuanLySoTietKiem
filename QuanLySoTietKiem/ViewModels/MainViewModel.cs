using QuanLySoTietKiem.Helpers;
using QuanLySoTietKiem.Models;
using QuanLySoTietKiem.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QuanLySoTietKiem.ViewModels
{
    /// <summary>
    /// ViewModel chính — quản lý form nhập liệu và danh sách sổ tiết kiệm.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly SavingsService _service = new SavingsService();

        // --- Collections ---
        public ObservableCollection<LoaiTietKiem> ListLoaiTK { get; set; }
        public ObservableCollection<SoTietKiem> DanhSachSo { get; set; }

        // --- Form binding properties ---
        private string _tenKH, _cccd, _diaChi, _soTienStr, _maSo = "<Giá trị tự động>";
        private string _searchKeyword;

        public string MaSo
        {
            get => _maSo;
            set { _maSo = value; OnPropertyChanged(nameof(MaSo)); }
        }
        public string TenKH
        {
            get => _tenKH;
            set { _tenKH = value; OnPropertyChanged(nameof(TenKH)); }
        }
        public string CCCD
        {
            get => _cccd;
            set { _cccd = value; OnPropertyChanged(nameof(CCCD)); }
        }
        public string DiaChi
        {
            get => _diaChi;
            set { _diaChi = value; OnPropertyChanged(nameof(DiaChi)); }
        }
        public string SoTienStr
        {
            get => _soTienStr;
            set { _soTienStr = value; OnPropertyChanged(nameof(SoTienStr)); }
        }
        public string SearchKeyword
        {
            get => _searchKeyword;
            set { _searchKeyword = value; OnPropertyChanged(nameof(SearchKeyword)); }
        }

        private DateTime? _ngaySinh;
        public DateTime? NgaySinh
        {
            get => _ngaySinh;
            set { _ngaySinh = value; OnPropertyChanged(nameof(NgaySinh)); }
        }

        private DateTime _ngayMoSo = DateTime.Now;
        public DateTime NgayMoSo
        {
            get => _ngayMoSo;
            set { _ngayMoSo = value; OnPropertyChanged(nameof(NgayMoSo)); }
        }

        private int? _selectedLoaiId;
        public int? SelectedLoaiId
        {
            get => _selectedLoaiId;
            set { _selectedLoaiId = value; OnPropertyChanged(nameof(SelectedLoaiId)); }
        }

        private SoTietKiem _selectedSo;
        /// <summary>
        /// Sổ tiết kiệm đang được chọn trong DataGrid.
        /// Khi chọn, form sẽ được tự động điền dữ liệu.
        /// </summary>
        public SoTietKiem SelectedSo
        {
            get => _selectedSo;
            set
            {
                _selectedSo = value;
                OnPropertyChanged(nameof(SelectedSo));
                if (value != null) PopulateFormFromSelected(value);
            }
        }

        // --- Trạng thái chỉnh sửa ---
        private bool _isEditing;
        /// <summary>
        /// True khi đang chỉnh sửa sổ đã tồn tại (chọn từ DataGrid).
        /// </summary>
        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(nameof(IsEditing)); }
        }

        // --- Commands ---
        public ICommand TiepNhanCommand { get; }
        public ICommand TaoMoiCommand { get; }
        public ICommand TimSoCommand { get; }
        public ICommand XoaSoCommand { get; }
        public ICommand CapNhatSoCommand { get; }
        public ICommand ThoatCommand { get; }

        public MainViewModel()
        {
            try
            {
                ListLoaiTK = new ObservableCollection<LoaiTietKiem>(_service.GetLoaiTietKiems());
                DanhSachSo = new ObservableCollection<SoTietKiem>(_service.GetAllSoTietKiem());
            }
            catch (Exception ex)
            {
                ListLoaiTK = new ObservableCollection<LoaiTietKiem>();
                DanhSachSo = new ObservableCollection<SoTietKiem>();
                MessageBox.Show("Không thể kết nối cơ sở dữ liệu:\n" + ex.Message,
                    "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            TiepNhanCommand = new RelayCommand(_ => HandleSubmit());
            TaoMoiCommand = new RelayCommand(_ => ResetForm());
            TimSoCommand = new RelayCommand(_ => HandleSearch());
            XoaSoCommand = new RelayCommand(_ => HandleDelete(), _ => SelectedSo != null);
            CapNhatSoCommand = new RelayCommand(_ => HandleUpdate(), _ => IsEditing);
            ThoatCommand = new RelayCommand(_ => Application.Current.Shutdown());
        }

        // =====================================================
        //  Handlers
        // =====================================================

        private void HandleSubmit()
        {
            if (!ValidateFormFields()) return;

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

            try
            {
                string res = _service.ValidateAndSubmit(so);
                if (res == "SUCCESS")
                {
                    MaSo = so.MaSo;
                    MessageBox.Show("Tiếp nhận thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshList();
                }
                else
                {
                    MessageBox.Show(res, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleSearch()
        {
            try
            {
                var results = string.IsNullOrWhiteSpace(SearchKeyword)
                    ? _service.GetAllSoTietKiem()
                    : _service.SearchSoTietKiem(SearchKeyword.Trim());

                DanhSachSo.Clear();
                foreach (var s in results)
                    DanhSachSo.Add(s);

                if (results.Count == 0)
                    MessageBox.Show("Không tìm thấy sổ tiết kiệm nào.", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleDelete()
        {
            if (SelectedSo == null) return;

            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn xóa sổ '{SelectedSo.MaSo}' của khách hàng '{SelectedSo.TenKH}'?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (_service.DeleteSoTietKiem(SelectedSo.MaSo))
                {
                    MessageBox.Show("Xóa thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    ResetForm();
                    RefreshList();
                }
                else
                {
                    MessageBox.Show("Không thể xóa sổ tiết kiệm.", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleUpdate()
        {
            if (!ValidateFormFields()) return;
            if (string.IsNullOrWhiteSpace(MaSo) || MaSo == "<Giá trị tự động>")
            {
                MessageBox.Show("Vui lòng chọn sổ cần cập nhật từ danh sách.", "Cảnh báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MaLoaiTietKiem = SelectedLoaiId.Value,
                NgayMoSo = NgayMoSo
            };

            try
            {
                string res = _service.ValidateAndUpdate(so);
                if (res == "SUCCESS")
                {
                    MessageBox.Show("Cập nhật thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshList();
                }
                else
                {
                    MessageBox.Show(res, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =====================================================
        //  Helpers
        // =====================================================

        private bool ValidateFormFields()
        {
            if (string.IsNullOrWhiteSpace(TenKH) || string.IsNullOrWhiteSpace(CCCD) ||
                string.IsNullOrWhiteSpace(DiaChi) || string.IsNullOrWhiteSpace(SoTienStr) ||
                NgaySinh == null || SelectedLoaiId == null)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tất cả các trường thông tin!",
                    "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(SoTienStr, out _))
            {
                MessageBox.Show("Số tiền không hợp lệ. Vui lòng nhập số.",
                    "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Điền thông tin sổ tiết kiệm đã chọn vào form.
        /// </summary>
        private void PopulateFormFromSelected(SoTietKiem so)
        {
            MaSo = so.MaSo;
            TenKH = so.TenKH;
            CCCD = so.CCCD;
            DiaChi = so.DiaChi;
            SoTienStr = so.SoTien.ToString("0");
            NgaySinh = so.NgaySinh;
            NgayMoSo = so.NgayMoSo;
            SelectedLoaiId = so.MaLoaiTietKiem;
            IsEditing = true;
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
            SelectedSo = null;
            IsEditing = false;
        }

        private void RefreshList()
        {
            try
            {
                var all = string.IsNullOrWhiteSpace(SearchKeyword)
                    ? _service.GetAllSoTietKiem()
                    : _service.SearchSoTietKiem(SearchKeyword.Trim());
                DanhSachSo.Clear();
                foreach (var s in all)
                    DanhSachSo.Add(s);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =====================================================
        //  INotifyPropertyChanged
        // =====================================================

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
