using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Sporting_Events.Models
{
    public class Result
    {
        public int Id { get; set; }

        [Required]
        public string CompResult { get; set; }

        public int AccountId { get; set; }
        public Account Account { get; set; }
        
        public int CompetitionId { get; set; }
        public Competition Competition { get; set; }
    }
}
