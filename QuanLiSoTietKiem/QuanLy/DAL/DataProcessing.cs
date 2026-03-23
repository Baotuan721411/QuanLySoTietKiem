using MySql.Data.MySqlClient;
using QuanLiSoTietKiem.QuanLy.Models;
using System.Collections.Generic;
namespace QuanLiSoTietKiem.QuanLy.DAL
{
    public class DataProcessing
    {
        private string connStr = "Server=localhost;Database=quan_ly_so_tiet_kiem;Uid=root;Pwd=123456;";

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
    }
}