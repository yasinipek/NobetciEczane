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

            // T�m illeri veritaban�ndan al
            var iller = await _dbContext.Iller.OrderBy(i => i.IlAdi).ToListAsync();

            // E�er ilId belirtilmemi�se ve iller listesi bo� de�ilse ilk ili se�
            if (ilId == null && iller.Any())
            {
                ilId = iller.First().Id;
            }

            ViewBag.Tarih = tarih;
            ViewBag.Iller = new SelectList(iller, "Id", "IlAdi", ilId);

            // Se�iliIl ad�n� bul
            var seciliIl = await _dbContext.Iller.FirstOrDefaultAsync(i => i.Id == ilId);
            ViewBag.SeciliIl = seciliIl?.IlAdi;

            // N�bet�i eczaneleri ili�kisel veritaban�na g�re sorgula
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
                // �l ID'sini il ad�na �evir
                var il = await _dbContext.Iller.FirstOrDefaultAsync(i => i.Id == ilId);
                string ilAdi = il?.IlAdi;

                // Parametreler hakk�nda log
                Console.WriteLine($"TetikleServis �a�r�ld� - �l ID: {ilId}, �l Ad�: {ilAdi}, Tarih: {tarih}");

                // Se�ilmi� il veya varsay�lan olarak ilk il
                if (ilId == null || string.IsNullOrEmpty(ilAdi))
                {
                    var ilkIl = await _dbContext.Iller.OrderBy(i => i.IlAdi).FirstOrDefaultAsync();
                    if (ilkIl != null)
                    {
                        ilId = ilkIl.Id;
                        ilAdi = ilkIl.IlAdi;
                    }
                }

                // E�er tarih belirtilmemi�se bug�n�n tarihini kullan
                if (string.IsNullOrEmpty(tarih))
                {
                    tarih = DateTime.Now.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);
                    Console.WriteLine($"Tarih parametresi bo� geldi, bug�n�n tarihi kullan�l�yor: {tarih}");
                }

                // Tarih format�n� kontrol et ve d�zelt
                DateTime parsedDate;
                if (DateTime.TryParse(tarih, out parsedDate))
                {
                    // E�er tarih parse edilebiliyorsa, do�ru formata �evir
                    tarih = parsedDate.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);
                    Console.WriteLine($"Tarih format� d�zeltildi: {tarih}");
                }

                // Chrome taray�c�s�n� ba�lat
                var options = new ChromeOptions();
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--start-maximized");

                using (IWebDriver driver = new ChromeDriver(options))
                {
                    // EczaneService'in ScrapeEczaneData metodunu �a��r
                    // NOT: EczaneService.ScrapeEczaneData metodunu da g�ncellemek gerekebilir
                    await _eczaneService.ScrapeEczaneData(driver, ilAdi, tarih, CancellationToken.None);
                }

                TempData["SuccessMessage"] = $"{ilAdi} ili i�in {tarih} tarihindeki n�bet�i eczane verileri ba�ar�yla g�ncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Servis �al��t�r�l�rken hata olu�tu: {ex.Message}";
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