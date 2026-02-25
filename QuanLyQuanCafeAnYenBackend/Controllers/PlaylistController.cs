using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;

        public PlaylistController(QuanLyQuanCafeDbContext context)
        {
            _context = context;
        }

        // 1. TÌM KIẾM NHẠC TỪ ITUNES
        [HttpGet("search")]
        public async Task<IActionResult> SearchMusic([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query))
                return BadRequest("Vui lòng nhập từ khóa tìm kiếm.");

            try
            {
                using var httpClient = new HttpClient();
                var encodedQuery = Uri.EscapeDataString(query);
                var url = $"https://itunes.apple.com/search?term={encodedQuery}&media=music&entity=song&limit=10";

                var response = await httpClient.GetStringAsync(url);
                var json = System.Text.Json.JsonDocument.Parse(response);

                var results = json.RootElement
                    .GetProperty("results")
                    .EnumerateArray()
                    .Select(t => new
                    {
                        SpotifyId = t.GetProperty("trackId").GetInt64().ToString(),
                        TenBaiHat = t.GetProperty("trackName").GetString(),
                        CaSi = t.GetProperty("artistName").GetString(),
                        ThoiLuong = t.GetProperty("trackTimeMillis").GetInt64() / 1000,
                        AnhBia = t.GetProperty("artworkUrl100").GetString(),
                        LinkNgheThu = t.TryGetProperty("previewUrl", out var preview)
                                        ? preview.GetString()
                                        : null
                    });

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

        // 2. LƯU BÀI HÁT VÀO DATABASE
        [HttpPost("add-to-library")]
        public async Task<IActionResult> AddToLibrary([FromBody] BaiHatDto input)
        {
            var exists = await _context.BaiHats.AnyAsync(b => b.SpotifyId == input.SpotifyId);
            if (exists)
                return BadRequest("Bài hát này đã có trong thư viện của quán!");

            var lastBaiHat = await _context.BaiHats
                .OrderByDescending(b => b.MaBaiHat)
                .FirstOrDefaultAsync();

            int nextId = 1;
            if (lastBaiHat != null && lastBaiHat.MaBaiHat.StartsWith("BH"))
            {
                int.TryParse(lastBaiHat.MaBaiHat.Substring(2), out nextId);
                nextId++;
            }
            string newMaBaiHat = $"BH{nextId:D3}";

            var newBaiHat = new BaiHat
            {
                MaBaiHat = newMaBaiHat,
                TenBaiHat = input.TenBaiHat,
                CaSi = input.CaSi,
                TheLoai = "Khác",
                ThoiLuong = input.ThoiLuong,
                AnhBia = input.AnhBia,
                TrangThai = true,
                SpotifyId = input.SpotifyId,   // Lưu trackId của iTunes
                LinkNgheThu = input.LinkNgheThu,
                LinkAudio = input.LinkNgheThu
            };

            _context.BaiHats.Add(newBaiHat);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã lưu bài hát vào thư viện thành công!", data = newBaiHat });
        }

        // 3. LẤY DANH SÁCH BÀI HÁT ĐÃ LƯU
        [HttpGet("library")]
        public async Task<IActionResult> GetLibrary()
        {
            var library = await _context.BaiHats
                .Where(b => b.TrangThai == true)
                .OrderByDescending(b => b.MaBaiHat)
                .ToListAsync();
            return Ok(library);
        }
        [HttpDelete("delete/{maBaiHat}")]
        public async Task<IActionResult> DeleteSong(string maBaiHat)
        {
            var song = await _context.BaiHats.FirstOrDefaultAsync(b => b.MaBaiHat == maBaiHat);

            if (song == null)
            {
                return NotFound("Không tìm thấy bài hát!");
            }

            // Xóa mềm: Chuyển trạng thái thành false (ẩn khỏi thư viện)
            song.TrangThai = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa bài hát thành công!" });
        }
        // ==========================================
        // QUẢN LÝ PLAYLIST
        // ==========================================

        [HttpGet("playlists")]
        public async Task<IActionResult> GetPlaylists()
        {
            var playlists = await _context.Playlists.OrderBy(p => p.ThuTu).ToListAsync();
            return Ok(playlists);
        }

        [HttpPost("playlist")]
        public async Task<IActionResult> CreatePlaylist([FromBody] Playlist input)
        {
            // Tự động tạo mã PL mới (ví dụ: PL005)
            var lastPl = await _context.Playlists.OrderByDescending(p => p.MaPlaylist).FirstOrDefaultAsync();
            int nextId = 1;
            if (lastPl != null && lastPl.MaPlaylist.StartsWith("PL"))
            {
                int.TryParse(lastPl.MaPlaylist.Substring(2), out nextId);
                nextId++;
            }
            input.MaPlaylist = $"PL{nextId:D3}";
            input.ThoiGianTao = DateTime.Now;

            _context.Playlists.Add(input);
            await _context.SaveChangesAsync();
            return Ok(input);
        }

        [HttpPut("playlist/{id}")]
        public async Task<IActionResult> UpdatePlaylist(string id, [FromBody] Playlist input)
        {
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist == null) return NotFound("Không tìm thấy Playlist");

            playlist.TenPlaylist = input.TenPlaylist;
            playlist.MoTa = input.MoTa;
            playlist.AnhBia = input.AnhBia;
            playlist.TrangThai = input.TrangThai;
            playlist.ThuTu = input.ThuTu;

            await _context.SaveChangesAsync();
            return Ok(playlist);
        }

        [HttpDelete("playlist/{id}")]
        public async Task<IActionResult> DeletePlaylist(string id)
        {
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist == null) return NotFound();

            // Sẽ tự động xóa ChiTietPlaylist nếu có cấu hình ON DELETE CASCADE trong DB
            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ==========================================
        // QUẢN LÝ BÀI HÁT TRONG PLAYLIST (ChiTietPlaylist)
        // ==========================================

        [HttpGet("playlist-details")]
        public async Task<IActionResult> GetPlaylistDetails()
        {
            var details = await _context.ChiTietPlaylists.OrderBy(c => c.ThuTu).ToListAsync();
            return Ok(details);
        }

        [HttpPost("playlist-song")]
        public async Task<IActionResult> AddSongToPlaylist([FromBody] ChiTietPlaylistDto input)
        {
            // Tìm mã chi tiết cuối cùng để tự động tăng (CP001, CP002...)
            var lastDetail = await _context.ChiTietPlaylists
                .OrderByDescending(c => c.MaChiTiet)
                .FirstOrDefaultAsync();

            int nextId = 1;
            if (lastDetail != null && lastDetail.MaChiTiet.StartsWith("CP"))
            {
                int.TryParse(lastDetail.MaChiTiet.Substring(2), out nextId);
                nextId++;
            }

            // Map dữ liệu từ DTO sang Model thực tế để lưu DB
            var newChiTiet = new ChiTietPlaylist
            {
                MaChiTiet = $"CP{nextId:D3}",
                ThuTu = input.ThuTu,
                MaPlaylist = input.MaPlaylist,
                MaBaiHat = input.MaBaiHat
            };

            _context.ChiTietPlaylists.Add(newChiTiet);
            await _context.SaveChangesAsync();

            return Ok(newChiTiet);
        }

        [HttpDelete("playlist-song/{id}")]
        public async Task<IActionResult> RemoveSongFromPlaylist(string id)
        {
            var detail = await _context.ChiTietPlaylists.FindAsync(id);
            if (detail == null) return NotFound();

            _context.ChiTietPlaylists.Remove(detail);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ==========================================
        // QUẢN LÝ LỊCH PHÁT NHẠC
        // ==========================================

        [HttpGet("schedules")]
        public async Task<IActionResult> GetSchedules()
        {
            var schedules = await _context.LichPhatNhacs.ToListAsync();
            return Ok(schedules);
        }

        [HttpPost("schedule")]
        public async Task<IActionResult> CreateSchedule([FromBody] LichPhatNhacDto input)
        {
            TimeOnly gioBatDau = TimeOnly.Parse(input.GioBatDau);
            TimeOnly gioKetThuc = TimeOnly.Parse(input.GioKetThuc);

            // Ép từ DateTime (của Frontend gửi) sang DateOnly (của Database)
            DateOnly ngayApThucTe = DateOnly.FromDateTime(input.NgayAp);

            if (gioBatDau >= gioKetThuc)
            {
                return BadRequest("Giờ kết thúc phải lớn hơn giờ bắt đầu.");
            }

            // Kiểm tra khoảng thời gian tối thiểu (30 phút)
            TimeSpan duration = gioKetThuc - gioBatDau;
            if (duration.TotalMinutes < 30)
            {
                return BadRequest("Thời gian phát nhạc tối thiểu phải là 30 phút.");
            }

            // Kiểm tra trùng lịch
            bool isOverlap = await _context.LichPhatNhacs.AnyAsync(l =>
                l.NgayAp == ngayApThucTe &&
                (l.MaTang == input.MaTang || l.MaTang == null || input.MaTang == null) &&
                l.GioBatDau < gioKetThuc &&
                l.GioKetThuc > gioBatDau
            );

            if (isOverlap) return BadRequest("Khung giờ này đã bị trùng với một lịch phát nhạc khác!");

            var lastSchedule = await _context.LichPhatNhacs.OrderByDescending(l => l.MaLich).FirstOrDefaultAsync();
            int nextId = 1;
            if (lastSchedule != null && lastSchedule.MaLich.StartsWith("LPN"))
            {
                int.TryParse(lastSchedule.MaLich.Substring(3), out nextId);
                nextId++;
            }

            var newSchedule = new LichPhatNhac
            {
                MaLich = $"LPN{nextId:D3}",
                NgayAp = ngayApThucTe,
                GioBatDau = gioBatDau,
                GioKetThuc = gioKetThuc,
                MaPlaylist = input.MaPlaylist,
                MaTang = input.MaTang,
                ChuThich = input.ChuThich
            };

            _context.LichPhatNhacs.Add(newSchedule);
            await _context.SaveChangesAsync();
            return Ok(newSchedule);
        }

        [HttpPut("schedule/{id}")]
        public async Task<IActionResult> UpdateSchedule(string id, [FromBody] LichPhatNhacDto input)
        {
            TimeOnly gioBatDau = TimeOnly.Parse(input.GioBatDau);
            TimeOnly gioKetThuc = TimeOnly.Parse(input.GioKetThuc);
            DateOnly ngayApThucTe = DateOnly.FromDateTime(input.NgayAp);

            if (gioBatDau >= gioKetThuc)
            {
                return BadRequest("Giờ kết thúc phải lớn hơn giờ bắt đầu.");
            }

            // Kiểm tra khoảng thời gian tối thiểu (30 phút)
            TimeSpan duration = gioKetThuc - gioBatDau;
            if (duration.TotalMinutes < 30)
            {
                return BadRequest("Thời gian phát nhạc tối thiểu phải là 30 phút.");
            }

            // Kiểm tra trùng lịch (bỏ qua lịch đang cập nhật)
            bool isOverlap = await _context.LichPhatNhacs.AnyAsync(l =>
                l.MaLich != id &&
                l.NgayAp == ngayApThucTe &&
                (l.MaTang == input.MaTang || l.MaTang == null || input.MaTang == null) &&
                l.GioBatDau < gioKetThuc &&
                l.GioKetThuc > gioBatDau
            );

            if (isOverlap) return BadRequest("Khung giờ này đã bị trùng với một lịch phát nhạc khác!");

            var schedule = await _context.LichPhatNhacs.FindAsync(id);
            if (schedule == null) return NotFound("Không tìm thấy lịch phát nhạc.");

            schedule.NgayAp = ngayApThucTe;
            schedule.GioBatDau = gioBatDau;
            schedule.GioKetThuc = gioKetThuc;
            schedule.MaPlaylist = input.MaPlaylist;
            schedule.MaTang = input.MaTang;
            schedule.ChuThich = input.ChuThich;

            await _context.SaveChangesAsync();
            return Ok(schedule);
        }

        [HttpDelete("schedule/{id}")]
        public async Task<IActionResult> DeleteSchedule(string id)
        {
            var schedule = await _context.LichPhatNhacs.FindAsync(id);
            if (schedule == null) return NotFound();

            _context.LichPhatNhacs.Remove(schedule);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    public class BaiHatDto
    {
        public string SpotifyId { get; set; } = null!;
        public string TenBaiHat { get; set; } = null!;
        public string CaSi { get; set; } = null!;
        public int ThoiLuong { get; set; }
        public string? AnhBia { get; set; }
        public string? LinkNgheThu { get; set; }
    }
    public class ChiTietPlaylistDto
    {
        public int ThuTu { get; set; }
        public string MaPlaylist { get; set; } = null!;
        public string MaBaiHat { get; set; } = null!;
    }
    public class LichPhatNhacDto
    {
        // SỬA CHỮ DateOnly THÀNH DateTime Ở DÒNG NÀY
        public DateTime NgayAp { get; set; }

        public string GioBatDau { get; set; } = null!;
        public string GioKetThuc { get; set; } = null!;
        public string MaPlaylist { get; set; } = null!;
        public string? MaTang { get; set; }
        public string? ChuThich { get; set; }
    }
}