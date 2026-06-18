using System;

namespace QuanLiSoTietKiem.QuanLy.Models
{
    /// <summary>
    /// Bộ lọc tìm kiếm nâng cao cho màn hình Tra cứu sổ tiết kiệm.
    /// Tất cả field đều nullable/optional – bỏ trống = bỏ qua điều kiện đó.
    /// </summary>
    public class SearchFilter
    {
        // Row 1 – Thông tin cơ bản
        public string MaSo { get; set; }
        public string TenKH { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string CCCD { get; set; }
        public string DiaChi { get; set; }
        public string TenLoaiTietKiem { get; set; }
        public string TrangThai { get; set; }

        // Row 2 – Khoảng ngày
        public DateTime? NgayMoSoTu { get; set; }
        public DateTime? NgayMoSoDen { get; set; }
        public DateTime? NgayCapNhatTu { get; set; }
        public DateTime? NgayCapNhatDen { get; set; }

        // Row 3 – Khoảng tiền
        public decimal? SoTienTu { get; set; }
        public decimal? SoTienDen { get; set; }
        public decimal? SoDuToiThieuTu { get; set; }
        public decimal? SoDuToiThieuDen { get; set; }

        // Row 4 – Quy định loại tiết kiệm
        public string QuyDinhThoiGian { get; set; }
        public string QuyDinhRutTien { get; set; }
        public string LaiSuat { get; set; }

        // Row 5 – Mã phiếu
        public string MaPhieuGoi { get; set; }
        public string MaPhieuRut { get; set; }

        // Row 6 – Khoảng ngày gởi / rút
        public DateTime? NgayGoiTu { get; set; }
        public DateTime? NgayGoiDen { get; set; }
        public DateTime? NgayRutTu { get; set; }
        public DateTime? NgayRutDen { get; set; }

        // Row 7 – Khoảng tiền gởi / rút
        public decimal? SoTienGoiTu { get; set; }
        public decimal? SoTienGoiDen { get; set; }
        public decimal? SoTienRutTu { get; set; }
        public decimal? SoTienRutDen { get; set; }
    }
}