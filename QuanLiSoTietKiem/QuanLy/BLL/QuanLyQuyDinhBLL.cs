using MySql.Data.MySqlClient;
using QuanLiSoTietKiem.QuanLy.DAL;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.Collections.Generic;

namespace QuanLiSoTietKiem.QuanLy.BLL
{
    public class QuanLyQuyDinhBLL
    {
        private readonly QuanLyQuyDinhDAL _dal = new QuanLyQuyDinhDAL();

        public List<LoaiTietKiem> GetAll() => _dal.GetAll();

        public LoaiTietKiem TimKiemTheoTen(string tenLoai) => _dal.TimKiemTheoTen(tenLoai);

        /// <summary>
        /// Lấy giai đoạn lãi suất đang có hiệu lực (NgayKetThuc IS NULL)
        /// của một loại tiết kiệm. Dùng để đổ NgayApDung, NgayKetThuc,
        /// LaiSuat chính xác lên form khi người dùng chọn một hàng.
        /// </summary>
        public LichSuLaiSuat LayLichSuHienTai(int maLoai) => _dal.LayLichSuHienTai(maLoai);

        public string ThemLoai(LoaiTietKiem loai, DateTime ngayApDung, DateTime? ngayKetThuc)
        {
            if (_dal.TenLoaiDaTonTai(loai.TenLoaiTietKiem))
                return $"Loại tiết kiệm \"{loai.TenLoaiTietKiem}\" đã tồn tại trong danh sách!";

            string check = ValidateSoLieu(loai);
            if (check != "SUCCESS") return check;

            using (var conn = _dal.OpenConnection())
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    int newId = _dal.InsertLoai(loai, conn, tran);
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

        public string CapNhatLoai(LoaiTietKiem loai, DateTime ngayApDung, DateTime? ngayKetThuc)
        {
            if (!_dal.MaLoaiTonTai(loai.MaLoaiTietKiem))
                return "Loại tiết kiệm không tồn tại trong danh sách. Vui lòng chọn lại!";

            string check = ValidateSoLieu(loai);
            if (check != "SUCCESS") return check;

            using (var conn = _dal.OpenConnection())
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    int rows = _dal.UpdateLoai(loai, conn, tran);
                    if (rows == 0) return "Không tìm thấy loại tiết kiệm để cập nhật!";

                    // Chỉ ghi lịch sử lãi suất mới khi lãi suất thực sự thay đổi
                    decimal? laiSuatCu = _dal.GetLaiSuatHienTai(loai.MaLoaiTietKiem);
                    bool laiSuatThayDoi = laiSuatCu == null || laiSuatCu.Value != loai.LaiSuat;

                    if (laiSuatThayDoi)
                    {
                        _dal.DongKyLichSuCu(loai.MaLoaiTietKiem, ngayApDung, conn, tran);
                        _dal.GhiLichSuLaiSuat(loai.MaLoaiTietKiem, loai.LaiSuat, ngayApDung, ngayKetThuc, conn, tran);
                    }

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

        public string XoaLoai(int maLoai)
        {
            if (!_dal.MaLoaiTonTai(maLoai))
                return "Loại tiết kiệm không tồn tại trong danh sách. Vui lòng chọn lại!";

            if (_dal.CoSoTietKiemChuaDaoHan(maLoai))
                return "Không thể xóa! Hiện có sổ tiết kiệm thuộc loại này chưa đáo hạn.";

            using (var conn = _dal.OpenConnection())
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    _dal.DeleteLichSuLaiSuat(maLoai, conn, tran);
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

        private string ValidateSoLieu(LoaiTietKiem loai)
        {
            if (loai.TienGoiToiThieu <= 0 || loai.TienGoiToiThieu != Math.Floor(loai.TienGoiToiThieu))
                return "Tiền gửi tối thiểu phải là số nguyên dương!";
            if (loai.ThoiGianRutTien <= 0)
                return "Thời gian gửi tối thiểu phải là số nguyên dương (> 0 ngày)!";
            if (loai.LaiSuat <= 0)
                return "Lãi suất của kỳ hạn phải là số dương!";
            return "SUCCESS";
        }
    }
}