using System;

namespace QuanLiSoTietKiem.QuanLy.ViewModels
{
    public class SoTietKiemDisplayModel
    {
        public int STT { get; set; }
        public string MaSo { get; set; }
        public string TenKH { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string CCCD { get; set; }
        public string DiaChi { get; set; }
        public DateTime? NgayMoSo { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public decimal SoTien { get; set; }
        public decimal SoDuToiThieu { get; set; }

        public string TenLoaiTietKiem { get; set; }

        public string TrangThai { get; set; }

        public string QuyDinhThoiGianRut { get; set; }
        public string QuyDinhRutTien { get; set; }

        public string LaiSuat { get; set; }
    }
}