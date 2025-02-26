using System;
using System.ComponentModel.DataAnnotations;

namespace LicencjatUG.Server.Models
{
    public class CalendarEvent
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }
}
