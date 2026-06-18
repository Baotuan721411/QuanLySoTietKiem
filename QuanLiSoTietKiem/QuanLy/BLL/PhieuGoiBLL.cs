using QuanLiSoTietKiem.QuanLy.DAL;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.Collections.Generic;

namespace QuanLiSoTietKiem.QuanLy.BLL
{
    public class PhieuGoiBLL
    {
        private readonly SoTietKiemDataProcessing _db = new SoTietKiemDataProcessing();
        private readonly PhieuGoiDataProcessing _dbGoi = new PhieuGoiDataProcessing();

        public string GetNextMaPhieuGoi() => _dbGoi.GetNextMaPhieuGoi();
        /// <summary>
        /// Hàm kiểm tra khi lập phiếu gởi
        /// </summary>
        public string LapPhieuGoi(PhieuGoi phieu, string loaiTietKiemCuaSo)
        {
            var ts = _db.GetThamSo();
            if (loaiTietKiemCuaSo != ts.LoaiTietKiemGoi)
                return $"Chỉ loại tiết kiệm '{ts.LoaiTietKiemGoi}' mới được phép gửi thêm tiền!";

            if (phieu.SoTienGoi < ts.SoTienGoiThemToiThieu)
                return $"Số tiền gửi thêm tối thiểu phải là {ts.SoTienGoiThemToiThieu:N0} VNĐ";

            return _dbGoi.SavePhieuGoiTien(phieu) ? "SUCCESS" : "Lỗi hệ thống";
        }

        public string ValidateAndSavePhieuGoi(PhieuGoi phieu, string tenKHNhapVao)
        {
            var listSo = _db.SearchSoTietKiem(phieu.MaSo);
            if (listSo == null || listSo.Count == 0)
                return "Mã sổ tiết kiệm không tồn tại!";

            var soGoc = listSo[0];

            if (string.IsNullOrWhiteSpace(tenKHNhapVao) ||
                !string.Equals(soGoc.TenKH.Trim(), tenKHNhapVao.Trim(), StringComparison.OrdinalIgnoreCase))
                return "Họ tên khách hàng không khớp với chủ sổ tiết kiệm!";

            var ts = _db.GetThamSo();
            if (ts == null) return "Không tìm thấy tham số quy định trong hệ thống!";

            var danhSachLoai = _db.GetLoaiTietKiems();
            var loaiCuaSoNay = danhSachLoai.Find(x => x.MaLoaiTietKiem == soGoc.MaLoaiTietKiem);

            if (loaiCuaSoNay == null || loaiCuaSoNay.TenLoaiTietKiem != ts.LoaiTietKiemGoi)
                return $"Quy định: Chỉ loại tiết kiệm '{ts.LoaiTietKiemGoi}' mới được phép gởi thêm tiền!";

            if (phieu.SoTienGoi < ts.SoTienGoiThemToiThieu)
                return $"Số tiền gởi thêm tối thiểu phải là {ts.SoTienGoiThemToiThieu:N0} VNĐ!";

            return _dbGoi.SavePhieuGoiTien(phieu) ? "SUCCESS" : "Lỗi hệ thống khi lưu phiếu gởi tiền!";
        }

        /// <summary>
        /// Tự động tạo phiếu gởi khi mở sổ mới thành công.
        /// NgayGoi = NgayMoSo, SoTienGoi = SoTien, MaSo = MaSo của sổ.
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
        /// Tự động xóa phiếu gởi có cùng MaSo và NgayGoi = NgayMoSo khi xóa sổ.
        /// </summary>
        public bool XoaPhieuGoiKhiXoaSo(string maSo, DateTime ngayMoSo)
        {
            return _dbGoi.DeletePhieuGoiBangMaSoVaNgay(maSo, ngayMoSo);
        }

        /// <summary>
        /// Tự động cập nhật phiếu gởi có cùng MaSo và NgayGoi = NgayMoSo (cũ) khi cập nhật sổ.
        /// Cập nhật: NgayGoi = NgayMoSo mới, SoTienGoi = SoTien mới.
        /// </summary>
        public bool CapNhatPhieuGoiKhiCapNhatSo(SoTietKiem soMoi, DateTime ngayMoSoCu)
        {
            return _dbGoi.UpdatePhieuGoiBangMaSoVaNgay(soMoi.MaSo, ngayMoSoCu, soMoi.NgayMoSo, soMoi.SoTien);
        }
    }
}