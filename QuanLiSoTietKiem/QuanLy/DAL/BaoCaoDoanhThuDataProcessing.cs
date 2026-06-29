using MySql.Data.MySqlClient;
using QuanLiSoTietKiem.QuanLy.Models;
using System.Collections.Generic;
using System.Configuration;

namespace QuanLiSoTietKiem.QuanLy.DAL
{
    public class BaoCaoDoanhThuDataProcessing
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        /// <summary>
        /// Lấy báo cáo doanh thu gom nhóm theo Loại tiết kiệm trong tháng.
        /// TongThu = tổng tiền gởi, TongChi = tổng tiền rút trong tháng đó.
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
        public List<BaoCaoDoanhThuModel> GetBaoCaoDoanhThuTheoThang(int nam, int thang)
        {
            var list = new List<BaoCaoDoanhThuModel>();
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = @"
                    SELECT
                        l.TenLoaiTietKiem,
                        COALESCE(SUM(pg.SoTienGoi), 0)      AS TongThu,
                        COALESCE(
                            (SELECT SUM(pr.SoTienRut)
                             FROM phieu_rut pr
                             JOIN so_tiet_kiem s2 ON s2.MaSo = pr.MaSo
                             WHERE s2.MaLoaiTietKiem = l.MaLoaiTietKiem
                               AND MONTH(pr.NgayRut) = @thang
                               AND YEAR(pr.NgayRut)  = @nam
                            ), 0)                           AS TongChi
                    FROM phieu_goi pg
                    JOIN so_tiet_kiem s  ON s.MaSo  = pg.MaSo
                    JOIN loai_tiet_kiem l ON l.MaLoaiTietKiem = s.MaLoaiTietKiem
                    WHERE MONTH(pg.NgayGoi) = @thang
                      AND YEAR(pg.NgayGoi)  = @nam
                    GROUP BY l.MaLoaiTietKiem, l.TenLoaiTietKiem
                    ORDER BY l.TenLoaiTietKiem";

                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@nam", nam);
                cmd.Parameters.AddWithValue("@thang", thang);

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new BaoCaoDoanhThuModel
                        {
                            TenLoaiTietKiem = rdr.GetString("TenLoaiTietKiem"),
                            TongThu = rdr.GetDecimal("TongThu"),
                            TongChi = rdr.GetDecimal("TongChi"),
                        });
                    }
                }
            }
            return list;
        }
    }
}