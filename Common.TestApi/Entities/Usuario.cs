using System.ComponentModel.DataAnnotations.Schema;
using Common.Core.Entities;

namespace Common.TestApi.Entities
{
    [Table("Usuario")]
    public class Usuario : BaseEntity
    {
        public string Username { get; set; }
        public string Email { get; set; }
    }
}
