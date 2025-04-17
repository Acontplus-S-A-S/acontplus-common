using Common.TestApi.DTOs;
using Common.TestApi.Entities;

namespace Common.TestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController(IUsuarioService usuarioService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UsuarioDto usuarioDto)
        {
            var usuario = ObjectMapper.Map<UsuarioDto, Usuario>(usuarioDto);

            return Ok(await usuarioService.AddAsync(usuario));
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] UsuarioDto usuarioDto)
        {
            var usuario = ObjectMapper.Map<UsuarioDto, Usuario>(usuarioDto);

            return Ok(await usuarioService.UpdateAsync(id, usuario));
        }
    }
}
