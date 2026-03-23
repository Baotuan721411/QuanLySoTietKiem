# QuanLySoTietKiem — Quản Lý Sổ Tiết Kiệm

Ứng dụng desktop quản lý sổ tiết kiệm, xây dựng bằng WPF (.NET Framework 4.7.2) kết nối MySQL.

## Tính năng chính

- **Tiếp nhận sổ mới** — nhập thông tin khách hàng, tạo mã sổ tự động, lưu vào DB
- **Tìm kiếm** — tìm sổ theo mã sổ hoặc tên khách hàng
- **Cập nhật** — chỉnh sửa thông tin sổ tiết kiệm đã tồn tại
- **Xóa** — xóa sổ tiết kiệm với xác nhận
- **Validation** — kiểm tra tuổi tối thiểu, số tiền tối thiểu, bội số tiền gửi (theo tham số hệ thống)

## Tech Stack

| Component | Technology |
|-----------|-----------|
| UI | WPF (XAML) |
| Framework | .NET Framework 4.7.2 |
| Pattern | MVVM |
| Database | MySQL 8.x |
| DB Connector | MySql.Data 8.0.33 |

## Cấu trúc thư mục

```
QuanLySoTietKiem/                  ← repo root
├── README.md
├── QuanLySoTietKiem.slnx          ← solution file
├── Database/
│   └── schema.sql                 ← script tạo database & dữ liệu mẫu
├── packages/                      ← NuGet packages (auto-restored)
└── QuanLySoTietKiem/              ← project folder
    ├── App.config                 ← connection string & config
    ├── App.xaml / App.xaml.cs
    ├── QuanLySoTietKiem.csproj
    ├── Data/
    │   └── SavingsRepository.cs   ← data access layer (MySQL CRUD)
    ├── Helpers/
    │   └── RelayCommand.cs        ← ICommand helper
    ├── Models/
    │   ├── LoaiTietKiem.cs        ← loại tiết kiệm
    │   ├── SoTietKiem.cs          ← sổ tiết kiệm
    │   └── ThamSo.cs              ← tham số hệ thống
    ├── Services/
    │   └── SavingsService.cs      ← business logic & validation
    ├── ViewModels/
    │   └── MainViewModel.cs       ← ViewModel chính
    ├── Views/
    │   └── MainWindow.xaml/.cs    ← giao diện chính
    └── Properties/
```

## Cài đặt Database

### Yêu cầu
- MySQL Server 8.x đang chạy trên `localhost`

### Các bước

1. Mở MySQL Workbench hoặc terminal MySQL
2. Chạy file `Database/schema.sql`:
   ```sql
   source Database/schema.sql;
   ```
3. Script sẽ tạo database `quan_ly_so_tiet_kiem` với 3 bảng:
   - `loai_tiet_kiem` — loại tiết kiệm (3 loại mặc định)
   - `so_tiet_kiem` — sổ tiết kiệm
   - `tham_so` — tham số hệ thống

### Cấu hình kết nối

Connection string nằm trong `QuanLySoTietKiem/App.config`:

```xml
<connectionStrings>
    <add name="DefaultConnection"
         connectionString="Server=localhost;Database=quan_ly_so_tiet_kiem;Uid=root;Pwd=123456;SslMode=None;"
         providerName="MySql.Data.MySqlClient" />
</connectionStrings>
```

> **Lưu ý:** Nếu MySQL của bạn dùng username/password khác, hãy sửa `Uid` và `Pwd` trong file này.

## Build & Run

### Yêu cầu
- Visual Studio 2019+ với workload **.NET desktop development**
- .NET Framework 4.7.2 targeting pack
- MySQL Server 8.x

### Cách chạy

1. Mở `QuanLySoTietKiem.slnx` bằng Visual Studio
2. Restore NuGet packages (tự động hoặc: Build → Restore NuGet Packages)
3. Chọn **Debug** → **Start Debugging** (F5)

Hoặc dùng command line:
```bash
msbuild QuanLySoTietKiem.slnx /t:Restore;Build /p:Configuration=Debug
```

## Hạn chế hiện tại

- Chưa có hệ thống đăng nhập / phân quyền
- Chưa hỗ trợ gửi thêm / rút tiền từ sổ đã mở
- Password DB lưu plaintext trong App.config (chỉ phù hợp cho local development)
- Chưa có báo cáo thống kê
