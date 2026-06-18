-- ============================================================
-- Tạo mới toàn bộ database quản lý sổ tiết kiệm
-- Chạy file này một lần duy nhất trên môi trường sạch
-- ============================================================

CREATE DATABASE IF NOT EXISTS quan_ly_so_tiet_kiem
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE quan_ly_so_tiet_kiem;

-- ============================================================
-- 1. BẢNG LOẠI TIẾT KIỆM
--    LaiSuat KHÔNG có ở đây — lãi suất quản lý qua lich_su_lai_suat
-- ============================================================
CREATE TABLE loai_tiet_kiem (
    MaLoaiTietKiem  INT            NOT NULL AUTO_INCREMENT,
    TenLoaiTietKiem VARCHAR(50)    NOT NULL UNIQUE,
    ThoiGianRutTien INT            NOT NULL,           -- đơn vị: ngày
    QuiDinhRutTien  INT            NOT NULL DEFAULT 0, -- 0: rút toàn bộ | 1: rút một phần
    TienGoiToiThieu DECIMAL(15,2)  NOT NULL DEFAULT 200000,

    PRIMARY KEY (MaLoaiTietKiem)
);

-- ============================================================
-- 2. BẢNG LỊCH SỬ LÃI SUẤT
--    NgayKetThuc = NULL → giai đoạn đang có hiệu lực
-- ============================================================
CREATE TABLE lich_su_lai_suat (
    MaLichSuLaiSuat INT            NOT NULL AUTO_INCREMENT,
    MaLoaiTietKiem  INT            NOT NULL,
    LaiSuatCuaKyHan DECIMAL(5,2)  NOT NULL,  -- ví dụ: 0.15 = 0.15%/kỳ
    NgayApDung      DATE           NOT NULL,
    NgayKetThuc     DATE           NULL,      -- NULL = đang áp dụng

    PRIMARY KEY (MaLichSuLaiSuat),
    CONSTRAINT fk_lsls_loai
        FOREIGN KEY (MaLoaiTietKiem)
        REFERENCES loai_tiet_kiem (MaLoaiTietKiem)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

-- ============================================================
-- 3. BẢNG THAM SỐ HỆ THỐNG
--    Chỉ có đúng 1 dòng (Id luôn = 1)
-- ============================================================
CREATE TABLE tham_so (
    Id                    INT            NOT NULL DEFAULT 1,
    BoiSoTienGui          DECIMAL(15,2)  NOT NULL,
    DoTuoiToiThieu        INT            NOT NULL,
    LoaiTietKiemGoi       VARCHAR(50)    NULL,
    SoTienGoiThemToiThieu DECIMAL(18,2)  NULL,

    PRIMARY KEY (Id),
    CONSTRAINT chk_only_one_row CHECK (Id = 1)
);

-- ============================================================
-- 4. BẢNG SỔ TIẾT KIỆM
-- ============================================================
CREATE TABLE so_tiet_kiem (
    MaSo                VARCHAR(20)    NOT NULL,
    MaLoaiTietKiem      INT            NOT NULL,
    TenKH               VARCHAR(100)   NOT NULL,
    SoTien              DECIMAL(15,2)  NOT NULL,
    NgaySinh            DATE           NOT NULL,
    CCCD                VARCHAR(20)    NOT NULL,
    NgayMoSo            DATE           NOT NULL DEFAULT (CURRENT_DATE),
    DiaChi              VARCHAR(255)   NULL,
    TrangThai           BOOLEAN        NOT NULL DEFAULT TRUE,  -- TRUE: đang mở | FALSE: đã đóng
    SoDuToiThieu        DECIMAL(15,2)  NOT NULL DEFAULT 0,
    NgayCapNhatGanNhat  DATE           NULL,                   -- NULL: chưa từng rút/gửi thêm

    PRIMARY KEY (MaSo),
    CONSTRAINT fk_stk_loai
        FOREIGN KEY (MaLoaiTietKiem)
        REFERENCES loai_tiet_kiem (MaLoaiTietKiem)
);

-- ============================================================
-- 5. BẢNG PHIẾU GỬI
-- ============================================================
CREATE TABLE phieu_goi (
    MaPhieuGoi  VARCHAR(10)    NOT NULL,
    MaSo        VARCHAR(20)    NOT NULL,
    SoTienGoi   DECIMAL(15,2)  NOT NULL,
    NgayGoi     DATE           NOT NULL,

    PRIMARY KEY (MaPhieuGoi),
    CONSTRAINT fk_pg_stk
        FOREIGN KEY (MaSo)
        REFERENCES so_tiet_kiem (MaSo)
);

-- ============================================================
-- 6. BẢNG PHIẾU RÚT
-- ============================================================
CREATE TABLE phieu_rut (
    MaPhieuRut  VARCHAR(10)    NOT NULL,
    MaSo        VARCHAR(20)    NOT NULL,
    SoTienRut   DECIMAL(15,2)  NOT NULL,
    NgayRut     DATE           NOT NULL,

    PRIMARY KEY (MaPhieuRut),
    CONSTRAINT fk_pr_stk
        FOREIGN KEY (MaSo)
        REFERENCES so_tiet_kiem (MaSo)
);

-- ============================================================
-- 7. DỮ LIỆU KHỞI TẠO — Tham số hệ thống
-- ============================================================
INSERT INTO tham_so (Id, BoiSoTienGui, DoTuoiToiThieu, LoaiTietKiemGoi, SoTienGoiThemToiThieu)
VALUES (1, 10000, 18, 'Không kỳ hạn', 100000);

-- ============================================================
-- 8. DỮ LIỆU KHỞI TẠO — Loại tiết kiệm
-- ============================================================
INSERT INTO loai_tiet_kiem (MaLoaiTietKiem, TenLoaiTietKiem, ThoiGianRutTien, QuiDinhRutTien, TienGoiToiThieu)
VALUES
(1, 'Không kỳ hạn', 15,  1, 200000),  -- rút một phần
(2, '6 tháng',      180, 0, 200000),  -- rút toàn bộ
(3, '12 tháng',     360, 0, 200000);  -- rút toàn bộ

-- ============================================================
-- 9. DỮ LIỆU KHỞI TẠO — Lịch sử lãi suất ban đầu
--    NgayApDung đặt là ngày tạo DB; điều chỉnh nếu cần
-- ============================================================
INSERT INTO lich_su_lai_suat (MaLoaiTietKiem, LaiSuatCuaKyHan, NgayApDung, NgayKetThuc)
VALUES
(1, 0.15, '2026-01-01', NULL),  -- Không kỳ hạn: 0.15%/kỳ, đang áp dụng
(2, 0.50, '2026-01-01', NULL),  -- 6 tháng:      0.50%/kỳ, đang áp dụng
(3, 0.55, '2026-01-01', NULL);  -- 12 tháng:     0.55%/kỳ, đang áp dụng

-- ============================================================
-- 10. DỮ LIỆU MẪU — Sổ tiết kiệm
-- ============================================================
INSERT INTO so_tiet_kiem
    (MaSo, MaLoaiTietKiem, TenKH, SoTien, NgaySinh, CCCD, NgayMoSo, DiaChi, TrangThai, SoDuToiThieu, NgayCapNhatGanNhat)
VALUES
('STK001', 1, 'Bảo Tuấn',   4000000, '1985-07-05', '23423',  '2026-05-17', 'Bảo Lộc', TRUE, 0, NULL),
('STK002', 2, 'Long Vũ',    3000000, '2006-02-23', '43546',  '2026-05-17', 'Huế',     TRUE, 0, NULL),
('STK003', 3, 'Thanh Tùng', 6000000, '2006-04-16', '3245',   '2026-05-17', 'Bảo Lâm', TRUE, 0, NULL),
('STK004', 1, 'Bảo Huy',   7000000, '2006-04-26', '646475', '2026-05-17', 'Đà Lạt',  TRUE, 0, NULL),
('STK005', 2, 'Hoàng Vũ',  8000000, '2006-04-14', '536586', '2026-05-17', 'Vũng Tàu',TRUE, 0, NULL);

-- ============================================================
-- 11. KIỂM TRA KẾT QUẢ
-- ============================================================
SELECT 'loai_tiet_kiem'   AS bang, COUNT(*) AS so_dong FROM loai_tiet_kiem
UNION ALL
SELECT 'lich_su_lai_suat', COUNT(*) FROM lich_su_lai_suat
UNION ALL
SELECT 'tham_so',          COUNT(*) FROM tham_so
UNION ALL
SELECT 'so_tiet_kiem',     COUNT(*) FROM so_tiet_kiem;
