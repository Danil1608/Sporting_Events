using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Sporting_Events.Models
{
    public class Account
    {
        public int Id { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string MiddleName { get; set; }

        [Required]
        public string LastName { get; set; }

        public int Age { get; set; }

        [Required]
        public string Phone { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }

        public List<Competition> Competitions { get; set; } = new();
        public List<Request> Requests { get; set; } = new();
        public List<Result> Results { get; set; } = new();
        public List<Competition> OrganizersCompetitions { get; set; } = new();
    }
}
