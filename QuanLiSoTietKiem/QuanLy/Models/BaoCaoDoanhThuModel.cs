using System;

namespace QuanLiSoTietKiem.QuanLy.Models
{
    public class BaoCaoDoanhThuModel
    {
        public int STT { get; set; }
        public string TenLoaiTietKiem { get; set; }
        public decimal TongChi { get; set; }
        public decimal TongThu { get; set; }
        public decimal ChenhLech => TongThu - TongChi;
        public bool ChenhLechAm => ChenhLech < 0;
    }
}