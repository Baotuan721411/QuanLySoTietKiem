using MySql.Data.MySqlClient;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace QuanLiSoTietKiem.QuanLy.DAL
{
    public class BaoCaoMoDongSoDataProcessing
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        /// <summary>
        /// Bước 04: Lấy danh sách Phiếu gởi theo Mã sổ, lọc theo tháng và năm.
        /// </summary>
        

        public int DemSoMoTheoNgayVaLoai(DateTime ngayN, int maLoai)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT COUNT(*) FROM so_tiet_kiem
                       WHERE MaLoaiTietKiem = @maloai
                         AND DATE(NgayMoSo) = @ngay";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@maloai", maLoai);
                cmd.Parameters.AddWithValue("@ngay", ngayN.Date);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public int DemSoDongTheoNgayVaLoai(DateTime ngayN, int maLoai)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT COUNT(*) FROM so_tiet_kiem
                       WHERE MaLoaiTietKiem = @maloai
                         AND TrangThai = 0
                         AND DATE(NgayCapNhatGanNhat) = @ngay";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@maloai", maLoai);
                cmd.Parameters.AddWithValue("@ngay", ngayN.Date);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}