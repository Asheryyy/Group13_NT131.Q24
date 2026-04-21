using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEapp.Models.Entities
{
	[Table("UserLogin")]
	public class User
	{
		[Key]
		[MaxLength(50)]
		[Column(TypeName = "varchar(50)")]
		public string Id { get; set; }

		[MaxLength(50)]
		[Column(TypeName = "varchar(50)")]
		public string UserName { get; set; }
		public string PasswordHash { get; set; }
		public string Email { get; set; }
		public DateTime CreatedAt { get; set; }

	}
}
