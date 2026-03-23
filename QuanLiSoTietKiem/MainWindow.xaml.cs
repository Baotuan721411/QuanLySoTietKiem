using System;
using System.Windows;
using System.Data;
using MySql.Data.MySqlClient;
using System.Globalization;

namespace QuanLiSoTietKiem
{
    public partial class MainWindow : Window
    {
        private string connectionString = "Server=localhost;Database=quan_ly_so_tiet_kiem;Uid=root;Pwd=123456;";

        public MainWindow()
        {
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage("vi-VN");
            InitializeComponent();

            dpNgayMo.SelectedDate = DateTime.Now;
            LoadComboBoxLoaiTK();
        }

        // Hàm lấy danh sách loại tiết kiệm từ Database
        private void LoadComboBoxLoaiTK()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT MaLoaiTietKiem, TenLoaiTietKiem FROM loai_tiet_kiem";
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cbLoaiTK.ItemsSource = dt.DefaultView;
                    cbLoaiTK.DisplayMemberPath = "TenLoaiTietKiem";
                    cbLoaiTK.SelectedValuePath = "MaLoaiTietKiem";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải danh sách loại tiết kiệm: " + ex.Message, "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnTiepNhan_Click(object sender, RoutedEventArgs e)
        {
            // 1. Kiểm tra các trường văn bản không được để trống
            if (string.IsNullOrEmpty(txtTenKH.Text) || string.IsNullOrEmpty(txtSoTien.Text) ||
                string.IsNullOrEmpty(txtCCCD.Text) || string.IsNullOrEmpty(txtDiaChi.Text) ||
                cbLoaiTK.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin khách hàng và loại tiết kiệm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Kiểm tra ngày sinh (Khắc phục lỗi "can not be null")
            if (dpNgaySinh.SelectedDate == null)
            {
                MessageBox.Show("Ngày sinh không được để trống! Vui lòng chọn ngày sinh.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // 3. Lấy tham số quy định từ bảng tham_so
                    decimal soTienGui = decimal.Parse(txtSoTien.Text);
                    decimal minAmount = 0;
                    decimal boiSo = 0;
                    int doTuoiToiThieu = 0;

                    string sqlThamSo = "SELECT SoTienToiThieu, BoiSoTienGui, DoTuoiToiThieu FROM tham_so WHERE Id = 1";
                    MySqlCommand cmdTS = new MySqlCommand(sqlThamSo, conn);
                    using (MySqlDataReader rdr = cmdTS.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            minAmount = rdr.GetDecimal("SoTienToiThieu");
                            boiSo = rdr.GetDecimal("BoiSoTienGui");
                            doTuoiToiThieu = rdr.GetInt32("DoTuoiToiThieu");
                        }
                    }

                    // --- KIỂM TRA LOGIC NGHIỆP VỤ ---

                    // A. Kiểm tra độ tuổi tối thiểu (Tính chính xác đến ngày hiện tại)
                    DateTime ngaySinh = dpNgaySinh.SelectedDate.Value;
                    int tuoi = DateTime.Now.Year - ngaySinh.Year;
                    if (ngaySinh.Date > DateTime.Now.AddYears(-tuoi)) tuoi--;

                    if (tuoi < doTuoiToiThieu)
                    {
                        MessageBox.Show($"Khách hàng chưa đủ tuổi mở sổ (Yêu cầu ít nhất {doTuoiToiThieu} tuổi, hiện tại {tuoi} tuổi).", "Quy định độ tuổi", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }

                    // B. Kiểm tra số tiền gửi tối thiểu
                    if (soTienGui < minAmount)
                    {
                        MessageBox.Show($"Số tiền gửi không được thấp hơn mức tối thiểu: {minAmount:N0} VNĐ", "Quy định số tiền", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }

                    // C. Kiểm tra bội số tiền gửi (Dùng toán tử % lấy dư)
                    if (soTienGui % boiSo != 0)
                    {
                        MessageBox.Show($"Số tiền gửi phải là bội số của {boiSo:N0} VNĐ.\nGợi ý: {Math.Ceiling(soTienGui / boiSo) * boiSo:N0} VNĐ", "Quy định bội số", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }

                    // --- NẾU MỌI THỨ HỢP LỆ -> LƯU VÀO DATABASE ---
                    string sql = "INSERT INTO so_tiet_kiem (MaSo, MaLoaiTietKiem, TenKH, SoTien, NgaySinh, CCCD, NgayMoSo, DiaChi) " +
                                 "VALUES (@maso, @maloai, @tenkh, @sotien, @ngaysinh, @cccd, @ngaymo, @diachi)";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);

                    // Phát sinh mã sổ dựa trên thời gian thực
                    string maTuDong = "STK" + DateTime.Now.ToString("yyyyMMddHHmmss");

                    cmd.Parameters.AddWithValue("@maso", maTuDong);
                    cmd.Parameters.AddWithValue("@maloai", cbLoaiTK.SelectedValue);
                    cmd.Parameters.AddWithValue("@tenkh", txtTenKH.Text);
                    cmd.Parameters.AddWithValue("@sotien", soTienGui);
                    cmd.Parameters.AddWithValue("@ngaysinh", ngaySinh);
                    cmd.Parameters.AddWithValue("@cccd", txtCCCD.Text);
                    cmd.Parameters.AddWithValue("@ngaymo", dpNgayMo.SelectedDate);
                    cmd.Parameters.AddWithValue("@diachi", txtDiaChi.Text);

                    cmd.ExecuteNonQuery();

                    txtMaSo.Text = maTuDong; // Hiển thị mã sổ vừa lưu
                    MessageBox.Show("Tiếp nhận sổ tiết kiệm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Số tiền gửi phải là ký tự số!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            txtMaSo.Text = "<Giá trị phát sinh>";
            txtTenKH.Clear();
            txtCCCD.Clear();
            txtSoTien.Clear();
            txtDiaChi.Clear();
            dpNgaySinh.SelectedDate = null;
            dpNgayMo.SelectedDate = DateTime.Now;
            cbLoaiTK.SelectedIndex = -1;
        }

        private void btnThoat_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn thoát chương trình?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        // Các hàm bổ trợ (Sẽ viết tiếp khi bạn làm chức năng tra cứu)
        private void btnTimKiem_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Tính năng tìm kiếm đang phát triển."); }
        private void btnXoa_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Tính năng xóa đang phát triển."); }
        private void btnCapNhat_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Tính năng cập nhật đang phát triển."); }
    }
}