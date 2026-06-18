namespace QuanLiSoTietKiem.QuanLy.Models
{
    public class BaoCaoMoDongSoModel
    {
        public int STT { get; set; }

        public string Ngay { get; set; }

        public int SoMo { get; set; }

        public int SoDong { get; set; }

        public int ChenhLech => SoMo - SoDong;

        public bool ChenhLechAm => ChenhLech < 0;
    }
}
