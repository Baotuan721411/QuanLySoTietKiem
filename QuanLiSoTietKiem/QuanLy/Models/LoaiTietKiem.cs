namespace QuanLiSoTietKiem.QuanLy.Models
{
    public class LoaiTietKiem
    {
        public int MaLoaiTietKiem { get; set; }

        public string TenLoaiTietKiem { get; set; }
        public int ThoiGianRutTien { get; set; }
        public int QuiDinhRutTien { get; set; }
        public decimal TienGoiToiThieu { get; set; }
        public decimal LaiSuat { get; set; }
        public string QuiDinhRutTienText => QuiDinhRutTien == 1 ? "Rút một phần" : "Rút toàn bộ";
    }
}