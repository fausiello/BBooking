using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBooking.Models.Entities;
  public class Utente
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public RuoloUtente Ruolo { get; set; } = RuoloUtente.Guest; // Default a Guest

        // Navigation Properties
        [InverseProperty(nameof(CasaVacanze.Host))]
        public ICollection<CasaVacanze> CaseOspitate { get; set; } = new List<CasaVacanze>();

        [InverseProperty(nameof(Prenotazione.Guest))]
        public ICollection<Prenotazione> PrenotazioniEffettuate { get; set; } = new List<Prenotazione>();
    }