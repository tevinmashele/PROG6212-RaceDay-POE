-- ============================================================
-- RACEDAY DATABASE SCRIPT
-- Part 1: System Planning and Database
-- PROG6212 - Programming 2B
-- ============================================================

-- Create the database
CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

-- ============================================================
-- 1. USER TABLE
-- ============================================================
CREATE TABLE [User] (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    [Role] NVARCHAR(20) NOT NULL CHECK ([Role] IN ('Organiser', 'Participant')),
    DateRegistered DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE()
);
GO

-- ============================================================
-- 2. ORGANISER TABLE
-- ============================================================
CREATE TABLE Organiser (
    OrganiserID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    OrganisationName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    ContactNumber NVARCHAR(20) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Organiser_User FOREIGN KEY (UserID) REFERENCES [User](UserID)
);
GO

-- ============================================================
-- 3. EVENT TABLE
-- ============================================================
CREATE TABLE Event (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    EventName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    EventDate DATETIME NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    Distance DECIMAL(5,2) NOT NULL,
    EventType NVARCHAR(20) NOT NULL CHECK (EventType IN ('Running', 'Walking', 'Cycling')),
    MaxParticipants INT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Event_Organiser FOREIGN KEY (OrganiserID) REFERENCES Organiser(OrganiserID)
);
GO

-- ============================================================
-- 4. CATEGORY TABLE
-- ============================================================
CREATE TABLE Category (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200) NULL,
    EntryFee DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    StartTime TIME NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Category_Event FOREIGN KEY (EventID) REFERENCES Event(EventID),
    CONSTRAINT UQ_Event_Category UNIQUE (EventID, CategoryName)
);
GO

-- ============================================================
-- 5. ENROLMENT TABLE
-- ============================================================
CREATE TABLE Enrolment (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATETIME DEFAULT GETDATE(),
    [Status] NVARCHAR(20) DEFAULT 'Confirmed' CHECK ([Status] IN ('Confirmed', 'Cancelled', 'Completed')),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Enrolment_User FOREIGN KEY (ParticipantID) REFERENCES [User](UserID),
    CONSTRAINT FK_Enrolment_Event FOREIGN KEY (EventID) REFERENCES Event(EventID),
    CONSTRAINT FK_Enrolment_Category FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID),
    CONSTRAINT UQ_Participant_Event UNIQUE (ParticipantID, EventID)
);
GO

-- ============================================================
-- 6. RESULT TABLE
-- ============================================================
CREATE TABLE Result (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL,
    FinishTime TIME NULL,
    OfficialTime TIME NULL,
    FinishPosition INT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Result_Enrolment FOREIGN KEY (EnrolmentID) REFERENCES Enrolment(EnrolmentID),
    CONSTRAINT UQ_Enrolment_Result UNIQUE (EnrolmentID)
);
GO

-- ============================================================
-- CREATE INDEXES FOR PERFORMANCE
-- ============================================================
CREATE INDEX IX_User_Email ON [User](Email);
CREATE INDEX IX_User_Role ON [User]([Role]);
CREATE INDEX IX_Event_OrganiserID ON Event(OrganiserID);
CREATE INDEX IX_Event_EventDate ON Event(EventDate);
CREATE INDEX IX_Category_EventID ON Category(EventID);
CREATE INDEX IX_Enrolment_ParticipantID ON Enrolment(ParticipantID);
CREATE INDEX IX_Enrolment_EventID ON Enrolment(EventID);
CREATE INDEX IX_Enrolment_CategoryID ON Enrolment(CategoryID);
CREATE INDEX IX_Result_EnrolmentID ON Result(EnrolmentID);
GO

-- ============================================================
-- SEED DATA - USERS (2 Organisers, 3 Participants)
-- ============================================================
INSERT INTO [User] (FirstName, LastName, Email, PasswordHash, [Role], DateRegistered)
VALUES 
    ('Thabo', 'Nkosi', 'thabo@raceday.co.za', 'hashed_password_123', 'Organiser', GETDATE()),
    ('Sarah', 'Van der Merwe', 'sarah@raceday.co.za', 'hashed_password_456', 'Organiser', GETDATE()),
    ('Michael', 'Johnson', 'michael@raceday.co.za', 'hashed_password_789', 'Participant', GETDATE()),
    ('Nomusa', 'Zulu', 'nomusa@raceday.co.za', 'hashed_password_101', 'Participant', GETDATE()),
    ('David', 'Smith', 'david@raceday.co.za', 'hashed_password_112', 'Participant', GETDATE());
GO

-- ============================================================
-- SEED DATA - ORGANISERS
-- ============================================================
INSERT INTO Organiser (UserID, OrganisationName, Description, ContactNumber)
VALUES 
    (1, 'Nkosi Sports Management', 'Professional sports event management company based in Gauteng', '0821234567'),
    (2, 'Cape Town Events', 'Cape Town-based event organisers specializing in road running events', '0837654321');
GO

-- ============================================================
-- SEED DATA - EVENTS (3 Events)
-- ============================================================
INSERT INTO Event (OrganiserID, EventName, Description, EventDate, Location, Distance, EventType, MaxParticipants)
VALUES 
    (1, 'Soweto Marathon', 'Annual marathon through the streets of Soweto', '2026-11-15 06:00:00', 'Soweto, Johannesburg', 42.20, 'Running', 5000),
    (1, 'Joburg 10km Challenge', 'Fast 10km road race in Johannesburg', '2026-10-20 07:00:00', 'Johannesburg CBD', 10.00, 'Running', 3000),
    (2, 'Cape Town Cycle Tour', 'The world\'s largest timed cycle race', '2026-03-08 06:30:00', 'Cape Town', 109.00, 'Cycling', 35000);
GO

-- ============================================================
-- SEED DATA - CATEGORIES (3 per event = 9 total)
-- ============================================================
INSERT INTO Category (EventID, CategoryName, Description, EntryFee, StartTime)
VALUES 
    (1, 'Open Male', 'Open male category (18-39 years)', 350.00, '06:00:00'),
    (1, 'Open Female', 'Open female category (18-39 years)', 350.00, '06:00:00'),
    (1, 'Veterans', 'Veterans category (40+ years)', 350.00, '06:00:00'),
    (2, 'Elite Men', 'Elite male runners', 150.00, '07:00:00'),
    (2, 'Elite Women', 'Elite female runners', 150.00, '07:00:00'),
    (2, 'Open (All)', 'Open category for all runners', 100.00, '07:00:00'),
    (3, 'Licensed Men', 'Licensed male cyclists', 550.00, '06:30:00'),
    (3, 'Licensed Women', 'Licensed female cyclists', 550.00, '06:30:00'),
    (3, 'Open (All)', 'Open category for all cyclists', 650.00, '06:30:00');
GO

-- ============================================================
-- SEED DATA - ENROLMENTS (6 enrolments)
-- ============================================================
INSERT INTO Enrolment (ParticipantID, EventID, CategoryID, [Status])
VALUES 
    (3, 1, 1, 'Confirmed'),
    (4, 1, 2, 'Confirmed'),
    (5, 2, 5, 'Confirmed'),
    (3, 2, 6, 'Confirmed'),
    (4, 3, 7, 'Confirmed'),
    (5, 3, 9, 'Confirmed');
GO

-- ============================================================
-- SEED DATA - RESULTS (3 results)
-- ============================================================
INSERT INTO Result (EnrolmentID, FinishTime, OfficialTime, FinishPosition)
VALUES 
    (1, '03:15:22', '03:15:22', 45),
    (2, '03:45:10', '03:45:10', 128),
    (3, '00:42:15', '00:42:15', 12);
GO

-- ============================================================
-- VERIFICATION QUERIES (Run these to check your data)
-- ============================================================
SELECT * FROM [User];
SELECT * FROM Organiser;
SELECT * FROM Event;
SELECT * FROM Category;
SELECT * FROM Enrolment;
SELECT * FROM Result;
GO