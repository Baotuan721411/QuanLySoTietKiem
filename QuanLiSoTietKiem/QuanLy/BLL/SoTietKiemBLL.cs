using QuanLiSoTietKiem.QuanLy.DAL;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.Collections.Generic;

namespace QuanLiSoTietKiem.QuanLy.BLL
{
    public class SoTietKiemBLL
    {
        private readonly SoTietKiemDataProcessing _db = new SoTietKiemDataProcessing();
        private readonly LoaiTietKiemDataProcessing _ltk = new LoaiTietKiemDataProcessing();
        private readonly LichSuLaiSuatDataProcessing _ls = new LichSuLaiSuatDataProcessing();
        private readonly BaoCaoMoDongSoDataProcessing _bcmodong = new BaoCaoMoDongSoDataProcessing();
        private readonly BaoCaoDoanhThuDataProcessing _bcdoanhthu = new BaoCaoDoanhThuDataProcessing();
        public List<LoaiTietKiem> GetLoaiTietKiems() => _ltk.GetLoaiTietKiems(); 

        public List<SoTietKiem> SearchSoTietKiem(string keyword) => _db.SearchSoTietKiem(keyword);

        public List<SoTietKiem> SearchSoTietKiemAdvanced(SearchFilter filter)
            => _db.SearchSoTietKiemAdvanced(filter);

        public bool DeleteSoTietKiem(string maSo) => _db.DeleteSoTietKiem(maSo);

        public string GetNextMaso() => _db.GetNextMaSo();

        public ThamSo GetThamSo() => _db.GetThamSo();

        public LoaiTietKiem GetLoaiTietKiemByMaSo(string maSo) => _ltk.GetLoaiTietKiemByMaSo(maSo);
        public List<LichSuLaiSuat> GetLichSuLaiSuat(int maLoaiTietKiem)
            => _ls.GetLichSuLaiSuat(maLoaiTietKiem);

        private static decimal TraLaiSuatTaiNgay(DateTime ngay, List<LichSuLaiSuat> lichSu)
        {
            foreach (var gs in lichSu)
            {
                bool batDauHopLe = gs.NgayApDung.Date <= ngay.Date;
                bool ketThucHopLe = !gs.NgayKetThuc.HasValue
                                    || gs.NgayKetThuc.Value.Date >= ngay.Date;

                if (batDauHopLe && ketThucHopLe)
                    return gs.LaiSuatCuaKyHan;
            }
            return 0;
        }

        /// <summary>
        /// Tính tổng tiền lãi theo quy tắc:
        ///   "Lãi suất áp dụng cho một kỳ hạn = lãi suất đang có hiệu lực
        ///    vào NGÀY BẮT ĐẦU của kỳ hạn đó."
        ///
        /// Thuật toán — duyệt từng kỳ hạn:
        ///   kyBatDau = ngayBatDau
        ///   Lặp:
        ///     kyKetThuc = kyBatDau + kyHanNgay
        ///     Nếu kyKetThuc > ngayRut → kỳ này chưa đáo hạn → dừng, không tính lãi
        ///     Tra lãi suất tại kyBatDau
        ///     laiKy = soTienGoc × (laiSuat / 100) × kyHanThang
        ///     tongLai += laiKy
        ///     kyBatDau = kyKetThuc   (chuyển sang kỳ tiếp theo)
        ///
        /// </summary>
        public static decimal TinhTienLaiTheoPhanKy(
            decimal soTienGoc,
            DateTime ngayBatDau,
            DateTime ngayRut,
            int kyHanNgay,
            decimal kyHanThang,
            List<LichSuLaiSuat> lichSu)
        {
            if (lichSu == null || lichSu.Count == 0) return 0;
            if (ngayRut.Date <= ngayBatDau.Date) return 0;

            decimal tongLai = 0;
            DateTime kyBatDau = ngayBatDau.Date;

            while (true)
            {
                DateTime kyKetThuc = kyBatDau.AddDays(kyHanNgay);

                // Kỳ này chưa đáo hạn (kết thúc sau ngày rút) → dừng, không tính lãi
                if (kyKetThuc > ngayRut.Date) break;

                // Tra lãi suất tại ngày BẮT ĐẦU của kỳ này
                decimal laiSuat = TraLaiSuatTaiNgay(kyBatDau, lichSu);

                // Tính lãi cho kỳ này
                tongLai += soTienGoc * (laiSuat / 100m) * kyHanThang;

                // Chuyển sang kỳ tiếp theo
                kyBatDau = kyKetThuc;
            }

            return Math.Round(tongLai, 0);
        }

        // Tiếp nhận (thêm mới)
        public string ValidateAndSubmit(SoTietKiem so)
        {
            string checkResult = CheckBusinessRules(so);
            if (checkResult != "SUCCESS") return checkResult;
            return _db.SaveSoTietKiem(so) ? "SUCCESS" : "Lỗi lưu Database!";
        }

        // Cập nhật sổ hiện có
        public string ValidateAndUpdate(SoTietKiem so)
        {
            string checkResult = CheckBusinessRules(so);
            if (checkResult != "SUCCESS") return checkResult;
            return _db.UpdateSoTietKiem(so) ? "SUCCESS" : "Lỗi cập nhật Database!";
        }

        // Kiểm tra quy định tuổi tại ngày mở sổ và tiền gửi
        private string CheckBusinessRules(SoTietKiem so)
        {
            var ts = _db.GetThamSo();
            if (ts == null) return "Lỗi hệ thống: Không thể lấy tham số!";
            var loai = _ltk.GetLoaiTietKiemById(so.MaLoaiTietKiem);
            if (loai == null) return "Lỗi hệ thống: Không tìm thấy loại tiết kiệm!";

            int tuoi = so.NgayMoSo.Year - (so.NgaySinh?.Year ?? so.NgayMoSo.Year);
            if (so.NgaySinh?.Date > so.NgayMoSo.AddYears(-tuoi)) tuoi--;

            if (tuoi < ts.DoTuoiToiThieu)
                return $"Tại ngày mở sổ ({so.NgayMoSo:dd/MM/yyyy}), khách hàng mới {tuoi} tuổi, chưa đủ {ts.DoTuoiToiThieu} tuổi!";

            if (so.SoTien < loai.TienGoiToiThieu)
                return $"Số tiền tối thiểu cho loại \"{loai.TenLoaiTietKiem}\": {loai.TienGoiToiThieu:N0} VNĐ";

            if (so.SoTien % ts.BoiSoTienGui != 0)
                return $"Tiền gửi phải là bội số của {ts.BoiSoTienGui:N0} VNĐ";

            return "SUCCESS";
        }

        public List<BaoCaoDoanhThuModel> GetBaoCaoDoanhThuTheoThang(int nam, int thang)
            => _bcdoanhthu.GetBaoCaoDoanhThuTheoThang(nam, thang);

        public List<SoTietKiem> GetSoTietKiemByLoai(int maLoaiTietKiem)
            => _db.GetSoTietKiemByLoai(maLoaiTietKiem);

        public List<PhieuGoi> GetPhieuGoiTheoThangNam(string maSo, int thang, int nam)
            => _bcdoanhthu.GetPhieuGoiTheoThangNam(maSo, thang, nam);

        public List<PhieuRut> GetPhieuRutTheoThangNam(string maSo, int thang, int nam)
            => _bcdoanhthu.GetPhieuRutTheoThangNam(maSo, thang, nam);

        public int DemSoMoTheoNgayVaLoai(DateTime ngayN, int maLoai)
            => _bcmodong.DemSoMoTheoNgayVaLoai(ngayN, maLoai);

        public int DemSoDongTheoNgayVaLoai(DateTime ngayN, int maLoai)
            => _bcmodong.DemSoDongTheoNgayVaLoai(ngayN, maLoai);
    }
}