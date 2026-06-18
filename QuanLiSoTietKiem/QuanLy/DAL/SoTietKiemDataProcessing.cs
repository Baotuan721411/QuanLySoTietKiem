using MySql.Data.MySqlClient;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
namespace QuanLiSoTietKiem.QuanLy.DAL
{
    public class SoTietKiemDataProcessing
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

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

        /// <summary>
        /// Lấy toàn bộ lịch sử lãi suất của một loại tiết kiệm, sắp xếp theo NgayApDung tăng dần.
        /// NgayKetThuc = null có nghĩa là giai đoạn hiện tại (chưa kết thúc).
        /// </summary>
        public List<LichSuLaiSuat> GetLichSuLaiSuat(int maLoaiTietKiem)
        {
            var list = new List<LichSuLaiSuat>();
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT MaLichSuLaiSuat, MaLoaiTietKiem, LaiSuatCuaKyHan, NgayApDung, NgayKetThuc
                      FROM lich_su_lai_suat
                      WHERE MaLoaiTietKiem = @maloai
                      ORDER BY NgayApDung ASC",
                    conn);
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
            }
            return list;
        }

        public ThamSo GetThamSo()
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT BoiSoTienGui, DoTuoiToiThieu, LoaiTietKiemGoi, SoTienGoiThemToiThieu FROM tham_so LIMIT 1",
                    conn);
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read()) return new ThamSo
                    {
                        BoiSoTienGui = rdr.GetDecimal(0),
                        DoTuoiToiThieu = rdr.GetInt32(1),
                        LoaiTietKiemGoi = rdr.GetString(2),
                        SoTienGoiThemToiThieu = rdr.GetDecimal(3)
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
                string sql = "INSERT INTO so_tiet_kiem (MaSo, MaLoaiTietKiem, TenKH, SoTien, NgaySinh, CCCD, NgayMoSo, DiaChi, TrangThai, SoDuToiThieu, NgayCapNhatGanNhat) " +
                    "VALUES (@maso, @maloai, @tenkh, @sotien, @ngaysinh, @cccd, @ngaymo, @diachi, @trangthai, @sodutoithieu, @ngaycapnhatgannhat)";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@maso", so.MaSo);
                cmd.Parameters.AddWithValue("@maloai", so.MaLoaiTietKiem);
                cmd.Parameters.AddWithValue("@tenkh", so.TenKH);
                cmd.Parameters.AddWithValue("@sotien", so.SoTien);
                cmd.Parameters.AddWithValue("@ngaysinh", so.NgaySinh);
                cmd.Parameters.AddWithValue("@cccd", so.CCCD);
                cmd.Parameters.AddWithValue("@ngaymo", so.NgayMoSo);
                cmd.Parameters.AddWithValue("@diachi", so.DiaChi);
                cmd.Parameters.AddWithValue("@trangthai", so.TrangThai);
                cmd.Parameters.AddWithValue("@sodutoithieu", so.SoDuToiThieu);
                cmd.Parameters.AddWithValue("@ngaycapnhatgannhat", so.NgayCapNhatGanNhat);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public List<SoTietKiem> SearchSoTietKiem(string keyword)
        {
            var list = new List<SoTietKiem>();
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT * 
                       FROM so_tiet_kiem 
                       WHERE MaSo LIKE @k 
                          OR TenKH LIKE @k";

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
                            MaLoaiTietKiem = rdr.GetInt32("MaLoaiTietKiem"),
                            TrangThai = rdr.GetBoolean("TrangThai"),
                            SoDuToiThieu = rdr.GetDecimal("SoDuToiThieu"),
                            NgayCapNhatGanNhat = rdr.IsDBNull(rdr.GetOrdinal("NgayCapNhatGanNhat"))
                                                       ? DateTime.MinValue
                                                       : rdr.GetDateTime("NgayCapNhatGanNhat")
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
                string sql = @"UPDATE so_tiet_kiem 
                       SET TenKH = @ten,
                           SoTien = @tien,
                           NgaySinh = @ns,
                           CCCD = @cccd,
                           DiaChi = @dc,
                           MaLoaiTietKiem = @loai,
                           NgayMoSo = @ngaymo,
                           TrangThai = @trangthai,
                           SoDuToiThieu = @sodutoithieu,
                           NgayCapNhatGanNhat = @ngaycapnhatgannhat
                       WHERE MaSo = @maso";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ten", so.TenKH);
                cmd.Parameters.AddWithValue("@tien", so.SoTien);
                cmd.Parameters.AddWithValue("@ns", so.NgaySinh);
                cmd.Parameters.AddWithValue("@cccd", so.CCCD);
                cmd.Parameters.AddWithValue("@dc", so.DiaChi);
                cmd.Parameters.AddWithValue("@loai", so.MaLoaiTietKiem);
                cmd.Parameters.AddWithValue("@ngaymo", so.NgayMoSo);
                cmd.Parameters.AddWithValue("@trangthai", so.TrangThai);
                cmd.Parameters.AddWithValue("@sodutoithieu", so.SoDuToiThieu);
                cmd.Parameters.AddWithValue("@maso", so.MaSo);
                cmd.Parameters.AddWithValue("@ngaycapnhatgannhat", so.NgayCapNhatGanNhat);
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

        public string GetNextMaSo()
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT CONCAT('STK', LPAD(
                COALESCE(MAX(CAST(SUBSTRING(MaSo, 4) AS UNSIGNED)), 0) + 1
              , 3, '0')) FROM so_tiet_kiem",
                    conn);
                return cmd.ExecuteScalar()?.ToString() ?? "STK001";
            }
        }

        /// <summary>
        /// Tìm kiếm nâng cao với tất cả bộ lọc từ màn hình Tra cứu.
        /// Mọi điều kiện đều tùy chọn – bỏ trống = bỏ qua điều kiện đó.
        /// Các filter liên quan đến loai_tiet_kiem, phieu_goi, phieu_rut được JOIN tương ứng.
        /// </summary>
        public List<SoTietKiem> SearchSoTietKiemAdvanced(SearchFilter f)
        {
            var list = new List<SoTietKiem>();
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();

                // Base query – JOIN loai để lọc theo TenLoai, ThoiGian, QuiDinh
                var sql = new System.Text.StringBuilder(@"
                    SELECT DISTINCT s.*
                    FROM so_tiet_kiem s
                    JOIN loai_tiet_kiem l ON s.MaLoaiTietKiem = l.MaLoaiTietKiem
                    WHERE 1=1");

                var p = new List<MySqlParameter>();

                // ── Row 1: Mã sổ, Tên KH, Ngày sinh, CCCD, Địa chỉ ──────────────
                if (!string.IsNullOrWhiteSpace(f.MaSo))
                {
                    sql.Append(" AND s.MaSo LIKE @maso");
                    p.Add(new MySqlParameter("@maso", "%" + f.MaSo.Trim() + "%"));
                }
                if (!string.IsNullOrWhiteSpace(f.TenKH))
                {
                    sql.Append(" AND s.TenKH LIKE @tenkh");
                    p.Add(new MySqlParameter("@tenkh", "%" + f.TenKH.Trim() + "%"));
                }
                if (f.NgaySinh.HasValue)
                {
                    sql.Append(" AND DATE(s.NgaySinh) = @ngaysinh");
                    p.Add(new MySqlParameter("@ngaysinh", f.NgaySinh.Value.Date));
                }
                if (!string.IsNullOrWhiteSpace(f.CCCD))
                {
                    sql.Append(" AND s.CCCD LIKE @cccd");
                    p.Add(new MySqlParameter("@cccd", "%" + f.CCCD.Trim() + "%"));
                }
                if (!string.IsNullOrWhiteSpace(f.DiaChi))
                {
                    sql.Append(" AND s.DiaChi LIKE @diachi");
                    p.Add(new MySqlParameter("@diachi", "%" + f.DiaChi.Trim() + "%"));
                }

                // ── Tên loại tiết kiệm (ComboBox) ────────────────────────────────
                if (!string.IsNullOrWhiteSpace(f.TenLoaiTietKiem) && f.TenLoaiTietKiem != "Tất cả")
                {
                    sql.Append(" AND l.TenLoaiTietKiem = @tenloai");
                    p.Add(new MySqlParameter("@tenloai", f.TenLoaiTietKiem));
                }

                // ── Trạng thái: "Mở" = 1, "Đóng" = 0 ───────────────────────────
                if (!string.IsNullOrWhiteSpace(f.TrangThai) && f.TrangThai != "Tất cả")
                {
                    sql.Append(" AND s.TrangThai = @trangthai");
                    p.Add(new MySqlParameter("@trangthai", f.TrangThai == "Mở" ? 1 : 0));
                }

                // ── Row 2: Ngày mở sổ (khoảng từ–đến) ──────────────────────────
                if (f.NgayMoSoTu.HasValue)
                {
                    sql.Append(" AND DATE(s.NgayMoSo) >= @ngaymotu");
                    p.Add(new MySqlParameter("@ngaymotu", f.NgayMoSoTu.Value.Date));
                }
                if (f.NgayMoSoDen.HasValue)
                {
                    sql.Append(" AND DATE(s.NgayMoSo) <= @ngaymoden");
                    p.Add(new MySqlParameter("@ngaymoden", f.NgayMoSoDen.Value.Date));
                }

                // ── Row 2: Ngày cập nhật gần nhất (khoảng từ–đến) ───────────────
                if (f.NgayCapNhatTu.HasValue)
                {
                    sql.Append(" AND DATE(s.NgayCapNhatGanNhat) >= @ngaycntu");
                    p.Add(new MySqlParameter("@ngaycntu", f.NgayCapNhatTu.Value.Date));
                }
                if (f.NgayCapNhatDen.HasValue)
                {
                    sql.Append(" AND DATE(s.NgayCapNhatGanNhat) <= @ngaycnden");
                    p.Add(new MySqlParameter("@ngaycnden", f.NgayCapNhatDen.Value.Date));
                }

                // ── Row 3: Số tiền (khoảng từ–đến) ──────────────────────────────
                if (f.SoTienTu.HasValue)
                {
                    sql.Append(" AND s.SoTien >= @sotientu");
                    p.Add(new MySqlParameter("@sotientu", f.SoTienTu.Value));
                }
                if (f.SoTienDen.HasValue)
                {
                    sql.Append(" AND s.SoTien <= @sotienden");
                    p.Add(new MySqlParameter("@sotienden", f.SoTienDen.Value));
                }

                // ── Row 3: Số dư tối thiểu (khoảng từ–đến) ──────────────────────
                if (f.SoDuToiThieuTu.HasValue)
                {
                    sql.Append(" AND s.SoDuToiThieu >= @sodutu");
                    p.Add(new MySqlParameter("@sodutu", f.SoDuToiThieuTu.Value));
                }
                if (f.SoDuToiThieuDen.HasValue)
                {
                    sql.Append(" AND s.SoDuToiThieu <= @soduden");
                    p.Add(new MySqlParameter("@soduden", f.SoDuToiThieuDen.Value));
                }

                // ── Row 4: Quy định thời gian rút tiền (ComboBox "X ngày") ───────
                if (!string.IsNullOrWhiteSpace(f.QuyDinhThoiGian) && f.QuyDinhThoiGian != "Tất cả")
                {
                    var parts = f.QuyDinhThoiGian.Split(' ');
                    if (parts.Length > 0 && int.TryParse(parts[0], out int thoiGian))
                    {
                        sql.Append(" AND l.ThoiGianRutTien = @thoigian");
                        p.Add(new MySqlParameter("@thoigian", thoiGian));
                    }
                }

                // ── Row 4: Quy định rút tiền (ComboBox) ─────────────────────────
                if (!string.IsNullOrWhiteSpace(f.QuyDinhRutTien) && f.QuyDinhRutTien != "Tất cả")
                {
                    sql.Append(" AND l.QuiDinhRutTien = @quydinhrut");
                    p.Add(new MySqlParameter("@quydinhrut", f.QuyDinhRutTien == "Rút 1 phần" ? 1 : 0));
                }

                // ── Row 4: Lãi suất — lọc qua lich_su_lai_suat (lãi suất hiện hành) ──
                // Vì LaiSuat đã bị xóa khỏi loai_tiet_kiem, lọc theo lãi suất đang áp dụng
                // (NgayApDung <= NOW() AND (NgayKetThuc IS NULL OR NgayKetThuc >= NOW()))
                if (!string.IsNullOrWhiteSpace(f.LaiSuat) && f.LaiSuat != "Tất cả")
                {
                    var lsStr = f.LaiSuat.Trim().TrimEnd('%').Trim();
                    bool parsed = decimal.TryParse(lsStr, System.Globalization.NumberStyles.Any,
                                                   System.Globalization.CultureInfo.InvariantCulture, out decimal ls)
                               || decimal.TryParse(lsStr, System.Globalization.NumberStyles.Any,
                                                   System.Globalization.CultureInfo.CurrentCulture, out ls);
                    if (parsed)
                    {
                        sql.Append(@" AND EXISTS (
                            SELECT 1 FROM lich_su_lai_suat ls
                            WHERE ls.MaLoaiTietKiem = l.MaLoaiTietKiem
                              AND ls.LaiSuatCuaKyHan = @laisuat
                              AND ls.NgayApDung <= NOW()
                              AND (ls.NgayKetThuc IS NULL OR ls.NgayKetThuc >= NOW()))");
                        p.Add(new MySqlParameter("@laisuat", ls));
                    }
                }

                // ── Row 5 & 6: Mã phiếu gởi, Ngày gởi, Số tiền gởi ─────────────
                bool hasGoiFilter = !string.IsNullOrWhiteSpace(f.MaPhieuGoi)
                                 || f.NgayGoiTu.HasValue || f.NgayGoiDen.HasValue
                                 || f.SoTienGoiTu.HasValue || f.SoTienGoiDen.HasValue;
                if (hasGoiFilter)
                {
                    sql.Append(@" AND EXISTS (
                        SELECT 1 FROM phieu_goi pg WHERE pg.MaSo = s.MaSo");
                    if (!string.IsNullOrWhiteSpace(f.MaPhieuGoi))
                    {
                        sql.Append(" AND pg.MaPhieuGoi LIKE @mapg");
                        p.Add(new MySqlParameter("@mapg", "%" + f.MaPhieuGoi.Trim() + "%"));
                    }
                    if (f.NgayGoiTu.HasValue)
                    {
                        sql.Append(" AND DATE(pg.NgayGoi) >= @ngaygoitu");
                        p.Add(new MySqlParameter("@ngaygoitu", f.NgayGoiTu.Value.Date));
                    }
                    if (f.NgayGoiDen.HasValue)
                    {
                        sql.Append(" AND DATE(pg.NgayGoi) <= @ngaygoiden");
                        p.Add(new MySqlParameter("@ngaygoiden", f.NgayGoiDen.Value.Date));
                    }
                    if (f.SoTienGoiTu.HasValue)
                    {
                        sql.Append(" AND pg.SoTienGoi >= @sotiengoitu");
                        p.Add(new MySqlParameter("@sotiengoitu", f.SoTienGoiTu.Value));
                    }
                    if (f.SoTienGoiDen.HasValue)
                    {
                        sql.Append(" AND pg.SoTienGoi <= @sotiengoiden");
                        p.Add(new MySqlParameter("@sotiengoiden", f.SoTienGoiDen.Value));
                    }
                    sql.Append(")");
                }

                // ── Row 5 & 6: Mã phiếu rút, Ngày rút, Số tiền rút ─────────────
                bool hasRutFilter = !string.IsNullOrWhiteSpace(f.MaPhieuRut)
                                 || f.NgayRutTu.HasValue || f.NgayRutDen.HasValue
                                 || f.SoTienRutTu.HasValue || f.SoTienRutDen.HasValue;
                if (hasRutFilter)
                {
                    sql.Append(@" AND EXISTS (
                        SELECT 1 FROM phieu_rut pr WHERE pr.MaSo = s.MaSo");
                    if (!string.IsNullOrWhiteSpace(f.MaPhieuRut))
                    {
                        sql.Append(" AND pr.MaPhieuRut LIKE @mapr");
                        p.Add(new MySqlParameter("@mapr", "%" + f.MaPhieuRut.Trim() + "%"));
                    }
                    if (f.NgayRutTu.HasValue)
                    {
                        sql.Append(" AND DATE(pr.NgayRut) >= @ngayruttu");
                        p.Add(new MySqlParameter("@ngayruttu", f.NgayRutTu.Value.Date));
                    }
                    if (f.NgayRutDen.HasValue)
                    {
                        sql.Append(" AND DATE(pr.NgayRut) <= @ngayruden");
                        p.Add(new MySqlParameter("@ngayruden", f.NgayRutDen.Value.Date));
                    }
                    if (f.SoTienRutTu.HasValue)
                    {
                        sql.Append(" AND pr.SoTienRut >= @sotienruttu");
                        p.Add(new MySqlParameter("@sotienruttu", f.SoTienRutTu.Value));
                    }
                    if (f.SoTienRutDen.HasValue)
                    {
                        sql.Append(" AND pr.SoTienRut <= @sotienruden");
                        p.Add(new MySqlParameter("@sotienruden", f.SoTienRutDen.Value));
                    }
                    sql.Append(")");
                }

                sql.Append(" ORDER BY s.MaSo");

                var cmd = new MySqlCommand(sql.ToString(), conn);
                cmd.Parameters.AddRange(p.ToArray());

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
                            MaLoaiTietKiem = rdr.GetInt32("MaLoaiTietKiem"),
                            TrangThai = rdr.GetBoolean("TrangThai"),
                            SoDuToiThieu = rdr.GetDecimal("SoDuToiThieu"),
                            NgayCapNhatGanNhat = rdr.IsDBNull(rdr.GetOrdinal("NgayCapNhatGanNhat"))
                                                     ? DateTime.MinValue
                                                     : rdr.GetDateTime("NgayCapNhatGanNhat")
                        });
                    }
                }
            }
            return list;
        }

        public LoaiTietKiem GetLoaiTietKiemByMaSo(string maSo)
        {
            SoTietKiem So = SearchSoTietKiem(maSo)[0];
            int maloaitietkiem = So.MaLoaiTietKiem;
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                // Đã bỏ LaiSuat khỏi SELECT
                var cmd = new MySqlCommand(
                    "SELECT MaLoaiTietKiem, TenLoaiTietKiem, ThoiGianRutTien, QuiDinhRutTien, TienGoiToiThieu FROM loai_tiet_kiem WHERE MaLoaiTietKiem = @maloaitietkiem",
                    conn);
                cmd.Parameters.AddWithValue("@maloaitietkiem", maloaitietkiem);
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

        // Phieu Rut tien

        /// <summary>
        /// Lấy báo cáo doanh thu gom nhóm theo Loại tiết kiệm trong tháng.
        /// TongThu = tổng tiền gởi, TongChi = tổng tiền rút trong tháng đó.
        /// </summary>
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

        // ============================================================
        // Bổ sung cho thuật toán lập báo cáo 9 bước
        // ============================================================

        /// <summary>
        /// Bước 03: Lấy danh sách Sổ tiết kiệm theo Mã loại tiết kiệm.
        /// </summary>
        public List<SoTietKiem> GetSoTietKiemByLoai(int maLoaiTietKiem)
        {
            var list = new List<SoTietKiem>();
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT * FROM so_tiet_kiem WHERE MaLoaiTietKiem = @maloai", conn);
                cmd.Parameters.AddWithValue("@maloai", maLoaiTietKiem);
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
                            MaLoaiTietKiem = rdr.GetInt32("MaLoaiTietKiem"),
                            TrangThai = rdr.GetBoolean("TrangThai"),
                            SoDuToiThieu = rdr.GetDecimal("SoDuToiThieu"),
                            NgayCapNhatGanNhat = rdr.IsDBNull(rdr.GetOrdinal("NgayCapNhatGanNhat"))
                                                     ? DateTime.MinValue
                                                     : rdr.GetDateTime("NgayCapNhatGanNhat")
                        });
                    }
                }
            }
            return list;
        }

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