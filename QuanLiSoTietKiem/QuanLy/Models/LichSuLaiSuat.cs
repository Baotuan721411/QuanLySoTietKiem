using System;

namespace QuanLiSoTietKiem.QuanLy.Models
{
    public class LichSuLaiSuat
    {
        public int MaLichSuLaiSuat { get; set; }

        public int MaLoaiTietKiem { get; set; }

        public decimal LaiSuatCuaKyHan { get; set; }

        public DateTime NgayApDung { get; set; }

        public DateTime? NgayKetThuc { get; set; }
    }
}