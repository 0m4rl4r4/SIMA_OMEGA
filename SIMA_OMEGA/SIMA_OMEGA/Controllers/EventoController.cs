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
        [Route ("api/[controller]")]
        public class EventosController : ControllerBase
        {
            private readonly ApplicationDbContext _context;

            public EventosController(ApplicationDbContext context)
            {
                _context = context;
            }

            [HttpPost("agregar")]
            public async Task<IActionResult> AgregarEvento([FromBody] EventoDTO dto)
            {
                if (string.IsNullOrWhiteSpace(dto.EventoNombre) || string.IsNullOrWhiteSpace(dto.Descripcion))
                {
                    return BadRequest("Evento y Descripción son requeridos.");
                }

                var nuevoEvento = new Evento
                {
                    EventoNombre = dto.EventoNombre,
                    Descripcion = dto.Descripcion,
                    FechaHora = DateTime.Now
                };

                _context.Eventos.Add(nuevoEvento);
                await _context.SaveChangesAsync();

                var eventoDto = new EventoDTO
                {
                    Id = nuevoEvento.Id,
                    EventoNombre = nuevoEvento.EventoNombre,
                    Descripcion = nuevoEvento.Descripcion,
                    FechaHora = nuevoEvento.FechaHora
                };
                return Ok(eventoDto);
            }

            [HttpGet("listar")]
            public async Task<ActionResult<IEnumerable<MostrarEventosDTO>>> ObtenerEventos()
            {
                var eventos = await _context.Eventos
                    .OrderByDescending(e => e.FechaHora)
                    .Select(e => new MostrarEventosDTO
                    {
                        Id = e.Id,
                        EventoNombre = e.EventoNombre,
                        Descripcion = e.Descripcion,
                        FechaHora = e.FechaHora
                    })
                    .ToListAsync();

                return Ok(eventos);
            }
        }
    }

