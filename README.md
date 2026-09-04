# 🏃 RaceDay Event Management System - Part 1

## 📖 System Description
RaceDay is a full-stack web-based event management system designed specifically for the South African road running, walking, and cycling community. The platform allows Event Organisers to create and manage events, categories, and participant results, while Participants can browse upcoming events, enter events, track their personal performance history, and prepare for race day using live weather and route information.

## 👥 Roles

### 🏅 Participant
- Browse and search for upcoming events
- Register for events and categories
- View personal entry history
- Track performance results
- Update personal profile

### 📋 Organiser
- Create and manage events
- Add categories to events
- View participant entries
- Submit and update race results
- Manage event details

## 📊 Database Design (Part 1)

The database consists of **6 entities**:

| Entity | Description |
|--------|-------------|
| **User** | Authentication and login details |
| **Participant** | Personal details for race participants |
| **Organiser** | Event organisers and their organisations |
| **Event** | Race/event details |
| **Entry** | Participant registrations for events |
| **Result** | Race results for participants |

## ✅ Part 1 Deliverables

| Section | Description | File |
|---------|-------------|------|
| **A** | Entity Relationship Diagram (ERD) | `docs/PROG6212 PART 1 2026.pdf` |
| **B** | API Endpoint Plan | `docs/PROG6212 PART 1 2026.pdf` |
| **C** | SQL Database Script | `docs/raceday-schema.sql` |
| **CI/CD** | GitHub Actions Workflow | `.github/workflows/ci.yml` |

## 🔧 CI/CD Status

✅ **Build Passing**

![Successful Build](screenshot-success.png)

## 🎥 YouTube Walkthrough

[Watch the complete walkthrough video here](YOUR_YOUTUBE_LINK_HERE)

The video covers:
- ERD design decisions and relationships
- API endpoint planning and choices
- SQL script demonstration (live run in SSMS)
- Database verification queries

## 👨‍💻 Author

**Name:** Innocentia Tlotleng  
**Course:** PROG6212  
**Assignment:** Part 1 - System Planning and Database

## 📅 Submission Date

September 2026

---

© The Independent Institute of Education (Pty) Ltd 2026
