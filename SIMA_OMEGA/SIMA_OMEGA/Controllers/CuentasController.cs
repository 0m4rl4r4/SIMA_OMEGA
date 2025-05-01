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

namespace SIMA_OMEGA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuentasController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration configuration;

        public CuentasController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
        }

        private async Task<RespuestaAuthentication> ConstruirToken(CredencialesUsuario credencialesUsuario)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Email, credencialesUsuario.Email)
    };

            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);
            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado");
            }

            // Agregar otros reclamos si existen
            var claimsRoles = await userManager.GetClaimsAsync(usuario);
            claims.AddRange(claimsRoles);

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["LlaveJWT"]));
            var cred = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddDays(1);

            var securityToken = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiracion,
                signingCredentials: cred
            );

            return new RespuestaAuthentication
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiration = expiracion,
            };
        }

        [HttpGet("RenovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAuthentication>> Renovar()
        {
            var emailClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
            if (emailClaim == null)
            {
                return Unauthorized(new MensajeError { Error = "Usuario no autenticado." });
            }

            var credencialesUsuario = new CredencialesUsuario
            {
                Email = emailClaim.Value
            };

            // Genera un nuevo token
            var nuevoToken = await ConstruirToken(credencialesUsuario);

            return Ok(nuevoToken);
        }


        [HttpPost("registrar")]
        [Consumes("multipart/form-data")] // Indica que se aceptan datos de formulario de varias partes
        public async Task<ActionResult<RespuestaAuthentication>> Registrar([FromForm] CredencialesUsuario credencialesUsuario)
        {
            var usuarioExistente = await userManager.FindByEmailAsync(credencialesUsuario.Email);
            if (usuarioExistente != null)
            {
                return BadRequest(new { Error = "El usuario ya está registrado." });
            }

            // Crear el usuario
            var usuario = new ApplicationUser
            {
                UserName = credencialesUsuario.Email,
                Email = credencialesUsuario.Email
            };



            // Crear usuario en Identity
            var resultado = await userManager.CreateAsync(usuario, credencialesUsuario.Password);
            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }

            var errores = resultado.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { Error = string.Join(", ", errores) });
        }



        [HttpPost("Login")]
        [AllowAnonymous] // Permite que el endpoint sea accesible sin autenticación previa
        public async Task<ActionResult<RespuestaAuthentication>> Login(CredencialesUsuario credencialesUsuario)
        {
            // Verificar si el usuario existe
            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);
            if (usuario == null)
            {
                return BadRequest(new MensajeError { Error = "Usuario no registrado." });
            }

            // Validar la contraseña utilizando SignInManager
            var resultado = await signInManager.PasswordSignInAsync(usuario.UserName,
                credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }

            return BadRequest(new MensajeError { Error = "Login incorrecto. Verifique sus credenciales." });
        }


        [HttpGet("perfil")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<UserProfileDTO>> GetUserProfile()
        {
            var emailClaim = HttpContext.User.FindFirst(ClaimTypes.Email);
            if (emailClaim == null)
            {
                return Unauthorized(new { Error = "Usuario no autenticado" });
            }

            var usuario = await userManager.FindByEmailAsync(emailClaim.Value);
            if (usuario == null)
            {
                return NotFound(new { Error = "Usuario no encontrado" });
            }

            var userProfile = new UserProfileDTO
            {
                Email = usuario.Email,
                UserName = usuario.UserName,
            };

            return Ok(userProfile);
        }

        //Recuperar contraseña
        //Endpoint para solicitar recuperacion
        [HttpPost("olvide-password")]
        public async Task<IActionResult> OlvidePassword([FromBody] OlvidePasswordDTO modelo)
        {
            var usuario = await userManager.FindByEmailAsync(modelo.Email);
            if (usuario == null)
            {
                return BadRequest(new { Error = "Usuario no encontrado." });
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(usuario);
            var urlReset = $"{modelo.UrlRedireccion}?email={modelo.Email}&token={Uri.EscapeDataString(token)}";

            // Enviar correo
            await EnviarCorreo(modelo.Email, "Recuperación de contraseña", $"Para restablecer tu contraseña haz clic aquí: <a href='{urlReset}'>Recuperar Contraseña</a>");

            return Ok(new { Mensaje = "Se ha enviado un enlace de recuperación al correo." });
        }

        //Metodo para enviar smtp

        private async Task EnviarCorreo(string destino, string asunto, string cuerpoHtml)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com") // Cambia por tu SMTP
            {
                Port = 587,
                Credentials = new NetworkCredential("omarlarapro260800@gmail.com", "ospm ilnu fcvd mfam"),
                EnableSsl = true,
            };

            var mensaje = new MailMessage
            {
                From = new MailAddress("tu_correo@dominio.com"),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true,
            };

            mensaje.To.Add(destino);
                
            await smtpClient.SendMailAsync(mensaje);
        }

        // EndPoint para restablecer contraseña
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO modelo)
        {
            var usuario = await userManager.FindByEmailAsync(modelo.Email);
            if (usuario == null)
            {
                return BadRequest(new { Error = "Usuario no encontrado." });
            }

            var resultado = await userManager.ResetPasswordAsync(usuario, modelo.Token, modelo.NuevaPassword);

            if (resultado.Succeeded)
            {
                return Ok(new { Mensaje = "Contraseña actualizada exitosamente." });
            }

            return BadRequest(new { Error = string.Join(", ", resultado.Errors.Select(e => e.Description)) });
        }





    }
}
