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
                _logger.LogInformation($"TetikleServis çaðrýldý - Ýl ID: {ilId}, Ýl Adý: {ilAdi}, Tarih: {tarih}");

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
                    _logger.LogInformation($"Tarih parametresi boþ geldi, bugünün tarihi kullanýlýyor: {tarih}");
                }

                // Tarih formatýný kontrol et ve düzelt
                DateTime parsedDate;
                if (DateTime.TryParse(tarih, out parsedDate))
                {
                    // Eðer tarih parse edilebiliyorsa, doðru formata çevir
                    tarih = parsedDate.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);
                    _logger.LogInformation($"Tarih formatý düzeltildi: {tarih}");
                }

                // Chrome tarayýcýsýný baþlat
                var options = new ChromeOptions();
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--start-maximized");

                using (IWebDriver driver = new ChromeDriver(options))
                {
                    // EczaneService'in ScrapeEczaneData metodunu çaðýr
                    await _eczaneService.ScrapeEczaneData(driver, ilAdi, tarih, CancellationToken.None);
                }

                TempData["SuccessMessage"] = $"{ilAdi} ili için {tarih} tarihindeki nöbetçi eczane verileri baþarýyla güncellendi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Servis çalýþtýrýlýrken hata oluþtu");
                TempData["ErrorMessage"] = $"Servis çalýþtýrýlýrken hata oluþtu: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { ilId, tarih });
        }

        [HttpPost]
        public async Task<IActionResult> TetikleTumIller(int? ilId, string tarih)
        {
            try
            {
                _logger.LogInformation($"Tüm iller için veri çekme iþlemi baþlatýldý. Tarih: {tarih}");

                // Eðer tarih belirtilmemiþse bugünün tarihini kullan
                if (string.IsNullOrEmpty(tarih))
                {
                    tarih = DateTime.Now.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);
                    _logger.LogInformation($"Tarih parametresi boþ geldi, bugünün tarihi kullanýlýyor: {tarih}");
                }

                // Tarih formatýný kontrol et ve düzelt
                DateTime parsedDate;
                if (DateTime.TryParse(tarih, out parsedDate))
                {
                    // Eðer tarih parse edilebiliyorsa, doðru formata çevir
                    tarih = parsedDate.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);
                    _logger.LogInformation($"Tarih formatý düzeltildi: {tarih}");
                }

                // Eðer belirli bir il ID'si belirtilmiþse, o ilden ve sonrasýnda gelen tüm illeri al
                var iller = new List<IlModel>();

                if (ilId.HasValue)
                {
                    try
                    {
                        // Tüm illeri veritabanýndaki ID sýrasýna göre çek
                        var tumIller = await _dbContext.Iller
                            .OrderBy(i => i.Id)  // ID'ye göre sýrala
                            .ToListAsync();

                        // Seçilen ilin indeksini bul
                        int baslangicIndeksi = tumIller.FindIndex(i => i.Id == ilId.Value);

                        if (baslangicIndeksi >= 0)
                        {
                            // Bulunan indeksten itibaren kalan tüm illeri al
                            iller = tumIller.GetRange(baslangicIndeksi, tumIller.Count - baslangicIndeksi);
                            var baslangicIli = tumIller[baslangicIndeksi];
                            _logger.LogInformation($"Seçilen '{baslangicIli.IlAdi}' ve sonrasýndaki {iller.Count} il iþlenecek");
                        }
                        else
                        {
                            // Ýl bulunamadýysa tüm illeri al
                            iller = tumIller;
                            _logger.LogWarning($"ID={ilId} olan il listede bulunamadý, tüm iller iþlenecek");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ýl listesi alýnýrken hata oluþtu");
                        // Hata durumunda tüm illeri al
                        iller = await _dbContext.Iller.OrderBy(i => i.Id).ToListAsync();
                    }
                }

                // Eðer il ID'si belirtilmemiþse veya geçerli deðilse, tüm illeri al
                if (!iller.Any())
                {
                    iller = await _dbContext.Iller.OrderBy(i => i.Id).ToListAsync();
                    _logger.LogInformation("Tüm iller ID sýrasýyla iþlenecek");
                }

                // Ýþlem sonuçlarýný takip etmek için deðiþkenler
                int basariliIller = 0;
                int hataliIller = 0;
                var hataliIllerListesi = new List<string>();

                // Chrome için ayarlarý yapýlandýr
                var options = new ChromeOptions();
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--start-maximized");

                // Using kullanmadan driver'ý manuel olarak yönet
                IWebDriver driver = null;
                try
                {
                    // Chrome driver'ý baþlat
                    driver = new ChromeDriver(options);

                    foreach (var il in iller)
                    {
                        try
                        {
                            _logger.LogInformation($"{il.IlAdi} ili için veri çekme iþlemi baþlatýldý");

                            // EczaneService'in ScrapeEczaneData metodunu çaðýr
                            await _eczaneService.ScrapeEczaneData(driver, il.IlAdi, tarih, CancellationToken.None);

                            basariliIller++;
                            _logger.LogInformation($"{il.IlAdi} ili için veri çekme iþlemi baþarýyla tamamlandý. Ýþlenen il sayýsý: {basariliIller}/{iller.Count}");
                        }
                        catch (Exception ex)
                        {
                            hataliIller++;
                            hataliIllerListesi.Add(il.IlAdi);
                            _logger.LogError(ex, $"{il.IlAdi} ili için veri çekme iþlemi sýrasýnda hata oluþtu");

                            // Hata durumunda driver'ý yeniden baþlatmak gerekebilir
                            try
                            {
                                driver.Navigate().GoToUrl("about:blank");
                            }
                            catch
                            {
                                // Driver tamamen çöktüyse yeniden baþlat
                                if (driver != null)
                                {
                                    try { driver.Quit(); } catch { /* Sessizce geç */ }
                                    driver.Dispose();
                                }

                                driver = new ChromeDriver(options);
                            }
                        }
                    }
                }
                finally
                {
                    // Driver'ý temizle
                    if (driver != null)
                    {
                        try { driver.Quit(); } catch { /* Sessizce geç */ }
                        driver.Dispose();
                    }
                }

                if (hataliIller > 0)
                {
                    TempData["SuccessMessage"] = $"Toplam {iller.Count} ilden {basariliIller} tanesi için veri çekme iþlemi baþarýyla tamamlandý.";
                    TempData["ErrorMessage"] = $"{hataliIller} il için veri çekme iþlemi baþarýsýz oldu: {string.Join(", ", hataliIllerListesi)}";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Tüm iller ({iller.Count}) için {tarih} tarihindeki nöbetçi eczane verileri baþarýyla güncellendi.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm iller için veri çekme iþlemi sýrasýnda kritik hata oluþtu");
                TempData["ErrorMessage"] = $"Tüm iller için veri çekme iþleminde kritik hata oluþtu: {ex.Message}";
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