namespace BBooking.Models.Entities;
public class Servizio
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        // Navigation Property: Collezione N:N verso CasaVacanze
        public ICollection<CasaVacanze> CaseVacanze { get; set; } = new List<CasaVacanze>();
    }
