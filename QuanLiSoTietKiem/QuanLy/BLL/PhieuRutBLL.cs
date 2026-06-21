using QuanLiSoTietKiem.QuanLy.DAL;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.Collections.Generic;

namespace QuanLiSoTietKiem.QuanLy.BLL
{
    public class PhieuRutBLL
    {
        private readonly SoTietKiemDataProcessing _db = new SoTietKiemDataProcessing();
        private readonly PhieuRutDataProcessing _dbRut = new PhieuRutDataProcessing();
        private readonly LoaiTietKiemDataProcessing _ltk = new LoaiTietKiemDataProcessing();
        private readonly LichSuLaiSuatDataProcessing _ls = new LichSuLaiSuatDataProcessing();
        public string GetNextMaPhieuRut() => _dbRut.GetNextMaPhieuRut();

        public string ValidateAndSavePhieuRut(PhieuRut phieu, string tenKH)
        {
            // ── 1. Kiểm tra sổ tồn tại và tên khách hàng ─────────────────────
            var listSo = _db.SearchSoTietKiem(phieu.MaSo);
            if (listSo == null || listSo.Count == 0)
                return "Mã sổ tiết kiệm không tồn tại trên hệ thống!";

            SoTietKiem stk = listSo[0];

            if (string.IsNullOrWhiteSpace(tenKH) ||
                !string.Equals(stk.TenKH.Trim(), tenKH.Trim(), StringComparison.OrdinalIgnoreCase))
                return "Họ tên khách hàng không khớp với chủ sở hữu sổ tiết kiệm này!";

            // ── 2. Đọc thông tin loại tiết kiệm ──────────────────────────────
            LoaiTietKiem loai = _ltk.GetLoaiTietKiemByMaSo(phieu.MaSo);
            if (loai == null)
                return "Lỗi hệ thống: Không tìm thấy loại tiết kiệm!";

            // ── 3. Kiểm tra thời gian tối thiểu được phép rút ────────────────
            int soNgayTuLucMoSo = (phieu.NgayRut.Date - stk.NgayMoSo.Date).Days;
            if (soNgayTuLucMoSo < loai.ThoiGianRutTien)
                return $"Thất bại: Sổ mới mở được {soNgayTuLucMoSo} ngày. " +
                       $"Theo quy định của loại sổ này, phải sau ít nhất {loai.ThoiGianRutTien} ngày " +
                       $"kể từ ngày mở sổ ({stk.NgayMoSo:dd/MM/yyyy}) mới được phép rút tiền!";

            // ── 4. Ngày bắt đầu tính lãi ─────────────────────────────────────
            //       = NgayMoSo nếu chưa rút lần nào
            //       = NgayCapNhatGanNhat nếu đã rút một phần trước đó
            //       (SearchSoTietKiem trả về DateTime.MinValue khi NgayCapNhatGanNhat là NULL)
            DateTime ngayCapNhat = stk.NgayCapNhatGanNhat.HasValue
                                       ? stk.NgayCapNhatGanNhat.Value
                                       : DateTime.MinValue;
            DateTime ngayBatDauTinhLai =
                (ngayCapNhat != DateTime.MinValue && ngayCapNhat > stk.NgayMoSo)
                    ? ngayCapNhat
                    : stk.NgayMoSo;

            // ── 5. Lấy lịch sử lãi suất ──────────────────────────────────────
            List<LichSuLaiSuat> lichSu = _ls.GetLichSuLaiSuat(stk.MaLoaiTietKiem);
            if (lichSu == null || lichSu.Count == 0)
                return "Lỗi hệ thống: Không tìm thấy lịch sử lãi suất cho loại tiết kiệm này!";

            // ── 6. Thông số kỳ hạn ────────────────────────────────────────────
            bool laKhongKyHan = loai.TenLoaiTietKiem.ToLower().Contains("không kỳ hạn");
            int kyHanNgay = laKhongKyHan ? 30 : loai.ThoiGianRutTien;
            decimal kyHanThang = laKhongKyHan ? 1m : (loai.ThoiGianRutTien / 30m);

            // ── 7. Tính lãi — gọi hàm dùng chung ở SoTietKiemBLL ────────────
            decimal tienLai = SoTietKiemBLL.TinhTienLaiTheoPhanKy(
                stk.SoTien,
                ngayBatDauTinhLai,
                phieu.NgayRut,
                kyHanNgay,
                kyHanThang,
                lichSu);

            decimal soTienHienCo = stk.SoTien + tienLai;
            decimal soTienConLai;

            // ── 8. Kiểm tra số tiền rút ───────────────────────────────────────
            if (laKhongKyHan)
            {
                if (phieu.SoTienRut > soTienHienCo)
                    return $"Thất bại: Số tiền yêu cầu rút ({phieu.SoTienRut:N0} VNĐ) " +
                           $"vượt quá số tiền hiện có trong sổ ({soTienHienCo:N0} VNĐ)!";

                soTienConLai = soTienHienCo - phieu.SoTienRut;
            }
            else
            {
                if (phieu.SoTienRut != soTienHienCo)
                    return $"Thất bại: Sổ tiết kiệm có kỳ hạn bắt buộc phải rút TOÀN BỘ " +
                           $"số tiền hiện có! Số tiền nhập vào phải chính xác là: {soTienHienCo:N0} VNĐ";

                soTienConLai = 0;
            }

            bool trangThaiMoi = soTienConLai > stk.SoDuToiThieu;

            // ── 9. Ghi DB ─────────────────────────────────────────────────────
            return _dbRut.SavePhieuRutTien(phieu, soTienConLai, trangThaiMoi);
        }
    }
}