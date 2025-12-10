# Prescription View Implementation - Code Explanation

## Table of Contents
1. [Overview](#overview)
2. [Master-Detail Data Model](#master-detail-data-model)
3. [Transactional Repository](#transactional-repository)
4. [ViewModel Strategy](#viewmodel-strategy)
5. [Nested UI Design](#nested-ui-design)

---

## Overview

The Prescription View is the most complex so far, implementing a **Master-Detail** relationship. A single Prescription (linked to a Client) can contain multiple Supplements, each with specific dosage instructions (Breakfast, Lunch, Dinner, Bedtime).

**Files Created:**
- [Prescription.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Models/Prescription.cs)
- [PrescriptionSupplement.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Models/PrescriptionSupplement.cs)
- [PrescriptionRepository.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Repositories/PrescriptionRepository.cs)
- [PrescriptionVM.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/ViewModel/PrescriptionVM.cs)
- [Prescription.xaml](file:///C:/Users/admin/source/repos/Client-Management-System_V4/View/Prescription.xaml)

---

## Master-Detail Data Model

We defined two models:
1.  **Prescription** (The Master): Holds Date, ClientID, Recommendations.
2.  **PrescriptionSupplement** (The Detail): Holds SupplementID, Dosage (Brk, Lun, Din, Bed), and links to PrescriptionID.

---

## Transactional Repository

Saving a prescription is a multi-step process:
1.  Insert the Prescription record.
2.  Get the new `PrescriptionID`.
3.  Insert all the `PrescriptionSupplement` records using that ID.

If step 3 fails, we shouldn't have a "floating" prescription with no supplements. We use a **Transaction** to ensure data integrity.

```csharp
public async Task<int> AddWithSupplementsAsync(...)
{
    using var transaction = connection.BeginTransaction();
    try 
    {
        // 1. Insert Master
        var id = await connection.ExecuteScalarAsync<int>(sqlMaster, ...);

        // 2. Insert Details
        foreach (var s in supplements)
        {
            s.PrescriptionID = id;
            await connection.ExecuteAsync(sqlDetail, s, transaction);
        }

        transaction.Commit(); // All or nothing
        return id;
    }
    catch 
    {
        transaction.Rollback();
        throw;
    }
}
```

---

## ViewModel Strategy

The ViewModel manages an **In-Memory List** of supplements (`CurrentSupplements`) while the user is editing.
- The user selects a supplement and adds it to the list.
- Nothing is saved to the database until the "Save" button is clicked.
- When "Save" is clicked, the entire collection is sent to the Repository.

---

## Nested UI Design

The View features a **Nested Form**:
1.  **Main DataGrid**: Lists existing prescriptions.
2.  **Detail Form**:
    - Metadata (Client, Date) on the left.
    - **Supplement Builder** on the right:
        - A small form to pick a supplement and type dosages.
        - "Add" button pushes it to the bottom DataGrid.
        - The bottom DataGrid displays what will be saved.

*Created: December 7, 2025*
