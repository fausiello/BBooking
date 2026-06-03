using Microsoft.EntityFrameworkCore;
using BBooking.Models.Entities;

namespace BBooking.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSet per tutte le entità del dominio
    public DbSet<Utente> Utenti { get; set; }
    public DbSet<Localita> Localita { get; set; }
    public DbSet<CasaVacanze> CaseVacanze { get; set; }
    public DbSet<Prenotazione> Prenotazioni { get; set; }
    public DbSet<Servizio> Servizi { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurazione precisione per i campi decimali (evita warning EF Core)
        modelBuilder.Entity<CasaVacanze>()
            .Property(c => c.PrezzoPerNotte)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Prenotazione>()
            .Property(p => p.Totale)
            .HasPrecision(18, 2);

        // 1. Configurazione Relazione N:N pura tra CasaVacanze e Servizio
        modelBuilder.Entity<CasaVacanze>()
            .HasMany(c => c.Servizi)
            .WithMany(s => s.CaseVacanze);

        // 2. Configurazione Relazione 1:N tra Utente (Guest) e Prenotazione (DeleteBehavior.Restrict)
        modelBuilder.Entity<Prenotazione>()
            .HasOne(p => p.Guest)
            .WithMany(u => u.PrenotazioniEffettuate)
            .HasForeignKey(p => p.GuestId)
            .OnDelete(DeleteBehavior.Restrict);

        // Mapping esplicito delle altre chiavi esterne (opzionale ma consigliato per robustezza)
        modelBuilder.Entity<Prenotazione>()
            .HasOne(p => p.CasaVacanze)
            .WithMany(c => c.Prenotazioni)
            .HasForeignKey(p => p.CasaVacanzeId);

        modelBuilder.Entity<CasaVacanze>()
            .HasOne(c => c.Host)
            .WithMany(u => u.CaseOspitate)
            .HasForeignKey(c => c.HostId);

        modelBuilder.Entity<CasaVacanze>()
            .HasOne(c => c.Localita)
            .WithMany(l => l.CaseVacanze)
            .HasForeignKey(c => c.LocalitaId);

        // 3. Seed Data

        // Seed Localita
        modelBuilder.Entity<Localita>().HasData(
            new Localita { Id = 1, Nome = "Roma", Regione = "Lazio" },
            new Localita { Id = 2, Nome = "Napoli", Regione = "Campania" }
        );

        // Seed Servizi
        modelBuilder.Entity<Servizio>().HasData(
            new Servizio { Id = 1, Nome = "Wi-Fi" },
            new Servizio { Id = 2, Nome = "Piscina" },
            new Servizio { Id = 3, Nome = "Aria Condizionata" }
        );

        // Seed Utenti
        modelBuilder.Entity<Utente>().HasData(
            new Utente { Id = 1, Nome = "Admin", Email = "admin@bbooking.com", Password = "Admin123", Ruolo = RuoloUtente.Admin },
            new Utente { Id = 2, Nome = "Host", Email = "host@bbooking.com", Password = "Host123", Ruolo = RuoloUtente.Host },
            new Utente { Id = 3, Nome = "Guest", Email = "guest@bbooking.com", Password = "Guest123", Ruolo = RuoloUtente.Guest }
        );
    }
}