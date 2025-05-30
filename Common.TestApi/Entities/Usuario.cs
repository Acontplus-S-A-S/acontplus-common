using System.ComponentModel.DataAnnotations.Schema;
using Common.Core.Base;

namespace Common.TestApi.Entities
{
    //[Table("usuarios", Schema = "seguridad")]
    public class Usuario : BaseEntity
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
    }
}
