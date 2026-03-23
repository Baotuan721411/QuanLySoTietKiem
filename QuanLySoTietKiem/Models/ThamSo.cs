namespace QuanLySoTietKiem.Models
{
    /// <summary>
    /// Tham số hệ thống — system configuration parameters (minimum deposit, deposit multiple, minimum age).
    /// </summary>
    public class ThamSo
    {
        public decimal SoTienToiThieu { get; set; }
        public decimal BoiSoTienGui { get; set; }
        public int DoTuoiToiThieu { get; set; }
    }
}
