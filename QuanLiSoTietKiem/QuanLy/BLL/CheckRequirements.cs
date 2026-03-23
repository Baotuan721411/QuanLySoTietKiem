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

        public string ValidateAndSubmit(SoTietKiem so)
        {
            var ts = _db.GetThamSo();
            if (ts == null) return "Lỗi hệ thống: Không thể lấy tham số!";

            int tuoi = DateTime.Now.Year - (so.NgaySinh?.Year ?? DateTime.Now.Year);
            if (so.NgaySinh?.Date > DateTime.Now.AddYears(-tuoi)) tuoi--;

            if (tuoi < ts.DoTuoiToiThieu) return $"Khách hàng chưa đủ {ts.DoTuoiToiThieu} tuổi!";
            if (so.SoTien < ts.SoTienToiThieu) return $"Số tiền tối thiểu: {ts.SoTienToiThieu:N0} VNĐ";
            if (so.SoTien % ts.BoiSoTienGui != 0) return $"Tiền gửi phải là bội số của {ts.BoiSoTienGui:N0} VNĐ";

            so.MaSo = "STK" + DateTime.Now.ToString("yyyyMMddHHmmss");
            return _db.SaveSoTietKiem(so) ? "SUCCESS" : "Lỗi lưu Database!";
        }
    }
}