using Lab3InmibiliariaVisual.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Lab3InmibiliariaVisual.Controllers
{
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    
    public class InmueblesController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment environment;

        public InmueblesController(DataContext context, IConfiguration config, IWebHostEnvironment environment)
        {
            contexto = context;
            this.config = config;
            this.environment = environment;
        }

        // GET: api/Inmuebles/5
        [HttpGet]
        //este metodo se usa en la vista Inmuebles.
        //Me devuelve una lista de todos los inmuebles del usuario actual
        public async Task<ActionResult<Inmueble>> GetListaInmuebles()
        {
            try
            {
                var usuario = User.Identity.Name;
                return Ok(contexto.Inmuebles.Include(e => e.Duenio).Include(e => e.Tipo).Where(e => e.Duenio.Email == usuario));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/Inmuebles/5
        //este metodo se usa en la vista detalle inmueble
        //y me devuelve los datos de un inmueble
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInmueblePorId(int id)
        {
            try
            {
                var usuario = User.Identity.Name;
                return Ok(contexto.Inmuebles.Include(e => e.Duenio).Where(e => e.Duenio.Email == usuario).Single(e => e.Id == id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        //Put api/Inmuebles/EditarEstado/5
        //Para cambiar el estado entre disponible y no disponible
     
        [HttpPut("{id}")] 
        public async Task<IActionResult> EditarEstado(int id)
        {
            try
            {
                var usuario = User.Identity.Name;
                var i = contexto.Inmuebles.Include(x => x.Duenio)
                    .FirstOrDefault(x => x.Id == id && x.Duenio.Email == usuario);
                if (i != null) { 
                    i.Disponible = !i.Disponible;
                    contexto.Inmuebles.Update(i);
                    await contexto.SaveChangesAsync();
                    return Ok(i);
                }
                return BadRequest();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        } 

       //este metodo se usa en la vista Inquilinos
       //Me devuelve una lista con los inmuebles alquilados del usuario actual
        [HttpGet("InmueblesConContrato")]
        public async Task<IActionResult> GetInmueblesAlquilados()
        {
            try
            {
                var usuario = User.Identity.Name;
                var fecha_actual = DateTime.Now;

                var query = from inmu in contexto.Inmuebles
                              join cont in contexto.Contratos
                                on inmu.Id equals cont.InmuebleId
                            where cont.FecInicio <= fecha_actual && cont.FecFin >= fecha_actual && usuario == inmu.Duenio.Email
                            select cont;
             return Ok(query);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] Inmueble inmueble){
            try{
                 inmueble.PropietarioId =  contexto.Propietarios.Single(x=>x.Email == User.Identity.Name).Id;
                //no trae el Id de Inmueble
                 if(ModelState.IsValid){
                    await contexto.Inmuebles.AddAsync(inmueble);
                    contexto.SaveChangesAsync();
                    if(inmueble.imagen!=null){
                        var imagePath = await guardarImagen(inmueble);
                        inmueble.imgUrl = imagePath;
                        await contexto.SaveChangesAsync();
                    }
                    
                    return CreatedAtAction(nameof(GetInmueblePorId), new { id = inmueble.Id }, inmueble);
                }
                
                return BadRequest("Model State no es valido.");
            
            }catch (Exception ex){
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }

        }

        //funcion asincrona para guardar la imagen y modificarle tamaño.
       public async Task<string> guardarImagen(Inmueble entidad)
        {
            try
            {
                string wwwPath = environment.WebRootPath;
                string path = Path.Combine(wwwPath, "uploads");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string fileName = "inmueble_" + entidad.Id + Path.GetExtension(entidad.imagen.FileName);
                string pathCompleto = Path.Combine(path, fileName);
                
                // Esta operación guarda la foto en memoria en la ruta que necesitamos
                using (FileStream stream = new FileStream(pathCompleto, FileMode.Create))
                {
                    await entidad.imagen.CopyToAsync(stream);
                    stream.Dispose();
                }
                using (var image = Image.Load(pathCompleto))
                {
                    image.Mutate(x => x.Resize(500, 500));
                    var resizedImagePath = Path.Combine(environment.WebRootPath, "uploads", Path.GetFileName(fileName));
                    image.Save(resizedImagePath);
                    return Path.Combine("uploads", Path.GetFileName(pathCompleto)).Replace("\\", "/");
                }
            }
            catch (Exception ex)
            {
                return "Excepcion en cargar imagen";
            }
        }
        // crear nuevo inmueble del propietario logueado.
        /*[HttpPost]
        public async Task<IActionResult> Post([FromForm] Inmueble inmueble){
            try{
              if(ModelState.IsValid){
                inmueble.PropietarioId =  contexto.Propietarios.Single(x=>x.Email== User.Identity.Name).Id;
                contexto.Inmuebles.Add(inmueble);
                await contexto.SaveChangesAsync();
                Console.WriteLine("inmueble.imagen: " + inmueble.imagen);
                if (inmueble.imagen != null && inmueble.Id > 0)
				{
					string wwwPath = environment.WebRootPath;
					string path = Path.Combine(wwwPath, "Uploads");
					if (!Directory.Exists(path))
					{
						Directory.CreateDirectory(path);
					}
					
					string fileName = "inmueble_" + inmueble.Id + Path.GetExtension(inmueble.imagen.FileName);
					string pathCompleto = Path.Combine(path, fileName);
					inmueble.imgUrl = Path.Combine("/Uploads", fileName);
					// Esta operación guarda la foto en memoria en la ruta que necesitamos
					using (FileStream stream = new FileStream(pathCompleto, FileMode.Create))
					{
						inmueble.imagen.CopyTo(stream);
					}
					contexto.inmuebles.Update(inmueble);
				}
                
                return CreatedAtAction(nameof(GetInmueblePorId), new { id = inmueble.Id }, inmueble);
            }
            else {
                return BadRequest("Model State no es valido.");
            }
            }catch (Exception ex){
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }

        }*/

        // POST api/<controller>
        //este metodo envia la foto del inmueble
       /*
       [HttpPost]
       public async Task<IActionResult> Post([FromBody] Inmueble entidad)
        {
            try
            {
                var direccion = entidad.Direccion;
                var duenioId = User.Identity.Name;

                if (entidad.imagen.Length > 0)
                {
                    if (ModelState.IsValid)
                    {
                        entidad.Id = contexto.Propietarios.Single(e => e.Email == User.Identity.Name).Id;
 
                        var stream1 = new MemoryStream(Convert.FromBase64String(entidad.imagen));
                        IFormFile ImagenInmo = new FormFile(stream1, 0, stream1.Length, "inmueble", ".jpg");
                        string wwwPath = environment.WebRootPath;
                        string path = Path.Combine(wwwPath, "imagenes");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        Random r = new Random();
                        //Path.GetFileName(u.AvatarFile.FileName);//este nombre se puede repetir
                        string fileName = "foto_" + entidad.Id + Path.GetExtension(entidad.imagen.FileName);
                        string pathCompleto = Path.Combine(path, fileName);

                        entidad.imagen = Path.Combine("/imagenes", fileName);
                        using (FileStream stream = new FileStream(pathCompleto, FileMode.Create))
                        {
                            entidad.imagen.CopyTo(stream);
                        }
                        
                        contexto.Inmuebles.Add(entidad);
                        contexto.SaveChanges();
                        return CreatedAtAction(nameof(GetInmueblePorId), new { id = entidad.Id }, entidad);

                    }
                    return BadRequest(ModelState);
                }
                return BadRequest(Ok("Imagen no encontrada"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }*/
    }
}