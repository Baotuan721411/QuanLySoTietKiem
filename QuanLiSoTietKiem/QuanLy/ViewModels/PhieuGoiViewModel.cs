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
    public class PhieuGoiViewModel : INotifyPropertyChanged
    {
        private readonly PhieuGoiBLL _bll = new PhieuGoiBLL();
        private readonly SoTietKiemBLL _bllSo = new SoTietKiemBLL();

        // Thuộc tính hiển thị "<Giá trị tự phát sinh>"
        private string _maPhieuGoiDisplay;
        public string MaPhieuGoiDisplay
        {
            get => _maPhieuGoiDisplay;
            set { _maPhieuGoiDisplay = value; OnPropertyChanged("MaPhieuGoiDisplay"); }
        }
        private string _maSo, _tenKH, _soTienGoiStr, _loaiTietKiemHienTai, _quyDinhLoaiTK, _quyDinhSoTien;
        private DateTime _ngayGoi = DateTime.Now; // Lấy ngày hiện tại

        public ObservableCollection<string> ListMaSo { get; set; } = new ObservableCollection<string>();

        public string MaSo
        {
            get => _maSo;
            set
            {
                _maSo = value;
                OnPropertyChanged("MaSo");
                if (!string.IsNullOrEmpty(_maSo))
                {
                    var so = _bllSo.SearchSoTietKiem(_maSo)[0];
                    if (so != null)
                    {
                        TenKH = so.TenKH;
                        var loai = _bllSo.GetLoaiTietKiemByMaSo(_maSo);
                        LoaiTietKiemHienTai = loai?.TenLoaiTietKiem;
                    }
                }
            }
        }
        public string TenKH { get => _tenKH; set { _tenKH = value; OnPropertyChanged("TenKH"); } }
        public string SoTienGoiStr { get => _soTienGoiStr; set { _soTienGoiStr = value; OnPropertyChanged("SoTienGoiStr"); } }
        public string LoaiTietKiemHienTai
        {
            get => _loaiTietKiemHienTai;
            set
            {
                _loaiTietKiemHienTai = value;
                OnPropertyChanged("LoaiTietKiemHienTai");

            }
        }
        public string QuyDinhLoaiTK { get => _quyDinhLoaiTK; set { _quyDinhLoaiTK = value; OnPropertyChanged("QuyDinhLoaiTK"); } }
        public string QuyDinhSoTien { get => _quyDinhSoTien; set { _quyDinhSoTien = value; OnPropertyChanged("QuyDinhSoTien"); } }
        public DateTime NgayGoi { get => _ngayGoi; set { _ngayGoi = value; OnPropertyChanged("NgayGoi"); } }

        public ICommand LapPhieuCommand { get; }
        public ICommand TraCuuCommand { get; }
        public ICommand ThoatCommand { get; }
        private void CapNhatTenLoaiTietKiem()
        {
            LoaiTietKiem ltk = _bllSo.GetLoaiTietKiemByMaSo(_maSo);
            _loaiTietKiemHienTai = ltk.TenLoaiTietKiem;
        }
        public PhieuGoiViewModel()
        {
            LoadData();
            LapPhieuCommand = new RelayCommand(o => HandleLapPhieu());
            TraCuuCommand = new RelayCommand(o => HandleTraCuu());
            ThoatCommand = new RelayCommand(o => (o as Window)?.Close());
        }

        private void LoadData()
        {
            // Load danh sách mã sổ vào ComboBox
            MaPhieuGoiDisplay = _bll.GetNextMaPhieuGoi();
            var allSo = _bllSo.SearchSoTietKiem("");
            foreach (var s in allSo) ListMaSo.Add(s.MaSo);

            // Load quy định
            var ts = _bllSo.GetThamSo();
            if (ts != null)
            {
                QuyDinhLoaiTK = ts.LoaiTietKiemGoi;
                QuyDinhSoTien = ts.SoTienGoiThemToiThieu.ToString("N0") + " VNĐ";
            }
        }

        private void HandleTraCuu()
        {
            string keyword = !string.IsNullOrWhiteSpace(TenKH) ? TenKH : (MaSo != "<Giá trị tự động>" ? MaSo : "");
            var so = _bllSo.SearchSoTietKiem(keyword)[0];
            if (so != null)
            {
                MaSo = so.MaSo;
                TenKH = so.TenKH;
                var loai = _bllSo.GetLoaiTietKiemByMaSo(so.MaSo);
                if (loai != null)
                {
                    LoaiTietKiemHienTai = loai.TenLoaiTietKiem;
                }
            }
            else MessageBox.Show("Không tìm thấy sổ!");
        }

        private void HandleLapPhieu()
        {
            if (string.IsNullOrEmpty(MaSo) || string.IsNullOrEmpty(TenKH))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin mã sổ và khách hàng!");
                return;
            }

            if (!decimal.TryParse(SoTienGoiStr, out decimal money))
            {
                MessageBox.Show("Số tiền gởi không hợp lệ!");
                return;
            }

            // Lấy mã thực tế từ DB ngay tại thời điểm bấm nút
            string maThucTe = _bll.GetNextMaPhieuGoi();

            var phieu = new PhieuGoi
            {
                MaPhieuGoi = maThucTe, // Dùng mã thực tế để lưu
                MaSo = MaSo,
                NgayGoi = NgayGoi,
                SoTienGoi = money
            };

            string res = _bll.ValidateAndSavePhieuGoi(phieu, TenKH);
            if (res == "SUCCESS")
            {
                MessageBox.Show($"Lập phiếu thành công! Mã phiếu của bạn là: {maThucTe}");
                // Reset form để nhập phiếu mới
                MaSo = "";
                TenKH = "";
                SoTienGoiStr = "";
                LoaiTietKiemHienTai = "";
            }
            else
            {
                MessageBox.Show(res);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}