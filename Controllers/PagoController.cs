using Lab3InmibiliariaVisual.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab3InmibiliariaVisual.Controllers
{
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PagosController : ControllerBase
    {
        private readonly DataContext contexto;

        public PagosController(DataContext context)
        {
            contexto = context;
        }

        // GET
        //este metodo se usa en la vista de Contratos en el boton pagos
        //me devuelve la lista de pagos de un contrato vigente
        [HttpGet("{id}")]
        public async Task<ActionResult<List<Pago>>> GetPagos(int id)
        {

            try
            {
                var pagos = await contexto.Pagos.Include(x => x.contrato).Where(x =>
                     x.ContratoId == id
                    ).ToListAsync();

                return Ok(pagos);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());

            }
        }

        }
}