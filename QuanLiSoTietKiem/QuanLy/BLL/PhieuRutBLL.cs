using QuanLiSoTietKiem.QuanLy.DAL;
using QuanLiSoTietKiem.QuanLy.Models;
using System;

namespace QuanLiSoTietKiem.QuanLy.BLL
{
    public class PhieuRutBLL
    {
        private readonly SoTietKiemDataProcessing _db = new SoTietKiemDataProcessing();
        private readonly PhieuRutDataProcessing _dbRut = new PhieuRutDataProcessing();

        public string GetNextMaPhieuRut() => _dbRut.GetNextMaPhieuRut();

        public string ValidateAndSavePhieuRut(PhieuRut phieu, string tenKH)
        {
            var listSo = _db.SearchSoTietKiem(phieu.MaSo);
            if (listSo == null || listSo.Count == 0)
                return "Mã sổ tiết kiệm không tồn tại trên hệ thống!";

            var so = listSo[0];

            if (string.IsNullOrWhiteSpace(tenKH) ||
                !string.Equals(so.TenKH.Trim(), tenKH.Trim(), StringComparison.OrdinalIgnoreCase))
                return "Họ tên khách hàng không khớp với chủ sở hữu sổ tiết kiệm này!";

            return _dbRut.SavePhieuRutTien(phieu);
        }
    }
}