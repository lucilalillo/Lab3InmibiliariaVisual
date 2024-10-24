using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Lab3InmibiliariaVisual.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Lab3InmibiliariaVisual.Controllers
{
     [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PropietariosController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;

        public PropietariosController(DataContext contexto, IConfiguration config)
        {
            this.contexto = contexto;
            this.config = config;
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
    }
}