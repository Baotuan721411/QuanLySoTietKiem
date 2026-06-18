using QuanLiSoTietKiem.QuanLy.BLL;
using QuanLiSoTietKiem.QuanLy.Helpers;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QuanLiSoTietKiem.QuanLy.ViewModels
{
    public class SoTietKiemViewModel : INotifyPropertyChanged
    {
        private readonly SoTietKiemBLL _bll = new SoTietKiemBLL();
        private readonly PhieuGoiBLL _phieuGoiBLL = new PhieuGoiBLL();

        public ObservableCollection<LoaiTietKiem> ListLoaiTK { get; set; }

        // --- Các thuộc tính Binding ---
        private string _tenKH, _cccd, _diaChi, _soTienStr, _maSo = string.Empty;
        public string TenKH { get => _tenKH; set { _tenKH = value; OnPropertyChanged("TenKH"); } }
        public string CCCD { get => _cccd; set { _cccd = value; OnPropertyChanged("CCCD"); } }
        public string DiaChi { get => _diaChi; set { _diaChi = value; OnPropertyChanged("DiaChi"); } }
        public string SoTienStr { get => _soTienStr; set { _soTienStr = value; OnPropertyChanged("SoTienStr"); } }
        public string MaSo { get => _maSo; set { _maSo = value; OnPropertyChanged("MaSo"); } }

        private DateTime? _ngaySinh;
        public DateTime? NgaySinh
        {
            get => _ngaySinh;
            set
            {
                _ngaySinh = value;
                OnPropertyChanged("NgaySinh");
                CapNhatDoTuoi();
            }
        }

        private DateTime _ngayMoSo = DateTime.Now;
        public DateTime NgayMoSo
        {
            get => _ngayMoSo;
            set
            {
                _ngayMoSo = value;
                OnPropertyChanged("NgayMoSo");
                CapNhatDoTuoi();
            }
        }

        private int? _selectedLoaiId;
        public int? SelectedLoaiId
        {
            get => _selectedLoaiId;
            set
            {
                _selectedLoaiId = value;
                OnPropertyChanged("SelectedLoaiId");
                CapNhatTienGuiToiThieu();   // Cập nhật quy định khi chọn loại
            }
        }

        private int? _quyDinhTuoi = 0;
        public int? QuyDinhTuoi { get => _quyDinhTuoi; set { _quyDinhTuoi = value; OnPropertyChanged("QuyDinhTuoi"); } }

        // Hiển thị quy định tiền gửi tối thiểu — null khi chưa chọn loại
        private string _quyDinhTienGuiToiThieu = "";
        public string QuyDinhTienGuiToiThieu
        {
            get => _quyDinhTienGuiToiThieu;
            set { _quyDinhTienGuiToiThieu = value; OnPropertyChanged("QuyDinhTienGuiToiThieu"); }
        }


        private int? _doTuoi = 0;
        public int? DoTuoi { get => _doTuoi; set { _doTuoi = value; OnPropertyChanged("DoTuoi"); } }

        // Lưu lại NgayMoSo gốc khi tìm kiếm, dùng để tra cứu phiếu gởi khi cập nhật
        private DateTime _ngayMoSoGoc;

        public ICommand TiepNhanCommand { get; }
        public ICommand TaoMoiCommand { get; }
        public ICommand TimKiemCommand { get; }
        public ICommand CapNhatCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand ThoatCommand { get; }

        public SoTietKiemViewModel()
        {
            ListLoaiTK = new ObservableCollection<LoaiTietKiem>(_bll.GetLoaiTietKiems());
            MaSo = _bll.GetNextMaso();
            QuyDinhTuoi = _bll.GetThamSo()?.DoTuoiToiThieu;
            DoTuoi = 0;
            QuyDinhTienGuiToiThieu = "";
            _ngayMoSoGoc = DateTime.MinValue;

            TiepNhanCommand = new RelayCommand(o => HandleSubmit());
            TaoMoiCommand = new RelayCommand(o => ResetForm());
            TimKiemCommand = new RelayCommand(o => HandleSearch());
            CapNhatCommand = new RelayCommand(o => HandleUpdate());
            XoaCommand = new RelayCommand(o => HandleDelete());
            ThoatCommand = new RelayCommand(o => Application.Current.Shutdown());
        }



        // Cập nhật QuyDinhTienGuiToiThieu khi người dùng chọn loại tiết kiệm
        private void CapNhatTienGuiToiThieu()
        {
            if (_selectedLoaiId.HasValue)
            {
                var loai = ListLoaiTK.FirstOrDefault(l => l.MaLoaiTietKiem == _selectedLoaiId.Value);
                if (loai != null)
                {
                    QuyDinhTienGuiToiThieu = loai.TienGoiToiThieu.ToString("N0") + " VNĐ";
                    return;
                }
            }
            // Chưa chọn hoặc không tìm thấy → ẩn ô
            QuyDinhTienGuiToiThieu = "";
        }

        private void CapNhatDoTuoi()
        {
            if (NgaySinh != null)
            {
                int tuoi = NgayMoSo.Year - NgaySinh.Value.Year;
                if (NgaySinh.Value.Date > NgayMoSo.AddYears(-tuoi)) tuoi--;
                DoTuoi = tuoi;
            }
            else DoTuoi = 0;
        }

        private void ResetForm()
        {
            MaSo = _bll.GetNextMaso();
            TenKH = string.Empty;
            CCCD = string.Empty;
            DiaChi = string.Empty;
            SoTienStr = string.Empty;
            NgaySinh = null;
            NgayMoSo = DateTime.Now;
            SelectedLoaiId = null;   // Sẽ tự gọi CapNhatTienGuiToiThieu → ẩn ô
            DoTuoi = 0;
            QuyDinhTuoi = _bll.GetThamSo()?.DoTuoiToiThieu;
            _ngayMoSoGoc = DateTime.MinValue;
        }

        private void HandleSubmit()
        {
            if (string.IsNullOrWhiteSpace(TenKH) || string.IsNullOrWhiteSpace(CCCD) ||
                string.IsNullOrWhiteSpace(DiaChi) || string.IsNullOrWhiteSpace(SoTienStr) ||
                NgaySinh == null || SelectedLoaiId == null)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tất cả các trường thông tin!", "Cảnh báo",
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
                NgayMoSo = NgayMoSo,
                TrangThai = true,
                SoDuToiThieu = 0,
            };

            string res = _bll.ValidateAndSubmit(so);
            if (res == "SUCCESS")
            {
                MaSo = so.MaSo;

                bool goiOk = _phieuGoiBLL.TaoPhieuGoiKhiMoSo(so);
                if (!goiOk)
                    MessageBox.Show("Mở sổ thành công nhưng không thể tạo phiếu gởi tự động. Vui lòng kiểm tra lại!",
                                    "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show("Tiếp nhận thành công! Phiếu gởi đã được tạo tự động.", "Thông báo",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(res, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleSearch()
        {
            string keyword = !string.IsNullOrWhiteSpace(TenKH)
                ? TenKH
                : (MaSo != "<Giá trị tự động>" ? MaSo : "");

            if (string.IsNullOrEmpty(keyword))
            {
                MessageBox.Show("Vui lòng nhập đúng Tên hoặc Mã sổ để tìm kiếm!");
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
                SelectedLoaiId = s.MaLoaiTietKiem;  // Tự gọi CapNhatTienGuiToiThieu

                _ngayMoSoGoc = s.NgayMoSo;
            }
            else
            {
                MessageBox.Show("Không tìm thấy sổ!");
            }
        }

        private void HandleUpdate()
        {
            if (!MaSo.StartsWith("STK") || _bll.SearchSoTietKiem(MaSo).Count == 0)
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
                NgayMoSo = NgayMoSo,
                TrangThai = true,
                SoDuToiThieu = 0
            };

            string res = _bll.ValidateAndUpdate(so);
            if (res == "SUCCESS")
            {
                DateTime ngayGocDeTim = (_ngayMoSoGoc != DateTime.MinValue) ? _ngayMoSoGoc : NgayMoSo;

                bool goiOk = _phieuGoiBLL.CapNhatPhieuGoiKhiCapNhatSo(so, ngayGocDeTim);

                _ngayMoSoGoc = so.NgayMoSo;

                if (!goiOk)
                    MessageBox.Show("Cập nhật sổ thành công nhưng không tìm thấy phiếu gởi tương ứng để cập nhật.",
                                    "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show("Cập nhật thành công! Phiếu gởi cũng đã được cập nhật.", "Thông báo",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(res, "Lỗi cập nhật", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleDelete()
        {
            if (MaSo == "<Giá trị tự động>" || !MaSo.StartsWith("STK")) return;

            if (MessageBox.Show($"Bạn có chắc muốn xóa sổ {MaSo}? Toàn bộ phiếu gởi/phiếu rút liên quan cũng sẽ bị xóa.", "Xác nhận xóa",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                string maSoCanXoa = MaSo;

                // DeleteSoTietKiem đã tự xóa toàn bộ phiếu gởi + phiếu rút liên quan
                // trong cùng 1 transaction trước khi xóa sổ, nên không cần gọi
                // XoaPhieuGoiKhiXoaSo riêng nữa (gọi riêng sẽ luôn báo "không tìm thấy"
                // vì phiếu đã bị xóa hết ở bước trên).
                if (_bll.DeleteSoTietKiem(maSoCanXoa))
                {
                    MessageBox.Show("Đã xóa sổ và toàn bộ phiếu gởi/phiếu rút liên quan thành công!", "Thông báo",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

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