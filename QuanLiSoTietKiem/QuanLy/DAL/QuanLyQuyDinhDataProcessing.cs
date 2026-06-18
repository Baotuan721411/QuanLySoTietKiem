using MySql.Data.MySqlClient;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace QuanLiSoTietKiem.QuanLy.DAL
{
    /// <summary>
    /// DAL cho QuanLyQuyDinh — chứa toàn bộ logic truy cập Database.
    /// LaiSuat đã được xóa khỏi loai_tiet_kiem; lãi suất quản lý qua lich_su_lai_suat.
    /// </summary>
    public class QuanLyQuyDinhDAL
    {
        private readonly string _connStr =
            ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        // ────────────────────────────────────────────────────────────────
        //  B02: Đọc D2 — Danh sách toàn bộ Loại tiết kiệm từ CSDL
        //  JOIN lich_su_lai_suat để lấy lãi suất đang áp dụng (NgayKetThuc IS NULL)
        //  hiển thị lên DataGrid.
        // ────────────────────────────────────────────────────────────────
        public List<LoaiTietKiem> GetAll()
        {
            var list = new List<LoaiTietKiem>();
            using (var conn = new MySqlConnection(_connStr))
            {
                conn.Open();
                // LEFT JOIN để vẫn hiện loại dù chưa có lịch sử lãi suất nào
                var cmd = new MySqlCommand(
                    @"SELECT l.MaLoaiTietKiem, l.TenLoaiTietKiem,
                             l.ThoiGianRutTien, l.QuiDinhRutTien, l.TienGoiToiThieu,
                             COALESCE(ls.LaiSuatCuaKyHan, 0) AS LaiSuatHienTai
                      FROM loai_tiet_kiem l
                      LEFT JOIN lich_su_lai_suat ls
                             ON ls.MaLoaiTietKiem = l.MaLoaiTietKiem
                            AND ls.NgayKetThuc IS NULL
                      ORDER BY l.MaLoaiTietKiem", conn);
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                        list.Add(MapReader(rdr));
                }
            }
            return list;
        }

        // ────────────────────────────────────────────────────────────────
        //  TÌM KIẾM THEO TÊN — dùng cho nút Tìm kiếm trên UI
        //  Trả về bản ghi khớp chính xác đầu tiên (kèm lãi suất hiện tại), hoặc null.
        // ────────────────────────────────────────────────────────────────
        public LoaiTietKiem TimKiemTheoTen(string tenLoai)
        {
            using (var conn = new MySqlConnection(_connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT l.MaLoaiTietKiem, l.TenLoaiTietKiem,
                             l.ThoiGianRutTien, l.QuiDinhRutTien, l.TienGoiToiThieu,
                             COALESCE(ls.LaiSuatCuaKyHan, 0) AS LaiSuatHienTai
                      FROM loai_tiet_kiem l
                      LEFT JOIN lich_su_lai_suat ls
                             ON ls.MaLoaiTietKiem = l.MaLoaiTietKiem
                            AND ls.NgayKetThuc IS NULL
                      WHERE l.TenLoaiTietKiem = @ten
                      LIMIT 1", conn);
                cmd.Parameters.AddWithValue("@ten", tenLoai);
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read()) return MapReader(rdr);
                }
            }
            return null;
        }

        // ────────────────────────────────────────────────────────────────
        //  THÊM — B03 (Thêm): Kiểm tra tên đã tồn tại trong CSDL chưa
        // ────────────────────────────────────────────────────────────────
        public bool TenLoaiDaTonTai(string tenLoai)
        {
            using (var conn = new MySqlConnection(_connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM loai_tiet_kiem WHERE TenLoaiTietKiem = @ten", conn);
                cmd.Parameters.AddWithValue("@ten", tenLoai);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // ────────────────────────────────────────────────────────────────
        //  SỬA — B03 (Sửa): Kiểm tra MaLoai có tồn tại trong CSDL không
        // ────────────────────────────────────────────────────────────────
        public bool MaLoaiTonTai(int maLoai)
        {
            using (var conn = new MySqlConnection(_connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM loai_tiet_kiem WHERE MaLoaiTietKiem = @ma", conn);
                cmd.Parameters.AddWithValue("@ma", maLoai);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // ────────────────────────────────────────────────────────────────
        //  XÓA — B03 (Xóa): Đọc D3 — Danh sách Sổ tiết kiệm chưa đáo hạn
        //  (TrangThai = 1 tức đang mở) thuộc loại cần xóa
        // ────────────────────────────────────────────────────────────────
        public bool CoSoTietKiemChuaDaoHan(int maLoai)
        {
            using (var conn = new MySqlConnection(_connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT COUNT(*) FROM so_tiet_kiem
                      WHERE MaLoaiTietKiem = @ma
                        AND TrangThai = 1", conn);
                cmd.Parameters.AddWithValue("@ma", maLoai);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // ────────────────────────────────────────────────────────────────
        //  B06 (Thêm): INSERT loại tiết kiệm — trả về ID vừa tạo
        //  Không còn cột LaiSuat trong loai_tiet_kiem.
        // ────────────────────────────────────────────────────────────────
        public int InsertLoai(LoaiTietKiem loai, MySqlConnection conn, MySqlTransaction tran)
        {
            var cmdMaxId = new MySqlCommand(
                "SELECT IFNULL(MAX(MaLoaiTietKiem), 0) + 1 FROM loai_tiet_kiem",
                conn, tran);
            int newId = Convert.ToInt32(cmdMaxId.ExecuteScalar());

            var cmd = new MySqlCommand(
                @"INSERT INTO loai_tiet_kiem
                    (MaLoaiTietKiem, TenLoaiTietKiem, ThoiGianRutTien, QuiDinhRutTien, TienGoiToiThieu)
                  VALUES
                    (@ma, @ten, @thoigian, @quydinh, @tiengui)",
                conn, tran);
            cmd.Parameters.AddWithValue("@ma", newId);
            cmd.Parameters.AddWithValue("@ten", loai.TenLoaiTietKiem);
            cmd.Parameters.AddWithValue("@thoigian", loai.ThoiGianRutTien);
            cmd.Parameters.AddWithValue("@quydinh", loai.QuiDinhRutTien);
            cmd.Parameters.AddWithValue("@tiengui", loai.TienGoiToiThieu);
            cmd.ExecuteNonQuery();
            return newId;
        }

        // ────────────────────────────────────────────────────────────────
        //  B06 (Sửa): UPDATE loại tiết kiệm — trả về số hàng ảnh hưởng
        //  Không còn cột LaiSuat trong loai_tiet_kiem.
        // ────────────────────────────────────────────────────────────────
        public int UpdateLoai(LoaiTietKiem loai, MySqlConnection conn, MySqlTransaction tran)
        {
            var cmd = new MySqlCommand(
                @"UPDATE loai_tiet_kiem
                  SET TenLoaiTietKiem = @ten,
                      ThoiGianRutTien = @thoigian,
                      QuiDinhRutTien  = @quydinh,
                      TienGoiToiThieu = @tiengui
                  WHERE MaLoaiTietKiem = @ma",
                conn, tran);
            cmd.Parameters.AddWithValue("@ma", loai.MaLoaiTietKiem);
            cmd.Parameters.AddWithValue("@ten", loai.TenLoaiTietKiem);
            cmd.Parameters.AddWithValue("@thoigian", loai.ThoiGianRutTien);
            cmd.Parameters.AddWithValue("@quydinh", loai.QuiDinhRutTien);
            cmd.Parameters.AddWithValue("@tiengui", loai.TienGoiToiThieu);
            return cmd.ExecuteNonQuery();
        }

        // ────────────────────────────────────────────────────────────────
        //  B06 (Xóa): Xóa toàn bộ lịch sử lãi suất của loại đó
        // ────────────────────────────────────────────────────────────────
        public void DeleteLichSuLaiSuat(int maLoai, MySqlConnection conn, MySqlTransaction tran)
        {
            var cmd = new MySqlCommand(
                "DELETE FROM lich_su_lai_suat WHERE MaLoaiTietKiem = @ma",
                conn, tran);
            cmd.Parameters.AddWithValue("@ma", maLoai);
            cmd.ExecuteNonQuery();
        }

        // ────────────────────────────────────────────────────────────────
        //  B07 (Xóa): Xóa bản ghi loại tiết kiệm — trả về số hàng ảnh hưởng
        // ────────────────────────────────────────────────────────────────
        public int DeleteLoai(int maLoai, MySqlConnection conn, MySqlTransaction tran)
        {
            var cmd = new MySqlCommand(
                "DELETE FROM loai_tiet_kiem WHERE MaLoaiTietKiem = @ma",
                conn, tran);
            cmd.Parameters.AddWithValue("@ma", maLoai);
            return cmd.ExecuteNonQuery();
        }

        // ────────────────────────────────────────────────────────────────
        //  B07 (Thêm/Sửa): Đóng kỳ lịch sử lãi suất hiện tại
        //  (NgayKetThuc = ngayApDung - 1 ngày)
        // ────────────────────────────────────────────────────────────────
        public void DongKyLichSuCu(int maLoai, DateTime ngayApDung,
                                    MySqlConnection conn, MySqlTransaction tran)
        {
            var cmd = new MySqlCommand(
                @"UPDATE lich_su_lai_suat
                  SET NgayKetThuc = @ngayKT
                  WHERE MaLoaiTietKiem = @ma
                    AND NgayKetThuc IS NULL",
                conn, tran);
            cmd.Parameters.AddWithValue("@ma", maLoai);
            cmd.Parameters.AddWithValue("@ngayKT", ngayApDung.AddDays(-1).Date);
            cmd.ExecuteNonQuery();
        }

        // ────────────────────────────────────────────────────────────────
        //  B07 (Thêm/Sửa): Ghi mới D4 vào CSDL Lịch sử lãi suất
        // ────────────────────────────────────────────────────────────────
        public void GhiLichSuLaiSuat(int maLoai, decimal laiSuat,
                                      DateTime ngayApDung, DateTime? ngayKetThuc,
                                      MySqlConnection conn, MySqlTransaction tran)
        {
            var cmdMaxId = new MySqlCommand(
                "SELECT IFNULL(MAX(MaLichSuLaiSuat), 0) + 1 FROM lich_su_lai_suat",
                conn, tran);
            int newId = Convert.ToInt32(cmdMaxId.ExecuteScalar());

            var cmd = new MySqlCommand(
                @"INSERT INTO lich_su_lai_suat
                    (MaLichSuLaiSuat, MaLoaiTietKiem, LaiSuatCuaKyHan, NgayApDung, NgayKetThuc)
                  VALUES
                    (@id, @ma, @ls, @ngayAD, @ngayKT)",
                conn, tran);
            cmd.Parameters.AddWithValue("@id", newId);
            cmd.Parameters.AddWithValue("@ma", maLoai);
            cmd.Parameters.AddWithValue("@ls", laiSuat);
            cmd.Parameters.AddWithValue("@ngayAD", ngayApDung.Date);
            cmd.Parameters.AddWithValue("@ngayKT", ngayKetThuc.HasValue
                                                    ? (object)ngayKetThuc.Value.Date
                                                    : DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        // ────────────────────────────────────────────────────────────────
        //  HELPER: map một hàng reader → LoaiTietKiem
        //  Cột trả về: 0=MaLoai, 1=TenLoai, 2=ThoiGian, 3=QuiDinh,
        //              4=TienGui, 5=LaiSuatHienTai (từ JOIN)
        // ────────────────────────────────────────────────────────────────
        private LoaiTietKiem MapReader(MySqlDataReader rdr) => new LoaiTietKiem
        {
            MaLoaiTietKiem = rdr.GetInt32("MaLoaiTietKiem"),
            TenLoaiTietKiem = rdr.GetString("TenLoaiTietKiem"),
            ThoiGianRutTien = rdr.GetInt32("ThoiGianRutTien"),
            QuiDinhRutTien = rdr.GetInt32("QuiDinhRutTien"),
            TienGoiToiThieu = rdr.GetDecimal("TienGoiToiThieu"),
            // LaiSuatHienTai là cột tính toán từ JOIN — map vào property tạm LaiSuat
            // để ViewModel hiển thị lên form (không còn lưu trong loai_tiet_kiem)
            LaiSuat = rdr.GetDecimal("LaiSuatHienTai")
        };

        // ────────────────────────────────────────────────────────────────
        //  HELPER: tạo connection mới đã mở (BLL dùng để mở transaction)
        // ────────────────────────────────────────────────────────────────
        public MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_connStr);
            conn.Open();
            return conn;
        }
    }
}