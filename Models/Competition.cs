using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Sporting_Events.Models
{
    public class Competition
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public DateTime ExpDate { get; set; }

        [Required]
        public string Location { get; set; }

        public int MembersCount { get; set; }
        public int PrizePool { get; set; }
        public int TypeId { get; set; }
        public CompetitionType CompetitionType { get; set; }

        public List<Account> Accounts = new List<Account>();
    }
}
