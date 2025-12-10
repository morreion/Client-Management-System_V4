-- ============================================================================
-- Database: Client Management System
-- Description: SQLite database for managing herbal remedy clinic clients,
--              prescriptions, supplements, and health assessments
-- Version: 2.0
-- Date: December 2025
-- Changes: Added Med_Hx table, Alt_Contact column, Meridian_Scan column,
--          Med_Hx_Supplements junction table
-- ============================================================================

-- ============================================================================
-- CORE TABLES
-- ============================================================================

-- Client Table
-- Stores primary client information and demographics
CREATE TABLE IF NOT EXISTS "Client" (
	"ClientID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"Name" VARCHAR(255) NOT NULL,
	"Address" VARCHAR(255),
	"DOB" DATE,
	"Mobile" VARCHAR(50),
	"Email" VARCHAR(255),
	"Occupation" VARCHAR(255),
	"Date_First_Consultation" DATE,
	"Date_Last_Consultation" DATE,
	"Marital_Status" VARCHAR(50),
	"Children" INTEGER,
	"Ref" VARCHAR(255),
	"Alt_Contact" VARCHAR(255)
);

-- Distributor Table
-- Stores supplement distributor/supplier information
CREATE TABLE IF NOT EXISTS "Distributor" (
	"DistributorID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"Name" TEXT NOT NULL,
	"Address" TEXT,
	"Work_Phone" VARCHAR(50),
	"Mobile" VARCHAR(50),
	"Email" VARCHAR(255),
	"Website" VARCHAR(255)
);

-- Supplements Table
-- Master list of available supplements and herbal remedies
CREATE TABLE IF NOT EXISTS "Supplements" (
	"SupplementID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"Name" TEXT NOT NULL,
	"Type" VARCHAR(100),
	"Description" TEXT,
	"Usage" TEXT,
	"DistributorID" INTEGER NOT NULL,
	FOREIGN KEY ("DistributorID") REFERENCES "Distributor"("DistributorID")
		ON UPDATE CASCADE ON DELETE RESTRICT
);

-- ============================================================================
-- CLIENT HEALTH ASSESSMENT TABLES
-- ============================================================================

-- Med_Hx Table
-- Medical history information for clients
-- Can track multiple medical history records over time
CREATE TABLE IF NOT EXISTS "Med_Hx" (
	"Med_HxID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"Blood_Test_Results" TEXT,
	"Medication" TEXT,
	"Supplements" TEXT,
	"Accidents_Previous_Illness" TEXT,
	"Menstrual_Notes" TEXT,
    "Vaccinations" VARCHAR(255),
	"Med_Hx" TEXT,
	"Family_Med_Hx" TEXT,
	"Assessment_Date" DATE,
	"ClientID" INTEGER NOT NULL,
	FOREIGN KEY ("ClientID") REFERENCES "Client"("ClientID")
		ON UPDATE CASCADE ON DELETE CASCADE
);

-- Med_Hx_Supplements Table
-- Junction table linking Med_Hx to Supplements
-- Resolves many-to-many relationship: One Med_Hx can have many Supplements,
-- and one Supplement can appear in many Med_Hx records
CREATE TABLE IF NOT EXISTS "Med_Hx_Supplements" (
	"Med_Hx_SupplementsID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"Med_HxID" INTEGER NOT NULL,
	"SupplementID" INTEGER NOT NULL,
	"Dosage" VARCHAR(255),
	"Frequency" VARCHAR(255),
	"Notes" TEXT,
	FOREIGN KEY ("Med_HxID") REFERENCES "Med_Hx"("Med_HxID")
		ON UPDATE CASCADE ON DELETE CASCADE,
	FOREIGN KEY ("SupplementID") REFERENCES "Supplements"("SupplementID")
		ON UPDATE CASCADE ON DELETE RESTRICT
);

-- Anthropometrics Table
-- Physical measurements and vital signs for each client visit
CREATE TABLE IF NOT EXISTS "Anthropometrics" (
	"AnthropometricsID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"Assessment_Date" DATE,
	"BP" VARCHAR(50),
	"Pulse" INTEGER,
	"SpO2_Percent" INTEGER,
	"PWA" VARCHAR(255),
	"Temp" Real,
	"Weight" Real,
	"Height" Real,
	"Zinc_Status" VARCHAR(100),
	"NOX_Status" VARCHAR(100),
	"ClientID" INTEGER NOT NULL,
	FOREIGN KEY ("ClientID") REFERENCES "Client"("ClientID")
		ON UPDATE CASCADE ON DELETE CASCADE
);

-- Body_Systems_Overview Table
-- Comprehensive review of body systems for each client visit
CREATE TABLE IF NOT EXISTS "Body_Systems_Overview" (
	"Body_Systems_OverviewID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"Assessment_Date" DATE,
	"Immune" VARCHAR(255),
	"Allergy" TEXT,
	"Sleep" VARCHAR(255),
	"Snore" VARCHAR(255),
	"Smoke_Alc" VARCHAR(255),
	"Exercise" VARCHAR(255),
	"Tongue" VARCHAR(255),
	"Cravings" VARCHAR(255),
	"Beverages" VARCHAR(255),
	"Digestion" TEXT,
	"Bowels" TEXT,
	"Urination" TEXT,
	"Head" TEXT,
	"ENT" VARCHAR(255),
	"Skin_Hair" TEXT,
	"Nails" VARCHAR(255),
	"Mind_Emotional" VARCHAR(255),
	"Thyroid" VARCHAR(255),
	"Backache" VARCHAR(255),
	"Joint_Pain" VARCHAR(255),
	"ClientID" INTEGER NOT NULL,
	FOREIGN KEY ("ClientID") REFERENCES "Client"("ClientID")
		ON UPDATE CASCADE ON DELETE CASCADE
);

-- Diet Table
-- Client dietary information for each visit
CREATE TABLE IF NOT EXISTS "Diet" (
	"DietID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"Diet_Date" DATE,
	"Breakfast" VARCHAR(500),
	"Lunch" VARCHAR(500),
	"Dinner" VARCHAR(500),
	"Snacks" VARCHAR(500),
	"ClientID" INTEGER NOT NULL,
	FOREIGN KEY ("ClientID") REFERENCES "Client"("ClientID")
		ON UPDATE CASCADE ON DELETE CASCADE
);

-- Eye_Analysis Table
-- Iridology and eye examination results
CREATE TABLE IF NOT EXISTS "Eye_Analysis" (
	"Eye_AnalysisID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"Analysis_Date" DATE,
	"Iris_Colour" VARCHAR(255),
	"Texture" VARCHAR(255),
	"Type" VARCHAR(255),
	"Pupil" VARCHAR(255),
	"Stomach" VARCHAR(255),
	"S_I_T" VARCHAR(255),
	"ANW" VARCHAR(255),
	"Bowel" VARCHAR(255),
    "Nox" VARCHAR(255),
	"Nerve_Rings" VARCHAR(255),
	"Scurf" VARCHAR(255),
	"Radii" VARCHAR(255),
	"Psora" VARCHAR(255),
	"Organs" VARCHAR(255),
	"Urine" VARCHAR(255),
	"Meridian_Scan" TEXT,
	"ClientID" INTEGER NOT NULL,
	FOREIGN KEY ("ClientID") REFERENCES "Client"("ClientID")
		ON UPDATE CASCADE ON DELETE CASCADE
);


-- Eye_Analysis_Sclera_Priorities Table (Junction Table)
-- Links Eye_Analysis records to specific Sclera Priority Types
CREATE TABLE IF NOT EXISTS "Eye_Analysis_Sclera_Priorities" (
    "EyeAnalysisScleraPriorityID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "Eye_AnalysisID" INTEGER NOT NULL,
    "ScleraPriorityTypeID" INTEGER NOT NULL,
    "Notes" TEXT, -- Optional: Specific notes for this priority in this client's eye analysis
    FOREIGN KEY ("Eye_AnalysisID") REFERENCES "Eye_Analysis"("Eye_AnalysisID")
        ON UPDATE CASCADE ON DELETE CASCADE,
    FOREIGN KEY ("ScleraPriorityTypeID") REFERENCES "Sclera_Priority_Types"("ScleraPriorityTypeID")
        ON UPDATE CASCADE ON DELETE RESTRICT,
    UNIQUE("Eye_AnalysisID", "ScleraPriorityTypeID") -- Ensures no duplicate priority types for the same eye analysis
);


-- Sclera_Priority_Types Table
-- Stores predefined types of Sclera Priorities for consistent data entry
CREATE TABLE IF NOT EXISTS "Sclera_Priority_Types" (
    "ScleraPriorityTypeID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "Priority_Name" VARCHAR(255) NOT NULL UNIQUE, -- e.g., 'Lymphatic Congestion', 'Liver Stress'
    "Description" TEXT -- Optional: detailed description of what this priority signifies
);



-- Eye_Scan Table
-- Stores eye scan images related to eye analysis
CREATE TABLE IF NOT EXISTS "Eye_Scan" (
	"Eye_ScanID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"Eye_Scan" BLOB,
	"Scan_Date" DATE,
	"Eye_Side" VARCHAR(10),
	"Eye_AnalysisID" INTEGER NOT NULL,
	FOREIGN KEY ("Eye_AnalysisID") REFERENCES "Eye_Analysis"("Eye_AnalysisID")
		ON UPDATE CASCADE ON DELETE CASCADE
);

-- Treatment Table
-- Treatment plans and clinical impressions
CREATE TABLE IF NOT EXISTS "Treatment" (
	"TreatmentID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"Treatment_Date" DATE,
	"Expectations_of_Treatment" TEXT,
	"Impression" TEXT,
	"Presenting_Symptoms" TEXT,
    "Rx" TEXT,
	"ClientID" INTEGER NOT NULL,
	FOREIGN KEY ("ClientID") REFERENCES "Client"("ClientID")
		ON UPDATE CASCADE ON DELETE CASCADE
);

-- Scanned_Notes Table
-- Stores scanned documents and notes as binary data
CREATE TABLE IF NOT EXISTS "Scanned_Notes" (
	"ScannedNotesID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"Notes" BLOB,
	"Upload_Date" DATE,
	"Document_Type" VARCHAR(100),
	"Description" TEXT,
	"ClientID" INTEGER NOT NULL,
	FOREIGN KEY ("ClientID") REFERENCES "Client"("ClientID")
		ON UPDATE CASCADE ON DELETE CASCADE
);

-- ============================================================================
-- PRESCRIPTION TABLES
-- ============================================================================

-- Prescription Table
-- Main prescription records for clients
CREATE TABLE IF NOT EXISTS "Prescription" (
	"PrescriptionID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"Prescription_Date" DATE NOT NULL,
	"Next_Appointment_Date" DATE,
	"Recommendations" TEXT,
	"ClientID" INTEGER NOT NULL,
	FOREIGN KEY ("ClientID") REFERENCES "Client"("ClientID")
		ON UPDATE CASCADE ON DELETE CASCADE
);

-- Prescription_Supplements Table
-- Junction table linking prescriptions to supplements with dosage schedules
-- Resolves many-to-many relationship between Prescription and Supplements
CREATE TABLE IF NOT EXISTS "Prescription_Supplements" (
	"Prescription_SupplementsID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"Breakfast" VARCHAR(255),
	"Lunch" VARCHAR(255),
	"Dinner" VARCHAR(255),
	"Bedtime" VARCHAR(255),
	"PrescriptionID" INTEGER NOT NULL,
	"SupplementID" INTEGER NOT NULL,
	FOREIGN KEY ("PrescriptionID") REFERENCES "Prescription"("PrescriptionID")
		ON UPDATE CASCADE ON DELETE CASCADE,
	FOREIGN KEY ("SupplementID") REFERENCES "Supplements"("SupplementID")
		ON UPDATE CASCADE ON DELETE RESTRICT
);

-- ============================================================================
-- INDEXES FOR PERFORMANCE
-- ============================================================================

-- Index on Client Name for faster searches
CREATE INDEX IF NOT EXISTS "idx_Client_Name" ON "Client"("Name");

-- Index on Client Email for faster lookups
CREATE INDEX IF NOT EXISTS "idx_Client_Email" ON "Client"("Email");

-- Index on foreign keys for faster joins
CREATE INDEX IF NOT EXISTS "idx_Med_Hx_ClientID" ON "Med_Hx"("ClientID");
CREATE INDEX IF NOT EXISTS "idx_Med_Hx_Supplements_Med_HxID" ON "Med_Hx_Supplements"("Med_HxID");
CREATE INDEX IF NOT EXISTS "idx_Med_Hx_Supplements_SupplementID" ON "Med_Hx_Supplements"("SupplementID");
CREATE INDEX IF NOT EXISTS "idx_Anthropometrics_ClientID" ON "Anthropometrics"("ClientID");
CREATE INDEX IF NOT EXISTS "idx_Body_Systems_Overview_ClientID" ON "Body_Systems_Overview"("ClientID");
CREATE INDEX IF NOT EXISTS "idx_Diet_ClientID" ON "Diet"("ClientID");
CREATE INDEX IF NOT EXISTS "idx_Eye_Analysis_ClientID" ON "Eye_Analysis"("ClientID");
CREATE INDEX IF NOT EXISTS "idx_Eye_Scan_Eye_AnalysisID" ON "Eye_Scan"("Eye_AnalysisID");
CREATE INDEX IF NOT EXISTS "idx_Treatment_ClientID" ON "Treatment"("ClientID");
CREATE INDEX IF NOT EXISTS "idx_Scanned_Notes_ClientID" ON "Scanned_Notes"("ClientID");
CREATE INDEX IF NOT EXISTS "idx_Prescription_ClientID" ON "Prescription"("ClientID");
CREATE INDEX IF NOT EXISTS "idx_Prescription_Date" ON "Prescription"("Prescription_Date");
CREATE INDEX IF NOT EXISTS "idx_Prescription_Supplements_PrescriptionID" ON "Prescription_Supplements"("PrescriptionID");
CREATE INDEX IF NOT EXISTS "idx_Prescription_Supplements_SupplementID" ON "Prescription_Supplements"("SupplementID");
CREATE INDEX IF NOT EXISTS "idx_Supplements_DistributorID" ON "Supplements"("DistributorID");
CREATE INDEX IF NOT EXISTS "idx_Supplements_Name" ON "Supplements"("Name");

-- NEW INDEXES FOR SCLERA PRIORITIES
-- Index on Eye_Analysis_Sclera_Priorities foreign key to Eye_Analysis for faster joins
CREATE INDEX IF NOT EXISTS "idx_Eye_Analysis_Sclera_Priorities_Eye_AnalysisID" ON "Eye_Analysis_Sclera_Priorities"("Eye_AnalysisID");

-- Index on Eye_Analysis_Sclera_Priorities foreign key to Sclera_Priority_Types for faster joins
CREATE INDEX IF NOT EXISTS "idx_Eye_Analysis_Sclera_Priorities_ScleraPriorityTypeID" ON "Eye_Analysis_Sclera_Priorities"("ScleraPriorityTypeID");

-- Index on Sclera_Priority_Types Name for faster lookups when selecting priority types
CREATE INDEX IF NOT EXISTS "idx_Sclera_Priority_Types_Name" ON "Sclera_Priority_Types"("Priority_Name");


-- ============================================================================
-- SAMPLE DATA (Optional - Remove if not needed)
-- ============================================================================

-- Sample Distributor
INSERT INTO "Distributor" ("Name", "Address", "Work_Phone", "Email", "Website")
VALUES 
	('Natural Health Supplies Ltd', '123 Wellness Road, Auckland', '09-123-4567', 'info@naturalhealthsupplies.co.nz', 'www.naturalhealthsupplies.co.nz'),
	('Herbal Remedies NZ', '456 Green Street, Wellington', '04-987-6543', 'contact@herbalremedies.co.nz', 'www.herbalremedies.co.nz');

-- Sample Supplements
INSERT INTO "Supplements" ("Name", "Type", "Description", "Usage", "DistributorID")
VALUES 
	('Vitamin D3', 'Vitamin', 'High potency Vitamin D3 for bone health and immune support', 'Take with food for better absorption', 1),
	('Fish Oil Omega-3', 'Essential Fatty Acid', 'Premium quality fish oil rich in EPA and DHA', 'Take with meals', 1),
	('Magnesium Glycinate', 'Mineral', 'Highly absorbable form of magnesium for muscle and nerve function', 'Take before bedtime', 1),
	('Probiotics', 'Digestive Health', 'Multi-strain probiotic for gut health', 'Take on empty stomach', 2),
	('Echinacea Extract', 'Herbal', 'Immune system support', 'Take at first sign of cold', 2);

-- ============================================================================
-- NOTES
-- ============================================================================
-- VERSION 2.0 CHANGES:
-- 1. Removed Med_Hx and Family_Med_Hx columns from Client table
-- 2. Added Alt_Contact column to Client table
-- 3. Created new Med_Hx table with medical history information
-- 4. Created Med_Hx_Supplements junction table to link Med_Hx to Supplements
-- 5. Added Meridian_Scan column to Eye_Analysis table
-- 6. Added Assessment_Date to Med_Hx table for historical tracking
-- 7. Med_Hx table still has "Supplements" TEXT column for free-text notes,
--    while Med_Hx_Supplements provides structured supplement tracking
--
-- GENERAL NOTES:
-- 1. All date fields use DATE type - store as 'YYYY-MM-DD' format
-- 2. AUTOINCREMENT ensures unique IDs and prevents reuse of deleted IDs
-- 3. Foreign key constraints maintain referential integrity
-- 4. ON DELETE CASCADE: Child records deleted when parent is deleted
-- 5. ON DELETE RESTRICT: Prevents deletion of parent if children exist
-- 6. Indexes improve query performance on frequently searched columns
-- 7. Med_Hx_Supplements allows tracking which specific supplements a client
--    is currently taking, with dosage and frequency information
-- ============================================================================

