using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using QuanLiSoTietKiem.QuanLy.BLL;
using QuanLiSoTietKiem.QuanLy.Helpers;
using QuanLiSoTietKiem.QuanLy.Models;

namespace QuanLiSoTietKiem.QuanLy.ViewModels
{
    public class BaoCaoDoanhThuViewModel : INotifyPropertyChanged
    {
        private readonly SoTietKiemBLL _bll = new SoTietKiemBLL();

        // ============================================================
        // INPUT
        // ============================================================

        private string _namBaoCao;
        public string NamBaoCao
        {
            get => _namBaoCao;
            set { _namBaoCao = value; OnPropertyChanged(nameof(NamBaoCao)); }
        }

        private string _thangBaoCao;
        public string ThangBaoCao
        {
            get => _thangBaoCao;
            set { _thangBaoCao = value; OnPropertyChanged(nameof(ThangBaoCao)); }
        }

        // ============================================================
        // OUTPUT
        // ============================================================

        public ObservableCollection<BaoCaoDoanhThuModel> ListBaoCao { get; }
            = new ObservableCollection<BaoCaoDoanhThuModel>();

        // ============================================================
        // COMMANDS
        // ============================================================

        public ICommand LapBaoCaoCommand { get; }
        public ICommand ThoatCommand { get; }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public BaoCaoDoanhThuViewModel()
        {
            LapBaoCaoCommand = new RelayCommand(_ => ExecuteLapBaoCao());
            NamBaoCao = DateTime.Now.Year.ToString();
            ThangBaoCao = DateTime.Now.Month.ToString();
            ThoatCommand = new RelayCommand(param =>
            {
                if (param is Window w) w.Close();
            });
        }

        private void ExecuteLapBaoCao()
        {
            // Bước 01: Nhận D1 – tháng và năm từ người dùng
            if (!int.TryParse(NamBaoCao, out int nam) || nam < 0)
            {
                MessageBox.Show("Vui lòng nhập năm hợp lệ.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(ThangBaoCao, out int thang) || thang < 1 || thang > 12)
            {
                MessageBox.Show("Vui lòng nhập tháng hợp lệ (1 – 12).",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            XoaKetQua();

            try
            {
                var dsLoai = _bll.GetLoaiTietKiems();

                int stt = 1;

                foreach (var loai in dsLoai)
                {
                    // Bước 03: Đọc D3 – danh sách Sổ tiết kiệm theo Mã loại tiết kiệm
                    var dsSo = _bll.GetSoTietKiemByLoai(loai.MaLoaiTietKiem);

                    decimal tongThu = 0;
                    decimal tongChi = 0;

                    foreach (var so in dsSo)
                    {
                        // Bước 04: Đọc D4 – Phiếu gởi trong tháng/năm theo Mã sổ
                        var dsGoi = _bll.GetPhieuGoiTheoThangNam(so.MaSo, thang, nam);
                        foreach (var pg in dsGoi)
                            tongThu += pg.SoTienGoi;

                        // Bước 05: Đọc D5 – Phiếu rút trong tháng/năm theo Mã sổ
                        var dsRut = _bll.GetPhieuRutTheoThangNam(so.MaSo, thang, nam);
                        foreach (var pr in dsRut)
                            tongChi += pr.SoTienRut;
                    }


                    ListBaoCao.Add(new BaoCaoDoanhThuModel
                    {
                        STT = stt++,
                        TenLoaiTietKiem = loai.TenLoaiTietKiem,
                        TongThu = tongThu,   // Bước 06
                        TongChi = tongChi,   // Bước 07
                    });
                }

                // Bước 09: Kết thúc
                if (ListBaoCao.Count == 0)
                    MessageBox.Show($"Không có giao dịch nào trong tháng {thang}/{nam}.",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lập báo cáo: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void XoaKetQua()
        {
            ListBaoCao.Clear();
        }

        // ============================================================
        // INotifyPropertyChanged
        // ============================================================

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}