using System.ComponentModel.DataAnnotations.Schema;
using Common.Core.Entities;

namespace Common.TestApi.Entities
{
    [Table("Usuario")]
    public class Usuario : BaseEntity
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
    }
}
