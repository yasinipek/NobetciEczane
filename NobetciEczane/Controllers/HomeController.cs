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
using Microsoft.Extensions.Logging;

namespace NobetciEczane.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly EczaneService _eczaneService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext dbContext, EczaneService eczaneService, ILogger<HomeController> logger)
        {
            _dbContext = dbContext;
            _eczaneService = eczaneService;
            _logger = logger;
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
                _logger.LogInformation($"TetikleServis �a�r�ld� - �l ID: {ilId}, �l Ad�: {ilAdi}, Tarih: {tarih}");

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
                    _logger.LogInformation($"Tarih parametresi bo� geldi, bug�n�n tarihi kullan�l�yor: {tarih}");
                }

                // Tarih format�n� kontrol et ve d�zelt
                DateTime parsedDate;
                if (DateTime.TryParse(tarih, out parsedDate))
                {
                    // E�er tarih parse edilebiliyorsa, do�ru formata �evir
                    tarih = parsedDate.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);
                    _logger.LogInformation($"Tarih format� d�zeltildi: {tarih}");
                }

                // Chrome taray�c�s�n� ba�lat
                var options = new ChromeOptions();
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--start-maximized");

                using (IWebDriver driver = new ChromeDriver(options))
                {
                    // EczaneService'in ScrapeEczaneData metodunu �a��r
                    await _eczaneService.ScrapeEczaneData(driver, ilAdi, tarih, CancellationToken.None);
                }

                TempData["SuccessMessage"] = $"{ilAdi} ili i�in {tarih} tarihindeki n�bet�i eczane verileri ba�ar�yla g�ncellendi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Servis �al��t�r�l�rken hata olu�tu");
                TempData["ErrorMessage"] = $"Servis �al��t�r�l�rken hata olu�tu: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { ilId, tarih });
        }

        [HttpPost]
        public async Task<IActionResult> TetikleTumIller(int? ilId, string tarih)
        {
            try
            {
                _logger.LogInformation($"T�m iller i�in veri �ekme i�lemi ba�lat�ld�. Tarih: {tarih}");

                // E�er tarih belirtilmemi�se bug�n�n tarihini kullan
                if (string.IsNullOrEmpty(tarih))
                {
                    tarih = DateTime.Now.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);
                    _logger.LogInformation($"Tarih parametresi bo� geldi, bug�n�n tarihi kullan�l�yor: {tarih}");
                }

                // Tarih format�n� kontrol et ve d�zelt
                DateTime parsedDate;
                if (DateTime.TryParse(tarih, out parsedDate))
                {
                    // E�er tarih parse edilebiliyorsa, do�ru formata �evir
                    tarih = parsedDate.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);
                    _logger.LogInformation($"Tarih format� d�zeltildi: {tarih}");
                }

                // E�er belirli bir il ID'si belirtilmi�se, o ilden ve sonras�nda gelen t�m illeri al
                var iller = new List<IlModel>();

                if (ilId.HasValue)
                {
                    try
                    {
                        // T�m illeri veritaban�ndaki ID s�ras�na g�re �ek
                        var tumIller = await _dbContext.Iller
                            .OrderBy(i => i.Id)  // ID'ye g�re s�rala
                            .ToListAsync();

                        // Se�ilen ilin indeksini bul
                        int baslangicIndeksi = tumIller.FindIndex(i => i.Id == ilId.Value);

                        if (baslangicIndeksi >= 0)
                        {
                            // Bulunan indeksten itibaren kalan t�m illeri al
                            iller = tumIller.GetRange(baslangicIndeksi, tumIller.Count - baslangicIndeksi);
                            var baslangicIli = tumIller[baslangicIndeksi];
                            _logger.LogInformation($"Se�ilen '{baslangicIli.IlAdi}' ve sonras�ndaki {iller.Count} il i�lenecek");
                        }
                        else
                        {
                            // �l bulunamad�ysa t�m illeri al
                            iller = tumIller;
                            _logger.LogWarning($"ID={ilId} olan il listede bulunamad�, t�m iller i�lenecek");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "�l listesi al�n�rken hata olu�tu");
                        // Hata durumunda t�m illeri al
                        iller = await _dbContext.Iller.OrderBy(i => i.Id).ToListAsync();
                    }
                }

                // E�er il ID'si belirtilmemi�se veya ge�erli de�ilse, t�m illeri al
                if (!iller.Any())
                {
                    iller = await _dbContext.Iller.OrderBy(i => i.Id).ToListAsync();
                    _logger.LogInformation("T�m iller ID s�ras�yla i�lenecek");
                }

                // ��lem sonu�lar�n� takip etmek i�in de�i�kenler
                int basariliIller = 0;
                int hataliIller = 0;
                var hataliIllerListesi = new List<string>();

                // Chrome i�in ayarlar� yap�land�r
                var options = new ChromeOptions();
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--start-maximized");

                // Using kullanmadan driver'� manuel olarak y�net
                IWebDriver driver = null;
                try
                {
                    // Chrome driver'� ba�lat
                    driver = new ChromeDriver(options);

                    foreach (var il in iller)
                    {
                        try
                        {
                            _logger.LogInformation($"{il.IlAdi} ili i�in veri �ekme i�lemi ba�lat�ld�");

                            // EczaneService'in ScrapeEczaneData metodunu �a��r
                            await _eczaneService.ScrapeEczaneData(driver, il.IlAdi, tarih, CancellationToken.None);

                            basariliIller++;
                            _logger.LogInformation($"{il.IlAdi} ili i�in veri �ekme i�lemi ba�ar�yla tamamland�. ��lenen il say�s�: {basariliIller}/{iller.Count}");
                        }
                        catch (Exception ex)
                        {
                            hataliIller++;
                            hataliIllerListesi.Add(il.IlAdi);
                            _logger.LogError(ex, $"{il.IlAdi} ili i�in veri �ekme i�lemi s�ras�nda hata olu�tu");

                            // Hata durumunda driver'� yeniden ba�latmak gerekebilir
                            try
                            {
                                driver.Navigate().GoToUrl("about:blank");
                            }
                            catch
                            {
                                // Driver tamamen ��kt�yse yeniden ba�lat
                                if (driver != null)
                                {
                                    try { driver.Quit(); } catch { /* Sessizce ge� */ }
                                    driver.Dispose();
                                }

                                driver = new ChromeDriver(options);
                            }
                        }
                    }
                }
                finally
                {
                    // Driver'� temizle
                    if (driver != null)
                    {
                        try { driver.Quit(); } catch { /* Sessizce ge� */ }
                        driver.Dispose();
                    }
                }

                if (hataliIller > 0)
                {
                    TempData["SuccessMessage"] = $"Toplam {iller.Count} ilden {basariliIller} tanesi i�in veri �ekme i�lemi ba�ar�yla tamamland�.";
                    TempData["ErrorMessage"] = $"{hataliIller} il i�in veri �ekme i�lemi ba�ar�s�z oldu: {string.Join(", ", hataliIllerListesi)}";
                }
                else
                {
                    TempData["SuccessMessage"] = $"T�m iller ({iller.Count}) i�in {tarih} tarihindeki n�bet�i eczane verileri ba�ar�yla g�ncellendi.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "T�m iller i�in veri �ekme i�lemi s�ras�nda kritik hata olu�tu");
                TempData["ErrorMessage"] = $"T�m iller i�in veri �ekme i�leminde kritik hata olu�tu: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { tarih });
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