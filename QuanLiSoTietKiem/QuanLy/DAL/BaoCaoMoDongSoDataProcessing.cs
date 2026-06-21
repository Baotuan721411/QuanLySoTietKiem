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
        public List<PhieuGoi> GetPhieuGoiTheoThangNam(string maSo, int thang, int nam)
        {
            var list = new List<PhieuGoi>();
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT MaPhieuGoi, MaSo, NgayGoi, SoTienGoi
                      FROM phieu_goi
                      WHERE MaSo = @maso
                        AND MONTH(NgayGoi) = @thang
                        AND YEAR(NgayGoi)  = @nam", conn);
                cmd.Parameters.AddWithValue("@maso", maSo);
                cmd.Parameters.AddWithValue("@thang", thang);
                cmd.Parameters.AddWithValue("@nam", nam);
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new PhieuGoi
                        {
                            MaPhieuGoi = rdr.GetString("MaPhieuGoi"),
                            MaSo = rdr.GetString("MaSo"),
                            NgayGoi = rdr.GetDateTime("NgayGoi"),
                            SoTienGoi = rdr.GetDecimal("SoTienGoi")
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Bước 05: Lấy danh sách Phiếu rút theo Mã sổ, lọc theo tháng và năm.
        /// </summary>
        public List<PhieuRut> GetPhieuRutTheoThangNam(string maSo, int thang, int nam)
        {
            var list = new List<PhieuRut>();
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT MaPhieuRut, MaSo, NgayRut, SoTienRut
                      FROM phieu_rut
                      WHERE MaSo = @maso
                        AND MONTH(NgayRut) = @thang
                        AND YEAR(NgayRut)  = @nam", conn);
                cmd.Parameters.AddWithValue("@maso", maSo);
                cmd.Parameters.AddWithValue("@thang", thang);
                cmd.Parameters.AddWithValue("@nam", nam);
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new PhieuRut
                        {
                            MaPhieuRut = rdr.GetString("MaPhieuRut"),
                            MaSo = rdr.GetString("MaSo"),
                            NgayRut = rdr.GetDateTime("NgayRut"),
                            SoTienRut = rdr.GetDecimal("SoTienRut")
                        });
                    }
                }
            }
            return list;
        }

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