using MySql.Data.MySqlClient;
using QuanLiSoTietKiem.QuanLy.DAL;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.Collections.Generic;

namespace QuanLiSoTietKiem.QuanLy.BLL
{
    /// <summary>
    /// BLL cho QuanLyQuyDinh — điều phối logic nghiệp vụ theo đúng đặc tả
    /// (Thêm / Sửa / Xóa).
    /// </summary>
    public class QuanLyQuyDinhBLL
    {
        private readonly QuanLyQuyDinhDAL _dal = new QuanLyQuyDinhDAL();

        // ────────────────────────────────────────────────────────────────
        //  B02: Đọc D2 — Danh sách Loại tiết kiệm
        // ────────────────────────────────────────────────────────────────
        public List<LoaiTietKiem> GetAll() => _dal.GetAll();

        // ────────────────────────────────────────────────────────────────
        //  Tìm kiếm theo tên (dùng cho nút Tìm kiếm trên UI)
        // ────────────────────────────────────────────────────────────────
        public LoaiTietKiem TimKiemTheoTen(string tenLoai) => _dal.TimKiemTheoTen(tenLoai);

        // ════════════════════════════════════════════════════════════════
        //  THÊM MỚI
        //  B01: Nhận D1
        //  B02: Đọc D2 (GetAll)
        //  B03: "Loại kỳ hạn" KHÔNG được thuộc D2 (tên chưa tồn tại)
        //  B04: Tiền gửi tối thiểu và Thời gian gửi phải là số nguyên dương
        //  B05: Lãi suất phải là số dương
        //  B06: Lưu D3 → CSDL Loại tiết kiệm
        //  B07: Lưu D4 → CSDL Lịch sử lãi suất
        // ════════════════════════════════════════════════════════════════
        public string ThemLoai(LoaiTietKiem loai, DateTime ngayApDung, DateTime? ngayKetThuc)
        {
            // B03: Tên chưa tồn tại trong CSDL
            if (_dal.TenLoaiDaTonTai(loai.TenLoaiTietKiem))
                return $"Loại tiết kiệm \"{loai.TenLoaiTietKiem}\" đã tồn tại trong danh sách!";

            // B04–B05: Validate dữ liệu số
            string check = ValidateSoLieu(loai);
            if (check != "SUCCESS") return check;

            using (var conn = _dal.OpenConnection())
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    // B06: Lưu D3 → CSDL Loại tiết kiệm
                    int newId = _dal.InsertLoai(loai, conn, tran);

                    // B07: Lưu D4 → CSDL Lịch sử lãi suất
                    _dal.GhiLichSuLaiSuat(newId, loai.LaiSuat, ngayApDung, ngayKetThuc, conn, tran);

                    tran.Commit();
                    loai.MaLoaiTietKiem = newId;
                    return "SUCCESS";
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return "Lỗi thêm Database: " + ex.Message;
                }
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  SỬA
        //  B01: Nhận D1
        //  B02: Đọc D2 (GetAll)
        //  B03: "Loại kỳ hạn" PHẢI thuộc D2 (MaLoai tồn tại)
        //  B04: Tiền gửi tối thiểu và Thời gian gửi phải là số nguyên dương
        //  B05: Lãi suất phải là số dương
        //  B06: Cập nhật D3 → CSDL Loại tiết kiệm
        //  B07: Lưu D4 → CSDL Lịch sử lãi suất (đóng kỳ cũ, ghi kỳ mới)
        // ════════════════════════════════════════════════════════════════
        public string CapNhatLoai(LoaiTietKiem loai, DateTime ngayApDung, DateTime? ngayKetThuc)
        {
            // B03: Loại kỳ hạn phải thuộc D2
            if (!_dal.MaLoaiTonTai(loai.MaLoaiTietKiem))
                return "Loại tiết kiệm không tồn tại trong danh sách. Vui lòng chọn lại!";

            // B04–B05: Validate dữ liệu số
            string check = ValidateSoLieu(loai);
            if (check != "SUCCESS") return check;

            using (var conn = _dal.OpenConnection())
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    // B06: Cập nhật D3 → CSDL Loại tiết kiệm
                    int rows = _dal.UpdateLoai(loai, conn, tran);
                    if (rows == 0) return "Không tìm thấy loại tiết kiệm để cập nhật!";

                    // B07: Đóng kỳ lịch sử lãi suất cũ, ghi kỳ mới D4
                    _dal.DongKyLichSuCu(loai.MaLoaiTietKiem, ngayApDung, conn, tran);
                    _dal.GhiLichSuLaiSuat(loai.MaLoaiTietKiem, loai.LaiSuat, ngayApDung, ngayKetThuc, conn, tran);

                    tran.Commit();
                    return "SUCCESS";
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return "Lỗi cập nhật Database: " + ex.Message;
                }
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  XÓA
        //  B01: Nhận D1
        //  B02: Đọc D2 (GetAll)
        //  B03: Đọc D3 — Danh sách Sổ tiết kiệm từ CSDL
        //  B04: "Loại kỳ hạn" phải thuộc D2 (MaLoai tồn tại)
        //  B05: Không được có Sổ tiết kiệm nào thuộc loại này chưa đáo hạn
        //  B06: Xóa lịch sử lãi suất liên quan
        //  B07: Xóa bản ghi Loại tiết kiệm
        // ════════════════════════════════════════════════════════════════
        public string XoaLoai(int maLoai)
        {
            // B04: Loại kỳ hạn phải thuộc D2
            if (!_dal.MaLoaiTonTai(maLoai))
                return "Loại tiết kiệm không tồn tại trong danh sách. Vui lòng chọn lại!";

            // B05: Kiểm tra D3 — còn sổ tiết kiệm chưa đáo hạn (TrangThai = 1)?
            if (_dal.CoSoTietKiemChuaDaoHan(maLoai))
                return "Không thể xóa! Hiện có sổ tiết kiệm thuộc loại này chưa đáo hạn.";

            using (var conn = _dal.OpenConnection())
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    // B06: Xóa các bản ghi Lịch sử lãi suất có khóa ngoại = maLoai
                    _dal.DeleteLichSuLaiSuat(maLoai, conn, tran);

                    // B07: Xóa bản ghi Loại tiết kiệm
                    int rows = _dal.DeleteLoai(maLoai, conn, tran);

                    tran.Commit();
                    return rows > 0 ? "SUCCESS" : "Không tìm thấy loại tiết kiệm để xóa!";
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return "Lỗi xóa Database: " + ex.Message;
                }
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  VALIDATE dữ liệu số — dùng chung cho Thêm và Sửa (B04 + B05)
        //  B04: Tiền gửi tối thiểu và Thời gian gửi tối thiểu phải là số nguyên dương
        //  B05: Lãi suất phải là số dương
        // ════════════════════════════════════════════════════════════════
        private string ValidateSoLieu(LoaiTietKiem loai)
        {
            // B04a: Tiền gửi tối thiểu — số nguyên dương
            if (loai.TienGoiToiThieu <= 0 || loai.TienGoiToiThieu != Math.Floor(loai.TienGoiToiThieu))
                return "Tiền gửi tối thiểu phải là số nguyên dương!";

            // B04b: Thời gian gửi tối thiểu — số nguyên dương
            if (loai.ThoiGianRutTien <= 0)
                return "Thời gian gửi tối thiểu phải là số nguyên dương (> 0 ngày)!";

            // B05: Lãi suất — số dương (không bắt buộc nguyên)
            if (loai.LaiSuat <= 0)
                return "Lãi suất của kỳ hạn phải là số dương!";

            return "SUCCESS";
        }
    }
}