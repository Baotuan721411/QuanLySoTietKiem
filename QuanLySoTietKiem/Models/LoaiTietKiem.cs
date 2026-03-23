namespace QuanLySoTietKiem.Models
{
    /// <summary>
    /// Loại tiết kiệm — savings account type (e.g. "Không kỳ hạn", "6 tháng").
    /// </summary>
    public class LoaiTietKiem
    {
        public int MaLoaiTietKiem { get; set; }
        public string TenLoaiTietKiem { get; set; }
    }
}
