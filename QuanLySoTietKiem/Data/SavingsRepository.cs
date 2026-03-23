using MySql.Data.MySqlClient;
using QuanLySoTietKiem.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace QuanLySoTietKiem.Data
{
    /// <summary>
    /// Data access layer — handles all direct MySQL database operations.
    /// </summary>
    public class SavingsRepository
    {
        private readonly string _connStr;

        public SavingsRepository()
        {
            _connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString
                       ?? throw new InvalidOperationException(
                           "Connection string 'DefaultConnection' not found in App.config.");
        }

        /// <summary>
        /// Lấy danh sách tất cả loại tiết kiệm.
        /// </summary>
        public List<LoaiTietKiem> GetLoaiTietKiems()
        {
            var list = new List<LoaiTietKiem>();
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        "SELECT MaLoaiTietKiem, TenLoaiTietKiem FROM loai_tiet_kiem", conn);
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new LoaiTietKiem
                            {
                                MaLoaiTietKiem = rdr.GetInt32(0),
                                TenLoaiTietKiem = rdr.GetString(1)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy danh sách loại tiết kiệm: " + ex.Message, ex);
            }
            return list;
        }

        /// <summary>
        /// Lấy tham số hệ thống (chỉ có 1 dòng).
        /// </summary>
        public ThamSo GetThamSo()
        {
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        "SELECT SoTienToiThieu, BoiSoTienGui, DoTuoiToiThieu FROM tham_so LIMIT 1", conn);
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                            return new ThamSo
                            {
                                SoTienToiThieu = rdr.GetDecimal(0),
                                BoiSoTienGui = rdr.GetDecimal(1),
                                DoTuoiToiThieu = rdr.GetInt32(2)
                            };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy tham số hệ thống: " + ex.Message, ex);
            }
            return null;
        }

        /// <summary>
        /// Lưu một sổ tiết kiệm mới vào database.
        /// </summary>
        public bool SaveSoTietKiem(SoTietKiem so)
        {
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    string sql = @"INSERT INTO so_tiet_kiem 
                        (MaSo, MaLoaiTietKiem, TenKH, SoTien, NgaySinh, CCCD, NgayMoSo, DiaChi) 
                        VALUES (@maso, @maloai, @tenkh, @sotien, @ngaysinh, @cccd, @ngaymo, @diachi)";
                    var cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@maso", so.MaSo);
                    cmd.Parameters.AddWithValue("@maloai", so.MaLoaiTietKiem);
                    cmd.Parameters.AddWithValue("@tenkh", so.TenKH);
                    cmd.Parameters.AddWithValue("@sotien", so.SoTien);
                    cmd.Parameters.AddWithValue("@ngaysinh", so.NgaySinh);
                    cmd.Parameters.AddWithValue("@cccd", so.CCCD);
                    cmd.Parameters.AddWithValue("@ngaymo", so.NgayMoSo);
                    cmd.Parameters.AddWithValue("@diachi", so.DiaChi);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lưu sổ tiết kiệm: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Lấy tất cả sổ tiết kiệm, kèm tên loại tiết kiệm.
        /// </summary>
        public List<SoTietKiem> GetAllSoTietKiem()
        {
            var list = new List<SoTietKiem>();
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    string sql = @"SELECT s.MaSo, s.MaLoaiTietKiem, s.TenKH, s.SoTien, 
                                          s.NgaySinh, s.CCCD, s.NgayMoSo, s.DiaChi, l.TenLoaiTietKiem
                                   FROM so_tiet_kiem s
                                   JOIN loai_tiet_kiem l ON s.MaLoaiTietKiem = l.MaLoaiTietKiem
                                   ORDER BY s.NgayMoSo DESC";
                    var cmd = new MySqlCommand(sql, conn);
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new SoTietKiem
                            {
                                MaSo = rdr.GetString(0),
                                MaLoaiTietKiem = rdr.GetInt32(1),
                                TenKH = rdr.GetString(2),
                                SoTien = rdr.GetDecimal(3),
                                NgaySinh = rdr.IsDBNull(4) ? (DateTime?)null : rdr.GetDateTime(4),
                                CCCD = rdr.GetString(5),
                                NgayMoSo = rdr.GetDateTime(6),
                                DiaChi = rdr.IsDBNull(7) ? null : rdr.GetString(7),
                                TenLoaiTietKiem = rdr.GetString(8)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy danh sách sổ tiết kiệm: " + ex.Message, ex);
            }
            return list;
        }

        /// <summary>
        /// Tìm sổ tiết kiệm theo mã sổ hoặc tên khách hàng.
        /// </summary>
        public List<SoTietKiem> SearchSoTietKiem(string keyword)
        {
            var list = new List<SoTietKiem>();
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    string sql = @"SELECT s.MaSo, s.MaLoaiTietKiem, s.TenKH, s.SoTien, 
                                          s.NgaySinh, s.CCCD, s.NgayMoSo, s.DiaChi, l.TenLoaiTietKiem
                                   FROM so_tiet_kiem s
                                   JOIN loai_tiet_kiem l ON s.MaLoaiTietKiem = l.MaLoaiTietKiem
                                   WHERE s.MaSo LIKE @kw OR s.TenKH LIKE @kw
                                   ORDER BY s.NgayMoSo DESC";
                    var cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new SoTietKiem
                            {
                                MaSo = rdr.GetString(0),
                                MaLoaiTietKiem = rdr.GetInt32(1),
                                TenKH = rdr.GetString(2),
                                SoTien = rdr.GetDecimal(3),
                                NgaySinh = rdr.IsDBNull(4) ? (DateTime?)null : rdr.GetDateTime(4),
                                CCCD = rdr.GetString(5),
                                NgayMoSo = rdr.GetDateTime(6),
                                DiaChi = rdr.IsDBNull(7) ? null : rdr.GetString(7),
                                TenLoaiTietKiem = rdr.GetString(8)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tìm kiếm sổ tiết kiệm: " + ex.Message, ex);
            }
            return list;
        }

        /// <summary>
        /// Xóa sổ tiết kiệm theo mã sổ.
        /// </summary>
        public bool DeleteSoTietKiem(string maSo)
        {
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("DELETE FROM so_tiet_kiem WHERE MaSo = @maso", conn);
                    cmd.Parameters.AddWithValue("@maso", maSo);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa sổ tiết kiệm: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Cập nhật sổ tiết kiệm đã tồn tại.
        /// </summary>
        public bool UpdateSoTietKiem(SoTietKiem so)
        {
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    string sql = @"UPDATE so_tiet_kiem SET 
                        MaLoaiTietKiem = @maloai, TenKH = @tenkh, SoTien = @sotien, 
                        NgaySinh = @ngaysinh, CCCD = @cccd, NgayMoSo = @ngaymo, DiaChi = @diachi
                        WHERE MaSo = @maso";
                    var cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@maso", so.MaSo);
                    cmd.Parameters.AddWithValue("@maloai", so.MaLoaiTietKiem);
                    cmd.Parameters.AddWithValue("@tenkh", so.TenKH);
                    cmd.Parameters.AddWithValue("@sotien", so.SoTien);
                    cmd.Parameters.AddWithValue("@ngaysinh", so.NgaySinh);
                    cmd.Parameters.AddWithValue("@cccd", so.CCCD);
                    cmd.Parameters.AddWithValue("@ngaymo", so.NgayMoSo);
                    cmd.Parameters.AddWithValue("@diachi", so.DiaChi);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật sổ tiết kiệm: " + ex.Message, ex);
            }
        }
    }
}
