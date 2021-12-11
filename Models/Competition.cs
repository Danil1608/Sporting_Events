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

        public int CompetitionTypeId { get; set; }
        public CompetitionType CompetitionType { get; set; }

        public int? OrganizerId { get; set; }
        public Account Account { get; set; }

        public int? AppFileId { get; set; }
        public AppFile AppFile { get; set; }

        public List<Account> Accounts { get; set; } = new();
    }
}
