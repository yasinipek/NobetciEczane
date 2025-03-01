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
        private readonly List<string> _iller = new List<string>
        {
            "ADANA", "ADIYAMAN", "AFYONKARAHÝSAR", "AÐRI", "AMASYA", "ANKARA", "ANTALYA", "ARTVÝN", "AYDIN", "BALIKESÝR",
            "BÝLECÝK", "BÝNGÖL", "BÝTLÝS", "BOLU", "BURDUR", "BURSA", "ÇANAKKALE", "ÇANKIRI", "ÇORUM", "DENÝZLÝ",
            "DÝYARBAKIR", "EDÝRNE", "ELAZIÐ", "ERZÝNCAN", "ERZURUM", "ESKÝÞEHÝR", "GAZÝANTEP", "GÝRESUN", "GÜMÜÞHANE",
            "HAKKARÝ", "HATAY", "ISPARTA", "MERSÝN", "ÝSTANBUL", "ÝZMÝR", "KARS", "KASTAMONU", "KAYSERÝ", "KIRKLARELÝ",
            "KIRÞEHÝR", "KOCAELÝ", "KONYA", "KÜTAHYA", "MALATYA", "MANÝSA", "KAHRAMANMARAÞ", "MARDÝN", "MUÐLA", "MUÞ",
            "NEVÞEHÝR", "NÝÐDE", "ORDU", "RÝZE", "SAKARYA", "SAMSUN", "SÝÝRT", "SÝNOP", "SÝVAS", "TEKÝRDAÐ",
            "TOKAT", "TRABZON", "TUNCELÝ", "ÞANLIURFA", "UÞAK", "VAN", "YOZGAT", "ZONGULDAK", "AKSARAY", "BAYBURT",
            "KARAMAN", "KIRIKKALE", "BATMAN", "ÞIRNAK", "BARTIN", "ARDAHAN", "IÐDIR", "YALOVA", "KARABÜK", "KÝLÝS",
            "OSMANÝYE", "DÜZCE"
        };

        public HomeController(ApplicationDbContext dbContext, EczaneService eczaneService)
        {
            _dbContext = dbContext;
            _eczaneService = eczaneService;
        }

        public async Task<IActionResult> Index(string il = null, string tarih = null)
        {
            if (string.IsNullOrEmpty(tarih))
            {
                tarih = DateTime.Now.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);
            }

            if (string.IsNullOrEmpty(il) && _iller.Count > 0)
            {
                il = _iller[0]; // Varsayýlan olarak ilk ili seç
            }

            ViewBag.Tarih = tarih;
            ViewBag.Iller = new SelectList(_iller, il);
            ViewBag.SeciliIl = il;

            var eczaneler = await _dbContext.Eczaneler
                .Where(e => e.Il == il && e.Tarih == tarih)
                .ToListAsync();

            return View(eczaneler);
        }

        [HttpPost]
        public async Task<IActionResult> TetikleServis(string il)
        {
            try
            {
                // Seçilmiþ il veya varsayýlan olarak ilk il
                if (string.IsNullOrEmpty(il) && _iller.Count > 0)
                {
                    il = _iller[0];
                }

                var tarih = DateTime.Now.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);

                // Chrome tarayýcýsýný baþlat
                var options = new ChromeOptions();
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--start-maximized");

                using (IWebDriver driver = new ChromeDriver(options))
                {
                    // EczaneService'in ScrapeEczaneData metodunu çaðýr
                    await _eczaneService.ScrapeEczaneData(driver, il, tarih, CancellationToken.None);
                }

                TempData["SuccessMessage"] = $"{il} ili için nöbetçi eczane verileri baþarýyla güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Servis çalýþtýrýlýrken hata oluþtu: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { il });
        }

        public async Task<IActionResult> Detay(int id)
        {
            var eczane = await _dbContext.Eczaneler.FindAsync(id);
            if (eczane == null)
            {
                return NotFound();
            }

            return View(eczane);
        }

        [HttpPost]
        public IActionResult IlSecimi(string il, string tarih)
        {
            return RedirectToAction("Index", new { il, tarih });
        }

        [HttpPost]
        public IActionResult TarihSecimi(string il, string tarih)
        {
            return RedirectToAction("Index", new { il, tarih });
        }
    }
}