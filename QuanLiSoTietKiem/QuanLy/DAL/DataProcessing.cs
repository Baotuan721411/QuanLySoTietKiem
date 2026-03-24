using MySql.Data.MySqlClient;
using QuanLiSoTietKiem.QuanLy.Models;
using System.Collections.Generic;
using System.Configuration;
namespace QuanLiSoTietKiem.QuanLy.DAL
{
    public class DataProcessing
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        public List<LoaiTietKiem> GetLoaiTietKiems()
        {
            var list = new List<LoaiTietKiem>();
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT MaLoaiTietKiem, TenLoaiTietKiem FROM loai_tiet_kiem", conn);
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new LoaiTietKiem { MaLoaiTietKiem = rdr.GetInt32(0), TenLoaiTietKiem = rdr.GetString(1) });
                    }
                }
            }
            return list;
        }

        public ThamSo GetThamSo()
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT SoTienToiThieu, BoiSoTienGui, DoTuoiToiThieu FROM tham_so LIMIT 1", conn);
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read()) return new ThamSo
                    {
                        SoTienToiThieu = rdr.GetDecimal(0),
                        BoiSoTienGui = rdr.GetDecimal(1),
                        DoTuoiToiThieu = rdr.GetInt32(2)
                    };
                }
            }
            return null;
        }

        public bool SaveSoTietKiem(SoTietKiem so)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = "INSERT INTO so_tiet_kiem (MaSo, MaLoaiTietKiem, TenKH, SoTien, NgaySinh, CCCD, NgayMoSo, DiaChi) VALUES (@maso, @maloai, @tenkh, @sotien, @ngaysinh, @cccd, @ngaymo, @diachi)";
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
        public List<SoTietKiem> SearchSoTietKiem(string keyword)
        {
            var list = new List<SoTietKiem>();
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = "SELECT * FROM so_tiet_kiem WHERE MaSo LIKE @k OR TenKH LIKE @k";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@k", "%" + keyword + "%");
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new SoTietKiem
                        {
                            MaSo = rdr.GetString("MaSo"),
                            TenKH = rdr.GetString("TenKH"),
                            SoTien = rdr.GetDecimal("SoTien"),
                            NgaySinh = rdr.GetDateTime("NgaySinh"),
                            CCCD = rdr.GetString("CCCD"),
                            NgayMoSo = rdr.GetDateTime("NgayMoSo"),
                            DiaChi = rdr.GetString("DiaChi"),
                            MaLoaiTietKiem = rdr.GetInt32("MaLoaiTietKiem")
                        });
                    }
                }
            }
            return list;
        }

        public bool UpdateSoTietKiem(SoTietKiem so)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = "UPDATE so_tiet_kiem SET TenKH=@ten, SoTien=@tien, NgaySinh=@ns, CCCD=@cccd, DiaChi=@dc, MaLoaiTietKiem=@loai, NgayMoSo=@ngaymo WHERE MaSo=@maso";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ten", so.TenKH);
                cmd.Parameters.AddWithValue("@tien", so.SoTien);
                cmd.Parameters.AddWithValue("@ns", so.NgaySinh);
                cmd.Parameters.AddWithValue("@cccd", so.CCCD);
                cmd.Parameters.AddWithValue("@dc", so.DiaChi);
                cmd.Parameters.AddWithValue("@loai", so.MaLoaiTietKiem);
                cmd.Parameters.AddWithValue("@ngaymo", so.NgayMoSo);
                cmd.Parameters.AddWithValue("@maso", so.MaSo);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool DeleteSoTietKiem(string maSo)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand("DELETE FROM so_tiet_kiem WHERE MaSo=@maso", conn);
                cmd.Parameters.AddWithValue("@maso", maSo);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}