using System;
namespace QuanLiSoTietKiem.QuanLy.Models
{
    public class SoTietKiem
    {
        public string MaSo { get; set; }
        public int MaLoaiTietKiem { get; set; }
        public string TenKH { get; set; }
        public decimal SoTien { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string CCCD { get; set; }
        public DateTime NgayMoSo { get; set; }
        public string DiaChi { get; set; }
    }
}