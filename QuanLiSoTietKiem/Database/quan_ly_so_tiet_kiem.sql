CREATE DATABASE IF NOT EXISTS quan_ly_so_tiet_kiem;
USE quan_ly_so_tiet_kiem;

-- 1. Bảng Loại Tiết Kiệm: Để ID tự động tăng
CREATE TABLE loai_tiet_kiem (
    MaLoaiTietKiem INT AUTO_INCREMENT PRIMARY KEY,
    TenLoaiTietKiem VARCHAR(50) NOT NULL UNIQUE
);

-- 2. Bảng Sổ Tiết Kiệm: Liên kết với ID của Loại Tiết Kiệm
CREATE TABLE so_tiet_kiem (
    MaSo VARCHAR(20) PRIMARY KEY,
    MaLoaiTietKiem INT NOT NULL,
    TenKH VARCHAR(100) NOT NULL,
    SoTien DECIMAL(15, 2) NOT NULL, 
    NgaySinh DATE NOT NULL,
    CCCD VARCHAR(20) NOT NULL,
    NgayMoSo DATE NOT NULL DEFAULT (CURRENT_DATE),
    DiaChi VARCHAR(255),
    FOREIGN KEY (MaLoaiTietKiem) REFERENCES loai_tiet_kiem(MaLoaiTietKiem)
);

-- 3. Bảng Tham Số: Có khóa chính để quản lý chuẩn hơn
CREATE TABLE tham_so (
    Id INT PRIMARY KEY DEFAULT 1, -- Luôn là dòng số 1
    SoTienToiThieu DECIMAL(15, 2) NOT NULL,
    BoiSoTienGui DECIMAL(15, 2) NOT NULL,
    DoTuoiToiThieu INT NOT NULL,so_tiet_kiem
    CONSTRAINT chk_only_one_row CHECK (Id = 1) -- Đảm bảo bảng chỉ có duy nhất 1 cấu hình
);

-- 4. Chèn dữ liệu khởi tạo
INSERT INTO quan_ly_so_tiet_kiem.loai_tiet_kiem (TenLoaiTietKiem) VALUES 
('Không kỳ hạn'),
('6 tháng'), 
('12 tháng');

INSERT INTO tham_so (Id, SoTienToiThieu, BoiSoTienGui, DoTuoiToiThieu) 
VALUES (1, 200000, 10000, 18);
delete from quan_ly_so_tiet_kiem.so_tiet_kiem
where MaLoaiTietKiem = 3;
-- 5. Tạo bảng phiếu gởi tiền 
CREATE TABLE phieu_goi (
    MaPhieuGoi VARCHAR(10) PRIMARY KEY,
    MaSo VARCHAR(20) NOT NULL,
    SoTienGoi DECIMAL(15, 2) NOT NULL,
    NgayGoi DATE NOT NULL,
    FOREIGN KEY (MaSo) REFERENCES so_tiet_kiem(MaSo)
);
ALTER TABLE tham_so 
ADD COLUMN LoaiTietKiemGoi VARCHAR(50),
ADD COLUMN SoTienGoiThemToiThieu DECIMAL(18,2);
UPDATE tham_so SET LoaiTietKiemGoi = 'Không kỳ hạn', SoTienGoiThemToiThieu = 100000 LIMIT 1;
