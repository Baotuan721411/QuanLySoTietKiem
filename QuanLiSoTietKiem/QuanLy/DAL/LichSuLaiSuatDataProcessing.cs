using MySql.Data.MySqlClient;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace QuanLiSoTietKiem.QuanLy.DAL
{
    public class LichSuLaiSuatDataProcessing
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        /// <summary>
        /// Lấy toàn bộ lịch sử lãi suất của một loại tiết kiệm, sắp xếp theo NgayApDung tăng dần.
        /// NgayKetThuc = null có nghĩa là giai đoạn hiện tại (chưa kết thúc).
        /// </summary>
        public List<LichSuLaiSuat> GetLichSuLaiSuat(int maLoaiTietKiem)
        {
            var list = new List<LichSuLaiSuat>();
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT MaLichSuLaiSuat, MaLoaiTietKiem, LaiSuatCuaKyHan, NgayApDung, NgayKetThuc
                      FROM lich_su_lai_suat
                      WHERE MaLoaiTietKiem = @maloai
                      ORDER BY NgayApDung ASC",
                    conn);
                cmd.Parameters.AddWithValue("@maloai", maLoaiTietKiem);
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new LichSuLaiSuat
                        {
                            MaLichSuLaiSuat = rdr.GetInt32("MaLichSuLaiSuat"),
                            MaLoaiTietKiem = rdr.GetInt32("MaLoaiTietKiem"),
                            LaiSuatCuaKyHan = rdr.GetDecimal("LaiSuatCuaKyHan"),
                            NgayApDung = rdr.GetDateTime("NgayApDung"),
                            NgayKetThuc = rdr.IsDBNull(rdr.GetOrdinal("NgayKetThuc"))
                                                    ? (DateTime?)null
                                                    : rdr.GetDateTime("NgayKetThuc")
                        });
                    }
                }
            }
            return list;
        }
    }
}