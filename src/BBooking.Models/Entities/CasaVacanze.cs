namespace BBooking.Models.Entities;
public class CasaVacanze
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        public string? Descrizione { get; set; } // Nullable

        public decimal PrezzoPerNotte { get; set; }

        // Foreign Keys
        public int HostId { get; set; }
        public int LocalitaId { get; set; }

        // Navigation Properties Singole
        [ForeignKey(nameof(HostId))]
        public Utente Host { get; set; } = null!;

        [ForeignKey(nameof(LocalitaId))]
        public Localita Localita { get; set; } = null!;

        // Navigation Properties Collezioni
        public ICollection<Prenotazione> Prenotazioni { get; set; } = new List<Prenotazione>();
        public ICollection<Servizio> Servizi { get; set; } = new List<Servizio>(); // Relazione N:N
    }