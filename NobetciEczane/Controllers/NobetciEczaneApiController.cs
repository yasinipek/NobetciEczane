using Microsoft.AspNetCore.Mvc;
using NobetciEczane.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NobetciEczane.Controllers
{
    [Route("api/scraping")]
    [ApiController]
    public class EczaneScrapingController : ControllerBase
    {
        private readonly EczaneService _eczaneService;
        private static CancellationTokenSource _cancellationTokenSource;

        public EczaneScrapingController(EczaneService eczaneService)
        {
            _eczaneService = eczaneService;
        }

        // POST: api/scraping/start
        [HttpPost("start")]
        public IActionResult StartScraping()
        {
            try
            {
                // Önceki işlem çalışıyorsa iptal et
                CancelExistingScraping();

                // Yeni işlem başlat
                _cancellationTokenSource = new CancellationTokenSource();

                // İşlemi background thread'de başlat ki API çağrısı hemen cevap versin
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _eczaneService.StartScraping(_cancellationTokenSource.Token);
                    }
                    catch (Exception)
                    {
                        // Loglama EczaneService içinde yapılıyor
                    }
                }, _cancellationTokenSource.Token);

                return Ok(new { message = "Nöbetçi eczane veri çekme işlemi başlatıldı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Bir hata oluştu: {ex.Message}" });
            }
        }

        // POST: api/scraping/city/ANKARA
        [HttpPost("city/{il}")]
        public IActionResult StartScrapingForCity(string il)
        {
            try
            {
                // Önceki işlem çalışıyorsa iptal et
                CancelExistingScraping();

                // Yeni işlem başlat
                _cancellationTokenSource = new CancellationTokenSource();

                // İşlemi background thread'de başlat ki API çağrısı hemen cevap versin
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _eczaneService.StartScrapingForCity(il, _cancellationTokenSource.Token);
                    }
                    catch (Exception)
                    {
                        // Loglama EczaneService içinde yapılıyor
                    }
                }, _cancellationTokenSource.Token);

                return Ok(new { message = $"{il} ili için nöbetçi eczane veri çekme işlemi başlatıldı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Bir hata oluştu: {ex.Message}" });
            }
        }

        // POST: api/scraping/stop
        [HttpPost("stop")]
        public IActionResult StopScraping()
        {
            CancelExistingScraping();
            return Ok(new { message = "Nöbetçi eczane veri çekme işlemi durduruldu." });
        }

        // GET: api/scraping/status
        [HttpGet("status")]
        public IActionResult GetScrapingStatus()
        {
            bool isRunning = _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested;
            return Ok(new { isRunning = isRunning });
        }

        private void CancelExistingScraping()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }
    }
}