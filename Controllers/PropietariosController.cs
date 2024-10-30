using Lab3InmibiliariaVisual.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DataContext = Lab3InmibiliariaVisual.Models.DataContext;
using MimeKit;
using Lab3InmibiliariaVisual.Services;

namespace Lab3InmibiliariaVisual.Controllers
{
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PropietariosController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;

        private readonly IWebHostEnvironment environment;

        public PropietariosController(DataContext contexto, IConfiguration config, IWebHostEnvironment environment)
        {
            this.contexto = contexto;
            this.config = config;
            this.environment = environment;
        }


        // GET: api/<PropietariosController>
        //obtiene datos del propietario logueado
        [HttpGet]
        public async Task<ActionResult<Propietario>> Get()
        {
            try
            {
                var usuario = User.Identity.Name;
                return await contexto.Propietarios.SingleOrDefaultAsync(x => x.Email == usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        // POST api/<controller>/login
        [HttpPost("login")]
        [AllowAnonymous]
        
        public async Task<IActionResult> Login([FromForm] Login login)
        {
            Propietario p = null;
            try
            {
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: login.Clave,
                    salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8));
                 p = await contexto.Propietarios.FirstOrDefaultAsync(x => x.Email == login.Email);
                if (p == null || p.Clave != hashed)
                {
                    return BadRequest("Nombre de usuario o clave incorrecta");
                }
                else
                {
                    var key = new SymmetricSecurityKey(
                        System.Text.Encoding.ASCII.GetBytes(config["TokenAuthentication:SecretKey"]));
                    var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, p.Email),
                        new Claim("FullName", p.Nombre + " " + p.Apellido),
                        new Claim(ClaimTypes.Role, "Propietario"),
                    };

                    var token = new JwtSecurityToken(
                        issuer: config["TokenAuthentication:Issuer"],
                        audience: config["TokenAuthentication:Audience"],
                        claims: claims,
                        expires: DateTime.Now.AddDays(360),
                        signingCredentials: credenciales
                    );
                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }

        //PUT api/Controller/5
        //edita los datos del propietario logueado
        [HttpPut()]
        public async Task<IActionResult> Put([FromBody] Propietario p) {
             try
            {
                if (ModelState.IsValid)
                {
                    
                    var propietario = await contexto.Propietarios.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Email == p.Email);
                    if(p.Clave == null)
                    {
                        p.Clave = propietario.Clave; 
                    }
                    
                    p.Id = propietario.Id;
                    contexto.Propietarios.Update(p);
                    await contexto.SaveChangesAsync();
                    return Ok(p);
                }
                Console.WriteLine("despues del if");
                return BadRequest();
            }
            catch (Exception ex) {
                return BadRequest(ex.Message.ToString());
            }
        }

        //editar contraseña
        [HttpPatch("cambiarPass")]
		public async Task<IActionResult> CambiarPass([FromForm] String clVieja, String clNueva ){
		
			var user = User.Identity.Name;
            var propietario = await contexto.Propietarios.FirstOrDefaultAsync(u=>u.Email==user);
			string hashed = Hashear(clVieja);
			try{
                if(propietario.Clave == hashed){
                    clNueva = Hashear(clNueva);
                    propietario.Clave = clNueva;
                    contexto.Propietarios.Update(propietario);
                    await contexto.SaveChangesAsync();
                    
                }
            
                return Ok(propietario);
            }
            catch(Exception ex){
                return BadRequest(ex.Message.ToString());
            }
		}

        //funcion para hashear clave
        private String Hashear(String clave){
           clave =  Convert.ToBase64String(KeyDerivation.Pbkdf2(
							password: clave,
							salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
							prf: KeyDerivationPrf.HMACSHA1,
							iterationCount: 1000,
							numBytesRequested: 256 / 8));
            return (clave);

        }

        // GET api/<controller>/token
		[HttpGet("token")]
		public async Task<IActionResult> Token()
		{
			try
			{ //este método si tiene autenticación, al entrar, generar clave aleatorio y enviarla por correo
				var perfil = new
				{
					Email = User.Identity.Name,
					Nombre = User.Claims.First(x => x.Type == "FullName").Value,
					Rol = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value
				};
				Random rand = new Random(Environment.TickCount);
				string randomChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
				string nuevaClave = "";
				for (int i = 0; i < 8; i++)
				{
					nuevaClave += randomChars[rand.Next(0, randomChars.Length)];
				}//!Falta hacer el hash a la clave y actualizar el usuario con dicha clave
				var message = new MimeKit.MimeMessage();
				message.To.Add(new MailboxAddress(perfil.Nombre, perfil.Email));
				message.From.Add(new MailboxAddress("Inmobiliaria Lillo", config["SMTPUser"]));
				message.Subject = "Prueba de Correo desde API";
				message.Body = new TextPart("html")
				{
					Text = @$"<h1>Hola</h1>
					<p>¡Bienvenido, {perfil.Nombre}!</p>",//falta enviar la clave generada (sin hashear)
				};
				message.Headers.Add("Encabezado", "Valor");//solo si hace falta
				message.Headers.Add("Otro", config["Valor"]);//otro ejemplo
				MailKit.Net.Smtp.SmtpClient client = new SmtpClient();
				client.ServerCertificateValidationCallback = (object sender,
					System.Security.Cryptography.X509Certificates.X509Certificate certificate,
					System.Security.Cryptography.X509Certificates.X509Chain chain,
					System.Net.Security.SslPolicyErrors sslPolicyErrors) =>
				{ return true; };
				client.Connect("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.Auto);
				client.Authenticate(config["SMTPUser"], config["SMTPPass"]);//estas credenciales deben estar en el user secrets
				await client.SendAsync(message);
				return Ok(perfil);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		// GET api/<controller>/email
		[HttpPost("email")]
		[AllowAnonymous]
		public async Task<IActionResult> GetByEmail([FromForm] string email)
		{
			try
			{ //método sin autenticar, busca el propietario x email
				var entidad = await contexto.Propietarios.FirstOrDefaultAsync(x => x.Email == email);
				//para hacer: si el propietario existe, mandarle un email con un enlace con el token
				//ese enlace servirá para resetear la contraseña
				//Dominio sirve para armar el enlace, en local será la ip y en producción será el dominio www...
				var url = this.GenerarUrlCompleta("Token", "Propietarios", environment);
				var dominio = environment.IsDevelopment() ? HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() : "www.misitio.com";
				//añadir: .....?access_token=token
				return entidad != null ? Ok(entidad) : NotFound();
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
    }
}