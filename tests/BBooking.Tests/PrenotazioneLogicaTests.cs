using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using BBooking.Data;
using BBooking.Models.Entities;

namespace BBooking.Tests;

public class PrenotazioneLogicaTests
{
    // Metodo helper per generare un DbContext in memoria isolato e popolato
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // DB Unico per ogni test
            .Options;

        var db = new AppDbContext(options);

        // Seed: Inseriamo una prenotazione confermata fissa dal 10 al 20 Giugno 2024
        db.Prenotazioni.Add(new Prenotazione
        {
            Id = 1,
            CasaVacanzeId = 1,
            GuestId = 1, // ID Utente fittizio
            DataInizio = new DateTime(2024, 6, 10),
            DataFine = new DateTime(2024, 6, 20),
            Stato = StatoPrenotazione.Confermata,
            Totale = 500 // Costo fittizio
        });
        
        db.SaveChanges();
        return db;
    }

    // Metodo helper che simula la logica del tuo Controller o Service
    private async Task<bool> IsOverbookingAsync(AppDbContext db, int casaVacanzeId, DateTime dataInizio, DateTime dataFine)
    {
        return await db.Prenotazioni.AnyAsync(p =>
            p.CasaVacanzeId == casaVacanzeId &&
            p.Stato == StatoPrenotazione.Confermata &&
            dataInizio < p.DataFine &&
            dataFine > p.DataInizio);
    }

    [Fact]
    public async Task Test_Prenotazione_In_Date_Completamente_Libere_Deve_Avere_Successo()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var inizio = new DateTime(2024, 6, 1);
        var fine = new DateTime(2024, 6, 5);

        // Act
        var hasOverbooking = await IsOverbookingAsync(db, 1, inizio, fine);

        // Assert
        Assert.False(hasOverbooking, "L'algoritmo non doveva trovare sovrapposizioni.");
    }

    [Fact]
    public async Task Test_Nuova_Prenotazione_Che_Inizia_Durante_Una_Esistente_Deve_Fallire()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var inizio = new DateTime(2024, 6, 15);
        var fine = new DateTime(2024, 6, 25);

        // Act
        var hasOverbooking = await IsOverbookingAsync(db, 1, inizio, fine);

        // Assert
        Assert.True(hasOverbooking, "L'algoritmo doveva bloccare la prenotazione (sovrapposizione data di inizio).");
    }

    [Fact]
    public async Task Test_Nuova_Prenotazione_Che_Finisce_Durante_Una_Esistente_Deve_Fallire()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var inizio = new DateTime(2024, 6, 5);
        var fine = new DateTime(2024, 6, 15);

        // Act
        var hasOverbooking = await IsOverbookingAsync(db, 1, inizio, fine);

        // Assert
        Assert.True(hasOverbooking, "L'algoritmo doveva bloccare la prenotazione (sovrapposizione data di fine).");
    }

    [Fact]
    public async Task Test_Nuova_Prenotazione_Che_Include_Completamente_Una_Esistente_Deve_Fallire()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var inizio = new DateTime(2024, 6, 5);
        var fine = new DateTime(2024, 6, 25);

        // Act
        var hasOverbooking = await IsOverbookingAsync(db, 1, inizio, fine);

        // Assert
        Assert.True(hasOverbooking, "L'algoritmo doveva bloccare la prenotazione (avvolge quella esistente).");
    }

    [Fact]
    public async Task Test_Prenotazione_Che_Inizia_Lo_Stesso_Giorno_In_Cui_Finisce_Un_Altra_Deve_Avere_Successo()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var inizio = new DateTime(2024, 6, 20); // Il check-in è il giorno del check-out della precedente
        var fine = new DateTime(2024, 6, 25);

        // Act
        var hasOverbooking = await IsOverbookingAsync(db, 1, inizio, fine);

        // Assert
        Assert.False(hasOverbooking, "L'algoritmo doveva permettere la prenotazione contigua.");
    }
}