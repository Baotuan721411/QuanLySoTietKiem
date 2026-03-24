using QuanLiSoTietKiem.QuanLy.DAL;
using QuanLiSoTietKiem.QuanLy.Models;
using System;
using System.Collections.Generic;

namespace QuanLiSoTietKiem.QuanLy.BLL
{
    public class CheckRequirements
    {
        private readonly DataProcessing _db = new DataProcessing();

        public List<LoaiTietKiem> GetLoaiTietKiems() => _db.GetLoaiTietKiems();

        // 1. Tìm sổ theo từ khóa
        public List<SoTietKiem> SearchSoTietKiem(string keyword) => _db.SearchSoTietKiem(keyword);

        // 2. Xóa sổ
        public bool DeleteSoTietKiem(string maSo) => _db.DeleteSoTietKiem(maSo);

        // 3. Tiếp nhận (Thêm mới)
        public string ValidateAndSubmit(SoTietKiem so)
        {
            string checkResult = CheckBusinessRules(so);
            if (checkResult != "SUCCESS") return checkResult;

            so.MaSo = "STK" + DateTime.Now.ToString("yyyyMMddHHmmss");
            return _db.SaveSoTietKiem(so) ? "SUCCESS" : "Lỗi lưu Database!";
        }

        // 4. Cập nhật sổ hiện có
        public string ValidateAndUpdate(SoTietKiem so)
        {
            string checkResult = CheckBusinessRules(so);
            if (checkResult != "SUCCESS") return checkResult;

            return _db.UpdateSoTietKiem(so) ? "SUCCESS" : "Lỗi cập nhật Database!";
        }

        // HÀM DÙNG CHUNG: Kiểm tra quy định tuổi tại ngày mở sổ và tiền gửi
        private string CheckBusinessRules(SoTietKiem so)
        {
            var ts = _db.GetThamSo();
            if (ts == null) return "Lỗi hệ thống: Không thể lấy tham số!";

            // Tính tuổi dựa trên ngày sinh so với ngày mở sổ
            int tuoi = so.NgayMoSo.Year - (so.NgaySinh?.Year ?? so.NgayMoSo.Year);
            if (so.NgaySinh?.Date > so.NgayMoSo.AddYears(-tuoi)) tuoi--;

            if (tuoi < ts.DoTuoiToiThieu)
                return $"Tại ngày mở sổ ({so.NgayMoSo:dd/MM/yyyy}), khách hàng mới {tuoi} tuổi, chưa đủ {ts.DoTuoiToiThieu} tuổi!";

            if (so.SoTien < ts.SoTienToiThieu)
                return $"Số tiền tối thiểu: {ts.SoTienToiThieu:N0} VNĐ";

            if (so.SoTien % ts.BoiSoTienGui != 0)
                return $"Tiền gửi phải là bội số của {ts.BoiSoTienGui:N0} VNĐ";

            return "SUCCESS";
        }
    }
}