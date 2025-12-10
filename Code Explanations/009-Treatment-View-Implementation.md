# Treatment View Implementation - Code Explanation

## Table of Contents
1. [Overview](#overview)
2. [Model Strategy](#model-strategy)
3. [Repository Logic](#repository-logic)
4. [ViewModel Design](#viewmodel-design)
5. [UI Implementation](#ui-implementation)

---

## Overview

The Treatment View enables the practitioner to record clinical impressions, presenting symptoms, expectations, and prescriptions (Rx) during a client consultation.

**Files Created:**
- [Treatment.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Models/Treatment.cs)
- [TreatmentRepository.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Repositories/TreatmentRepository.cs)
- [TreatmentVM.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/ViewModel/TreatmentVM.cs)
- [Treatment.xaml](file:///C:/Users/admin/source/repos/Client-Management-System_V4/View/Treatment.xaml)

---

## Model Strategy

### Treatment.cs
The model uses `int?` for the ID to differentiate between a new record (null) and an existing one (assigned value). It links to the Client via `ClientID`.

```csharp
public class Treatment
{
    public int? TreatmentID { get; set; }
    public DateTime? Treatment_Date { get; set; } = DateTime.Now;
    public string? Presenting_Symptoms { get; set; }
    public string? Rx { get; set; }  // The prescription text
    public int ClientID { get; set; }
    
    // Computed for display
    public string? ClientName { get; set; }
}
```

---

## Repository Logic

### TreatmentRepository.cs
A standard repository implementation with Dapper.

#### Key Feature: Filtered Search
The search function is robust, checking multiple text fields:
```csharp
WHERE c.Name LIKE @Search 
   OR t.Presenting_Symptoms LIKE @Search 
   OR t.Impression LIKE @Search
```
This allows the user to find past treatments by searching for "headache" (symptom) or "John" (client name).

---

## ViewModel Design

### TreatmentVM.cs
Controls the application logic.

1. **Initialization**: Loads Clients and Treatments concurrently.
2. **Commands**:
   - `SaveCommand`: Validates that a Client is selected before saving.
   - `DeleteCommand`: Asks for confirmation before removing a record.
3. **State Management**:
   - `IsSelectionActive` enables/disables buttons based on whether a grid row is selected.

---

## UI Implementation

### Treatment.xaml
The UI handles dense information input.

1. **Multiline TextBoxes**:
   Crucial for fields like "Presenting Symptoms" or "Impression", which can be paragraphs long.
   ```xml
   <TextBox TextWrapping="Wrap" 
            AcceptsReturn="True" 
            MinHeight="60" ... />
   ```

2. **Dark Theme Components**:
   - **Date Picker**: Custom styling applied via resources.
   - **ComboBox**: Uses the global `MyCustomComboBoxStyle`.

3. **Responsive Grid**:
   - Top: Search & List
   - Bottom: Detailed Entry Form

---

*Created: December 7, 2025*
