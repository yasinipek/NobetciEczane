using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NobetciEczane.Data;
using NobetciEczane.Models;
using NobetciEczane.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace NobetciEczane.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly EczaneService _eczaneService;

        public HomeController(ApplicationDbContext dbContext, EczaneService eczaneService)
        {
            _dbContext = dbContext;
            _eczaneService = eczaneService;
        }

        public async Task<IActionResult> Index(int? ilId = null, string tarih = null)
        {
            if (string.IsNullOrEmpty(tarih))
            {
                tarih = DateTime.Now.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);
            }

            // Tüm illeri veritabanýndan al
            var iller = await _dbContext.Iller.OrderBy(i => i.IlAdi).ToListAsync();

            // Eðer ilId belirtilmemiþse ve iller listesi boþ deðilse ilk ili seç
            if (ilId == null && iller.Any())
            {
                ilId = iller.First().Id;
            }

            ViewBag.Tarih = tarih;
            ViewBag.Iller = new SelectList(iller, "Id", "IlAdi", ilId);

            // SeçiliIl adýný bul
            var seciliIl = await _dbContext.Iller.FirstOrDefaultAsync(i => i.Id == ilId);
            ViewBag.SeciliIl = seciliIl?.IlAdi;

            // Nöbetçi eczaneleri iliþkisel veritabanýna göre sorgula
            var eczaneler = await _dbContext.Eczaneler
                .Include(e => e.Il)
                .Where(e => e.IlId == ilId && e.Tarih == tarih)
                .ToListAsync();

            return View(eczaneler);
        }

        [HttpPost]
        public async Task<IActionResult> TetikleServis(int? ilId, string tarih)
        {
            try
            {
                // Ýl ID'sini il adýna çevir
                var il = await _dbContext.Iller.FirstOrDefaultAsync(i => i.Id == ilId);
                string ilAdi = il?.IlAdi;

                // Parametreler hakkýnda log
                Console.WriteLine($"TetikleServis çaðrýldý - Ýl ID: {ilId}, Ýl Adý: {ilAdi}, Tarih: {tarih}");

                // Seçilmiþ il veya varsayýlan olarak ilk il
                if (ilId == null || string.IsNullOrEmpty(ilAdi))
                {
                    var ilkIl = await _dbContext.Iller.OrderBy(i => i.IlAdi).FirstOrDefaultAsync();
                    if (ilkIl != null)
                    {
                        ilId = ilkIl.Id;
                        ilAdi = ilkIl.IlAdi;
                    }
                }

                // Eðer tarih belirtilmemiþse bugünün tarihini kullan
                if (string.IsNullOrEmpty(tarih))
                {
                    tarih = DateTime.Now.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);
                    Console.WriteLine($"Tarih parametresi boþ geldi, bugünün tarihi kullanýlýyor: {tarih}");
                }

                // Tarih formatýný kontrol et ve düzelt
                DateTime parsedDate;
                if (DateTime.TryParse(tarih, out parsedDate))
                {
                    // Eðer tarih parse edilebiliyorsa, doðru formata çevir
                    tarih = parsedDate.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);
                    Console.WriteLine($"Tarih formatý düzeltildi: {tarih}");
                }

                // Chrome tarayýcýsýný baþlat
                var options = new ChromeOptions();
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--start-maximized");

                using (IWebDriver driver = new ChromeDriver(options))
                {
                    // EczaneService'in ScrapeEczaneData metodunu çaðýr
                    // NOT: EczaneService.ScrapeEczaneData metodunu da güncellemek gerekebilir
                    await _eczaneService.ScrapeEczaneData(driver, ilAdi, tarih, CancellationToken.None);
                }

                TempData["SuccessMessage"] = $"{ilAdi} ili için {tarih} tarihindeki nöbetçi eczane verileri baþarýyla güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Servis çalýþtýrýlýrken hata oluþtu: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { ilId, tarih });
        }

        public async Task<IActionResult> Detay(int id)
        {
            var eczane = await _dbContext.Eczaneler
                .Include(e => e.Il)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eczane == null)
            {
                return NotFound();
            }

            return View(eczane);
        }

        [HttpPost]
        public IActionResult IlSecimi(int ilId, string tarih)
        {
            return RedirectToAction("Index", new { ilId, tarih });
        }

        [HttpPost]
        public IActionResult TarihSecimi(int ilId, string tarih)
        {
            return RedirectToAction("Index", new { ilId, tarih });
        }
    }
}