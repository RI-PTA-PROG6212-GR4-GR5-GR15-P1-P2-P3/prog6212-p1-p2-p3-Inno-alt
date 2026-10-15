# RaceDay Event Management System

## About This Project

This is a full-stack event management system called RaceDay, built for the South African road running, walking, and cycling community. The project was completed across two parts for PROG6212.

Part 1 covered system planning, database design, and SQL scripting.

Part 2 covers the RESTful API built with ASP.NET Core.

## Roles

The system has two user roles:

**Participant** - Can browse events, register for events, view their entry history, and track their performance results.

**Organiser** - Can create and manage events, view participant entries, and submit race results.

## Database Design

The database has 6 entities:

| Entity | Purpose |
|--------|---------|
| User | Authentication and login details |
| Participant | Personal details for race participants |
| Organiser | Event organisers and their organisations |
| Event | Race and event details |
| Entry | Participant registrations for events |
| Result | Race results for participants |

## What Was Built in Part 2

A complete RESTful API using ASP.NET Core 9, Entity Framework Core, and SQL Server.

### API Endpoints

| Resource | Count | What It Does |
|----------|-------|--------------|
| Auth | 2 | Register and Login with JWT |
| Participants | 4 | Profile management and personal data |
| Organisers | 3 | Profile and event listings |
| Events | 6 | Create, read, update, delete, filter, upcoming |
| Categories | 5 | Event categories by age and distance |
| Entries | 6 | Enrolments, cancellations, payments |
| Results | 6 | Submit results, view leaderboards |

Total: approximately 32 endpoints.

### Authentication

Users register and log in with email and password. Passwords are hashed using BCrypt before storage.

On login, the API returns a JWT token containing the user's ID and role. This token is required for protected endpoints.

Role-based access is enforced so that Organisers and Participants can only access their own functionality. For example, only Organisers can create or delete events, and only Participants can enrol in events.

### Unit Testing

21 unit tests were written using xUnit, Moq, and FluentAssertions. The tests cover:

- User registration for both roles
- Login with valid and invalid credentials
- Password hashing verification
- Event creation by Organisers
- Rejection of events with invalid dates
- Role-based access enforcement
- Event enrolment by Participants
- Prevention of duplicate enrolments
- Event capacity limits
- Entry cancellation

All 21 tests pass.

### CI/CD

GitHub Actions runs on every push to main. The workflow restores packages, builds the API project, and reports the build status.

## Technologies Used

- ASP.NET Core 9
- Entity Framework Core 9 (Code-First approach)
- SQL Server (LocalDB)
- JWT for authentication
- BCrypt.Net for password hashing
- Swagger for API documentation
- xUnit, Moq, and FluentAssertions for testing
- GitHub Actions for CI/CD

## Project Structure

Part2/
└── RaceDayAPI/
    ├── Controllers/
    ├── Data/
    ├── DTOs/
    ├── Migrations/
    ├── Models/
    ├── Services/
    ├── Program.cs
    ├── appsettings.json
    └── RaceDayAPI.csproj

## Screenshots

The API running in Swagger:

![Swagger](<img width="1038" height="929" alt="Screenshot 2026-10-15 113008" src="https://github.com/user-attachments/assets/c38605cc-1b6b-4db6-8a7f-407d27302459" />
screenshot-success.png)

## Video Walkthrough

A full walkthrough of the API is available here:

[Part 2 Walkthrough Video](https://youtu.be/rj8jsEPCZyw)

## Author

Innocentia Tlotleng
PROG6212
October 2026

---

