namespace BBooking.Models.Entities;

    public class Localita
    {
        public int Id { get; set; }
        public required string Nome { get; set; } = string.Empty;
        public required string Regione { get; set; } = string.Empty;
        public ICollection<CasaVacanze> CaseVacanze { get; set; } = new List<CasaVacanze>();
    }