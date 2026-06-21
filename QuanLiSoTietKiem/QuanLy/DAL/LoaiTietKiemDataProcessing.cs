using MySql.Data.MySqlClient;
using QuanLiSoTietKiem.QuanLy.Models;
using System.Collections.Generic;
using System.Configuration;

namespace QuanLiSoTietKiem.QuanLy.DAL
{
    public class LoaiTietKiemDataProcessing
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;
        private readonly SoTietKiemDataProcessing _soTietKiemDP;

        public LoaiTietKiemDataProcessing()
        {
            _soTietKiemDP = new SoTietKiemDataProcessing();
        }

        public List<LoaiTietKiem> GetLoaiTietKiems()
        {
            var list = new List<LoaiTietKiem>();
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                // Đã bỏ cột LaiSuat khỏi query
                var cmd = new MySqlCommand(
                    "SELECT MaLoaiTietKiem, TenLoaiTietKiem, ThoiGianRutTien, QuiDinhRutTien, TienGoiToiThieu FROM loai_tiet_kiem",
                    conn);
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new LoaiTietKiem
                        {
                            MaLoaiTietKiem = rdr.GetInt32(0),
                            TenLoaiTietKiem = rdr.GetString(1),
                            ThoiGianRutTien = rdr.GetInt32(2),
                            QuiDinhRutTien = rdr.GetInt32(3),
                            TienGoiToiThieu = rdr.GetDecimal(4)
                        });
                    }
                }
            }
            return list;
        }

        public LoaiTietKiem GetLoaiTietKiemByMaSo(string maSo)
        {
            SoTietKiem so = _soTietKiemDP.SearchSoTietKiem(maSo)[0];
            int maLoaiTietKiem = so.MaLoaiTietKiem;
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                // Đã bỏ LaiSuat khỏi SELECT
                var cmd = new MySqlCommand(
                    "SELECT MaLoaiTietKiem, TenLoaiTietKiem, ThoiGianRutTien, QuiDinhRutTien, TienGoiToiThieu FROM loai_tiet_kiem WHERE MaLoaiTietKiem = @maloaitietkiem",
                    conn);
                cmd.Parameters.AddWithValue("@maloaitietkiem", maLoaiTietKiem);
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        return new LoaiTietKiem
                        {
                            MaLoaiTietKiem = rdr.GetInt32("MaLoaiTietKiem"),
                            TenLoaiTietKiem = rdr.GetString("TenLoaiTietKiem"),
                            ThoiGianRutTien = rdr.GetInt32("ThoiGianRutTien"),
                            QuiDinhRutTien = rdr.GetInt32("QuiDinhRutTien"),
                            TienGoiToiThieu = rdr.GetDecimal("TienGoiToiThieu"),
                        };
                    }
                }
            }
            return null;
        }

        public LoaiTietKiem GetLoaiTietKiemById(int maLoai)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                // Đã bỏ LaiSuat khỏi SELECT
                var cmd = new MySqlCommand(
                    "SELECT MaLoaiTietKiem, TenLoaiTietKiem, ThoiGianRutTien, QuiDinhRutTien, TienGoiToiThieu FROM loai_tiet_kiem WHERE MaLoaiTietKiem = @maloai",
                    conn);
                cmd.Parameters.AddWithValue("@maloai", maLoai);
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                        return new LoaiTietKiem
                        {
                            MaLoaiTietKiem = rdr.GetInt32(0),
                            TenLoaiTietKiem = rdr.GetString(1),
                            ThoiGianRutTien = rdr.GetInt32(2),
                            QuiDinhRutTien = rdr.GetInt32(3),
                            TienGoiToiThieu = rdr.GetDecimal(4),
                        };
                }
            }
            return null;
        }
    }
}