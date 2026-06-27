using MySql.Data.MySqlClient;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace QuanLiSoTietKiem.QuanLy.DAL
{
    public class QuanLyQuyDinhDAL
    {
        private readonly string _connStr =
            ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        // ────────────────────────────────────────────────────────────────
        //  Danh sách toàn bộ loại tiết kiệm — JOIN lãi suất đang có hiệu lực
        //  tại thời điểm hiện tại (CURDATE() nằm trong khoảng NgayApDung..NgayKetThuc)
        //  để hiển thị lên DataGrid.
        // ────────────────────────────────────────────────────────────────
        public List<LoaiTietKiem> GetAll()
        {
            var list = new List<LoaiTietKiem>();
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
                            AND ls.NgayApDung <= CURDATE()
                            AND (ls.NgayKetThuc IS NULL OR ls.NgayKetThuc >= CURDATE())
                      ORDER BY l.MaLoaiTietKiem", conn);
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read()) list.Add(MapLoaiReader(rdr));
            }
            return list;
        }

        // ────────────────────────────────────────────────────────────────
        //  Lấy giai đoạn lãi suất đang có hiệu lực tại thời điểm hiện tại
        //  của một loại tiết kiệm: NgayApDung <= CURDATE() và
        //  (NgayKetThuc IS NULL hoặc NgayKetThuc >= CURDATE()).
        //  Dùng để đổ NgayApDung, NgayKetThuc, LaiSuat chính xác lên form
        //  khi người dùng click chọn một hàng trên DataGrid.
        // ────────────────────────────────────────────────────────────────
        public LichSuLaiSuat LayLichSuHienTai(int maLoai)
        {
            using (var conn = new MySqlConnection(_connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT MaLichSuLaiSuat, MaLoaiTietKiem,
                             LaiSuatCuaKyHan, NgayApDung, NgayKetThuc
                      FROM lich_su_lai_suat
                      WHERE MaLoaiTietKiem = @ma
                        AND NgayApDung <= CURDATE()
                        AND (NgayKetThuc IS NULL OR NgayKetThuc >= CURDATE())
                      ORDER BY NgayApDung DESC
                      LIMIT 1", conn);
                cmd.Parameters.AddWithValue("@ma", maLoai);
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read()) return MapLichSuReader(rdr);
                }
            }
            return null;
        }

        // ────────────────────────────────────────────────────────────────
        //  Tìm kiếm theo tên — trả về loại + lãi suất hiện tại, hoặc null
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
                            AND ls.NgayApDung <= CURDATE()
                            AND (ls.NgayKetThuc IS NULL OR ls.NgayKetThuc >= CURDATE())
                      WHERE l.TenLoaiTietKiem = @ten
                      LIMIT 1", conn);
                cmd.Parameters.AddWithValue("@ten", tenLoai);
                using (var rdr = cmd.ExecuteReader())
                    if (rdr.Read()) return MapLoaiReader(rdr);
            }
            return null;
        }

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

        public bool CoSoTietKiemChuaDaoHan(int maLoai)
        {
            using (var conn = new MySqlConnection(_connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT COUNT(*) FROM so_tiet_kiem
                      WHERE MaLoaiTietKiem = @ma AND TrangThai = 1", conn);
                cmd.Parameters.AddWithValue("@ma", maLoai);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public int InsertLoai(LoaiTietKiem loai, MySqlConnection conn, MySqlTransaction tran)
        {
            var cmdMaxId = new MySqlCommand(
                "SELECT IFNULL(MAX(MaLoaiTietKiem), 0) + 1 FROM loai_tiet_kiem", conn, tran);
            int newId = Convert.ToInt32(cmdMaxId.ExecuteScalar());

            var cmd = new MySqlCommand(
                @"INSERT INTO loai_tiet_kiem
                    (MaLoaiTietKiem, TenLoaiTietKiem, ThoiGianRutTien, QuiDinhRutTien, TienGoiToiThieu)
                  VALUES (@ma, @ten, @thoigian, @quydinh, @tiengui)",
                conn, tran);
            cmd.Parameters.AddWithValue("@ma", newId);
            cmd.Parameters.AddWithValue("@ten", loai.TenLoaiTietKiem);
            cmd.Parameters.AddWithValue("@thoigian", loai.ThoiGianRutTien);
            cmd.Parameters.AddWithValue("@quydinh", loai.QuiDinhRutTien);
            cmd.Parameters.AddWithValue("@tiengui", loai.TienGoiToiThieu);
            cmd.ExecuteNonQuery();
            return newId;
        }

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

        public void DeleteLichSuLaiSuat(int maLoai, MySqlConnection conn, MySqlTransaction tran)
        {
            var cmd = new MySqlCommand(
                "DELETE FROM lich_su_lai_suat WHERE MaLoaiTietKiem = @ma", conn, tran);
            cmd.Parameters.AddWithValue("@ma", maLoai);
            cmd.ExecuteNonQuery();
        }

        public int DeleteLoai(int maLoai, MySqlConnection conn, MySqlTransaction tran)
        {
            var cmd = new MySqlCommand(
                "DELETE FROM loai_tiet_kiem WHERE MaLoaiTietKiem = @ma", conn, tran);
            cmd.Parameters.AddWithValue("@ma", maLoai);
            return cmd.ExecuteNonQuery();
        }

        public void DongKyLichSuCu(int maLoai, DateTime ngayApDung,
                                    MySqlConnection conn, MySqlTransaction tran)
        {
            var cmd = new MySqlCommand(
                @"UPDATE lich_su_lai_suat
                  SET NgayKetThuc = @ngayKT
                  WHERE MaLoaiTietKiem = @ma AND NgayKetThuc IS NULL",
                conn, tran);
            cmd.Parameters.AddWithValue("@ma", maLoai);
            cmd.Parameters.AddWithValue("@ngayKT", ngayApDung.AddDays(-1).Date);
            cmd.ExecuteNonQuery();
        }

        public void GhiLichSuLaiSuat(int maLoai, decimal laiSuat,
                                      DateTime ngayApDung, DateTime? ngayKetThuc,
                                      MySqlConnection conn, MySqlTransaction tran)
        {
            var cmdMaxId = new MySqlCommand(
                "SELECT IFNULL(MAX(MaLichSuLaiSuat), 0) + 1 FROM lich_su_lai_suat", conn, tran);
            int newId = Convert.ToInt32(cmdMaxId.ExecuteScalar());

            var cmd = new MySqlCommand(
                @"INSERT INTO lich_su_lai_suat
                    (MaLichSuLaiSuat, MaLoaiTietKiem, LaiSuatCuaKyHan, NgayApDung, NgayKetThuc)
                  VALUES (@id, @ma, @ls, @ngayAD, @ngayKT)",
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

        // ── Helpers ──────────────────────────────────────────────────────────

        private LoaiTietKiem MapLoaiReader(MySqlDataReader rdr) => new LoaiTietKiem
        {
            MaLoaiTietKiem = rdr.GetInt32("MaLoaiTietKiem"),
            TenLoaiTietKiem = rdr.GetString("TenLoaiTietKiem"),
            ThoiGianRutTien = rdr.GetInt32("ThoiGianRutTien"),
            QuiDinhRutTien = rdr.GetInt32("QuiDinhRutTien"),
            TienGoiToiThieu = rdr.GetDecimal("TienGoiToiThieu"),
            LaiSuat = rdr.GetDecimal("LaiSuatHienTai")
        };

        private LichSuLaiSuat MapLichSuReader(MySqlDataReader rdr) => new LichSuLaiSuat
        {
            MaLichSuLaiSuat = rdr.GetInt32("MaLichSuLaiSuat"),
            MaLoaiTietKiem = rdr.GetInt32("MaLoaiTietKiem"),
            LaiSuatCuaKyHan = rdr.GetDecimal("LaiSuatCuaKyHan"),
            NgayApDung = rdr.GetDateTime("NgayApDung"),
            NgayKetThuc = rdr.IsDBNull(rdr.GetOrdinal("NgayKetThuc"))
                                  ? (DateTime?)null
                                  : rdr.GetDateTime("NgayKetThuc")
        };

        /// <summary>
        /// Lấy lãi suất đang có hiệu lực của một loại để so sánh trước khi ghi lịch sử mới.
        /// Trả về null nếu chưa có bản ghi nào.
        /// </summary>
        public decimal? GetLaiSuatHienTai(int maLoai)
        {
            using (var conn = new MySqlConnection(_connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT LaiSuatCuaKyHan
                      FROM lich_su_lai_suat
                      WHERE MaLoaiTietKiem = @ma
                        AND NgayApDung <= CURDATE()
                        AND (NgayKetThuc IS NULL OR NgayKetThuc >= CURDATE())
                      ORDER BY NgayApDung DESC
                      LIMIT 1", conn);
                cmd.Parameters.AddWithValue("@ma", maLoai);
                var result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value
                       ? (decimal?)null
                       : Convert.ToDecimal(result);
            }
        }

        public MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_connStr);
            conn.Open();
            return conn;
        }
    }
}