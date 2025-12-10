# SQLite Database Integration Plan - UPDATED
## Health Client Management System V4
### Based on Normalized Schema (Version 2.0)

---

## Table of Contents
1. [Database Schema Overview](#database-schema-overview)
2. [Table Relationships](#table-relationships)
3. [View-Specific Implementation Plans](#view-specific-implementation-plans)
4. [Model Classes Structure](#model-classes-structure)
5. [Repository Pattern Implementation](#repository-pattern-implementation)
6. [Implementation Roadmap](#implementation-roadmap)

---

## Database Schema Overview

Your database has been properly normalized with **15 core tables** organized into logical groups:

### Core Tables
- `Client` - Primary client demographics and contact information
- `Distributor` - Supplement suppliers
- `Supplements` - Master supplement catalog
- `Sclera_Priority_Types` - Lookup table for eye analysis priorities

### Client Health Assessment Tables
- `Med_Hx` - Medical history records (one-to-many with Client)
- `Anthropometrics` - Physical measurements per visit
- `Body_Systems_Overview` - Body systems review per visit  
- `Diet` - Dietary information per visit
- `Eye_Analysis` - Iridology examination data
- `Eye_Scan` - Eye scan images (BLOB storage)
- `Treatment` - Treatment plans and clinical impressions
- `Scanned_Notes` - Document storage (BLOB)

### Prescription Tables
- `Prescription` - Main prescription records
- `Prescription_Supplements` - Junction table (Prescription ↔ Supplements)

### Junction Tables (Many-to-Many Resolution)
- `Med_Hx_Supplements` - Links medical history to supplements
- `Eye_Analysis_Sclera_Priorities` - Links eye analysis to sclera priority types
- `Prescription_Supplements` - Links prescriptions to supplements with dosage schedule

---

## Table Relationships

### Client (Hub Table)
```
Client (1) → (Many) Med_Hx
Client (1) → (Many) Anthropometrics
Client (1) → (Many) Body_Systems_Overview
Client (1) → (Many) Diet
Client (1) → (Many) Eye_Analysis
Client (1) → (Many) Treatment
Client (1) → (Many) Scanned_Notes
Client (1) → (Many) Prescription
```

### Supplements Relationships
```
Distributor (1) → (Many) Supplements
Supplements (Many) ↔ (Many) Med_Hx [via Med_Hx_Supplements]
Supplements (Many) ↔ (Many) Prescription [via Prescription_Supplements]
```

### Eye Analysis Relationships
```
Eye_Analysis (1) → (Many) Eye_Scan
Eye_Analysis (Many) ↔ (Many) Sclera_Priority_Types [via Eye_Analysis_Sclera_Priorities]
```

---

## View-Specific Implementation Plans

### 1. Client View - Master-Detail Pattern

**Database Tables Used:**
- `Client` (primary)

**UI Layout:**
```
┌──────────────────────────────────────────────────────┐
│  [🔍 Search Name/Email] [➕ Add New Client]           │
├──────────────────────────────────────────────────────┤
│  CLIENT LIST (DataGrid - Top 60% height)             │
│  ┌────────────────────────────────────────────────┐  │
│  │ ID │ Name          │ DOB        │ Mobile      │  │
│  │ 1  │ John Doe      │ 1980-01-01 │ 021-555... │  │
│  │ 2  │ Jane Smith    │ 1975-05-15 │ 022-555... │  │
│  │ 3  │ Bob Johnson   │ 1990-03-22 │ 027-555... │  │
│  └────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────┤
│  CLIENT DETAILS (Form - Bottom 40% height)           │
│  ┌─────────────────────┐ ┌───────────────────────┐  │
│  │ Name:               │ │ DOB:                  │  │
│  │ [_______________]   │ │ [Date Picker]         │  │
│  │                     │ │                       │  │
│  │ Address:            │ │ Mobile:               │  │
│  │ [_______________]   │ │ [_______________]     │  │
│  │                     │ │                       │  │
│  │ Email:              │ │ Occupation:           │  │
│  │ [_______________]   │ │ [_______________]     │  │
│  │                     │ │                       │  │
│  │ Marital Status:     │ │ Children:             │  │
│  │ [Dropdown ▼]        │ │ [___]                 │  │
│  │                     │ │                       │  │
│  │ First Consultation: │ │ Last Consultation:    │  │
│  │ [Date Picker]       │ │ [Date Picker]         │  │
│  │                     │ │                       │  │
│  │ Referral Source:    │ │ Alt Contact:          │  │
│  │ [_______________]   │ │ [_______________]     │  │
│  └─────────────────────┘ └───────────────────────┘  │
│                                                      │
│  [💾 Save] [🗑️ Delete] [❌ Cancel] [📁 View Records] │
└──────────────────────────────────────────────────────┘
```

**ViewModel Properties:**
```csharp
ObservableCollection<Client> Clients
Client SelectedClient
string SearchText
ICommand AddCommand
ICommand SaveCommand
ICommand DeleteCommand
ICommand SearchCommand
ICommand ViewRecordsCommand
```

**Key Features:**
- Search by name or email (indexed columns)
- Inline editing in form
- View Records button navigates to other health assessments
- Auto-update Last_Consultation date when creating new assessments

---

### 2. Anthropometrics View - Form + History Grid

**Database Tables Used:**
- `Client` (for selection)
- `Anthropometrics` (measurements)

**UI Layout:**
```
┌──────────────────────────────────────────────────────┐
│  Client: [Jane Smith ▼]  Assessment Date: [Today ▼]  │
├──────────────────────────────────────────────────────┤
│  CURRENT MEASUREMENTS                                │
│  ┌──────────────────┐ ┌──────────────────┐          │
│  │ BP:    [120/80]  │ │ Pulse: [72]  bpm │          │
│  │ SpO2:  [98] %    │ │ Temp:  [36.5] °C │          │
│  │ Weight:[75.5] kg │ │ Height:[175] cm  │          │
│  │ PWA:   [_______] │ │ Zinc:  [Good ▼]  │          │
│  │ NOX:   [Normal ▼]│ │                  │          │
│  └──────────────────┘ └──────────────────┘          │
│                                                      │
│  [💾 Save Measurement] [🔄 Clear Form]               │
├──────────────────────────────────────────────────────┤
│  MEASUREMENT HISTORY (DataGrid with scroll)         │
│  ┌────────────────────────────────────────────────┐  │
│  │ Date       │ BP      │ Pulse │ Temp │ Weight  │  │
│  │ 2025-12-01 │ 120/80  │ 72    │ 36.5 │ 75.5kg  │  │
│  │ 2025-11-01 │ 122/82  │ 75    │ 36.6 │ 76.0kg  │  │
│  │ 2025-10-01 │ 118/78  │ 70    │ 36.4 │ 76.5kg  │  │
│  └────────────────────────────────────────────────┘  │
│  [📊 View Trends] [🗑️ Delete Selected]               │
└──────────────────────────────────────────────────────┘
```

**ViewModel Properties:**
```csharp
Client SelectedClient
Anthropometrics CurrentMeasurement
ObservableCollection<Anthropometrics> MeasurementHistory
ICommand SaveCommand
ICommand ClearCommand
ICommand ViewTrendsCommand
```

**Key Features:**
- Combo boxes for Zinc_Status and NOX_Status
- Auto-populate date with today
- History ordered by Assessment_Date DESC
- Trend visualization (optional chart)

---

### 3. Body Systems Overview - Form Entry

**Database Tables Used:**
- `Client` (for selection)
- `Body_Systems_Overview`

**UI Layout:**
```
┌──────────────────────────────────────────────────────┐
│  Client: [Select Client ▼]  Date: [2025-12-04 ▼]     │
├──────────────────────────────────────────────────────┤
│  BODY SYSTEMS ASSESSMENT                             │
│  ┌──────────────────────────────────────────────┐    │
│  │ Lifestyle & Habits                           │    │
│  │  Sleep:     [___________________________]    │    │
│  │  Snore:     [___________________________]    │    │
│  │  Smoke/Alc: [___________________________]    │    │
│  │  Exercise:  [___________________________]    │    │
│  │  Cravings:  [___________________________]    │    │
│  │  Beverages: [___________________________]    │    │
│  ├──────────────────────────────────────────────┤    │
│  │ Physical Examination                         │    │
│  │  Tongue:    [___________________________]    │    │
│  │  Nails:     [___________________________]    │    │
│  │  Skin/Hair: [___________________________]    │    │
│  │  ENT:       [___________________________]    │    │
│  ├──────────────────────────────────────────────┤    │
│  │ Systems Review                               │    │
│  │  Immune:    [___________________________]    │    │
│  │  Allergy:   [___________________________]    │    │
│  │  Digestion: [___________________________]    │    │
│  │  Bowels:    [___________________________]    │    │
│  │  Urination: [___________________________]    │    │
│  │  Head:      [___________________________]    │    │
│  │  Mind/Emotional: [______________________]    │    │
│  │  Thyroid:   [___________________________]    │    │
│  │  Backache:  [___________________________]    │    │
│  │  Joint Pain:[___________________________]    │    │
│  └──────────────────────────────────────────────┘    │
│                                                      │
│  [💾 Save] [📋 Load Previous] [❌ Cancel]             │
└──────────────────────────────────────────────────────┘
```

**Features:**
- Scroll view for all fields
- "Load Previous" copies last assessment as template
- Use Expanders/GroupBoxes to organize sections

---

### 4. Eye Analysis View - Complex Form with Images

**Database Tables Used:**
- `Client`
- `Eye_Analysis`
- `Eye_Scan` (for images)
- `Sclera_Priority_Types` (lookup)
- `Eye_Analysis_Sclera_Priorities` (junction)

**UI Layout:**
```
┌──────────────────────────────────────────────────────┐
│  Client: [Select ▼]  Analysis Date: [2025-12-04 ▼]   │
├──────────────────────────────────────────────────────┤
│  ┌───────────────────┐        ┌───────────────────┐  │
│  │   LEFT EYE        │        │   RIGHT EYE       │  │
│  │ ┌───────────────┐ │        │ ┌───────────────┐ │  │
│  │ │ [Image/BLOB]  │ │        │ │ [Image/BLOB]  │ │  │
│  │ │               │ │        │ │               │ │  │
│  │ └───────────────┘ │        │ └───────────────┘ │  │
│  │ [📁 Upload Image] │        │ [📁 Upload Image] │  │
│  └───────────────────┘        └───────────────────┘  │
├──────────────────────────────────────────────────────┤
│  IRIS ANALYSIS                                       │
│  Iris Colour: [_______] Texture: [_______]           │
│  Type: [_______]        Pupil: [_______]             │
│                                                      │
│  ORGAN ZONES                                         │
│  Stomach: [_______]  S_I_T: [_______]  Bowel: [____] │
│  ANW: [_______]  Nox: [_______]  Urine: [_______]    │
│                                                      │
│  CONSTITUTIONAL SIGNS                                │
│  Nerve Rings: [_______]  Scurf: [_______]            │
│  Radii: [_______]  Psora: [_______]                  │
│  Organs: [_______]                                   │
│                                                      │
│  SCLERA PRIORITIES (Multi-select CheckListBox)      │
│  ☐ Lymphatic Congestion  ☐ Liver Stress             │
│  ☐ Kidney Burden         ☐ Cardiovascular Signs     │
│  ☐ Digestive Issues      ☐ Other                    │
│  Notes: [___________________________________________] │
│                                                      │
│  MERIDIAN SCAN                                       │
│  [________________________________________________]   │
│                                                      │
│  [💾 Save] [❌ Cancel]                                │
└──────────────────────────────────────────────────────┘
```

**ViewModel Properties:**
```csharp
Client SelectedClient
EyeAnalysis CurrentAnalysis
ObservableCollection<EyeScan> EyeScans
ObservableCollection<ScleraPriorityType> AvailablePriorities
ObservableCollection<ScleraPriorityType> SelectedPriorities
ICommand UploadLeftEyeCommand
ICommand UploadRightEyeCommand
ICommand SaveCommand
```

**Special Features:**
- **BLOB handling** for eye scan images
- **Many-to-many** relationship for Sclera Priorities via junction table
- Multi-select CheckListBox or ListBox for priorities
- Image display control for eye scans

---

### 5. Diet View - Daily Meal Entry

**Database Tables Used:**
- `Client`
- `Diet`

**UI Layout:**
```
┌──────────────────────────────────────────────────────┐
│  Client: [Select Client ▼]  Diet Date: [Today ▼]     │
├──────────────────────────────────────────────────────┤
│  DAILY DIET RECORD                                   │
│                                                      │
│  Breakfast:                                          │
│  ┌────────────────────────────────────────────────┐  │
│  │                                                │  │
│  │ [Multi-line TextBox - 500 chars max]          │  │
│  │                                                │  │
│  └────────────────────────────────────────────────┘  │
│                                                      │
│  Lunch:                                              │
│  ┌────────────────────────────────────────────────┐  │
│  │ [Multi-line TextBox - 500 chars max]          │  │
│  └────────────────────────────────────────────────┘  │
│                                                      │
│  Dinner:                                             │
│  ┌────────────────────────────────────────────────┐  │
│  │ [Multi-line TextBox - 500 chars max]          │  │
│  └────────────────────────────────────────────────┘  │
│                                                      │
│  Snacks:                                             │
│  ┌────────────────────────────────────────────────┐  │
│  │ [Multi-line TextBox - 500 chars max]          │  │
│  └────────────────────────────────────────────────┘  │
│                                                      │
│  [💾 Save] [📋 Copy From Previous] [❌ Clear]         │
├──────────────────────────────────────────────────────┤
│  DIET HISTORY                                        │
│  ┌────────────────────────────────────────────────┐  │
│  │ Date       │ Summary                           │  │
│  │ 2025-12-04 │ Oatmeal, Chicken salad, Salmon... │  │
│  │ 2025-12-03 │ Eggs, Turkey wrap, Pasta...       │  │
│  └────────────────────────────────────────────────┘  │
│  [👁️ View Details]                                   │
└──────────────────────────────────────────────────────┘
```

**Features:**
- Large text boxes (AcceptsReturn=True, MaxLength=500)
- Text wrapping enabled
- Scroll viewer for vertical scrolling
- History grid shows summary (first 50 chars of combined meals)

---

### 6. Treatment View - Clinical Notes

**Database Tables Used:**
- `Client`
- `Treatment`

**UI Layout:**
```
┌──────────────────────────────────────────────────────┐
│  Client: [Select Client ▼]  Treatment Date: [Today ▼]│
├──────────────────────────────────────────────────────┤
│  TREATMENT RECORD                                    │
│                                                      │
│  Presenting Symptoms:                                │
│  ┌────────────────────────────────────────────────┐  │
│  │ [Multi-line TextBox]                           │  │
│  └────────────────────────────────────────────────┘  │
│                                                      │
│  Expectations of Treatment:                          │
│  ┌────────────────────────────────────────────────┐  │
│  │ [Multi-line TextBox]                           │  │
│  └────────────────────────────────────────────────┘  │
│                                                      │
│  Clinical Impression:                                │
│  ┌────────────────────────────────────────────────┐  │
│  │ [Multi-line TextBox]                           │  │
│  └────────────────────────────────────────────────┘  │
│                                                      │
│  Treatment Plan (Rx):                                │
│  ┌────────────────────────────────────────────────┐  │
│  │ [Multi-line TextBox]                           │  │
│  └────────────────────────────────────────────────┘  │
│                                                      │
│  [💾 Save Treatment] [❌ Cancel]                      │
├──────────────────────────────────────────────────────┤
│  TREATMENT HISTORY                                   │
│  ┌────────────────────────────────────────────────┐  │
│  │ Date       │ Symptoms        │ Impression      │  │
│  │ 2025-12-04 │ Headache,       │ Liver qi        │  │
│  │ 2025-11-15 │ Fatigue         │ Kidney defic... │  │
│  └────────────────────────────────────────────────┘  │
│  [👁️ View Full Details] [🗑️ Delete]                  │
└──────────────────────────────────────────────────────┘
```

---

### 7. Prescription View - Supplement Schedule

**Database Tables Used:**
- `Client`
- `Prescription`
- `Prescription_Supplements` (junction)
- `Supplements`

**UI Layout:**
```
┌──────────────────────────────────────────────────────┐
│  Client: [Jane Smith ▼]  Rx Date: [2025-12-04 ▼]     │
│  Next Appointment: [2025-12-18 ▼]                    │
├──────────────────────────────────────────────────────┤
│  SUPPLEMENT SCHEDULE                                 │
│  ┌────────────────────────────────────────────────┐  │
│  │Supplement    │Breakfast│Lunch │Dinner│Bedtime│  │
│  │──────────────┼─────────┼──────┼──────┼───────│  │
│  │Vitamin D3    │ 1 cap   │      │      │       │  │
│  │Fish Oil      │         │2 caps│      │       │  │
│  │Magnesium     │         │      │      │2 caps │  │
│  │Probiotics    │         │      │1 cap │       │  │
│  └────────────────────────────────────────────────┘  │
│  [➕ Add Supplement] [✏️ Edit] [🗑️ Remove]             │
│                                                      │
│  Recommendations:                                    │
│  ┌────────────────────────────────────────────────┐  │
│  │ Take with food. Avoid caffeine with magnesium. │  │
│  │ Continue for 3 months, then reassess.         │  │
│  └────────────────────────────────────────────────┘  │
│                                                      │
│  [💾 Save Prescription] [🖨️ Print] [❌ Cancel]        │
├──────────────────────────────────────────────────────┤
│  PRESCRIPTION HISTORY                                │
│  ┌────────────────────────────────────────────────┐  │
│  │ Date       │ Supplements Count │ Next Appt     │  │
│  │ 2025-12-04 │ 4                 │ 2025-12-18    │  │
│  │ 2025-11-20 │ 3                 │ 2025-12-04    │  │
│  └────────────────────────────────────────────────┘  │
│  [👁️ View] [📋 Copy to New]                          │
└──────────────────────────────────────────────────────┘
```

**Features:**
- **DataGrid** for Prescription_Supplements
- **Popup dialog** for adding supplements:
  - ComboBox to select from Supplements table
  - TextBoxes for Breakfast/Lunch/Dinner/Bedtime dosages
- Junction table saves multiple rows (one per supplement in prescription)
- "Copy to New" creates new prescription with same supplements

---

### 8. Supplements View - Product Catalog

**Database Tables Used:**
- `Supplements`
- `Distributor`

**UI Layout:**
```
┌──────────────────────────────────────────────────────┐
│  [🔍 Search Name/Type]  [➕ Add Supplement]           │
├──────────────────────────────────────────────────────┤
│  SUPPLEMENT CATALOG (DataGrid)                       │
│  ┌────────────────────────────────────────────────┐  │
│  │ ID │ Name       │ Type    │ Distributor       │  │
│  │ 1  │ Vitamin D3 │ Vitamin │ Natural Health... │  │
│  │ 2  │ Fish Oil   │ EFA     │ Natural Health... │  │
│  │ 3  │ Magnesium  │ Mineral │ Natural Health... │  │
│  │ 4  │ Probiotics │ Digest  │ Herbal Remedies..│  │
│  │ 5  │ Echinacea  │ Herbal  │ Herbal Remedies..│  │
│  └────────────────────────────────────────────────┘  │
│                                                      │
│  SELECTED SUPPLEMENT DETAILS                         │
│  Name:        [Vitamin D3_____________]              │
│  Type:        [Vitamin________________]              │
│  Distributor: [Natural Health Supplies ▼]            │
│  Description: [________________________]             │
│               [________________________]             │
│  Usage:       [________________________]             │
│               [________________________]             │
│                                                      │
│  [💾 Save] [✏️ Edit] [🗑️ Delete] [❌ Cancel]           │
└──────────────────────────────────────────────────────┘
```

**Features:**
- Master-detail pattern
- Foreign key to Distributor (ComboBox)
- Search filters DataGrid in real-time

---

### 9. Distributor View - Supplier Management

**Database Tables Used:**
- `Distributor`

**UI Layout:**
```
┌──────────────────────────────────────────────────────┐
│  [🔍 Search Name/Email]  [➕ Add Distributor]         │
├──────────────────────────────────────────────────────┤
│  DISTRIBUTORS (DataGrid)                             │
│  ┌────────────────────────────────────────────────┐  │
│  │ ID │ Name              │ Email      │ Mobile   │  │
│  │ 1  │ Natural Health... │ info@...   │ 09-123...│  │
│  │ 2  │ Herbal Remedies..│ contact@...│ 04-987...│  │
│  └────────────────────────────────────────────────┘  │
│                                                      │
│  DISTRIBUTOR DETAILS                                 │
│  Name:       [______________________________]        │
│  Address:    [______________________________]        │
│  Work Phone: [______________]                        │
│  Mobile:     [______________]                        │
│  Email:      [______________________________]        │
│  Website:    [______________________________]        │
│                                                      │
│  [💾 Save] [🗑️ Delete] [❌ Cancel]                    │
│  [📦 View Products from this Distributor]            │
└──────────────────────────────────────────────────────┘
```

---

## Model Classes Structure

Based on your schema, here are the C# model classes needed:

### Client.cs
```csharp
public class Client
{
    public int ClientID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime? DOB { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Occupation { get; set; }
    public DateTime? Date_First_Consultation { get; set; }
    public DateTime? Date_Last_Consultation { get; set; }
    public string? Marital_Status { get; set; }
    public int? Children { get; set; }
    public string? Ref { get; set; }
    public string? Alt_Contact { get; set; }
}
```

### Anthropometrics.cs
```csharp
public class Anthropometrics
{
    public int AnthropometricsID { get; set; }
    public DateTime? Assessment_Date { get; set; }
    public string? BP { get; set; }
    public int? Pulse { get; set; }
    public int? SpO2_Percent { get; set; }
    public string? PWA { get; set; }
    public double? Temp { get; set; }
    public double? Weight { get; set; }
    public double? Height { get; set; }
    public string? Zinc_Status { get; set; }
    public string? NOX_Status { get; set; }
    public int ClientID { get; set; }
}
```

### EyeAnalysis.cs
```csharp
public class EyeAnalysis
{
    public int Eye_AnalysisID { get; set; }
    public DateTime? Analysis_Date { get; set; }
    public string? Iris_Colour { get; set; }
    public string? Texture { get; set; }
    public string? Type { get; set; }
    public string? Pupil { get; set; }
    public string? Stomach { get; set; }
    public string? S_I_T { get; set; }
    public string? ANW { get; set; }
    public string? Bowel { get; set; }
    public string? Nox { get; set; }
    public string? Nerve_Rings { get; set; }
    public string? Scurf { get; set; }
    public string? Radii { get; set; }
    public string? Psora { get; set; }
    public string? Organs { get; set; }
    public string? Urine { get; set; }
    public string? Meridian_Scan { get; set; }
    public int ClientID { get; set; }
}
```

### Prescription.cs
```csharp
public class Prescription
{
    public int PrescriptionID { get; set; }
    public DateTime Prescription_Date { get; set; }
    public DateTime? Next_Appointment_Date { get; set; }
    public string? Recommendations { get; set; }
    public int ClientID { get; set; }
}
```

### PrescriptionSupplement.cs (Junction Table Model)
```csharp
public class PrescriptionSupplement
{
    public int Prescription_SupplementsID { get; set; }
    public string? Breakfast { get; set; }
    public string? Lunch { get; set; }
    public string? Dinner { get; set; }
    public string? Bedtime { get; set; }
    public int PrescriptionID { get; set; }
    public int SupplementID { get; set; }
    
    // Navigation properties (not in DB, for convenience)
    public string? SupplementName { get; set; }
}
```

### Supplements.cs
```csharp
public class Supplements
{
    public int SupplementID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Description { get; set; }
    public string? Usage { get; set; }
    public int DistributorID { get; set; }
    
    // Navigation property
    public string? DistributorName { get; set; }
}
```

### Distributor.cs
```csharp
public class Distributor
{
    public int DistributorID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Work_Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
}
```

*Similar models needed for all other tables...*

---

## Repository Pattern Implementation

### Base Repository Interface
```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<int> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}
```

### Client Repository Example
```csharp
public class ClientRepository : IRepository<Client>
{
    private readonly string _connectionString;

    public ClientRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<Client>> GetAllAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        return await connection.QueryAsync<Client>(
            "SELECT * FROM Client ORDER BY Name"
        );
    }

    public async Task<Client?> GetByIdAsync(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<Client>(
            "SELECT * FROM Client WHERE ClientID = @Id",
            new { Id = id }
        );
    }

    public async Task<int> AddAsync(Client client)
    {
        using var connection = new SqliteConnection(_connectionString);
        const string sql = @"
            INSERT INTO Client (Name, Address, DOB, Mobile, Email, Occupation, 
                              Date_First_Consultation, Marital_Status, Children, Ref, Alt_Contact)
            VALUES (@Name, @Address, @DOB, @Mobile, @Email, @Occupation,
                   @Date_First_Consultation, @Marital_Status, @Children, @Ref, @Alt_Contact);
            SELECT last_insert_rowid();";
        return await connection.ExecuteScalarAsync<int>(sql, client);
    }

    public async Task<bool> UpdateAsync(Client client)
    {
        using var connection = new SqliteConnection(_connectionString);
        const string sql = @"
            UPDATE Client 
            SET Name = @Name, Address = @Address, DOB = @DOB, 
                Mobile = @Mobile, Email = @Email, Occupation = @Occupation,
                Date_Last_Consultation = @Date_Last_Consultation,
                Marital_Status = @Marital_Status, Children = @Children,
                Ref = @Ref, Alt_Contact = @Alt_Contact
            WHERE ClientID = @ClientID";
        int rowsAffected = await connection.ExecuteAsync(sql, client);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        int rowsAffected = await connection.ExecuteAsync(
            "DELETE FROM Client WHERE ClientID = @Id",
            new { Id = id }
        );
        return rowsAffected > 0;
    }

    // Custom method for search
    public async Task<IEnumerable<Client>> SearchAsync(string searchTerm)
    {
        using var connection = new SqliteConnection(_connectionString);
        return await connection.QueryAsync<Client>(
            @"SELECT * FROM Client 
              WHERE Name LIKE @Search OR Email LIKE @Search
              ORDER BY Name",
            new { Search = $"%{searchTerm}%" }
        );
    }
}
```

### Prescription Repository with Junction Table
```csharp
public class PrescriptionRepository
{
    private readonly string _connectionString;

    public async Task<int> AddPrescriptionWithSupplementsAsync(
        Prescription prescription, 
        List<PrescriptionSupplement> supplements)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Insert prescription
            const string prescriptionSql = @"
                INSERT INTO Prescription (Prescription_Date, Next_Appointment_Date, Recommendations, ClientID)
                VALUES (@Prescription_Date, @Next_Appointment_Date, @Recommendations, @ClientID);
                SELECT last_insert_rowid();";
            
            int prescriptionId = await connection.ExecuteScalarAsync<int>(
                prescriptionSql, prescription, transaction);

            // Insert supplements
            const string supplementSql = @"
                INSERT INTO Prescription_Supplements 
                (Breakfast, Lunch, Dinner, Bedtime, PrescriptionID, SupplementID)
                VALUES (@Breakfast, @Lunch, @Dinner, @Bedtime, @PrescriptionID, @SupplementID)";

            foreach (var supp in supplements)
            {
                supp.PrescriptionID = prescriptionId;
                await connection.ExecuteAsync(supplementSql, supp, transaction);
            }

            transaction.Commit();
            return prescriptionId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<List<PrescriptionSupplement>> GetPrescriptionSupplementsAsync(int prescriptionId)
    {
        using var connection = new SqliteConnection(_connectionString);
        const string sql = @"
            SELECT ps.*, s.Name as SupplementName
            FROM Prescription_Supplements ps
            INNER JOIN Supplements s ON ps.SupplementID = s.SupplementID
            WHERE ps.PrescriptionID = @PrescriptionID";
        
        var result = await connection.QueryAsync<PrescriptionSupplement>(
            sql, new { PrescriptionID = prescriptionId });
        return result.ToList();
    }
}
```

---

## Implementation Roadmap

### Phase 1: Foundation (Week 1)
- ✅ Install NuGet packages (Dapper, System.Data.SQLite)
- ✅ Create database from your SQL script
- ✅ Set up connection string management
- ✅ Create all Model classes (15 classes)
- ✅ Create base repository interface
- ✅ Test database connection

### Phase 2: Client View Prototype (Week 2)
- ✅ Implement ClientRepository
- ✅ Update ClientVM with CRUD commands
- ✅ Build Client view UI (DataGrid + Form)
- ✅ Test full CRUD operations
- ✅ Add search functionality
- ✅ Create code documentation

### Phase 3: Simple Assessment Views (Week 3)
- ✅ Anthropometrics view + repository
- ✅ Diet view + repository
- ✅ Body Systems Overview view + repository
- ✅ Treatment view + repository

### Phase 4: Complex Views (Week 4)
- ✅ Eye Analysis view with multi-select priorities
- ✅ Eye Scan BLOB handling
- ✅ Prescription view with junction table
- ✅ Supplement/Distributor management

### Phase 5: Polish & Features (Week 5)
- ✅ Add validation
- ✅ Error handling
- ✅ Loading indicators
- ✅ Print functionality (prescriptions)
- ✅ Report generation
- ✅ Performance optimization

---

## Key Implementation Notes

### Junction Tables
Your schema has **3 junction tables**:
1. `Med_Hx_Supplements` - Medical history supplements
2. `Eye_Analysis_Sclera_Priorities` - Eye analysis priorities  
3. `Prescription_Supplements` - Prescription dosage schedule

**Implementation approach:**
- Use **transactions** when inserting/updating to maintain data integrity
- Use **JOIN queries** to fetch related data
- Use **batch operations** for multiple rows

### BLOB Storage
Two tables use BLOB:
- `Eye_Scan.Eye_Scan` - Eye images
- `Scanned_Notes.Notes` - Scanned documents

**Implementation:**
- Use `byte[]` in C# models
- Use `BitmapImage` for display in WPF
- Consider file size limits
- Provide image compression option

### Lookup Tables
- `Sclera_Priority_Types` - Pre-populated lookup for eye analysis

**Implementation:**
- Load once at startup
- Use ComboBox/CheckedListBox for selection
- Allow admin to add new types

---

## Next Steps

1. **Confirm approach** - Do you approve this updated plan?
2. **Provide sample data** - Would you like me to create more comprehensive sample data?
3. **Start implementation** - Ready to begin with Client view prototype?
4. **Answer questions:**
   - Do you want image file paths instead of BLOBs? (easier to manage)
   - Do you need user authentication/roles?
   - Do you need audit trails (who/when modified)?
   - Do you need data export (PDF reports, Excel)?

---

*Document created: 2025-12-04*  
*Based on schema: client_mgmt_schema_Updated_3Dec25_10H30M.sql*  
*Project: Health Client Management System V4*
