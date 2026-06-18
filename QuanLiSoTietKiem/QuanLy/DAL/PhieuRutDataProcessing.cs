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

        // ── Lấy lịch sử lãi suất trong cùng transaction ──────────────────────
        private List<LichSuLaiSuat> LayLichSuLaiSuat(
            int maLoaiTietKiem, MySqlConnection conn, MySqlTransaction trans)
        {
            var list = new List<LichSuLaiSuat>();
            var cmd = new MySqlCommand(
                @"SELECT MaLichSuLaiSuat, MaLoaiTietKiem, LaiSuatCuaKyHan, NgayApDung, NgayKetThuc
                  FROM lich_su_lai_suat
                  WHERE MaLoaiTietKiem = @maloai
                  ORDER BY NgayApDung ASC",
                conn, trans);
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
            return list;
        }

        // ── Tra lãi suất tại ngày bắt đầu của kỳ hạn ────────────────────────
        private static decimal TraLaiSuatTaiNgay(DateTime ngay, List<LichSuLaiSuat> lichSu)
        {
            foreach (var gs in lichSu)
            {
                bool batDauHopLe = gs.NgayApDung.Date <= ngay.Date;
                bool ketThucHopLe = !gs.NgayKetThuc.HasValue
                                    || gs.NgayKetThuc.Value.Date >= ngay.Date;
                if (batDauHopLe && ketThucHopLe)
                    return gs.LaiSuatCuaKyHan;
            }
            return 0;
        }

        /// <summary>
        /// Tính tổng tiền lãi theo quy tắc:
        ///   Lãi suất của một kỳ hạn = lãi suất đang có hiệu lực vào NGÀY BẮT ĐẦU kỳ đó.
        ///   Nếu lãi suất thay đổi giữa kỳ → không ảnh hưởng đến kỳ đang chạy,
        ///   chỉ áp dụng từ kỳ tiếp theo.
        ///
        /// Thuật toán:
        ///   kyBatDau = ngayBatDau
        ///   Lặp:
        ///     kyKetThuc = kyBatDau + kyHanNgay
        ///     Nếu kyKetThuc > ngayRut → kỳ chưa đáo hạn → dừng
        ///     Tra lãi suất tại kyBatDau → tính lãi kỳ này
        ///     kyBatDau = kyKetThuc
        /// </summary>
        private static decimal TinhTienLai(
            decimal soTienGoc,
            DateTime ngayBatDau,
            DateTime ngayRut,
            int kyHanNgay,
            decimal kyHanThang,
            List<LichSuLaiSuat> lichSu)
        {
            if (lichSu == null || lichSu.Count == 0) return 0;
            if (ngayRut.Date <= ngayBatDau.Date) return 0;

            decimal tongLai = 0;
            DateTime kyBatDau = ngayBatDau.Date;

            while (true)
            {
                DateTime kyKetThuc = kyBatDau.AddDays(kyHanNgay);

                // Kỳ chưa đáo hạn → không tính lãi, dừng
                if (kyKetThuc > ngayRut.Date) break;

                // Lãi suất theo ngày bắt đầu kỳ này
                decimal laiSuat = TraLaiSuatTaiNgay(kyBatDau, lichSu);

                tongLai += soTienGoc * (laiSuat / 100m) * kyHanThang;

                kyBatDau = kyKetThuc;
            }

            return Math.Round(tongLai, 0);
        }

        public string SavePhieuRutTien(PhieuRut phieu)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // ── 1. Đọc thông tin sổ và loại ──────────────────────────────────
                        string sqlSelect = @"
                            SELECT s.SoTien, s.NgayMoSo, s.NgayCapNhatGanNhat, s.SoDuToiThieu,
                                   l.MaLoaiTietKiem, l.TenLoaiTietKiem, l.ThoiGianRutTien
                            FROM so_tiet_kiem s
                            JOIN loai_tiet_kiem l ON s.MaLoaiTietKiem = l.MaLoaiTietKiem
                            WHERE s.MaSo = @maso";

                        decimal soTienGoc = 0;
                        DateTime ngayMoSo = DateTime.Now;
                        DateTime ngayCapNhatGanNhat = DateTime.MinValue;
                        decimal soDuToiThieu = 0;
                        int maLoaiTietKiem = 0;
                        string tenLoai = "";
                        int thoiGianRutTien = 0;

                        using (var cmdSelect = new MySqlCommand(sqlSelect, conn, trans))
                        {
                            cmdSelect.Parameters.AddWithValue("@maso", phieu.MaSo);
                            using (var rdr = cmdSelect.ExecuteReader())
                            {
                                if (rdr.Read())
                                {
                                    soTienGoc = rdr.GetDecimal("SoTien");
                                    ngayMoSo = rdr.GetDateTime("NgayMoSo");
                                    ngayCapNhatGanNhat = rdr.IsDBNull(rdr.GetOrdinal("NgayCapNhatGanNhat"))
                                                             ? DateTime.MinValue
                                                             : rdr.GetDateTime("NgayCapNhatGanNhat");
                                    soDuToiThieu = rdr.GetDecimal("SoDuToiThieu");
                                    maLoaiTietKiem = rdr.GetInt32("MaLoaiTietKiem");
                                    tenLoai = rdr.GetString("TenLoaiTietKiem");
                                    thoiGianRutTien = rdr.GetInt32("ThoiGianRutTien");
                                }
                                else
                                {
                                    return "Mã sổ tiết kiệm không tồn tại trong hệ thống!";
                                }
                            }
                        }

                        // ── 2. Kiểm tra thời gian tối thiểu được phép rút ────────────────
                        int soNgayTuLucMoSo = (phieu.NgayRut.Date - ngayMoSo.Date).Days;
                        if (soNgayTuLucMoSo < thoiGianRutTien)
                            return $"Thất bại: Sổ mới mở được {soNgayTuLucMoSo} ngày. " +
                                   $"Theo quy định của loại sổ này, phải sau ít nhất {thoiGianRutTien} ngày " +
                                   $"kể từ ngày mở sổ ({ngayMoSo:dd/MM/yyyy}) mới được phép rút tiền!";

                        // ── 3. Ngày bắt đầu tính lãi ─────────────────────────────────────
                        //       = NgayMoSo nếu chưa rút lần nào
                        //       = NgayCapNhatGanNhat nếu đã rút một phần trước đó
                        DateTime ngayBatDauTinhLai =
                            (ngayCapNhatGanNhat != DateTime.MinValue && ngayCapNhatGanNhat > ngayMoSo)
                                ? ngayCapNhatGanNhat
                                : ngayMoSo;

                        // ── 4. Lấy lịch sử lãi suất ──────────────────────────────────────
                        List<LichSuLaiSuat> lichSu = LayLichSuLaiSuat(maLoaiTietKiem, conn, trans);
                        if (lichSu == null || lichSu.Count == 0)
                            return "Lỗi hệ thống: Không tìm thấy lịch sử lãi suất cho loại tiết kiệm này!";

                        // ── 5. Thông số kỳ hạn ────────────────────────────────────────────
                        bool laKhongKyHan = tenLoai.ToLower().Contains("không kỳ hạn");
                        int kyHanNgay = laKhongKyHan ? 30 : thoiGianRutTien;
                        decimal kyHanThang = laKhongKyHan ? 1m : (thoiGianRutTien / 30m);

                        // ── 6. Tính lãi theo kỳ hạn — lãi suất tra tại ngày bắt đầu kỳ ──
                        decimal tienLai = TinhTienLai(
                            soTienGoc,
                            ngayBatDauTinhLai,
                            phieu.NgayRut,
                            kyHanNgay,
                            kyHanThang,
                            lichSu);

                        decimal soTienHienCo = soTienGoc + tienLai;
                        decimal soTienConLai = 0;

                        // ── 7. Kiểm tra số tiền rút ───────────────────────────────────────
                        if (laKhongKyHan)
                        {
                            if (phieu.SoTienRut > soTienHienCo)
                                return $"Thất bại: Số tiền yêu cầu rút ({phieu.SoTienRut:N0} VNĐ) " +
                                       $"vượt quá số tiền hiện có trong sổ ({soTienHienCo:N0} VNĐ)!";

                            soTienConLai = soTienHienCo - phieu.SoTienRut;
                        }
                        else
                        {
                            if (phieu.SoTienRut != soTienHienCo)
                                return $"Thất bại: Sổ tiết kiệm có kỳ hạn bắt buộc phải rút TOÀN BỘ " +
                                       $"số tiền hiện có! Số tiền nhập vào phải chính xác là: {soTienHienCo:N0} VNĐ";

                            soTienConLai = 0;
                        }

                        bool trangThaiMoi = soTienConLai > soDuToiThieu;

                        // ── 8. Ghi phiếu rút ──────────────────────────────────────────────
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

                        // ── 9. Cập nhật sổ tiết kiệm ──────────────────────────────────────
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