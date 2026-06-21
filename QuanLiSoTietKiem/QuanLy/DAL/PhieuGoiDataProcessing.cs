using MySql.Data.MySqlClient;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.Configuration;

namespace QuanLiSoTietKiem.QuanLy.DAL
{
    /// <summary>
    /// DAL thuần túy — chỉ truy xuất và lưu dữ liệu, KHÔNG chứa logic tính toán.
    /// Mọi tính toán lãi suất được thực hiện ở BLL và truyền vào qua tham số.
    /// </summary>
    public class PhieuGoiDataProcessing
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        public string GetNextMaPhieuGoi()
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT CONCAT('PG', LPAD(COALESCE(MAX(CAST(SUBSTRING(MaPhieuGoi, 3) AS UNSIGNED)), 0) + 1, 3, '0')) FROM phieu_goi",
                    conn);
                return cmd.ExecuteScalar()?.ToString() ?? "PG001";
            }
        }

        /// <summary>
        /// Đọc thông tin cần thiết để BLL tính lãi:
        /// SoTien hiện tại, NgayMoSo, NgayCapNhatGanNhat, MaLoaiTietKiem.
        /// Trả về null nếu không tìm thấy sổ.
        /// </summary>
        public SoTietKiem LayThongTinSo(string maSo)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT MaSo, MaLoaiTietKiem, SoTien, NgayMoSo, NgayCapNhatGanNhat
                      FROM so_tiet_kiem
                      WHERE MaSo = @maso",
                    conn);
                cmd.Parameters.AddWithValue("@maso", maSo);
                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read()) return null;
                    return new SoTietKiem
                    {
                        MaSo = rdr.GetString("MaSo"),
                        MaLoaiTietKiem = rdr.GetInt32("MaLoaiTietKiem"),
                        SoTien = rdr.GetDecimal("SoTien"),
                        NgayMoSo = rdr.GetDateTime("NgayMoSo"),
                        NgayCapNhatGanNhat = rdr.IsDBNull(rdr.GetOrdinal("NgayCapNhatGanNhat"))
                                                ? DateTime.MinValue
                                                : rdr.GetDateTime("NgayCapNhatGanNhat")
                    };
                }
            }
        }

        /// <summary>
        /// INSERT phiếu gởi và UPDATE số tiền sổ.
        /// tienLaiTichLuy: tiền lãi đã tích lũy từ kỳ trước — do BLL tính và truyền vào.
        /// DAL chỉ cộng vào DB, không tự tính.
        /// </summary>
        public bool SavePhieuGoiTien(PhieuGoi phieu, decimal tienLaiTichLuy)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // INSERT phiếu gởi
                        var cmdInsert = new MySqlCommand(
                            @"INSERT INTO phieu_goi (MaPhieuGoi, MaSo, NgayGoi, SoTienGoi)
                              VALUES (@maphieu, @maso, @ngay, @sotien)",
                            conn, trans);
                        cmdInsert.Parameters.AddWithValue("@maphieu", phieu.MaPhieuGoi);
                        cmdInsert.Parameters.AddWithValue("@maso", phieu.MaSo);
                        cmdInsert.Parameters.AddWithValue("@ngay", phieu.NgayGoi);
                        cmdInsert.Parameters.AddWithValue("@sotien", phieu.SoTienGoi);
                        cmdInsert.ExecuteNonQuery();

                        // UPDATE sổ: cộng tiền gởi mới + lãi tích lũy vào số dư hiện tại
                        var cmdUpdate = new MySqlCommand(
                            @"UPDATE so_tiet_kiem
                              SET SoTien             = SoTien + @sotienGoi + @tienLai,
                                  NgayCapNhatGanNhat = @ngayCapNhat
                              WHERE MaSo = @maso",
                            conn, trans);
                        cmdUpdate.Parameters.AddWithValue("@sotienGoi", phieu.SoTienGoi);
                        cmdUpdate.Parameters.AddWithValue("@tienLai", tienLaiTichLuy);
                        cmdUpdate.Parameters.AddWithValue("@ngayCapNhat", phieu.NgayGoi);
                        cmdUpdate.Parameters.AddWithValue("@maso", phieu.MaSo);
                        cmdUpdate.ExecuteNonQuery();

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
        /// Chỉ INSERT phiếu gởi — KHÔNG cộng tiền vào sổ.
        /// Dùng khi tự động tạo phiếu gởi lúc mở sổ mới
        /// (SoTien đã đúng từ lúc INSERT sổ).
        /// </summary>
        public bool SavePhieuGoiKhiMoSo(PhieuGoi phieu)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"INSERT INTO phieu_goi (MaPhieuGoi, MaSo, NgayGoi, SoTienGoi)
                      VALUES (@maphieu, @maso, @ngay, @sotien)",
                    conn);
                cmd.Parameters.AddWithValue("@maphieu", phieu.MaPhieuGoi);
                cmd.Parameters.AddWithValue("@maso", phieu.MaSo);
                cmd.Parameters.AddWithValue("@ngay", phieu.NgayGoi);
                cmd.Parameters.AddWithValue("@sotien", phieu.SoTienGoi);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Xóa phiếu gởi theo MaSo và NgayGoi = NgayMoSo.
        /// Được gọi khi xóa sổ tiết kiệm.
        /// </summary>
        public bool DeletePhieuGoiBangMaSoVaNgay(string maSo, DateTime ngayMoSo)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "DELETE FROM phieu_goi WHERE MaSo = @maso AND DATE(NgayGoi) = @ngay",
                    conn);
                cmd.Parameters.AddWithValue("@maso", maSo);
                cmd.Parameters.AddWithValue("@ngay", ngayMoSo.Date);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Cập nhật NgayGoi và SoTienGoi của phiếu gởi khi cập nhật sổ.
        /// </summary>
        public bool UpdatePhieuGoiBangMaSoVaNgay(string maSo, DateTime ngayMoSoCu,
                                                  DateTime ngayMoSoMoi, decimal soTienMoi)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"UPDATE phieu_goi
                      SET NgayGoi   = @ngaymoi,
                          SoTienGoi = @tienmoi
                      WHERE MaSo = @maso
                        AND DATE(NgayGoi) = @ngaycu",
                    conn);
                cmd.Parameters.AddWithValue("@ngaymoi", ngayMoSoMoi);
                cmd.Parameters.AddWithValue("@tienmoi", soTienMoi);
                cmd.Parameters.AddWithValue("@maso", maSo);
                cmd.Parameters.AddWithValue("@ngaycu", ngayMoSoCu.Date);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}