using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMA_OMEGA;
using SIMA_OMEGA.DTOs;
using SIMA_OMEGA.Entities;
using System;

[ApiController]
[Route("api/[controller]")]
public class SembradiosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SembradiosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/sembradios
    [HttpPost]
    public async Task<ActionResult<SembradiosDTO>> Crear(SembradiosDTO dto)
    {
        var sembradio = new Sembradio
        {
            TipoPlanta = dto.TipoPlanta,
            ExtensionMts2 = dto.ExtensionMts2,
            FotoSembradio = dto.FotoSembradio
            // FechaRegistro se establece automáticamente
        };

        _context.Sembradios.Add(sembradio);
        await _context.SaveChangesAsync();

        var result = new SembradiosDTO
        {
            Id = sembradio.Id,
            TipoPlanta = sembradio.TipoPlanta,
            ExtensionMts2 = sembradio.ExtensionMts2,
            FotoSembradio = sembradio.FotoSembradio,
            FechaRegistro = sembradio.FechaRegistro
        };

        return CreatedAtAction(nameof(GetTodos), new { id = sembradio.Id }, result);
    }

    // GET: api/sembradios
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MostrarSembradiosDTO>>> GetTodos()
    {
        var sembradios = await _context.Sembradios
            .Select(s => new MostrarSembradiosDTO
            {
                Id = s.Id,
                TipoPlanta = s.TipoPlanta,
                ExtensionMts2 = s.ExtensionMts2,
                FotoSembradio = s.FotoSembradio,
                FechaRegistro = s.FechaRegistro
            })
            .ToListAsync();

        return Ok(sembradios);
    }
}