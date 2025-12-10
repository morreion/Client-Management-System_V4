# Medical History View Implementation - Code Explanation

## Table of Contents
1. [Overview](#overview)
2. [Data Model: Master-Detail](#data-model-master-detail)
3. [Transactional Repository](#transactional-repository)
4. [ViewModel: Managing In-Memory Details](#viewmodel-managing-in-memory-details)
5. [UI Design: Tabs and Forms](#ui-design-tabs-and-forms)

---

## Overview

The Medical History (`MedHx`) view provides a specialized interface for tracking clinical history and current supplements. It sits between the Client list and Anthropometrics in the workflow.

**Files Created:**
- [MedHx.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Models/MedHx.cs)
- [MedHxSupplement.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Models/MedHxSupplement.cs)
- [MedHxRepository.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Repositories/MedHxRepository.cs)
- [MedHxVM.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/ViewModel/MedHxVM.cs)
- [MedHx.xaml](file:///C:/Users/admin/source/repos/Client-Management-System_V4/View/MedHx.xaml)

---

## Data Model: Master-Detail

The data is split into two tables:
1.  **Med_Hx** (Master): Contains text fields (Blood Tests, Medication, Vaccinations, etc.).
    *   *Note: The column `Med_Hx` was mapped to property `HistoryNotes` to avoid naming conflicts.*
2.  **Med_Hx_Supplements** (Detail): A junction table linking a Medical History record to specific Supplements (with Dosage/Freq/Notes).

---

## Transactional Repository

To ensure data integrity, saving is transactional:
```csharp
public async Task<int> AddWithSupplementsAsync(...)
{
    using var transaction = connection.BeginTransaction();
    // 1. Insert Master Record -> Get ID
    // 2. Insert all Supplement Details using that ID
    transaction.Commit();
}
```

---

## ViewModel: Managing In-Memory Details

The `MedHxVM` manages the complexity of the detail list.
- **`CurrentSupplements`**: An `ObservableCollection` representing the supplements for the *currently selected* history record.
- **Adding Items**: When "Add Item" is clicked in the UI, it adds to this collection *in memory*. 
- **Saving**: Only when "Save" is clicked does the VM send the master record and the collection to the Repository to be persisted.

---

## UI Design: Tabs and Forms

To handle the large amount of information without clutter:
- **TabControl**: Separates the view into two logical areas:
    1.  **Medical History**: A form for text-heavy fields (Blood tests, etc.).
    2.  **Supplements Tracker**: A specialized Master-Detail sub-form for adding/removing supplements.

The Navigation Menu in `MainWindow.xaml` was updated to include a **Medical History** button (with a custom generated icon) positioned correctly between Clients and Anthropometrics.

*Created: December 7, 2025*
