﻿using Microsoft.EntityFrameworkCore;
using NobetciEczane.Data;
using NobetciEczane.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace NobetciEczane.Services
{
    public class EczaneService
    {
        private readonly ILogger<EczaneService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public EczaneService(
            ILogger<EczaneService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        // Bu metot artık manuel olarak çağrılacak
        public async Task StartScraping(CancellationToken stoppingToken = default)
        {
            _logger?.LogInformation("Nöbetçi eczane veri çekme işlemi başlatıldı: {time}", DateTimeOffset.Now);

            var tarih = DateTime.Now.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);

            // Veritabanından illeri al
            List<IlModel> iller;
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                iller = await dbContext.Iller.OrderBy(i => i.IlAdi).ToListAsync();
            }

            var options = new ChromeOptions();
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--start-maximized");

            foreach (var il in iller)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    using (IWebDriver driver = new ChromeDriver(options))
                    {
                        await ScrapeEczaneData(driver, il.IlAdi, tarih, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "{Il} ili için Chrome sürücüsü başlatılırken hata oluştu", il.IlAdi);
                }
            }

            _logger?.LogInformation("Nöbetçi eczane veri çekme işlemi tamamlandı.");
        }

        // Belirli bir il için veri çekmeyi başlatan metot - tek il için çalıştırmak istenirse
        public async Task StartScrapingForCity(string ilAdi, CancellationToken stoppingToken = default)
        {
            _logger?.LogInformation("{Il} ili için nöbetçi eczane veri çekme işlemi başlatıldı: {time}", ilAdi, DateTimeOffset.Now);

            var tarih = DateTime.Now.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);

            var options = new ChromeOptions();
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--start-maximized");

            try
            {
                using (IWebDriver driver = new ChromeDriver(options))
                {
                    await ScrapeEczaneData(driver, ilAdi, tarih, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "{Il} ili için Chrome sürücüsü başlatılırken hata oluştu", ilAdi);
                throw; // Hata yukarıya iletilsin ki controller'da yakalanabilsin
            }

            _logger?.LogInformation("{Il} ili için nöbetçi eczane veri çekme işlemi tamamlandı.", ilAdi);
        }

        // Eczane bilgilerini kesin olarak almanın fonksiyonu
        private string GetElementTextWithRetry(IWebDriver driver, WebDriverWait wait, string xpath, string fieldName, int maxRetries = 5)
        {
            string text = "";
            int retryCount = 0;
            bool success = false;

            while (!success && retryCount < maxRetries)
            {
                try
                {
                    // Elementin yüklenmesini bekle
                    wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.XPath(xpath)));

                    // Elementi bul
                    var element = driver.FindElement(By.XPath(xpath));

                    // Metni al ve temizle
                    text = element.Text.Trim();

                    // Eğer metin boş gelirse
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        retryCount++;
                        _logger?.LogWarning("{Field} bilgisi boş geldi, {RetryCount}. deneme yapılıyor...", fieldName, retryCount);

                        // JavaScript ile tekrar deneyelim
                        try
                        {
                            text = ((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].textContent", element).ToString().Trim();
                        }
                        catch
                        {
                            // JavaScript başarısız olursa yine boş
                            text = "";
                        }

                        // Hala boş mu?
                        if (string.IsNullOrWhiteSpace(text))
                        {
                            // Biraz bekleyip tekrar deneyeceğiz
                            Thread.Sleep(1000);
                            continue;
                        }
                    }

                    success = true;
                    _logger?.LogInformation("{Field} bilgisi başarıyla alındı: {Value}", fieldName, text);
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger?.LogWarning(ex, "{Field} bilgisi alınırken hata oluştu, {RetryCount}. deneme yapılıyor...", fieldName, retryCount);

                    if (retryCount >= maxRetries)
                    {
                        _logger?.LogError("Maksimum deneme sayısına ulaşıldı, {Field} bilgisi alınamadı.", fieldName);
                        throw;
                    }

                    // Biraz bekleyip tekrar deneyelim
                    Thread.Sleep(1000);
                }
            }

            return text;
        }

        public async Task ScrapeEczaneData(IWebDriver driver, string ilAdi, string tarih, CancellationToken stoppingToken)
        {
            try
            {
                _logger?.LogInformation("{Il} ili için {Tarih} tarihinde veri çekme işlemi başladı", ilAdi, tarih);

                // İl ID'sini bul
                int ilId = 0;
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Önce veritabanı sorgusunu temel karşılaştırma ile yap
                    var il = await dbContext.Iller
                        .Where(i => i.IlAdi.ToLower() == ilAdi.ToLower())
                        .FirstOrDefaultAsync();

                    // Eğer bulunamadıysa, tüm illeri çekip memory'de karşılaştır
                    if (il == null)
                    {
                        var tumIller = await dbContext.Iller.ToListAsync();
                        il = tumIller.FirstOrDefault(i =>
                            i.IlAdi.Equals(ilAdi, StringComparison.OrdinalIgnoreCase));

                        if (il == null)
                        {
                            // Hala bulunamadıysa, biraz daha esnek bir arama yap
                            il = tumIller.FirstOrDefault(i =>
                                i.IlAdi.Contains(ilAdi, StringComparison.OrdinalIgnoreCase) ||
                                ilAdi.Contains(i.IlAdi, StringComparison.OrdinalIgnoreCase));
                        }
                    }

                    if (il == null)
                    {
                        _logger?.LogWarning("{Il} ili veritabanında bulunamadı", ilAdi);
                        return;
                    }

                    ilId = il.Id;
                }

                // e-Devlet nöbetçi eczane sayfasına git
                driver.Navigate().GoToUrl("https://www.turkiye.gov.tr/saglik-titck-nobetci-eczane-sorgulama");
                _logger?.LogInformation("{Il} ili için e-Devlet sayfasına gidildi", ilAdi);

                // Sayfanın yüklenmesini bekle
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(d => d.FindElement(By.Id("plakaKodu")));
                _logger?.LogInformation("Sayfa yüklendi, şehir seçiliyor...");

                // İl seçimini yap
                var ilSelect = new SelectElement(driver.FindElement(By.Id("plakaKodu")));
                ilSelect.SelectByText(ilAdi);
                _logger?.LogInformation("{Il} şehri seçildi", ilAdi);

                // Tarih seçimini yap
                wait.Until(d => d.FindElement(By.Id("nobetTarihi")));
                var dateInput = driver.FindElement(By.Id("nobetTarihi"));
                dateInput.Clear();
                dateInput.SendKeys(tarih);
                _logger?.LogInformation("{Tarih} tarihi seçildi", tarih);

                // Sorgula butonuna tıkla
                var searchButton = driver.FindElement(By.CssSelector("input[type='submit'][value='Sorgula']"));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", searchButton);
                _logger?.LogInformation("Sorgula butonuna tıklandı");

                // Sonuçların yüklenmesini bekle
                try
                {
                    wait.Until(d => d.FindElement(By.CssSelector("table, .warningContainer")));
                    _logger?.LogInformation("Sonuçlar yüklendi");
                }
                catch (WebDriverTimeoutException)
                {
                    _logger?.LogWarning("{Il} ili için sonuçlar yüklenemedi.", ilAdi);
                    return;
                }

                // Nöbetçi eczane bilgilerini çek
                var warningElement = driver.FindElements(By.CssSelector(".warningContainer"));
                List<EczaneModel> eczaneler = new List<EczaneModel>();

                if (warningElement.Count == 0)
                {
                    // Tablo mevcut, eczaneleri çek
                    var tableExists = driver.FindElements(By.CssSelector("table tbody")).Count > 0;

                    if (!tableExists)
                    {
                        _logger?.LogWarning("{Il} ili için eczane tablosu bulunamadı.", ilAdi);
                        return;
                    }

                    var eczaneElements = driver.FindElements(By.CssSelector("table tbody tr"));
                    _logger?.LogInformation("{Il} ili için {Count} adet eczane bulundu", ilAdi, eczaneElements.Count);

                    for (int i = 0; i < eczaneElements.Count; i++)
                    {
                        if (stoppingToken.IsCancellationRequested)
                        {
                            break;
                        }

                        try
                        {
                            // Elementi her seferinde yeniden bul
                            eczaneElements = driver.FindElements(By.CssSelector("table tbody tr"));

                            // İndeks kontrolü
                            if (i >= eczaneElements.Count)
                            {
                                _logger?.LogWarning("{Il} ili için eczane indeksi ({Index}) listeden büyük, döngü sonlandırılıyor.", ilAdi, i);
                                break;
                            }

                            var eczaneElement = eczaneElements[i];

                            // İlçe bilgisini al
                            var ilceElements = eczaneElement.FindElements(By.CssSelector("td[data-cell-order='1']"));
                            if (ilceElements.Count == 0)
                            {
                                _logger?.LogWarning("İlçe elementi bulunamadı, bu eczane atlanıyor.");
                                continue;
                            }
                            var ilce = ilceElements[0].Text;

                            // Tekrar deneme ile ilçe bilgisini garantileyelim
                            if (string.IsNullOrWhiteSpace(ilce))
                            {
                                int retryCount = 0;
                                while (string.IsNullOrWhiteSpace(ilce) && retryCount < 3)
                                {
                                    Thread.Sleep(500);
                                    ilce = ilceElements[0].Text.Trim();
                                    retryCount++;
                                }

                                // Hala boşsa, Javascript ile deneyelim
                                if (string.IsNullOrWhiteSpace(ilce))
                                {
                                    try
                                    {
                                        ilce = ((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].textContent", ilceElements[0]).ToString().Trim();
                                    }
                                    catch
                                    {
                                        // JS başarısız oldu, varsayılan değer kullan
                                        ilce = "Merkez"; // Varsayılan değer
                                    }
                                }
                            }

                            // Konum linkini al
                            var locationElements = eczaneElement.FindElements(By.CssSelector("td[data-cell-order='4'] a"));
                            if (locationElements.Count == 0)
                            {
                                _logger?.LogWarning("Konum linki bulunamadı, bu eczane atlanıyor.");
                                continue;
                            }
                            var locationUrl = locationElements[0].GetAttribute("href");

                            // Harita sayfasına git ve bilgileri al
                            driver.Navigate().GoToUrl(locationUrl);

                            // Retry mekanizması ile konum bilgilerini al
                            int coordRetryCount = 0;
                            double lat = 0, lng = 0;
                            bool coordSuccess = false;

                            while (!coordSuccess && coordRetryCount < 5)
                            {
                                try
                                {
                                    wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return typeof latti !== 'undefined' && typeof longi !== 'undefined';"));

                                    var latObject = ((IJavaScriptExecutor)driver).ExecuteScript("return latti;");
                                    var lngObject = ((IJavaScriptExecutor)driver).ExecuteScript("return longi;");

                                    if (latObject != null && lngObject != null)
                                    {
                                        lat = Convert.ToDouble(latObject);
                                        lng = Convert.ToDouble(lngObject);
                                        coordSuccess = true;
                                        _logger?.LogInformation("Konum bilgileri başarıyla alındı: {Lat}, {Lng}", lat, lng);
                                    }
                                    else
                                    {
                                        coordRetryCount++;
                                        _logger?.LogWarning("Konum bilgileri boş, {RetryCount}. deneme yapılıyor...", coordRetryCount);
                                        Thread.Sleep(1000);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    coordRetryCount++;
                                    _logger?.LogWarning(ex, "Konum bilgileri alınırken hata, {RetryCount}. deneme yapılıyor...", coordRetryCount);
                                    Thread.Sleep(1000);
                                }
                            }

                            if (!coordSuccess)
                            {
                                _logger?.LogError("Konum bilgileri alınamadı, varsayılan değerler kullanılacak.");
                                lat = 39.92077; // Türkiye ortası varsayılan değer
                                lng = 32.85411;
                            }

                            // Retry mekanizması ile detay bilgilerini al
                            string isim = GetElementTextWithRetry(driver, wait, "//dt[contains(text(), 'Adı')]/following-sibling::dd", "Eczane adı");
                            string telefon = GetElementTextWithRetry(driver, wait, "//dt[contains(text(), 'Telefon Numarası')]/following-sibling::dd", "Telefon");
                            string adres = GetElementTextWithRetry(driver, wait, "//dt[contains(text(), 'Adresi')]/following-sibling::dd", "Adres");

                            // Veritabanında bu eczane kaydı var mı kontrol et
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                                bool eczaneExists = await dbContext.Eczaneler
                                    .Where(e => e.IlId == ilId && e.Tarih == tarih)
                                    .AnyAsync(e => e.Isim == isim);

                                if (!eczaneExists)
                                {
                                    var eczane = new EczaneModel
                                    {
                                        Isim = isim,
                                        IlId = ilId, // İlişkisel yapı için IlId kullanılıyor
                                        Ilce = ilce,
                                        Telefon = telefon,
                                        Adres = adres,
                                        Enlem = lat.ToString(CultureInfo.InvariantCulture),
                                        Boylam = lng.ToString(CultureInfo.InvariantCulture),
                                        Tarih = tarih,
                                        KayitZamani = DateTime.Now
                                    };

                                    eczaneler.Add(eczane);
                                    _logger?.LogInformation("Eczane bilgileri başarıyla kaydedildi: {Isim}, {Il}, {Ilce}", isim, ilAdi, ilce);
                                }
                                else
                                {
                                    _logger?.LogInformation("Eczane zaten veritabanında mevcut: {Isim}, {Il}", isim, ilAdi);
                                }
                            }

                            driver.Navigate().Back();

                            // Listeye geri döndüğünde tablonun yüklenmesini bekle
                            try
                            {
                                wait.Until(d => d.FindElement(By.CssSelector("table tbody")));
                            }
                            catch (WebDriverTimeoutException)
                            {
                                _logger?.LogWarning("Listeye dönüşte tablo yüklenemedi, arama sayfasına yeniden gidiliyor.");

                                // Sayfaya tekrar git
                                driver.Navigate().GoToUrl("https://www.turkiye.gov.tr/saglik-titck-nobetci-eczane-sorgulama");
                                wait.Until(d => d.FindElement(By.Id("plakaKodu")));

                                // İl ve tarih seçimini tekrar yap
                                ilSelect = new SelectElement(driver.FindElement(By.Id("plakaKodu")));
                                ilSelect.SelectByText(ilAdi);

                                dateInput = driver.FindElement(By.Id("nobetTarihi"));
                                dateInput.Clear();
                                dateInput.SendKeys(tarih);

                                // Sorgula butonuna tıkla
                                searchButton = driver.FindElement(By.CssSelector("input[type='submit'][value='Sorgula']"));
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", searchButton);

                                wait.Until(d => d.FindElement(By.CssSelector("table tbody")));

                                // Kalan eczanelerin sırasını ayarla
                                eczaneElements = driver.FindElements(By.CssSelector("table tbody tr"));
                                i = i + 1 >= eczaneElements.Count ? eczaneElements.Count - 1 : i;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Eczane listesinde hata oluştu, işleme devam ediliyor.");

                            try
                            {
                                // Sayfaya tekrar git ve kaldığın yerden devam et
                                driver.Navigate().GoToUrl("https://www.turkiye.gov.tr/saglik-titck-nobetci-eczane-sorgulama");
                                wait.Until(d => d.FindElement(By.Id("plakaKodu")));

                                // İl ve tarih seçimini tekrar yap
                                ilSelect = new SelectElement(driver.FindElement(By.Id("plakaKodu")));
                                ilSelect.SelectByText(ilAdi);

                                dateInput = driver.FindElement(By.Id("nobetTarihi"));
                                dateInput.Clear();
                                dateInput.SendKeys(tarih);

                                // Sorgula butonuna tıkla
                                searchButton = driver.FindElement(By.CssSelector("input[type='submit'][value='Sorgula']"));
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", searchButton);

                                wait.Until(d => d.FindElement(By.CssSelector("table tbody")));

                                // Kalan eczanelerin sırasını ayarla
                                eczaneElements = driver.FindElements(By.CssSelector("table tbody tr"));
                                i = i + 1 >= eczaneElements.Count ? eczaneElements.Count - 1 : i;
                            }
                            catch
                            {
                                // Kritik hata, bu il için işlemi sonlandır
                                _logger?.LogError("Kritik hata, {Il} ili için veri çekme işlemi sonlandırılıyor.", ilAdi);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    _logger?.LogInformation("{Il} ili için nöbetçi eczane bulunamadı.", ilAdi);
                }

                // Veritabanına eczane bilgilerini kaydet
                if (eczaneler.Count > 0)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        await dbContext.Eczaneler.AddRangeAsync(eczaneler);
                        await dbContext.SaveChangesAsync();
                        _logger?.LogInformation("{Il} ili için {Count} adet eczane veritabanına kaydedildi", ilAdi, eczaneler.Count);
                    }
                }
                else
                {
                    _logger?.LogInformation("{Il} ili için kaydedilecek yeni eczane bulunamadı", ilAdi);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "{Il} ili için {Tarih} tarihinde veri çekme hatası", ilAdi, tarih);
                throw; // Manuel çağrılarda hatayı yukarıya ilet
            }
        }
    }
}