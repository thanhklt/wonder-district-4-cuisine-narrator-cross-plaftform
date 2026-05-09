using Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories
{
    public static class ImageAutoSeeder
    {
        public static async Task SeedMissingImagesAsync(
            AppDbContext ctx, IWebHostEnvironment env, HttpClient http)
        {
            var pois = await ctx.Pois
                .Include(p => p.Images)
                .Where(p => p.Status == "Approved" && p.IsActive)
                .ToListAsync();

            bool changed = false;

            foreach (var poi in pois)
            {
                int have = poi.Images.Count;
                if (have >= 4) continue;

                // Nếu POI có ảnh do user upload (không phải seed-*),
                // user tự quản lý ảnh → seeder không thêm tự động
                if (poi.Images.Any(i => !i.ImageUrl.Contains("seed-"))) continue;

                for (int slot = have; slot < 4; slot++)
                {
                    // seed deterministic: poiId * 10 + slot → ảnh không đổi giữa các lần restart
                    var url = await DownloadAndSaveAsync(http, env, poi.PoiID * 10 + slot);
                    if (url == null) continue;

                    bool isCover = slot == 0 && !poi.Images.Any(i => i.IsCover);
                    ctx.PoiImages.Add(new PoiImage
                    {
                        PoiID        = poi.PoiID,
                        ImageUrl     = url,
                        IsCover      = isCover,
                        DisplayOrder = slot + 1
                    });
                    changed = true;
                }
            }

            if (changed)
                await ctx.SaveChangesAsync();
        }

        private static async Task<string?> DownloadAndSaveAsync(
            HttpClient http, IWebHostEnvironment env, int seed)
        {
            var webRoot = env.WebRootPath
                ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine(webRoot, "images", "pois");
            Directory.CreateDirectory(folder);

            var fileName = $"seed-{seed}.jpg";
            var filePath = Path.Combine(folder, fileName);

            if (File.Exists(filePath))
                return $"/images/pois/{fileName}";

            try
            {
                var bytes = await http.GetByteArrayAsync(
                    $"https://picsum.photos/seed/{seed}/800/600");
                await File.WriteAllBytesAsync(filePath, bytes);
                Console.WriteLine($"[ImageAutoSeeder] Downloaded seed-{seed}.jpg for slot {seed}");
                return $"/images/pois/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ImageAutoSeeder] Failed to download seed {seed}: {ex.Message}");
                return null;
            }
        }
    }
}
