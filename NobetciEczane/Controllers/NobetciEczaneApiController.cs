using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NobetciEczane.Data;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Linq;

namespace NobetciEczane.Controllers
{
    [Route("api")]
    [ApiController]
    public class EczaneApiController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public EczaneApiController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("eczaneler")]
        public async Task<IActionResult> GetEczaneler(string il, string tarih)
        {
            if (string.IsNullOrEmpty(tarih))
            {
                tarih = DateTime.Now.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);
            }

            var eczaneler = await _dbContext.Eczaneler
                .Where(e => e.Il == il && e.Tarih == tarih)
                .ToListAsync();

            return Ok(eczaneler);
        }

        [HttpGet]
        [Route("eczane/{id}")]
        public async Task<IActionResult> GetEczane(int id)
        {
            var eczane = await _dbContext.Eczaneler.FindAsync(id);
            if (eczane == null)
            {
                return NotFound();
            }

            return Ok(eczane);
        }
    }
}