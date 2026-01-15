using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSV.Entity;
using QLSV.Models;
using System.Data;
using System.Linq;
using System.Security.Claims;
using static QLSV.Models.StudentDashboardVM;

namespace QLSV.Controllers
{
    [Authorize(Roles = "HocSinh")]
    public class HocSinhController : Controller
    {
        private readonly QlsvContext _db;
        public HocSinhController(QlsvContext db) => _db = db;

        // ===========================
        // Helpers
        // ===========================
        private int? GetCurrentIdTaiKhoan()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out var id) ? id : (int?)null;
        }

        private async Task<HocSinh?> GetCurrentHocSinhAsync(int idTaiKhoan)
        {
            return await _db.HocSinhs
                .Include(h => h.IdTaiKhoanNavigation)
                .Include(h => h.IdLopHocNavigation)
                    .ThenInclude(l => l!.IdGiaoVienChuNhiemNavigation)
                .FirstOrDefaultAsync(h => h.IdTaiKhoan == idTaiKhoan);
        }

        private void SetStudentViewBag(HocSinh hs)
        {
            ViewBag.TenHocSinh = hs.TenHocSinh ?? "";
            ViewBag.TenLop = hs.IdLopHocNavigation?.TenLopHoc ?? "";
        }

        private IQueryable<BangDiem> ScoresQ(int idHocSinh)
            => _db.BangDiems.AsNoTracking().Where(b => b.IdHocSinh == idHocSinh);

        private static double? TongHopInline(double? d15, double? d45, double? dhk)
        {
            if (!d15.HasValue && !d45.HasValue && !dhk.HasValue) return null;
            var v = (d15 ?? 0) * 0.1 + (d45 ?? 0) * 0.3 + (dhk ?? 0) * 0.6;
            return Math.Round(v, 2);
        }

        private async Task<DiemHocKiViewModel> BuildHocKiVM(int idHocSinh, string? namHoc, int kiHoc)
        {
            var raw = await (
                from bd in _db.BangDiems.AsNoTracking()
                join mh in _db.MonHocs.AsNoTracking() on bd.IdMonHoc equals mh.IdMonHoc
                where bd.IdHocSinh == idHocSinh
                      && mh.KiHoc == kiHoc
                      && (string.IsNullOrWhiteSpace(namHoc) || mh.NamHoc == namHoc)
                select new
                {
                    mh.IdMonHoc,
                    mh.TenMonHoc,
                    mh.SoTietHoc,
                    mh.SoTinChi,
                    mh.NamHoc,
                    mh.KiHoc,

                    bd.Diem15phut,
                    bd.Diem45phut,
                    bd.DiemHocKi,
                    bd.NgayCapNhat
                }
            ).ToListAsync();

            var latestBySubject = raw
                .GroupBy(x => x.IdMonHoc)
                .Select(g => g.OrderByDescending(t => t.NgayCapNhat).First())
                .OrderBy(x => x.TenMonHoc)
                .ToList();

            var rows = latestBySubject.Select(x =>
            {
                var tongKet10 = TongHopInline(x.Diem15phut, x.Diem45phut, x.DiemHocKi);
                var tongKet4 = Convert10To4(tongKet10);

                return new DiemMonRow
                {
                    IdMonHoc = x.IdMonHoc,
                    TenMonHoc = x.TenMonHoc ?? "Môn học",
                    SoTietHoc = x.SoTietHoc,
                    SoTinChi = x.SoTinChi,

                    Diem15p = x.Diem15phut,
                    Diem45p = x.Diem45phut,
                    DiemHK = x.DiemHocKi,

                    DiemTongKetMon = tongKet10,
                    DiemTongKetHe4 = tongKet4
                };
            }).ToList();

            // DTB hệ 10 (trung bình các môn)
            var vals10 = rows.Where(r => r.DiemTongKetMon.HasValue)
                             .Select(r => r.DiemTongKetMon!.Value)
                             .ToList();
            double? dtb10 = vals10.Count > 0 ? Math.Round(vals10.Average(), 2) : (double?)null;

            // GPA hệ 4 theo tín chỉ
            var gpa4 = CalcGpa4ByCredits(rows);

            return new DiemHocKiViewModel
            {
                NamHoc = namHoc ?? "",
                KiHoc = kiHoc,
                Rows = rows,
                DTB = dtb10,
                GPA4 = gpa4
            };
        }


        // ===========================
        // DASHBOARD helpers
        // ===========================
        private async Task<PagedResult<RecentScoreItem>> BuildRecentPagedAsync(
            IQueryable<BangDiem> scoresQ, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 8 : pageSize;

            var total = await scoresQ.CountAsync();

            var items = await scoresQ
                .OrderByDescending(x => x.NgayCapNhat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RecentScoreItem
                {
                    TenMonHoc = x.IdMonHocNavigation != null
                        ? (x.IdMonHocNavigation.TenMonHoc ?? "Môn học")
                        : "Môn học",
                    Diem15 = x.Diem15phut,
                    Diem45 = x.Diem45phut,
                    DiemHocKi = x.DiemHocKi,
                    DiemTongHop =
                        (x.Diem15phut == null && x.Diem45phut == null && x.DiemHocKi == null)
                            ? (double?)null
                            : ((x.Diem15phut ?? 0) * 0.1) + ((x.Diem45phut ?? 0) * 0.3) + ((x.DiemHocKi ?? 0) * 0.6),
                    NgayCapNhat = x.NgayCapNhat
                })
                .ToListAsync();

            foreach (var it in items)
                if (it.DiemTongHop.HasValue) it.DiemTongHop = Math.Round(it.DiemTongHop.Value, 2);

            return new PagedResult<RecentScoreItem>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };
        }

        private IQueryable<SubjectScoreItem> LatestBySubjectQ(IQueryable<BangDiem> scoresQ)
        {
            var latestKeyQ = scoresQ
                .Where(x => x.IdMonHoc != null)
                .GroupBy(x => x.IdMonHoc)
                .Select(g => new
                {
                    IdMonHoc = g.Key!.Value,
                    NgayCapNhat = g.Max(t => t.NgayCapNhat)
                });

            var q =
                from b in scoresQ
                join k in latestKeyQ
                    on new { IdMonHoc = b.IdMonHoc!.Value, b.NgayCapNhat }
                    equals new { k.IdMonHoc, k.NgayCapNhat }
                join m in _db.MonHocs.AsNoTracking()
                    on b.IdMonHoc equals m.IdMonHoc
                join gv in _db.GiaoViens.AsNoTracking()
                    on m.IdGiaoVien equals gv.IdGiaoVien into gvs
                from gv in gvs.DefaultIfEmpty()
                let tongHop =
                    (b.Diem15phut == null && b.Diem45phut == null && b.DiemHocKi == null)
                        ? (double?)null
                        : ((b.Diem15phut ?? 0) * 0.1) + ((b.Diem45phut ?? 0) * 0.3) + ((b.DiemHocKi ?? 0) * 0.6)
                select new SubjectScoreItem
                {
                    IdMonHoc = m.IdMonHoc,
                    TenMonHoc = m.TenMonHoc ?? "Môn học",
                    KiHoc = m.KiHoc,
                    NamHoc = m.NamHoc,

                    TenGiaoVien = gv != null ? gv.TenGiaoVien : null,

                    Diem15 = b.Diem15phut,
                    Diem45 = b.Diem45phut,
                    DiemHocKi = b.DiemHocKi,
                    DiemTongHop = tongHop,
                    NgayCapNhat = b.NgayCapNhat
                };

            return q;
        }


        private async Task<PagedResult<SubjectScoreItem>> BuildLatestPagedAsync(
            IQueryable<SubjectScoreItem> latestQ, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 8 : pageSize;

            var total = await latestQ.CountAsync();

            var items = await latestQ
                .OrderBy(x => x.TenMonHoc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            foreach (var it in items)
                if (it.DiemTongHop.HasValue) it.DiemTongHop = Math.Round(it.DiemTongHop.Value, 2);

            return new PagedResult<SubjectScoreItem>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };
        }

        private static double? Convert10To4(double? he10)
        {
            if (!he10.HasValue) return null;
            var x = he10.Value;

            // Thang phổ biến (có thể chỉnh)
            if (x >= 8.5) return 4.0;
            if (x >= 8.0) return 3.5;
            if (x >= 7.0) return 3.0;
            if (x >= 6.5) return 2.5;
            if (x >= 5.5) return 2.0;
            if (x >= 5.0) return 1.5;
            if (x >= 4.0) return 1.0;
            return 0.0;
        }

        private static double? CalcGpa4ByCredits(IEnumerable<DiemMonRow> rows)
        {
            var valid = rows
                .Where(r => r.DiemTongKetHe4.HasValue && (r.SoTinChi ?? 0) > 0)
                .ToList();

            var sumCredits = valid.Sum(r => r.SoTinChi!.Value);
            if (sumCredits <= 0) return null;

            var weighted = valid.Sum(r => r.DiemTongKetHe4!.Value * r.SoTinChi!.Value);
            return Math.Round(weighted / sumCredits, 2);
        }

        // ===========================
        // RANK helpers (MỚI) - xếp hạng theo ĐTB tổng hợp trong lớp
        // ===========================
        private async Task<(int? rank, int total, double? topPercent)> CalcRankInClassAsync(int idHocSinh, int idLopHoc)
        {
            // 1) Lấy danh sách HS trong lớp
            var studentIds = await _db.HocSinhs.AsNoTracking()
                .Where(h => h.IdLopHoc == idLopHoc)
                .Select(h => h.IdHocSinh)
                .ToListAsync();

            if (studentIds.Count == 0) return (null, 0, null);

            // 2) Lấy toàn bộ bảng điểm của lớp (chỉ field cần thiết)
            var raw = await _db.BangDiems.AsNoTracking()
                .Where(b => b.IdHocSinh != null && studentIds.Contains(b.IdHocSinh.Value) && b.IdMonHoc != null)
                .Select(b => new
                {
                    b.IdHocSinh,
                    IdMonHoc = b.IdMonHoc!.Value,
                    b.NgayCapNhat,
                    b.Diem15phut,
                    b.Diem45phut,
                    b.DiemHocKi
                })
                .ToListAsync();

            if (raw.Count == 0) return (null, studentIds.Count, null);

            // 3) Latest theo từng (HocSinh, MonHoc)
            var latest = raw
                .GroupBy(x => new { x.IdHocSinh, x.IdMonHoc })
                .Select(g => g.OrderByDescending(t => t.NgayCapNhat).First())
                .ToList();

            // 4) Tính ĐTB mỗi học sinh
            var dtbByStudent = latest
                .Select(x => new
                {
                    x.IdHocSinh,
                    TongHop = TongHopInline(x.Diem15phut, x.Diem45phut, x.DiemHocKi)
                })
                .Where(x => x.TongHop.HasValue)
                .GroupBy(x => x.IdHocSinh)
                .Select(g => new
                {
                    IdHocSinh = g.Key,
                    Dtb = Math.Round(g.Average(t => t.TongHop!.Value), 2)
                })
                .ToList();

            var ranked = dtbByStudent
                .OrderByDescending(x => x.Dtb)
                .ToList();

            var total = ranked.Count;
            if (total == 0) return (null, studentIds.Count, null);

            // Dense-rank theo Dtb
            int dense = 0;
            double? prev = null;

            foreach (var x in ranked)
            {
                if (prev == null || Math.Abs(x.Dtb - prev.Value) > 0.0001)
                {
                    dense++;
                    prev = x.Dtb;
                }

                if (x.IdHocSinh == idHocSinh)
                {
                    var topPercent = Math.Round(dense * 100.0 / total, 2);
                    return (dense, studentIds.Count, topPercent);
                }
            }

            // học sinh hiện tại chưa có điểm => không có hạng
            return (null, studentIds.Count, null);
        }

        private async Task FillRankBlocksAsync(StudentDashboardVM vm, int idHocSinh, int idLopHoc, int topN = 10, int around = 3)
        {
            var sql = @"
            -- build ranked into temp table
            IF OBJECT_ID('tempdb..#ranked') IS NOT NULL DROP TABLE #ranked;

            ;WITH hs AS (
                SELECT IdHocSinh, TenHocSinh
                FROM HocSinh
                WHERE IdLopHoc = @lop
            ),
            latest AS (
                SELECT 
                    bd.IdHocSinh,
                    bd.IdMonHoc,
                    bd.Diem15phut,
                    bd.Diem45phut,
                    bd.DiemHocKi,
                    ROW_NUMBER() OVER (PARTITION BY bd.IdHocSinh, bd.IdMonHoc ORDER BY bd.NgayCapNhat DESC) AS rn
                FROM BangDiem bd
                INNER JOIN hs ON hs.IdHocSinh = bd.IdHocSinh
                WHERE bd.IdMonHoc IS NOT NULL
            ),
            mon AS (
                SELECT 
                    IdHocSinh,
                    (ISNULL(Diem15phut,0)*0.1 + ISNULL(Diem45phut,0)*0.3 + ISNULL(DiemHocKi,0)*0.6) AS TongHop
                FROM latest
                WHERE rn = 1
            ),
            dtb AS (
                SELECT 
                    m.IdHocSinh,
                    CAST(ROUND(AVG(m.TongHop),2) AS float) AS Dtb
                FROM mon m
                GROUP BY m.IdHocSinh
            ),
            ranked AS (
                SELECT
                    d.IdHocSinh,
                    hs.TenHocSinh,
                    d.Dtb,
                    DENSE_RANK() OVER (ORDER BY d.Dtb DESC) AS Hang,
                    ROW_NUMBER() OVER (ORDER BY d.Dtb DESC) AS Pos,
                    COUNT(*) OVER () AS Total
                FROM dtb d
                INNER JOIN hs ON hs.IdHocSinh = d.IdHocSinh
            )
            SELECT IdHocSinh, TenHocSinh, Dtb, Hang, Pos, Total
            INTO #ranked
            FROM ranked;

            DECLARE @myPos INT = (SELECT TOP 1 Pos FROM #ranked WHERE IdHocSinh = @me);
            DECLARE @myRank INT = (SELECT TOP 1 Hang FROM #ranked WHERE IdHocSinh = @me);
            DECLARE @total BIGINT = (SELECT TOP 1 Total FROM #ranked);

            -- Result set #1: rank info
            SELECT 
                @myRank AS MyRank,
                @total AS TotalInClass,
                CASE WHEN @myRank IS NULL OR @total IS NULL OR @total = 0
                     THEN NULL
                     ELSE CAST(ROUND(@myRank * 100.0 / @total, 2) AS float)
                END AS TopPercent;

            -- Result set #2: Top N
            SELECT IdHocSinh, TenHocSinh, Dtb, Hang
            FROM #ranked
            WHERE Pos <= @topN
            ORDER BY Pos;

            -- Result set #3: Window quanh bạn (chỉ khi bạn không nằm trong TopN)
            SELECT IdHocSinh, TenHocSinh, Dtb, Hang
            FROM #ranked
            WHERE @myPos IS NOT NULL
              AND @myPos > @topN
              AND Pos BETWEEN @myPos - @around AND @myPos + @around
            ORDER BY Pos;
            ";


            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            void AddParam(string name, object value)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = name;
                p.Value = value;
                cmd.Parameters.Add(p);
            }

            AddParam("@lop", idLopHoc);
            AddParam("@me", idHocSinh);
            AddParam("@topN", topN);
            AddParam("@around", around);

            await using var reader = await cmd.ExecuteReaderAsync();

            // ===== Result set #1: rank info =====
            if (await reader.ReadAsync())
            {
                // MyRank: có thể là bigint -> đọc bằng Convert.ToInt32
                vm.XepHangTrongLop = reader.IsDBNull(0) ? (int?)null : Convert.ToInt32(reader.GetValue(0));

                // TotalInClass: COUNT OVER thường là bigint
                vm.TongHocSinhTrongLop = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1));

                vm.TopPercentTrongLop = reader.IsDBNull(2) ? (double?)null : Convert.ToDouble(reader.GetValue(2));
            }
            else
            {
                vm.XepHangTrongLop = null;
                vm.TongHocSinhTrongLop = 0;
                vm.TopPercentTrongLop = null;
            }

            // ===== Result set #2: top N =====
            vm.BangXepHangTop10 ??= new List<StudentDashboardVM.ClassRankRow>();
            vm.BangXepHangTop10.Clear();

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.BangXepHangTop10.Add(new StudentDashboardVM.ClassRankRow
                    {
                        IdHocSinh = Convert.ToInt32(reader.GetValue(0)),
                        TenHocSinh = reader.IsDBNull(1) ? "Học sinh" : reader.GetString(1),
                        Dtb = reader.IsDBNull(2) ? (double?)null : Convert.ToDouble(reader.GetValue(2)),
                        Hang = Convert.ToInt32(reader.GetValue(3)) // Hang có thể bigint
                    });
                }
            }

            // ===== Result set #3: quanh bạn =====
            vm.BangXepHangQuanhBan ??= new List<StudentDashboardVM.ClassRankRow>();
            vm.BangXepHangQuanhBan.Clear();

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.BangXepHangQuanhBan.Add(new StudentDashboardVM.ClassRankRow
                    {
                        IdHocSinh = Convert.ToInt32(reader.GetValue(0)),
                        TenHocSinh = reader.IsDBNull(1) ? "Học sinh" : reader.GetString(1),
                        Dtb = reader.IsDBNull(2) ? (double?)null : Convert.ToDouble(reader.GetValue(2)),
                        Hang = Convert.ToInt32(reader.GetValue(3))
                    });
                }
            }
        }



        // ===========================
        // 1) DASHBOARD
        // ===========================
        public async Task<IActionResult> Dashboard(
            int recentPage = 1, int recentPageSize = 8,
            int subjectPage = 1, int subjectPageSize = 8
        )
        {
            var idTaiKhoan = GetCurrentIdTaiKhoan();
            if (!idTaiKhoan.HasValue) return RedirectToAction("Login", "Auth");

            var hs = await GetCurrentHocSinhAsync(idTaiKhoan.Value);
            if (hs == null) return RedirectToAction("AccessDenied", "Auth");
            SetStudentViewBag(hs);

            var lop = hs.IdLopHocNavigation;
            var gvcn = lop?.IdGiaoVienChuNhiemNavigation;

            var scoresQ = ScoresQ(hs.IdHocSinh);
            var latestQ = LatestBySubjectQ(scoresQ);

            var capNhatGanNhat = await scoresQ
                .OrderByDescending(x => x.NgayCapNhat)
                .Select(x => (DateTime?)x.NgayCapNhat)
                .FirstOrDefaultAsync();

            var recentPaged = await BuildRecentPagedAsync(scoresQ, recentPage, recentPageSize);
            var latestPaged = await BuildLatestPagedAsync(latestQ, subjectPage, subjectPageSize);

            var tongHopList = await latestQ
                .Where(x => x.DiemTongHop.HasValue)
                .Select(x => x.DiemTongHop!.Value)
                .ToListAsync();

            double? tb = tongHopList.Count > 0 ? Math.Round(tongHopList.Average(), 2) : (double?)null;

            var diemGioi = tongHopList.Count(x => x >= 8.0);
            var diemKha = tongHopList.Count(x => x >= 6.5 && x < 8.0);
            var diemTrungBinh = tongHopList.Count(x => x >= 5.0 && x < 6.5);
            var diemYeu = tongHopList.Count(x => x < 5.0);

            var soMonCoDiem = await latestQ.CountAsync();

            var tongSoDiem15 = await scoresQ.CountAsync(x => x.Diem15phut.HasValue);
            var tongSoDiem45 = await scoresQ.CountAsync(x => x.Diem45phut.HasValue);
            var tongSoDiemHK = await scoresQ.CountAsync(x => x.DiemHocKi.HasValue);

            var top3High = await latestQ
                .Where(x => x.DiemTongHop.HasValue)
                .OrderByDescending(x => x.DiemTongHop)
                .Take(3)
                .ToListAsync();

            var top3Low = await latestQ
                .Where(x => x.DiemTongHop.HasValue)
                .OrderBy(x => x.DiemTongHop)
                .Take(3)
                .ToListAsync();

            foreach (var it in top3High)
                if (it.DiemTongHop.HasValue) it.DiemTongHop = Math.Round(it.DiemTongHop.Value, 2);
            foreach (var it in top3Low)
                if (it.DiemTongHop.HasValue) it.DiemTongHop = Math.Round(it.DiemTongHop.Value, 2);

            var vm = new StudentDashboardVM
            {
                IdHocSinh = hs.IdHocSinh,
                IdLopHoc = hs.IdLopHoc,

                TenHocSinh = hs.TenHocSinh ?? "",
                GioiTinh = hs.GioiTinh,
                NgaySinh = hs.NgaySinh,

                TenLopHoc = lop?.TenLopHoc ?? "",
                TenKhoi = lop?.TenKhoi,
                NamHoc = lop?.NamHoc,
                TenGiaoVienChuNhiem = gvcn?.TenGiaoVien,

                SoMonCoDiem = soMonCoDiem,
                CapNhatGanNhat = capNhatGanNhat,
                DiemTrungBinhTongHop = tb,

                TongSoDiem15 = tongSoDiem15,
                TongSoDiem45 = tongSoDiem45,
                TongSoDiemHK = tongSoDiemHK,

                DiemGioi = diemGioi,
                DiemKha = diemKha,
                DiemTrungBinh = diemTrungBinh,
                DiemYeu = diemYeu,

                Top3MonDiemCao = top3High,
                Top3MonCanCaiThien = top3Low,

                RecentScoresPaged = recentPaged,
                LatestBySubjectPaged = latestPaged
            };

            // ====== (MỚI) Bổ sung xếp hạng trong lớp ======
            if (hs.IdLopHoc.HasValue)
            {
                await FillRankBlocksAsync(vm, hs.IdHocSinh, hs.IdLopHoc.Value, topN: 7, around: 2);
            }

            return View(vm);
        }

        // ===========================
        // 1.1) Dashboard partials
        // ===========================
        [HttpGet]
        public async Task<IActionResult> RecentScoresPartial(int recentPage = 1, int recentPageSize = 8)
        {
            var idTaiKhoan = GetCurrentIdTaiKhoan();
            if (!idTaiKhoan.HasValue) return Unauthorized();

            var hs = await GetCurrentHocSinhAsync(idTaiKhoan.Value);
            if (hs == null) return Unauthorized();

            var scoresQ = ScoresQ(hs.IdHocSinh);
            var recentPaged = await BuildRecentPagedAsync(scoresQ, recentPage, recentPageSize);

            var vm = new StudentDashboardVM { RecentScoresPaged = recentPaged };
            return PartialView("_RecentScoresTable", vm);
        }

        [HttpGet]
        public async Task<IActionResult> LatestBySubjectPartial(int subjectPage = 1, int subjectPageSize = 8)
        {
            var idTaiKhoan = GetCurrentIdTaiKhoan();
            if (!idTaiKhoan.HasValue) return Unauthorized();

            var hs = await GetCurrentHocSinhAsync(idTaiKhoan.Value);
            if (hs == null) return Unauthorized();

            var scoresQ = ScoresQ(hs.IdHocSinh);
            var latestQ = LatestBySubjectQ(scoresQ);
            var latestPaged = await BuildLatestPagedAsync(latestQ, subjectPage, subjectPageSize);

            var vm = new StudentDashboardVM { LatestBySubjectPaged = latestPaged };
            return PartialView("_LatestBySubjectList", vm);
        }

        // ====== (MỚI) Rank partial ======
        [HttpGet]
        public async Task<IActionResult> RankSummaryPartial()
        {
            var idTaiKhoan = GetCurrentIdTaiKhoan();
            if (!idTaiKhoan.HasValue) return Unauthorized();

            var hs = await GetCurrentHocSinhAsync(idTaiKhoan.Value);
            if (hs == null || !hs.IdLopHoc.HasValue) return Unauthorized();

            var vm = new StudentDashboardVM
            {
                IdHocSinh = hs.IdHocSinh,
                IdLopHoc = hs.IdLopHoc
            };

            await FillRankBlocksAsync(vm, hs.IdHocSinh, hs.IdLopHoc.Value, topN: 10, around: 3);

            return PartialView("_RankSummary", vm);
        }


        // ===========================
        // 2) PROFILE
        // ===========================
        public async Task<IActionResult> Profile()
        {
            var idTaiKhoan = GetCurrentIdTaiKhoan();
            if (!idTaiKhoan.HasValue) return RedirectToAction("Login", "Auth");

            var hs = await GetCurrentHocSinhAsync(idTaiKhoan.Value);
            if (hs == null) return RedirectToAction("AccessDenied", "Auth");
            SetStudentViewBag(hs);

            var vm = new ProfileViewModel
            {
                HocSinh = hs,
                TaiKhoan = hs.IdTaiKhoanNavigation!,
                LopHoc = hs.IdLopHocNavigation!,
                GVCN = hs.IdLopHocNavigation?.IdGiaoVienChuNhiemNavigation!
            };

            return View(vm);
        }

        // ===========================
        // 8) DIEM
        // ===========================
        public async Task<IActionResult> Diem(string? namHoc)
        {
            var idTaiKhoan = GetCurrentIdTaiKhoan();
            if (!idTaiKhoan.HasValue) return RedirectToAction("Login", "Auth");

            var hs = await GetCurrentHocSinhAsync(idTaiKhoan.Value);
            if (hs == null) return RedirectToAction("AccessDenied", "Auth");
            SetStudentViewBag(hs);

            namHoc ??= hs.IdLopHocNavigation?.NamHoc;

            var hk1 = await BuildHocKiVM(hs.IdHocSinh, namHoc, 1);
            var hk2 = await BuildHocKiVM(hs.IdHocSinh, namHoc, 2);

            var allSubjects = hk1.Rows
                .Select(r => new { r.IdMonHoc, r.TenMonHoc, r.SoTietHoc, r.SoTinChi })
                .Union(hk2.Rows.Select(r => new { r.IdMonHoc, r.TenMonHoc, r.SoTietHoc, r.SoTinChi }))
                .GroupBy(x => x.IdMonHoc)
                .Select(g => g.First())
                .OrderBy(x => x.TenMonHoc)
                .ToList();

            var tongKetRows = allSubjects.Select(s =>
            {
                var r1 = hk1.Rows.FirstOrDefault(x => x.IdMonHoc == s.IdMonHoc);
                var r2 = hk2.Rows.FirstOrDefault(x => x.IdMonHoc == s.IdMonHoc);

                // Điểm năm hệ 10: (HK1 + HK2)/2; thiếu kỳ nào thì lấy kỳ đó
                double? mon10 = null;
                var v1 = r1?.DiemTongKetMon;
                var v2 = r2?.DiemTongKetMon;

                if (v1.HasValue && v2.HasValue) mon10 = Math.Round((v1.Value + v2.Value) / 2.0, 2);
                else if (v1.HasValue) mon10 = v1;
                else if (v2.HasValue) mon10 = v2;

                // Điểm năm hệ 4 (đổi từ điểm năm hệ 10)
                var mon4 = Convert10To4(mon10);

                return new DiemMonRow
                {
                    IdMonHoc = s.IdMonHoc,
                    TenMonHoc = s.TenMonHoc,
                    SoTietHoc = s.SoTietHoc,
                    SoTinChi = s.SoTinChi,

                    Diem15p = null,
                    Diem45p = null,
                    DiemHK = null,

                    DiemTongKetMon = mon10,
                    DiemTongKetHe4 = mon4
                };
            }).ToList();

            // DTB năm hệ 10
            var valsYear10 = tongKetRows.Where(x => x.DiemTongKetMon.HasValue)
                                        .Select(x => x.DiemTongKetMon!.Value)
                                        .ToList();
            var dtbYear10 = valsYear10.Count > 0 ? Math.Round(valsYear10.Average(), 2) : (double?)null;

            // GPA năm hệ 4 theo tín chỉ
            var gpaYear4 = CalcGpa4ByCredits(tongKetRows);

            var tongKet = new DiemHocKiViewModel
            {
                NamHoc = namHoc ?? "",
                KiHoc = 0,
                Rows = tongKetRows,
                DTB = dtbYear10,
                GPA4 = gpaYear4
            };


            return View("Diem", (hk1, hk2, tongKet));
        }
    }
}
