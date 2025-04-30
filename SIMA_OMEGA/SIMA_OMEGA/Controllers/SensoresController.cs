using SIMA_OMEGA.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SIMA_OMEGA.DTOs;
using SIMA_OMEGA;
using System.Net.Mail;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System;

namespace SIMA_OMEGA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SensoresController(ApplicationDbContext context)
        {
            _context = context;
        }


        // POST api/sensores
        [HttpPost]
        public async Task<IActionResult> RegistrarDatos([FromBody] SensorDataDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sensorData = new SensorData
            {
                Temperatura = dto.Temperatura,
                Humedad = dto.Humedad
            };

            _context.Sensores.Add(sensorData);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Datos guardados correctamente" });
        }

        // GET api/sensores/historial
        [HttpGet("historial")]
        public async Task<IActionResult> ObtenerHistorial()
        {
            var historial = await _context.Sensores
                .OrderByDescending(s => s.FechaRegistro)
                .Select(s => new HistorialSensorDTO
                {
                    Temperatura = s.Temperatura,
                    Humedad = s.Humedad,
                    FechaRegistro = s.FechaRegistro
                })
                .ToListAsync();

            return Ok(historial);
        }
    }
}
