using Common.TestApi.Entities;
using Common.TestApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Common.TestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController(IUsuarioService usuarioService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Usuario usuario)
        {
            return Ok(await usuarioService.AddAsync(usuario));
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Usuario usuario)
        {
            return Ok(await usuarioService.UpdateAsync(id, usuario));
        }
    }
}
