# BBooking - Ecosistema Backend per Affitti Brevi

Benvenuto nel repository ufficiale di BBooking, un ecosistema backend enterprise-grade sviluppato in .NET 8/9 progettato per la gestione di strutture ricettive e affitti brevi. Il progetto adotta rigorosamente i principi della Clean Architecture (Onion Architecture) e integra comunicazioni sincrone ad alte prestazioni tramite gRPC.

---

## 🏗️ Architettura della Soluzione

La soluzione è strutturata in 5 progetti separati per garantire il massimo disaccoppiamento, manutenibilità e testabilità isolata dei componenti:

- BBooking.Models: Strato Core (Domain Entities, Enums) - Zero dipendenze esterne
- BBooking.Data: Strato Infrastructure (DbContext, Fluent API, Migrazioni EF Core)
- BBooking.Api: Strato Presentation (REST Web API, Controllers, JWT Auth, Middleware)
- BBooking.Grpc: Strato Microservizi (Server gRPC per l'elaborazione dei pagamenti)
- BBooking.Tests: Unit Testing (xUnit, EF Core InMemory, Testing logico)

### 🗃️ Modello dei Dati & Relazioni
- Utente VS CasaVacanze (1:N): Un Host può possedere molteplici strutture. Mappato tramite chiave esterna HostId.
- CasaVacanze VS Servizio (N:N Pura): Associazione dinamica dei servizi (Wi-Fi, Piscina, AC) alle case senza entità di join esplicita nel codice.
- Utente VS Prenotazione (1:N): Un Guest effettua le prenotazioni. Configurato esplicitamente con DeleteBehavior.Restrict per disattivare la cancellazione a cascata multipla in SQL Server.
- Località vs CasaVacanze (1:N): Una località contiene più strutture.

---

## 🔐 Sicurezza & Autorizzazione (RBAC)

L'autenticazione è completamente stateless ed è implementata tramite Token JWT (JSON Web Tokens). I permessi sono distribuiti su tre ruoli distinti (RuoloUtente):

1. Admin: Accesso totale al sistema. Può creare capillarmente nuove Località e censire nuovi Servizi.
2. Host: Può creare, modificare ed eliminare le proprie CaseVacanze. È presente un controllo integrato anti-escalation di privilegi: un Host non può manipolare gli annunci di un altro Host.
3. Guest: Può esplorare le strutture disponibili, creare prenotazioni e visualizzare lo storico dei propri ordini (/api/v1/prenotazioni/mie).

---

## ⚙️ Logiche di Business Avanzate

### 1. Algoritmo di Prevenzione dell'Overbooking
Prima di confermare una prenotazione, il sistema esegue una query LINQ asincrona e ottimizzata per rilevare conflitti temporali. La formula matematica applicata per identificare la sovrapposizione delle date è:
DataInizioRichiesta < p.DataFine E DataFineRichiesta > p.DataInizio

Nota alberghiera: Se il check-in di una prenotazione coincide esattamente con il giorno di check-out della precedente, il sistema considera la camera disponibile, accettando la transazione.

### 2. Integrazione Microservizi gRPC (Pagamenti)
Il flusso della prenotazione è transazionale ed è integrato nativamente con il progetto BBooking.Grpc tramite protocollo binario forte basato su file pagamento.proto:
- La Web API funge da gRPC Client.
- Il server gRPC convalida e autorizza l'importo finanziario.
- Lo stato della prenotazione sul database viene promosso a "Confermata" o "Rifiutata" in tempo reale in base alla risposta del microservizio.

---

## 🚀 Configurazione e Avvio del Progetto

### Prerequisiti
- .NET 8.0 SDK o successivo
- SQL Server LocalDB o istanza Docker attiva
- Strumenti CLI di EF Core installati (dotnet ef)

### 1. Ripristino dei pacchetti e compilazione
Dalla root della soluzione, esegui:
dotnet restore
dotnet build

### 2. Configurazione dei pacchetti di Design
Il pacchetto di design è necessario nel progetto di avvio per permettere la generazione dei metadati a design-time:
dotnet add src/BBooking.Api package Microsoft.EntityFrameworkCore.Design

### 3. Migrazioni e Generazione del Database
Applica i file di migrazione contenuti nel modulo infrastrutturale sfruttando i flag per la configurazione multi-progetto:

Genera lo schema di migrazione iniziale:
dotnet ef migrations add InitialCreate --project src/BBooking.Data --startup-project src/BBooking.Api

Allinea SQL Server applicando le tabelle e il Seed Data iniziale:
dotnet ef database update --project src/BBooking.Data --startup-project src/BBooking.Api

### 4. Esecuzione dei Progetti
Per testare il flusso completo E2E, avvia contemporaneamente il server gRPC e l'applicazione Web API:

In un terminale dedicato al Server gRPC:
dotnet run --project src/BBooking.Grpc

In un secondo terminale dedicato alle Web API REST:
dotnet run --project src/BBooking.Api

Una volta avviato il progetto API, naviga sull'URL di localhost per esplorare la documentazione interattiva OpenAPI di Swagger.

---

## 🧪 Validazione & Unit Testing

Il progetto adotta un approccio orientato all'affidabilità logica. La suite contiene unit test implementati con xUnit focalizzati sull'algoritmo di overbooking su database isolato in memoria (Microsoft.EntityFrameworkCore.InMemory).

Per eseguire la suite di test ed analizzare i risultati:
dotnet test

I test coprono gli scenari di:
- Prenotazione in date completamente libere (Successo)
- Sovrapposizione parziale a inizio periodo (Blocco)
- Sovrapposizione parziale a fine periodo (Blocco)
- Inclusione totale o "avvolgimento" di date esistenti (Blocco)
- Coincidenza esatta tra Check-In e Check-Out (Successo)

---
Progetto sviluppato come Capstone per la validazione delle competenze architetturali enterprise in ambiente C# e .NET.