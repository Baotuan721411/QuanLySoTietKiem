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
    public class BaoCaoMoDongSoViewModel : INotifyPropertyChanged
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

        private LoaiTietKiem _selectedLoai;
        public LoaiTietKiem SelectedLoai
        {
            get => _selectedLoai;
            set { _selectedLoai = value; OnPropertyChanged(nameof(SelectedLoai)); }
        }

        public ObservableCollection<LoaiTietKiem> ListLoaiTK { get; }
            = new ObservableCollection<LoaiTietKiem>();

        // ============================================================
        // OUTPUT
        // ============================================================

        public ObservableCollection<BaoCaoMoDongSoModel> ListBaoCao { get; }
            = new ObservableCollection<BaoCaoMoDongSoModel>();

        // ============================================================
        // COMMANDS
        // ============================================================

        public ICommand LapBaoCaoCommand { get; }
        public ICommand ThoatCommand { get; }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public BaoCaoMoDongSoViewModel()
        {
            // Nạp danh sách loại tiết kiệm cho ComboBox
            foreach (var loai in _bll.GetLoaiTietKiems())
                ListLoaiTK.Add(loai);
            NamBaoCao = DateTime.Now.Year.ToString();
            ThangBaoCao = DateTime.Now.Month.ToString();
            LapBaoCaoCommand = new RelayCommand(_ => ExecuteLapBaoCao());
            ThoatCommand = new RelayCommand(param =>
            {
                if (param is Window w) w.Close();
            });
        }

        // ============================================================
        // LOGIC
        // ============================================================

        private void ExecuteLapBaoCao()
        {
            // Bước 01: Nhận D1 – Năm, Tháng và Loại tiết kiệm từ người dùng
            if (!int.TryParse(NamBaoCao, out int nam) || nam < 1900)
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

            if (SelectedLoai == null)
            {
                MessageBox.Show("Vui lòng chọn loại tiết kiệm.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ListBaoCao.Clear();

            try
            {
                // Bước 02: Lấy Mã loại tiết kiệm từ lựa chọn của người dùng
                int maLoai = SelectedLoai.MaLoaiTietKiem;

                // Bước 03: Xác định danh sách ngày từ ngày 1 đến ngày cuối tháng
                int soNgayTrongThang = DateTime.DaysInMonth(nam, thang);

                int stt = 1;
                for (int ngay = 1; ngay <= soNgayTrongThang; ngay++)
                {
                    var ngayN = new DateTime(nam, thang, ngay);

                    // Bước 04: Đếm số sổ mở trong ngày N theo loại tiết kiệm
                    int soMo = _bll.DemSoMoTheoNgayVaLoai(ngayN, maLoai);

                    // Bước 05: Đếm số sổ đóng trong ngày N theo loại tiết kiệm
                    //          (TrangThai = 0 và NgayCapNhatGanNhat = ngayN)
                    int soDong = _bll.DemSoDongTheoNgayVaLoai(ngayN, maLoai);

                    // Bước 06: Tính chênh lệch (được tính tự động qua property ChenhLech)
                    ListBaoCao.Add(new BaoCaoMoDongSoModel
                    {
                        STT    = stt++,
                        Ngay   = ngayN.ToString("dd/MM/yyyy"),
                        SoMo   = soMo,
                        SoDong = soDong,
                    });
                }

                // Bước 07: Kết thúc
                if (ListBaoCao.Count == 0)
                    MessageBox.Show($"Không có dữ liệu trong tháng {thang}/{nam}.",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lập báo cáo: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================================================
        // INotifyPropertyChanged
        // ============================================================

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
