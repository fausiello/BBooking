# 📦 BBooking API

> Un backend RESTful per la gestione di affitti brevi e prenotazioni di case vacanze, progettato in .NET seguendo i principi della Clean Architecture.

## Panoramica del Progetto
Il sistema permette a utenti con ruoli differenti di interagire con la piattaforma in modo sicuro:
* **Host**: Possono pubblicare, modificare e gestire il proprio catalogo di case vacanze.
* **Guest**: Possono esplorare le strutture, verificare la disponibilità ed effettuare prenotazioni. La business logic previene algoritmicamente le sovrapposizioni di date (overbooking) per la stessa struttura.

## Architettura e Tecnologie
La soluzione è suddivisa in progetti separati per garantire modularità e manutenibilità:
* **BBooking.Models (Domain)**: Contiene le entità base (Utente, CasaVacanze, Prenotazione, Servizio, Localita) e gli Enum. Non ha dipendenze esterne.
* **BBooking.Data**: Gestisce la persistenza tramite **Entity Framework Core** (Code-First) su SQL Server. Configura le relazioni (incluse le N:N tra Case e Servizi) tramite Fluent API.
* **BBooking.Api**: Esponde gli endpoint tramite **Minimal API**. La sicurezza è garantita da **JWT**, che isola gli accessi in base al ruolo (RBAC) e assicura che un Guest veda solo le proprie prenotazioni.
* **BBooking.Grpc**: Un microservizio interno isolato che comunica con l'API principale per simulare in modo asincrono le transazioni di pagamento.

## Setup Locale
Per eseguire il progetto in locale, assicurati di avere l'.NET SDK installato e un'istanza locale di SQL Server.

1. **Configura il Database:** Inserisci la tua stringa di connessione SQL Server nel file `appsettings.json` del progetto `BBooking.Api`.
2. **Applica le Migrazioni:** Crea lo schema e popola il database con i dati iniziali:
   ```bash
   dotnet ef database update --project src/BBooking.Data --startup-project src/BBooking.Api
