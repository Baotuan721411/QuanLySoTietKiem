using MySql.Data.MySqlClient;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.Configuration;

namespace QuanLiSoTietKiem.QuanLy.DAL
{
    public class PhieuGoiDataProcessing
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        public string GetNextMaPhieuGoi()
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT CONCAT('PG', LPAD(COALESCE(MAX(CAST(SUBSTRING(MaPhieuGoi, 3) AS UNSIGNED)), 0) + 1, 3, '0')) FROM phieu_goi", conn);
                return cmd.ExecuteScalar()?.ToString() ?? "PG001";
            }
        }

        public bool SavePhieuGoiTien(PhieuGoi phieu)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        string sqlSelectSo = @"
                    SELECT s.SoTien, s.NgayMoSo, s.NgayCapNhatGanNhat, l.LaiSuat 
                    FROM so_tiet_kiem s
                    JOIN loai_tiet_kiem l ON s.MaLoaiTietKiem = l.MaLoaiTietKiem
                    WHERE s.MaSo = @maso";

                        decimal soTienHienTai = 0;
                        DateTime ngayMoSo = DateTime.Now;
                        DateTime? ngayCapNhatGanNhat = null;
                        decimal laiSuatThang = 0;

                        using (var cmdSelect = new MySqlCommand(sqlSelectSo, conn, trans))
                        {
                            cmdSelect.Parameters.AddWithValue("@maso", phieu.MaSo);
                            using (var rdr = cmdSelect.ExecuteReader())
                            {
                                if (rdr.Read())
                                {
                                    soTienHienTai = rdr.GetDecimal("SoTien");
                                    ngayMoSo = rdr.GetDateTime("NgayMoSo");
                                    ngayCapNhatGanNhat = rdr.IsDBNull(rdr.GetOrdinal("NgayCapNhatGanNhat"))
                                        ? (DateTime?)null
                                        : rdr.GetDateTime("NgayCapNhatGanNhat");
                                    laiSuatThang = rdr.GetDecimal("LaiSuat");
                                }
                                else
                                {
                                    trans.Rollback();
                                    return false;
                                }
                            }
                        }

                        DateTime ngayBatDauTinhLai = ngayCapNhatGanNhat ?? ngayMoSo;
                        DateTime ngayKetThucTinhLai = phieu.NgayGoi;

                        int soNgayGui = (ngayKetThucTinhLai.Date - ngayBatDauTinhLai.Date).Days;
                        if (soNgayGui < 0) soNgayGui = 0;

                        int soLanDaoHan = soNgayGui / 30;
                        int kyHan = 1;

                        decimal tienLai = soTienHienTai * soLanDaoHan * (laiSuatThang / 100m) * kyHan;
                        tienLai = Math.Round(tienLai, 0);

                        string sqlInsertPhieu = @"INSERT INTO phieu_goi (MaPhieuGoi, MaSo, NgayGoi, SoTienGoi) 
                                         VALUES (@maphieu, @maso, @ngay, @sotien)";
                        using (var cmdInsert = new MySqlCommand(sqlInsertPhieu, conn, trans))
                        {
                            cmdInsert.Parameters.AddWithValue("@maphieu", phieu.MaPhieuGoi);
                            cmdInsert.Parameters.AddWithValue("@maso", phieu.MaSo);
                            cmdInsert.Parameters.AddWithValue("@ngay", phieu.NgayGoi);
                            cmdInsert.Parameters.AddWithValue("@sotien", phieu.SoTienGoi);
                            cmdInsert.ExecuteNonQuery();
                        }

                        string sqlUpdateSo = @"
                    UPDATE so_tiet_kiem 
                    SET SoTien = SoTien + @sotienGoi + @tienLai, 
                        NgayCapNhatGanNhat = @ngayCapNhat
                    WHERE MaSo = @maso";

                        using (var cmdUpdate = new MySqlCommand(sqlUpdateSo, conn, trans))
                        {
                            cmdUpdate.Parameters.AddWithValue("@sotienGoi", phieu.SoTienGoi);
                            cmdUpdate.Parameters.AddWithValue("@tienLai", tienLai);
                            cmdUpdate.Parameters.AddWithValue("@ngayCapNhat", phieu.NgayGoi);
                            cmdUpdate.Parameters.AddWithValue("@maso", phieu.MaSo);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Chỉ INSERT phiếu gởi đơn thuần — KHÔNG tính lãi, KHÔNG cộng tiền vào sổ.
        /// Dùng khi tự động tạo phiếu gởi lúc mở sổ mới (SoTien đã đúng từ lúc insert sổ).
        /// </summary>
        public bool SavePhieuGoiKhiMoSo(PhieuGoi phieu)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = @"INSERT INTO phieu_goi (MaPhieuGoi, MaSo, NgayGoi, SoTienGoi)
                               VALUES (@maphieu, @maso, @ngay, @sotien)";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@maphieu", phieu.MaPhieuGoi);
                cmd.Parameters.AddWithValue("@maso", phieu.MaSo);
                cmd.Parameters.AddWithValue("@ngay", phieu.NgayGoi);
                cmd.Parameters.AddWithValue("@sotien", phieu.SoTienGoi);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Xóa phiếu gởi có cùng MaSo và NgayGoi = NgayMoSo của sổ (so sánh theo DATE).
        /// Được gọi tự động khi xóa sổ tiết kiệm.
        /// </summary>
        public bool DeletePhieuGoiBangMaSoVaNgay(string maSo, DateTime ngayMoSo)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = "DELETE FROM phieu_goi WHERE MaSo = @maso AND DATE(NgayGoi) = @ngay";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@maso", maSo);
                cmd.Parameters.AddWithValue("@ngay", ngayMoSo.Date);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Cập nhật NgayGoi và SoTienGoi của phiếu gởi có cùng MaSo và NgayGoi = ngayMoSoCu.
        /// Được gọi tự động khi cập nhật sổ tiết kiệm.
        /// </summary>
        public bool UpdatePhieuGoiBangMaSoVaNgay(string maSo, DateTime ngayMoSoCu, DateTime ngayMoSoMoi, decimal soTienMoi)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = @"UPDATE phieu_goi
                               SET NgayGoi   = @ngaymoi,
                                   SoTienGoi = @tienmoi
                               WHERE MaSo = @maso
                                 AND DATE(NgayGoi) = @ngaycu";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ngaymoi", ngayMoSoMoi);
                cmd.Parameters.AddWithValue("@tienmoi", soTienMoi);
                cmd.Parameters.AddWithValue("@maso", maSo);
                cmd.Parameters.AddWithValue("@ngaycu", ngayMoSoCu.Date);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}