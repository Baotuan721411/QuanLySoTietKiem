using System;

namespace QuanLySoTietKiem.Models
{
    /// <summary>
    /// Sổ tiết kiệm — represents a savings account record.
    /// </summary>
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

        /// <summary>
        /// Tên loại tiết kiệm (chỉ dùng để hiển thị, không lưu DB).
        /// </summary>
        public string TenLoaiTietKiem { get; set; }
    }
}
