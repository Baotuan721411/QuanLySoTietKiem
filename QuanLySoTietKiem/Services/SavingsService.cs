using QuanLySoTietKiem.Data;
using QuanLySoTietKiem.Models;
using System;
using System.Collections.Generic;

namespace QuanLySoTietKiem.Services
{
    /// <summary>
    /// Business logic layer — validates input and delegates to the repository.
    /// </summary>
    public class SavingsService
    {
        private readonly SavingsRepository _repo = new SavingsRepository();

        public List<LoaiTietKiem> GetLoaiTietKiems() => _repo.GetLoaiTietKiems();

        public List<SoTietKiem> GetAllSoTietKiem() => _repo.GetAllSoTietKiem();

        public List<SoTietKiem> SearchSoTietKiem(string keyword) => _repo.SearchSoTietKiem(keyword);

        /// <summary>
        /// Xác nhận dữ liệu và lưu sổ tiết kiệm mới.
        /// Trả về "SUCCESS" nếu thành công, hoặc chuỗi lỗi mô tả.
        /// </summary>
        public string ValidateAndSubmit(SoTietKiem so)
        {
            var ts = _repo.GetThamSo();
            if (ts == null) return "Lỗi hệ thống: Không thể lấy tham số!";

            // Tính tuổi tại ngày mở sổ
            int tuoi = so.NgayMoSo.Year - (so.NgaySinh?.Year ?? so.NgayMoSo.Year);
            if (so.NgaySinh?.Date > so.NgayMoSo.AddYears(-tuoi)) tuoi--;

            if (tuoi < ts.DoTuoiToiThieu)
                return $"Tại ngày mở sổ ({so.NgayMoSo:dd/MM/yyyy}), khách hàng mới {tuoi} tuổi, chưa đủ {ts.DoTuoiToiThieu} tuổi theo quy định!";

            if (so.SoTien < ts.SoTienToiThieu)
                return $"Số tiền tối thiểu: {ts.SoTienToiThieu:N0} VNĐ";

            if (so.SoTien % ts.BoiSoTienGui != 0)
                return $"Tiền gửi phải là bội số của {ts.BoiSoTienGui:N0} VNĐ";

            // Tạo mã sổ tự động theo thời gian
            so.MaSo = "STK" + DateTime.Now.ToString("yyyyMMddHHmmss");

            return _repo.SaveSoTietKiem(so) ? "SUCCESS" : "Lỗi lưu Database!";
        }

        /// <summary>
        /// Xác nhận dữ liệu và cập nhật sổ tiết kiệm đã tồn tại.
        /// </summary>
        public string ValidateAndUpdate(SoTietKiem so)
        {
            var ts = _repo.GetThamSo();
            if (ts == null) return "Lỗi hệ thống: Không thể lấy tham số!";

            // Tính tuổi tại ngày mở sổ
            int tuoi = so.NgayMoSo.Year - (so.NgaySinh?.Year ?? so.NgayMoSo.Year);
            if (so.NgaySinh?.Date > so.NgayMoSo.AddYears(-tuoi)) tuoi--;

            if (tuoi < ts.DoTuoiToiThieu)
                return $"Tại ngày mở sổ ({so.NgayMoSo:dd/MM/yyyy}), khách hàng mới {tuoi} tuổi, chưa đủ {ts.DoTuoiToiThieu} tuổi theo quy định!";

            if (so.SoTien < ts.SoTienToiThieu)
                return $"Số tiền tối thiểu: {ts.SoTienToiThieu:N0} VNĐ";

            if (so.SoTien % ts.BoiSoTienGui != 0)
                return $"Tiền gửi phải là bội số của {ts.BoiSoTienGui:N0} VNĐ";

            return _repo.UpdateSoTietKiem(so) ? "SUCCESS" : "Lỗi cập nhật Database!";
        }

        /// <summary>
        /// Xóa sổ tiết kiệm theo mã sổ.
        /// </summary>
        public bool DeleteSoTietKiem(string maSo) => _repo.DeleteSoTietKiem(maSo);
    }
}
