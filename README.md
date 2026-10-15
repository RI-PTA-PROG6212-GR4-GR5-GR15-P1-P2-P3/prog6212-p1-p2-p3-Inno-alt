# RaceDay Event Management System

## System Description

RaceDay is a full-stack web-based event management system designed for the South African road running, walking, and cycling community. The platform allows Event Organisers to create and manage events, while Participants can browse events, register, and track their personal performance.

## Roles

### Participant
- Browse and search for upcoming events
- Register for events
- View personal entry history
- Track performance results

### Organiser
- Create and manage events
- View participant entries
- Submit and update race results

---

## Database Design

The database consists of 6 entities:

| Entity | Description |
|--------|-------------|
| User | Authentication and login details |
| Participant | Personal details for race participants |
| Organiser | Event organisers and their organisations |
| Event | Race/event details |
| Entry | Participant registrations for events |
| Result | Race results for participants |

---

## Deliverables

### Part 1 - System Planning and Database

| Section | Description | File |
|---------|-------------|------|
| A | Entity Relationship Diagram (ERD) | docs/PROG6212 PART 1 2026.pdf |
| B | API Endpoint Plan | docs/PROG6212 PART 1 2026.pdf |
| C | SQL Database Script | docs/raceday-schema.sql |

### Part 2 - RESTful API Development

| Component | Description | Location |
|-----------|-------------|----------|
| API Project | ASP.NET Core 9 Web API | Part2/RaceDayAPI/ |
| Test Project | xUnit unit tests | Part2/RaceDayAPI.Tests/ |
| CI/CD | GitHub Actions workflow | .github/workflows/ |

---

## Tech Stack

| Technology | Purpose |
|------------|---------|
| ASP.NET Core 9 | Web API framework |
| Entity Framework Core 9 | ORM for SQL Server (Code-First) |
| SQL Server (LocalDB) | Database |
| JWT (JSON Web Tokens) | Authentication |
| BCrypt.Net | Password hashing |
| Swagger / OpenAPI | API documentation and testing |
| xUnit + Moq + FluentAssertions | Unit testing |
| GitHub Actions | CI/CD |

---

## Project Structure

prog6212-p1-p2-p3-Inno-alt/
├── README.md
├── docs/
│   ├── PROG6212 PART 1 2026.pdf
│   └── raceday-schema.sql
└── Part2/
    ├── RaceDayAPI/                  Main API project
    │   ├── Controllers/             API endpoints (7 files)
    │   ├── Data/                    DbContext
    │   ├── DTOs/                    Data Transfer Objects
    │   ├── Migrations/              EF Core migrations
    │   ├── Models/                  Entity classes (7 files)
    │   ├── Services/                Business logic (AuthService)
    │   ├── Program.cs
    │   ├── appsettings.json
    │   └── RaceDayAPI.csproj
    │
    └── RaceDayAPI.Tests/            Unit test project
        ├── AuthServiceTests.cs
        ├── EventsControllerTests.cs
        ├── EntriesControllerTests.cs
        └── RaceDayAPI.Tests.csproj

---

## Setup and Run Instructions

### Prerequisites

- .NET 9 SDK
- SQL Server or SQL Server LocalDB
- Visual Studio 2022 (or VS Code with C# extension)
- SQL Server Management Studio (optional, for viewing database)

### Step 1: Clone the Repository

git clone https://github.com/RI-PTA-PROG6212-GR4-GR5-GR15-P1-P2-P3/prog6212-p1-p2-p3-Inno-alt.git
cd prog6212-p1-p2-p3-Inno-alt

### Step 2: Navigate to the API Project

cd Part2/RaceDayAPI

### Step 3: Restore NuGet Packages

dotnet restore

### Step 4: Create the Database

dotnet ef database update

This creates the RaceDayDB database with all tables.

### Step 5: Run the API

dotnet run

The API will start and display:

Now listening on: https://localhost:7080
Now listening on: http://localhost:5030

### Step 6: Open Swagger UI

Open a browser and navigate to:

https://localhost:7080/swagger

All endpoints will be visible and testable.

### Step 7: Run Unit Tests

cd ../RaceDayAPI.Tests
dotnet test

Expected output: 21 tests passing.

---

## Authentication Flow

1. Register a new user at POST /api/Auth/register
   - Provide: email, password, role (Participant or Organiser), and profile details
2. Login at POST /api/Auth/login
   - Provide: email and password
   - Receive: JWT token with user ID and role claims
3. Use the token in subsequent requests:
   - Header: Authorization: Bearer {token}
4. Role-based access:
   - Endpoints marked [Authorize(Roles = "Organiser")] require an Organiser token
   - Endpoints marked [Authorize(Roles = "Participant")] require a Participant token

Passwords are hashed using BCrypt before storage. Plain-text passwords are never saved.

---

## API Endpoints

| Resource | Count | Description |
|----------|-------|-------------|
| Auth | 2 | Register, Login |
| Participants | 4 | Profile (GET, PUT), entries, results |
| Organisers | 3 | Profile (GET, PUT), events list |
| Events | 6 | CRUD, upcoming, filter by city/province |
| Categories | 5 | CRUD by event |
| Entries | 6 | Enrol, cancel, payment, view |
| Results | 6 | Submit, view, leaderboard, history |

Total: approximately 32 endpoints, all testable through Swagger UI.

---

## Unit Testing

21 unit tests covering the following scenarios:

### Authentication
- Register Participant successfully
- Register Organiser successfully
- Reject duplicate email
- Reject invalid role
- Login with valid credentials
- Reject wrong password
- Reject non-existent email
- Verify password is hashed (BCrypt)

### Events Management
- Organiser can create event
- Reject event with past date
- Reject event with invalid deadline
- Get all active events
- Get event by ID
- Return 404 for non-existent event
- Organiser can update own event
- Organiser can soft-delete own event

### Enrolments
- Participant can enrol in event
- Reject enrolment after deadline
- Reject duplicate enrolment
- Reject enrolment when event is full
- Organiser can view event entries
- Participant can cancel own entry

All tests pass locally and via GitHub Actions CI/CD.

---

## CI/CD

Continuous Integration is configured via GitHub Actions.

The workflow file is located at:

.github/workflows/part2-ci.yml

What it does on every push to main:

1. Checks out the repository
2. Sets up .NET 9
3. Restores NuGet packages
4. Builds the API project
5. Reports success or failure

Status: Passing

---

## YouTube Walkthrough

Watch the complete Part 2 walkthrough here:

[Part 2 Walkthrough Video](https://youtu.be/rj8jsEPCZyw)

The video covers:
- Running the API in Visual Studio
- Testing endpoints in Swagger UI
- Authentication flow (register, login, JWT)
- Database verification in SSMS
- Unit tests passing (21 tests)
- CI/CD pipeline explanation

---

## Author

Name: Innocentia Tlotleng
Course: PROG6212
Assignment: Part 1 and Part 2 - RaceDay Event Management System

## Submission Date

October 2026

---

Copyright The Independent Institute of Education (Pty) Ltd 2026
