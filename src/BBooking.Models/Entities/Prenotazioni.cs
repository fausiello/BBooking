using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBooking.Models.Entities;
  public class Prenotazione
    {
        public int Id { get; set; }

        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }

        public StatoPrenotazione Stato { get; set; } = StatoPrenotazione.InAttesa; // Default a Pendente

        public decimal Totale { get; set; }

        // Foreign Keys
        public int GuestId { get; set; }
        public int CasaVacanzeId { get; set; }

        // Navigation Properties Singole
        [ForeignKey(nameof(GuestId))]
        public Utente Guest { get; set; } = null!;

        [ForeignKey(nameof(CasaVacanzeId))]
        public CasaVacanze CasaVacanze { get; set; } = null!;
    }