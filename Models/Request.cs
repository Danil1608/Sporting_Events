using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Sporting_Events.Models
{
    public class Request
    {
        public int Id { get; set; }

        public int AccountId { get; set; }
        public Account Account { get; set; }

        public int CompetitionId { get; set; }
        public Competition Competition { get; set; }

        [Required]
        public string Status { get; set; }
    }
}
