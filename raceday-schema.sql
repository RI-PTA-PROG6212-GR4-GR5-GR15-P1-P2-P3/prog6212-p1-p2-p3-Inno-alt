-- ============================================================
-- DATABASE: RaceDay Event Management System
-- Author: Faith Sadiki
-- Date: 2026-09-04
-- Description: Full database schema for RaceDay system
-- ============================================================

-- ============================================================
-- DROP DATABASE IF EXISTS (Clean start)
-- ============================================================

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'RaceDayDB')
BEGIN
    ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDayDB;
END

GO

-- ============================================================
-- CREATE DATABASE
-- ============================================================

CREATE DATABASE RaceDayDB;

GO

USE RaceDayDB;

GO

-- ============================================================
-- TABLE 1: USER (Authentication)
-- ============================================================

CREATE TABLE [User] (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1,
    LastLogin DATETIME NULL
);

-- ============================================================
-- TABLE 2: PARTICIPANT (Extends User)
-- ============================================================

CREATE TABLE Participant (
    ParticipantID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT UNIQUE NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    IDNumber NVARCHAR(13) UNIQUE NOT NULL,
    PhoneNumber NVARCHAR(15) NULL,
    DateOfBirth DATE NULL,
    Gender NVARCHAR(10) NULL CHECK (Gender IN ('Male', 'Female', 'Other')),
    EmergencyContactName NVARCHAR(100) NULL,
    EmergencyContactNumber NVARCHAR(15) NULL,
    MedicalConditions NVARCHAR(500) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Participant_User FOREIGN KEY (UserID) 
        REFERENCES [User](UserID) ON DELETE CASCADE
);

-- ============================================================
-- TABLE 3: ORGANISER (Extends User)
-- ============================================================

CREATE TABLE Organiser (
    OrganiserID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT UNIQUE NOT NULL,
    OrganisationName NVARCHAR(100) NOT NULL,
    RegistrationNumber NVARCHAR(50) UNIQUE NULL,
    ContactPerson NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(15) NULL,
    Email NVARCHAR(100) NOT NULL,
    Website NVARCHAR(200) NULL,
    IsVerified BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Organiser_User FOREIGN KEY (UserID) 
        REFERENCES [User](UserID) ON DELETE CASCADE
);

-- ============================================================
-- TABLE 4: EVENT
-- ============================================================

CREATE TABLE Event (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    EventName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    EventDate DATETIME NOT NULL,
    RegistrationDeadline DATETIME NOT NULL,
    Venue NVARCHAR(200) NOT NULL,
    City NVARCHAR(50) NOT NULL,
    Province NVARCHAR(30) NOT NULL,
    MaxParticipants INT NULL,
    CurrentParticipants INT DEFAULT 0,
    EntryFee DECIMAL(10,2) NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Event_Organiser FOREIGN KEY (OrganiserID) 
        REFERENCES Organiser(OrganiserID) ON DELETE CASCADE,
    CONSTRAINT CHK_Event_Date CHECK (EventDate > RegistrationDeadline),
    CONSTRAINT CHK_Event_CurrentParticipants CHECK (CurrentParticipants >= 0)
);

-- ============================================================
-- TABLE 5: ENTRY (Participant Registration)
-- FIXED: Removed UNIQUE constraint from BibNumber
-- ============================================================

CREATE TABLE Entry (
    EntryID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    EntryDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT 'Pending' 
        CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled', 'Waitlisted')),
    BibNumber INT NULL,
    PaymentStatus NVARCHAR(20) DEFAULT 'Pending' 
        CHECK (PaymentStatus IN ('Pending', 'Paid', 'Refunded', 'Failed')),
    PaymentDate DATETIME NULL,
    PaymentReference NVARCHAR(50) NULL,
    EntryFeePaid DECIMAL(10,2) NULL,
    TShirtSize NVARCHAR(10) NULL 
        CHECK (TShirtSize IN ('XS', 'S', 'M', 'L', 'XL', 'XXL')),
    DietaryRequirements NVARCHAR(200) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Entry_Participant FOREIGN KEY (ParticipantID) 
        REFERENCES Participant(ParticipantID) ON DELETE CASCADE,
    CONSTRAINT FK_Entry_Event FOREIGN KEY (EventID) 
        REFERENCES Event(EventID) ON DELETE NO ACTION,
    CONSTRAINT UQ_Entry_EventParticipant UNIQUE (EventID, ParticipantID)
);

-- ============================================================
-- TABLE 6: RESULT
-- ============================================================

CREATE TABLE Result (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EntryID INT UNIQUE NOT NULL,
    FinishTime TIME NULL,
    Position INT NULL,
    GunTime TIME NULL,
    ChipTime TIME NULL,
    Pace DECIMAL(5,2) NULL,
    Status NVARCHAR(20) NULL 
        CHECK (Status IN ('DNS', 'DNF', 'Finished', 'Disqualified')),
    OverallPosition INT NULL,
    AgeGroupPosition INT NULL,
    GenderPosition INT NULL,
    CertificateURL NVARCHAR(500) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Result_Entry FOREIGN KEY (EntryID) 
        REFERENCES Entry(EntryID) ON DELETE CASCADE
);

-- ============================================================
-- CREATE INDEXES FOR PERFORMANCE
-- ============================================================

-- User indexes
CREATE INDEX IX_User_Email ON [User](Email);
CREATE INDEX IX_User_IsActive ON [User](IsActive);

-- Participant indexes
CREATE INDEX IX_Participant_IDNumber ON Participant(IDNumber);
CREATE INDEX IX_Participant_UserID ON Participant(UserID);
CREATE INDEX IX_Participant_FullName ON Participant(FullName);

-- Organiser indexes
CREATE INDEX IX_Organiser_UserID ON Organiser(UserID);
CREATE INDEX IX_Organiser_Email ON Organiser(Email);
CREATE INDEX IX_Organiser_OrganisationName ON Organiser(OrganisationName);

-- Event indexes
CREATE INDEX IX_Event_OrganiserID ON Event(OrganiserID);
CREATE INDEX IX_Event_EventDate ON Event(EventDate);
CREATE INDEX IX_Event_City ON Event(City);
CREATE INDEX IX_Event_Province ON Event(Province);
CREATE INDEX IX_Event_IsActive ON Event(IsActive);

-- Entry indexes
CREATE INDEX IX_Entry_ParticipantID ON Entry(ParticipantID);
CREATE INDEX IX_Entry_EventID ON Entry(EventID);
CREATE INDEX IX_Entry_Status ON Entry(Status);
CREATE INDEX IX_Entry_BibNumber ON Entry(BibNumber);
CREATE INDEX IX_Entry_PaymentStatus ON Entry(PaymentStatus);

-- Result indexes
CREATE INDEX IX_Result_EntryID ON Result(EntryID);
CREATE INDEX IX_Result_Position ON Result(Position);
CREATE INDEX IX_Result_Status ON Result(Status);
CREATE INDEX IX_Result_OverallPosition ON Result(OverallPosition);

-- ============================================================
-- INSERT SEED DATA
-- ============================================================

-- ============================================================
-- 1. INSERT USERS (CUSTOMIZED WITH YOUR NAMES)
-- ============================================================

INSERT INTO [User] (Email, PasswordHash, CreatedAt, IsActive) VALUES
('faith.sadiki@gmail.com', 'hashed_password_123', GETDATE(), 1),
('kgothatso.mosipa@gmail.com', 'hashed_password_456', GETDATE(), 1),
('tsholofelo.molomo@gmail.com', 'hashed_password_789', GETDATE(), 1),
('odirile.rakoma@gmail.com', 'hashed_password_101', GETDATE(), 1),
('athletics.sa@gmail.com', 'hashed_password_112', GETDATE(), 1),
('capetown.cycletour@gmail.com', 'hashed_password_131', GETDATE(), 1);

-- ============================================================
-- 2. INSERT PARTICIPANTS (CUSTOMIZED WITH YOUR NAMES)
-- ============================================================

INSERT INTO Participant (
    UserID, FullName, IDNumber, PhoneNumber, DateOfBirth, 
    Gender, EmergencyContactName, EmergencyContactNumber, 
    MedicalConditions, CreatedAt
) VALUES
(1, 'Faith Sadiki', '9001011234567', '0821234567', '1990-01-01', 
 'Female', 'Thabo Sadiki', '0829876543', 'None', GETDATE()),

(2, 'Kgothatso Mosipa', '8505159876543', '0837654321', '1985-05-15', 
 'Female', 'Sello Mosipa', '0837654322', 'Asthma', GETDATE()),

(3, 'Tsholofelo Molomo', '9503204567891', '0723456789', '1995-03-20', 
 'Male', 'Lerato Molomo', '0723456790', 'None', GETDATE()),

(4, 'Odirile Rakoma', '8808127891234', '0712345678', '1988-08-12', 
 'Male', 'Mpho Rakoma', '0712345679', 'Diabetes', GETDATE());

-- ============================================================
-- 3. INSERT ORGANISERS (CUSTOMIZED)
-- ============================================================

INSERT INTO Organiser (
    UserID, OrganisationName, RegistrationNumber, ContactPerson, 
    PhoneNumber, Email, Website, IsVerified, CreatedAt
) VALUES
(5, 'Athletics South Africa', 'ASA2026001', 'Mike Johnson', 
 '0111234567', 'info@athleticssa.co.za', 'www.athleticssa.co.za', 1, GETDATE()),

(6, 'Cape Town Cycle Tour Association', 'CTCT2026002', 'Sarah Williams', 
 '0217654321', 'info@capetowncycletour.com', 'www.capetowncycletour.com', 1, GETDATE());

-- ============================================================
-- 4. INSERT EVENTS
-- ============================================================

INSERT INTO Event (
    OrganiserID, EventName, Description, EventDate, RegistrationDeadline, 
    Venue, City, Province, MaxParticipants, CurrentParticipants, 
    EntryFee, IsActive, CreatedAt
) VALUES
(1, 'Comrades Marathon', 'Ultimate human race from Pietermaritzburg to Durban', 
 '2026-06-15 05:30:00', '2026-05-30 23:59:59', 
 'Pietermaritzburg City Hall', 'Pietermaritzburg', 'KwaZulu-Natal', 
 5000, 1500, 450.00, 1, GETDATE()),

(1, 'Soweto Marathon', 'Iconic Soweto race through the streets of Soweto', 
 '2026-11-02 06:00:00', '2026-10-15 23:59:59', 
 'FNB Stadium', 'Johannesburg', 'Gauteng', 
 30000, 8500, 350.00, 1, GETDATE()),

(2, 'Cape Town Cycle Tour', 'World''s largest timed cycling event', 
 '2026-03-08 07:00:00', '2026-02-20 23:59:59', 
 'Grand Parade', 'Cape Town', 'Western Cape', 
 35000, 12000, 500.00, 1, GETDATE()),

(1, 'Two Oceans Marathon', 'Ultra marathon around Cape Peninsula', 
 '2026-04-04 06:30:00', '2026-03-15 23:59:59', 
 'Main Road', 'Cape Town', 'Western Cape', 
 12000, 3200, 420.00, 1, GETDATE()),

(2, 'Durban 10km City Run', 'Coastal city run through Durban', 
 '2026-07-20 07:00:00', '2026-07-05 23:59:59', 
 'North Beach', 'Durban', 'KwaZulu-Natal', 
 5000, 800, 150.00, 1, GETDATE());

-- ============================================================
-- 5. INSERT ENTRIES (CUSTOMIZED WITH YOUR NAMES)
-- FIXED: Removed duplicate BibNumber conflicts
-- ============================================================

INSERT INTO Entry (
    ParticipantID, EventID, EntryDate, Status, BibNumber, 
    PaymentStatus, PaymentDate, PaymentReference, EntryFeePaid, 
    TShirtSize, DietaryRequirements, CreatedAt
) VALUES
-- Faith Sadiki entries (ParticipantID = 1)
(1, 1, GETDATE(), 'Confirmed', 1234, 
 'Paid', GETDATE(), 'PAY20260001', 450.00, 
 'M', 'Vegetarian', GETDATE()),

(1, 2, GETDATE(), 'Confirmed', 2345, 
 'Paid', GETDATE(), 'PAY20260002', 350.00, 
 'L', 'None', GETDATE()),

-- Kgothatso Mosipa entries (ParticipantID = 2)
(2, 1, GETDATE(), 'Confirmed', 3456, 
 'Paid', GETDATE(), 'PAY20260003', 450.00, 
 'S', 'Gluten-free', GETDATE()),

(2, 3, GETDATE(), 'Pending', 4567, 
 'Pending', NULL, NULL, 500.00, 
 'M', 'None', GETDATE()),

-- Tsholofelo Molomo entries (ParticipantID = 3)
(3, 2, GETDATE(), 'Confirmed', 5678, 
 'Paid', GETDATE(), 'PAY20260004', 350.00, 
 'XL', 'None', GETDATE()),

(3, 4, GETDATE(), 'Pending', 6789, 
 'Pending', NULL, NULL, 420.00, 
 'L', 'Vegetarian', GETDATE()),

-- Odirile Rakoma entries (ParticipantID = 4)
(4, 3, GETDATE(), 'Confirmed', 7890, 
 'Paid', GETDATE(), 'PAY20260005', 500.00, 
 'M', 'None', GETDATE()),

(4, 5, GETDATE(), 'Confirmed', 8901, 
 'Paid', GETDATE(), 'PAY20260006', 150.00, 
 'S', 'None', GETDATE()),

-- Extra entries for testing
(1, 3, GETDATE(), 'Confirmed', 9012, 
 'Paid', GETDATE(), 'PAY20260007', 500.00, 
 'M', 'None', GETDATE()),

(2, 4, GETDATE(), 'Confirmed', 12345, 
 'Paid', GETDATE(), 'PAY20260008', 420.00, 
 'M', 'None', GETDATE());

-- ============================================================
-- 6. INSERT RESULTS (CUSTOMIZED WITH YOUR NAMES)
-- ============================================================

INSERT INTO Result (
    EntryID, FinishTime, Position, GunTime, ChipTime, 
    Pace, Status, OverallPosition, AgeGroupPosition, 
    GenderPosition, CertificateURL, CreatedAt
) VALUES
-- Comrades Marathon results (EntryID 1, 3)
(1, '05:30:45', 150, '05:32:00', '05:30:45', 
 4.8, 'Finished', 150, 25, 15, 
 'https://raceday.com/certificates/faith_1234.pdf', GETDATE()),

(3, '04:45:30', 35, '04:46:00', '04:45:30', 
 4.2, 'Finished', 35, 5, 8, 
 'https://raceday.com/certificates/kgothatso_3456.pdf', GETDATE()),

-- Soweto Marathon results (EntryID 2, 5)
(2, '02:30:15', 200, '02:32:00', '02:30:15', 
 3.8, 'Finished', 200, 45, 180, 
 'https://raceday.com/certificates/faith_2345.pdf', GETDATE()),

(5, '02:25:30', 145, '02:26:00', '02:25:30', 
 3.7, 'Finished', 145, 30, 130, 
 'https://raceday.com/certificates/tsholofelo_5678.pdf', GETDATE()),

-- Cape Town Cycle Tour results (EntryID 7)
(7, '03:15:20', 220, '03:16:00', '03:15:20', 
 4.0, 'Finished', 220, 55, 42, 
 'https://raceday.com/certificates/odirile_7890.pdf', GETDATE()),

-- Durban 10km results (EntryID 8)
(8, '00:42:15', 12, '00:42:30', '00:42:15', 
 4.2, 'Finished', 12, 3, 5, 
 'https://raceday.com/certificates/odirile_8901.pdf', GETDATE()),

-- Additional results (EntryID 9, 10)
(9, '03:20:10', 280, '03:21:00', '03:20:10', 
 4.1, 'Finished', 280, 60, 250, 
 'https://raceday.com/certificates/faith_9012.pdf', GETDATE()),

(10, '05:45:20', 320, '05:46:00', '05:45:20', 
 5.0, 'Finished', 320, 70, 45, 
 'https://raceday.com/certificates/kgothatso_12345.pdf', GETDATE());

GO

-- ============================================================
-- CREATE VIEWS FOR COMMON QUERIES
-- ============================================================

CREATE VIEW vw_EventDetails AS
SELECT 
    e.EventID,
    e.EventName,
    e.Description,
    e.EventDate,
    e.RegistrationDeadline,
    e.Venue,
    e.City,
    e.Province,
    e.MaxParticipants,
    e.CurrentParticipants,
    e.EntryFee,
    e.IsActive,
    o.OrganisationName AS OrganiserName,
    o.ContactPerson AS OrganiserContact,
    o.Email AS OrganiserEmail
FROM Event e
INNER JOIN Organiser o ON e.OrganiserID = o.OrganiserID;

GO

CREATE VIEW vw_EntryDetails AS
SELECT 
    en.EntryID,
    en.EntryDate,
    en.Status AS EntryStatus,
    en.BibNumber,
    en.PaymentStatus,
    en.EntryFeePaid,
    en.TShirtSize,
    en.DietaryRequirements,
    p.FullName AS ParticipantName,
    p.IDNumber,
    p.PhoneNumber,
    e.EventName,
    e.EventDate,
    e.EventID,
    o.OrganisationName AS OrganiserName
FROM Entry en
INNER JOIN Participant p ON en.ParticipantID = p.ParticipantID
INNER JOIN Event e ON en.EventID = e.EventID
INNER JOIN Organiser o ON e.OrganiserID = o.OrganiserID;

GO

CREATE VIEW vw_ResultDetails AS
SELECT 
    r.ResultID,
    r.FinishTime,
    r.Position,
    r.Pace,
    r.Status AS ResultStatus,
    r.OverallPosition,
    r.AgeGroupPosition,
    r.GenderPosition,
    r.CertificateURL,
    en.EntryID,
    en.BibNumber,
    p.FullName AS ParticipantName,
    e.EventName,
    e.EventDate,
    o.OrganisationName AS OrganiserName
FROM Result r
INNER JOIN Entry en ON r.EntryID = en.EntryID
INNER JOIN Participant p ON en.ParticipantID = p.ParticipantID
INNER JOIN Event e ON en.EventID = e.EventID
INNER JOIN Organiser o ON e.OrganiserID = o.OrganiserID;

GO

-- ============================================================
-- CREATE STORED PROCEDURES
-- ============================================================

CREATE PROCEDURE sp_GetEventEntries
    @EventID INT,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        en.EntryID,
        en.EntryDate,
        en.Status,
        en.BibNumber,
        en.PaymentStatus,
        p.FullName,
        p.PhoneNumber,
        en.TShirtSize,
        en.DietaryRequirements
    FROM Entry en
    INNER JOIN Participant p ON en.ParticipantID = p.ParticipantID
    WHERE en.EventID = @EventID
    ORDER BY en.EntryID
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;

GO

CREATE PROCEDURE sp_GetParticipantResults
    @ParticipantID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.EventName,
        e.EventDate,
        e.City,
        e.Province,
        r.FinishTime,
        r.Position,
        r.Pace,
        r.Status AS ResultStatus,
        r.CertificateURL
    FROM Result r
    INNER JOIN Entry en ON r.EntryID = en.EntryID
    INNER JOIN Event e ON en.EventID = e.EventID
    WHERE en.ParticipantID = @ParticipantID
    ORDER BY e.EventDate DESC;
END;

GO

CREATE PROCEDURE sp_GetEventLeaderboard
    @EventID INT,
    @Limit INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Limit)
        r.Position,
        p.FullName AS ParticipantName,
        r.FinishTime,
        r.Pace,
        r.OverallPosition,
        en.BibNumber
    FROM Result r
    INNER JOIN Entry en ON r.EntryID = en.EntryID
    INNER JOIN Participant p ON en.ParticipantID = p.ParticipantID
    WHERE en.EventID = @EventID 
        AND r.Status = 'Finished'
    ORDER BY r.Position ASC;
END;

GO

-- ============================================================
-- VERIFY DATA - QUERY TO CHECK ALL TABLES
-- ============================================================

-- Count records in each table
SELECT 'User' AS TableName, COUNT(*) AS RecordCount FROM [User]
UNION ALL
SELECT 'Participant', COUNT(*) FROM Participant
UNION ALL
SELECT 'Organiser', COUNT(*) FROM Organiser
UNION ALL
SELECT 'Event', COUNT(*) FROM Event
UNION ALL
SELECT 'Entry', COUNT(*) FROM Entry
UNION ALL
SELECT 'Result', COUNT(*) FROM Result;

-- Show all events with participant counts
SELECT 
    e.EventName,
    e.EventDate,
    e.City,
    e.Province,
    e.CurrentParticipants,
    COUNT(en.EntryID) AS ActualEntries
FROM Event e
LEFT JOIN Entry en ON e.EventID = en.EventID
WHERE en.Status != 'Cancelled' OR en.Status IS NULL
GROUP BY e.EventID, e.EventName, e.EventDate, e.City, e.Province, e.CurrentParticipants
ORDER BY e.EventDate;

-- Show all participants with their entry counts
SELECT 
    p.FullName,
    COUNT(en.EntryID) AS TotalEntries,
    COUNT(CASE WHEN en.Status = 'Confirmed' THEN 1 END) AS ConfirmedEntries,
    COUNT(CASE WHEN en.Status = 'Pending' THEN 1 END) AS PendingEntries
FROM Participant p
LEFT JOIN Entry en ON p.ParticipantID = en.ParticipantID
GROUP BY p.ParticipantID, p.FullName
ORDER BY TotalEntries DESC;

-- ============================================================
-- END OF SCRIPT
-- ============================================================

