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
            "ADANA", "ADIYAMAN", "AFYONKARAH�SAR", "A�RI", "AMASYA", "ANKARA", "ANTALYA", "ARTV�N", "AYDIN", "BALIKES�R",
            "B�LEC�K", "B�NG�L", "B�TL�S", "BOLU", "BURDUR", "BURSA", "�ANAKKALE", "�ANKIRI", "�ORUM", "DEN�ZL�",
            "D�YARBAKIR", "ED�RNE", "ELAZI�", "ERZ�NCAN", "ERZURUM", "ESK��EH�R", "GAZ�ANTEP", "G�RESUN", "G�M��HANE",
            "HAKKAR�", "HATAY", "ISPARTA", "MERS�N", "�STANBUL", "�ZM�R", "KARS", "KASTAMONU", "KAYSER�", "KIRKLAREL�",
            "KIR�EH�R", "KOCAEL�", "KONYA", "K�TAHYA", "MALATYA", "MAN�SA", "KAHRAMANMARA�", "MARD�N", "MU�LA", "MU�",
            "NEV�EH�R", "N��DE", "ORDU", "R�ZE", "SAKARYA", "SAMSUN", "S��RT", "S�NOP", "S�VAS", "TEK�RDA�",
            "TOKAT", "TRABZON", "TUNCEL�", "�ANLIURFA", "U�AK", "VAN", "YOZGAT", "ZONGULDAK", "AKSARAY", "BAYBURT",
            "KARAMAN", "KIRIKKALE", "BATMAN", "�IRNAK", "BARTIN", "ARDAHAN", "I�DIR", "YALOVA", "KARAB�K", "K�L�S",
            "OSMAN�YE", "D�ZCE"
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
                il = _iller[0]; // Varsay�lan olarak ilk ili se�
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
                // Se�ilmi� il veya varsay�lan olarak ilk il
                if (string.IsNullOrEmpty(il) && _iller.Count > 0)
                {
                    il = _iller[0];
                }

                var tarih = DateTime.Now.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);

                // Chrome taray�c�s�n� ba�lat
                var options = new ChromeOptions();
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--start-maximized");

                using (IWebDriver driver = new ChromeDriver(options))
                {
                    // EczaneService'in ScrapeEczaneData metodunu �a��r
                    await _eczaneService.ScrapeEczaneData(driver, il, tarih, CancellationToken.None);
                }

                TempData["SuccessMessage"] = $"{il} ili i�in n�bet�i eczane verileri ba�ar�yla g�ncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Servis �al��t�r�l�rken hata olu�tu: {ex.Message}";
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