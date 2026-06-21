using QuanLiSoTietKiem.QuanLy.DAL;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.Collections.Generic;

namespace QuanLiSoTietKiem.QuanLy.BLL
{
    /// <summary>
    /// BLL cho phiếu gởi — chịu trách nhiệm toàn bộ logic nghiệp vụ:
    /// validate, tính lãi tích lũy, rồi mới gọi DAL để lưu.
    /// </summary>
    public class PhieuGoiBLL
    {
        private readonly SoTietKiemDataProcessing _db = new SoTietKiemDataProcessing();
        private readonly PhieuGoiDataProcessing _dbGoi = new PhieuGoiDataProcessing();

        public string GetNextMaPhieuGoi() => _dbGoi.GetNextMaPhieuGoi();

        /// <summary>
        /// Validate và lưu phiếu gởi tiền thêm.
        /// Luồng xử lý:
        ///   1. Kiểm tra sổ tồn tại và tên KH khớp
        ///   2. Kiểm tra loại tiết kiệm được phép gởi thêm
        ///   3. Kiểm tra số tiền gởi tối thiểu
        ///   4. Tính lãi tích lũy từ kỳ trước đến ngày gởi (theo từng giai đoạn lãi suất)
        ///   5. Gọi DAL lưu phiếu + cộng (tiền gởi mới + lãi) vào sổ
        /// </summary>
        public string ValidateAndSavePhieuGoi(PhieuGoi phieu, string tenKHNhapVao)
        {
            // ── 1. Kiểm tra sổ và tên KH ─────────────────────────────────────
            var listSo = _db.SearchSoTietKiem(phieu.MaSo);
            if (listSo == null || listSo.Count == 0)
                return "Mã sổ tiết kiệm không tồn tại!";

            var soGoc = listSo[0];

            if (string.IsNullOrWhiteSpace(tenKHNhapVao) ||
                !string.Equals(soGoc.TenKH.Trim(), tenKHNhapVao.Trim(), StringComparison.OrdinalIgnoreCase))
                return "Họ tên khách hàng không khớp với chủ sổ tiết kiệm!";

            // ── 2. Kiểm tra loại tiết kiệm được phép gởi thêm ────────────────
            var ts = _db.GetThamSo();
            if (ts == null) return "Không tìm thấy tham số quy định trong hệ thống!";

            var loaiCuaSo = _db.GetLoaiTietKiemById(soGoc.MaLoaiTietKiem);
            if (loaiCuaSo == null) return "Không tìm thấy loại tiết kiệm của sổ này!";

            if (loaiCuaSo.TenLoaiTietKiem != ts.LoaiTietKiemGoi)
                return $"Quy định: Chỉ loại tiết kiệm '{ts.LoaiTietKiemGoi}' mới được phép gởi thêm tiền!";

            // ── 3. Kiểm tra số tiền gởi tối thiểu ────────────────────────────
            if (phieu.SoTienGoi < ts.SoTienGoiThemToiThieu)
                return $"Số tiền gởi thêm tối thiểu phải là {ts.SoTienGoiThemToiThieu:N0} VNĐ!";

            // ── 4. Tính lãi tích lũy từ kỳ trước đến ngày gởi ────────────────
            decimal tienLai = TinhLaiTichLuy(soGoc, loaiCuaSo, phieu.NgayGoi);

            // ── 5. Lưu phiếu + cộng (tiền gởi + lãi) vào sổ ─────────────────
            return _dbGoi.SavePhieuGoiTien(phieu, tienLai)
                ? "SUCCESS"
                : "Lỗi hệ thống khi lưu phiếu gởi tiền!";
        }

        /// <summary>
        /// Tính lãi tích lũy từ ngày bắt đầu tính lãi đến ngày gởi thêm.
        /// Dùng cùng thuật toán với PhieuRut:
        ///   lãi suất của kỳ = lãi suất tại ngày BẮT ĐẦU kỳ đó.
        ///   Kỳ chưa đáo hạn (kết thúc sau ngayGoi) → không tính.
        /// </summary>
        private decimal TinhLaiTichLuy(SoTietKiem so, LoaiTietKiem loai, DateTime ngayGoi)
        {
            // Ngày bắt đầu tính lãi
            DateTime ngayBatDau = (so.NgayCapNhatGanNhat != null
                                   && so.NgayCapNhatGanNhat != DateTime.MinValue
                                   && so.NgayCapNhatGanNhat > so.NgayMoSo)
                                      ? (DateTime)so.NgayCapNhatGanNhat
                                      : so.NgayMoSo;

            // Kỳ hạn — không kỳ hạn dùng 30 ngày/kỳ
            bool laKhongKyHan = loai.TenLoaiTietKiem.ToLower().Contains("không kỳ hạn");
            int kyHanNgay = laKhongKyHan ? 30 : loai.ThoiGianRutTien;
            decimal kyHanThang = laKhongKyHan ? 1m : (loai.ThoiGianRutTien / 30m);

            // Lấy lịch sử lãi suất
            List<LichSuLaiSuat> lichSu = _db.GetLichSuLaiSuat(loai.MaLoaiTietKiem);

            // Dùng hàm tính lãi chung từ SoTietKiemBLL
            return SoTietKiemBLL.TinhTienLaiTheoPhanKy(
                so.SoTien,
                ngayBatDau,
                ngayGoi,
                kyHanNgay,
                kyHanThang,
                lichSu);
        }

        /// <summary>
        /// Tự động tạo phiếu gởi khi mở sổ mới.
        /// NgayGoi = NgayMoSo, SoTienGoi = SoTien ban đầu.
        /// Không tính lãi vì sổ vừa mở.
        /// </summary>
        public bool TaoPhieuGoiKhiMoSo(SoTietKiem so)
        {
            var phieu = new PhieuGoi
            {
                MaPhieuGoi = _dbGoi.GetNextMaPhieuGoi(),
                MaSo = so.MaSo,
                NgayGoi = so.NgayMoSo,
                SoTienGoi = so.SoTien
            };
            return _dbGoi.SavePhieuGoiKhiMoSo(phieu);
        }

        /// <summary>
        /// Tự động xóa phiếu gởi khi xóa sổ.
        /// </summary>
        public bool XoaPhieuGoiKhiXoaSo(string maSo, DateTime ngayMoSo)
            => _dbGoi.DeletePhieuGoiBangMaSoVaNgay(maSo, ngayMoSo);

        /// <summary>
        /// Tự động cập nhật phiếu gởi khi cập nhật sổ.
        /// </summary>
        public bool CapNhatPhieuGoiKhiCapNhatSo(SoTietKiem soMoi, DateTime ngayMoSoCu)
            => _dbGoi.UpdatePhieuGoiBangMaSoVaNgay(soMoi.MaSo, ngayMoSoCu, soMoi.NgayMoSo, soMoi.SoTien);
    }
}