using Lab3InmibiliariaVisual.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab3InmibiliariaVisual.Controllers
{
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ContratosController : ControllerBase
    {
        private readonly DataContext contexto;

        public ContratosController(DataContext context)
        {
            contexto = context;
        }

        // GET: api/Contratos
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContrato(int id)
        {
            try
            {
                var usuario = User.Identity.Name;
                var contrato = await contexto.Contratos
                                    .Include(x => x.Inquilino)
                                    .Include(x => x.Inmueble)
                                    .Where(x => x.Inmueble.Duenio.Email == usuario)
                                    .SingleOrDefaultAsync(x => x.Id == id);
                return contrato != null ? Ok(contrato) : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetListaContratos()
        {
            try
            {
                var usuario = User.Identity.Name;
                var lista = await contexto.Contratos
                                .Include(x => x.Inquilino)
                                .Include(x => x.Inmueble)
                               // .Include(x=> x.Garante)
                                .Where(x => x.Inmueble.Duenio.Email == usuario).ToListAsync();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}