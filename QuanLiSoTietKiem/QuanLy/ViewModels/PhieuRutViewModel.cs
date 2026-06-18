using QuanLiSoTietKiem.QuanLy.BLL;
using QuanLiSoTietKiem.QuanLy.Helpers;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace QuanLiSoTietKiem.QuanLy.ViewModels
{
    public class PhieuRutViewModel : INotifyPropertyChanged
    {
        private readonly PhieuRutBLL _bll = new PhieuRutBLL();
        private readonly SoTietKiemBLL _bllSo = new SoTietKiemBLL();

        private SoTietKiem _soGoc = null;
        private LoaiTietKiem _loaiHienTai = null;
        private List<LichSuLaiSuat> _lichSuLaiSuat = null;

        // ── Mã phiếu rút ──────────────────────────────────────────────────────
        private string _maPhieuRutDisplay;
        public string MaPhieuRutDisplay
        {
            get => _maPhieuRutDisplay;
            set { _maPhieuRutDisplay = value; OnPropertyChanged("MaPhieuRutDisplay"); }
        }

        // ── Các trường hiển thị ───────────────────────────────────────────────
        private string _maSo, _tenKH, _soTienRutStr;
        private string _thoiGianRutTien, _quiDinhRutTien, _soTienHienTai, _thoiGianMoSo;
        private DateTime _ngayRut = DateTime.Now;

        public ObservableCollection<string> ListMaSo { get; set; } = new ObservableCollection<string>();

        public string ThoiGianRutTien
        {
            get => _thoiGianRutTien;
            set { _thoiGianRutTien = value; OnPropertyChanged("ThoiGianRutTien"); }
        }
        public string QuiDinhRutTien
        {
            get => _quiDinhRutTien;
            set { _quiDinhRutTien = value; OnPropertyChanged("QuiDinhRutTien"); }
        }
        public string ThoiGianMoSo
        {
            get => _thoiGianMoSo;
            set { _thoiGianMoSo = value; OnPropertyChanged("ThoiGianMoSo"); }
        }
        public string SoTienHienTaiStr
        {
            get => _soTienHienTai;
            set { _soTienHienTai = value; OnPropertyChanged("SoTienHienTaiStr"); }
        }

        // ── MaSo: khi thay đổi, tải thông tin sổ + lịch sử lãi suất ─────────
        public string MaSo
        {
            get => _maSo;
            set
            {
                _maSo = value;
                OnPropertyChanged("MaSo");
                if (!string.IsNullOrEmpty(_maSo))
                {
                    try
                    {
                        var listSo = _bllSo.SearchSoTietKiem(_maSo);
                        if (listSo != null && listSo.Count > 0)
                        {
                            var so = listSo[0];
                            var loai = _bllSo.GetLoaiTietKiemByMaSo(_maSo);
                            if (so != null && loai != null)
                            {
                                TenKH = so.TenKH;
                                ThoiGianRutTien = loai.ThoiGianRutTien.ToString() + " Ngày";
                                QuiDinhRutTien = loai.QuiDinhRutTien == 1 ? "Rút một phần" : "Rút toàn bộ";

                                _soGoc = so;
                                _loaiHienTai = loai;
                                _lichSuLaiSuat = _bllSo.GetLichSuLaiSuat(loai.MaLoaiTietKiem);

                                TinhVaCapNhatSoTienHienCo();
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        public string TenKH
        {
            get => _tenKH;
            set { _tenKH = value; OnPropertyChanged("TenKH"); }
        }
        public string SoTienRutStr
        {
            get => _soTienRutStr;
            set { _soTienRutStr = value; OnPropertyChanged("SoTienRutStr"); }
        }

        public DateTime NgayRut
        {
            get => _ngayRut;
            set
            {
                _ngayRut = value;
                OnPropertyChanged("NgayRut");
                TinhVaCapNhatSoTienHienCo();
            }
        }

        // ── Commands ──────────────────────────────────────────────────────────
        public ICommand LapPhieuCommand { get; }
        public ICommand TraCuuCommand { get; }
        public ICommand ThoatCommand { get; }

        public PhieuRutViewModel()
        {
            LoadData();
            LapPhieuCommand = new RelayCommand(o => HandleLapPhieu());
            TraCuuCommand = new RelayCommand(o => HandleTraCuu());
            ThoatCommand = new RelayCommand(o => (o as Window)?.Close());
        }

        private void LoadData()
        {
            MaPhieuRutDisplay = _bll.GetNextMaPhieuRut();
            var allSo = _bllSo.SearchSoTietKiem("");
            foreach (var s in allSo) ListMaSo.Add(s.MaSo);
        }

        /// <summary>
        /// Tính số tiền hiện có để hiển thị lên UI.
        /// Dùng cùng thuật toán với DAL:
        ///   - Duyệt từng kỳ hạn theo ngayBatDauTinhLai
        ///   - Tra lãi suất tại ngày bắt đầu mỗi kỳ
        ///   - Kỳ chưa đáo hạn (kết thúc sau NgayRut) → không tính
        /// </summary>
        private void TinhVaCapNhatSoTienHienCo()
        {
            if (_soGoc == null || _loaiHienTai == null) return;

            TimeSpan chenhLech = NgayRut.Date - _soGoc.NgayMoSo.Date;
            if (chenhLech.Days < 0)
            {
                ThoiGianMoSo = "Ngày rút không hợp lệ (trước ngày mở sổ)";
                SoTienHienTaiStr = _soGoc.SoTien.ToString("N0") + " VNĐ";
                return;
            }
            ThoiGianMoSo = chenhLech.Days.ToString() + " Ngày";

            // Ngày bắt đầu tính lãi
            DateTime ngayBatDauTinhLai = _soGoc.NgayMoSo;
            if (_soGoc.NgayCapNhatGanNhat != null &&
                _soGoc.NgayCapNhatGanNhat != DateTime.MinValue &&
                _soGoc.NgayCapNhatGanNhat > _soGoc.NgayMoSo)
            {
                ngayBatDauTinhLai = (DateTime)_soGoc.NgayCapNhatGanNhat;
            }

            bool laKhongKyHan = _loaiHienTai.TenLoaiTietKiem.ToLower().Contains("không kỳ hạn");
            int kyHanNgay = laKhongKyHan ? 30 : _loaiHienTai.ThoiGianRutTien;
            decimal kyHanThang = laKhongKyHan ? 1m : (_loaiHienTai.ThoiGianRutTien / 30m);

            decimal tienLai = SoTietKiemBLL.TinhTienLaiTheoPhanKy(
                _soGoc.SoTien,
                ngayBatDauTinhLai,
                NgayRut,
                kyHanNgay,
                kyHanThang,
                _lichSuLaiSuat ?? new List<LichSuLaiSuat>());

            decimal soTienHienCo = _soGoc.SoTien + tienLai;
            SoTienHienTaiStr = soTienHienCo.ToString("N0") + " VNĐ";
        }

        private void HandleTraCuu()
        {
            string keyword = !string.IsNullOrWhiteSpace(TenKH)
                ? TenKH
                : (MaSo != "<Giá trị tự động>" ? MaSo : "");

            var listSo = _bllSo.SearchSoTietKiem(keyword);
            if (listSo != null && listSo.Count > 0)
            {
                var so = listSo[0];
                // Gán MaSo sẽ tự trigger setter → tải lịch sử + tính toán
                MaSo = so.MaSo;
                TenKH = so.TenKH;

                var loaiTK = _bllSo.GetLoaiTietKiemByMaSo(so.MaSo);
                if (loaiTK != null)
                {
                    ThoiGianRutTien = loaiTK.ThoiGianRutTien.ToString() + " ngày";
                    QuiDinhRutTien = loaiTK.QuiDinhRutTien == 1 ? "Rút một phần" : "Rút toàn bộ";

                    _soGoc = so;
                    _loaiHienTai = loaiTK;
                    _lichSuLaiSuat = _bllSo.GetLichSuLaiSuat(loaiTK.MaLoaiTietKiem);

                    TinhVaCapNhatSoTienHienCo();
                }
            }
            else
            {
                MessageBox.Show("Không tìm thấy sổ!");
            }
        }

        private void HandleLapPhieu()
        {
            if (string.IsNullOrEmpty(MaSo) || string.IsNullOrEmpty(TenKH))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin mã sổ và khách hàng!");
                return;
            }
            if (!decimal.TryParse(SoTienRutStr, out decimal money))
            {
                MessageBox.Show("Số tiền rút không hợp lệ!");
                return;
            }

            var phieu = new PhieuRut
            {
                MaPhieuRut = MaPhieuRutDisplay,
                MaSo = MaSo,
                NgayRut = NgayRut,
                SoTienRut = money
            };

            string res = _bll.ValidateAndSavePhieuRut(phieu, TenKH);
            if (res == "SUCCESS")
            {
                MessageBox.Show($"Lập phiếu rút thành công! Mã phiếu: {phieu.MaPhieuRut}");

                _soGoc = null;
                _loaiHienTai = null;
                _lichSuLaiSuat = null;

                MaSo = "";
                TenKH = "";
                SoTienRutStr = "";
                ThoiGianMoSo = "";
                SoTienHienTaiStr = "";
                MaPhieuRutDisplay = _bll.GetNextMaPhieuRut();
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