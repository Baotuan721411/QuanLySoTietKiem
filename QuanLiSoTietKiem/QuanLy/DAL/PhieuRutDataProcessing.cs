using MySql.Data.MySqlClient;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace QuanLiSoTietKiem.QuanLy.DAL
{
    public class PhieuRutDataProcessing
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        public string GetNextMaPhieuRut()
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT COUNT(*) FROM phieu_rut", conn);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return "PR" + (count + 1).ToString("D3");
            }
        }

        /// <summary>
        /// Ghi phiếu rút và cập nhật sổ tiết kiệm trong một transaction.
        /// Nhận kết quả đã tính sẵn từ BLL: soTienConLai, trangThaiMoi.
        /// </summary>
        public string SavePhieuRutTien(PhieuRut phieu, decimal soTienConLai, bool trangThaiMoi)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmdInsert = new MySqlCommand(
                            "INSERT INTO phieu_rut (MaPhieuRut, MaSo, SoTienRut, NgayRut) " +
                            "VALUES (@ma, @maso, @sotien, @ngay)",
                            conn, trans))
                        {
                            cmdInsert.Parameters.AddWithValue("@ma", phieu.MaPhieuRut);
                            cmdInsert.Parameters.AddWithValue("@maso", phieu.MaSo);
                            cmdInsert.Parameters.AddWithValue("@sotien", phieu.SoTienRut);
                            cmdInsert.Parameters.AddWithValue("@ngay", phieu.NgayRut);
                            cmdInsert.ExecuteNonQuery();
                        }

                        using (var cmdUpdate = new MySqlCommand(
                            @"UPDATE so_tiet_kiem
                              SET SoTien             = @soTienMoi,
                                  NgayCapNhatGanNhat = @ngayCapNhat,
                                  TrangThai          = @trangThaiMoi
                              WHERE MaSo = @maso",
                            conn, trans))
                        {
                            cmdUpdate.Parameters.AddWithValue("@soTienMoi", soTienConLai);
                            cmdUpdate.Parameters.AddWithValue("@ngayCapNhat", phieu.NgayRut);
                            cmdUpdate.Parameters.AddWithValue("@trangThaiMoi", trangThaiMoi);
                            cmdUpdate.Parameters.AddWithValue("@maso", phieu.MaSo);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        trans.Commit();
                        return "SUCCESS";
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return "Lỗi hệ thống cơ sở dữ liệu: " + ex.Message;
                    }
                }
            }
        }
    }
}