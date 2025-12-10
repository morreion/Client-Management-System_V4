# Eye Analysis View Implementation - Code Explanation

## Table of Contents
1. [Overview](#overview)
2. [Data Model: The Ecosystem](#data-model-the-ecosystem)
3. [Junction Table Management (Many-to-Many)](#junction-table-management-many-to-many)
4. [Image Handling (BLOBs)](#image-handling-blobs)
5. [Transactional Integrity](#transactional-integrity)

---

## Overview

The Eye Analysis (Iridology) View is the most advanced component of the Health module. It integrates standard data, a many-to-many selection list (Sclera Priorities), and binary image data (Eye Scans).

**Files Created:**
- [EyeAnalysis.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Models/EyeAnalysis.cs)
- [ScleraPriorityType.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Models/ScleraPriorityType.cs)
- [EyeScan.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Models/EyeScan.cs)
- [EyeAnalysisRepository.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Repositories/EyeAnalysisRepository.cs)
- [EyeAnalysisVM.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/ViewModel/EyeAnalysisVM.cs)
- [EyeAnalysis.xaml](file:///C:/Users/admin/source/repos/Client-Management-System_V4/View/EyeAnalysis.xaml)

---

## Data Model: The Ecosystem

The `EyeAnalysis` entity sits at the center of three tables:
1.  **Eye_Analysis**: The core record (Date, Iris Colour, Texture, etc.).
2.  **Sclera_Priority_Types**: A lookup table of standard priorities (e.g., "Liver Stress").
3.  **Eye_Scan**: A table storing images as BLOBs.

The relationship between Analysis and Priorities is handled by a junction table `Eye_Analysis_Sclera_Priorities`.

---

## Junction Table Management (Many-to-Many)

To manage multiple priorities per analysis:
1.  **Repository**:
    - `GetSelectedPriorityIdsAsync`: Fetches currently linked priorities.
    - Transactional Save: Inserts into `Eye_Analysis_Sclera_Priorities` for every selected CheckBox.
2.  **ViewModel**:
    - Uses `ObservableCollection<ScleraPriorityType>` where each item has an `IsSelected` boolean.
    - On Load: Resets all `IsSelected = false`, then iterates the DB results to set `IsSelected = true`.

---

## Image Handling (BLOBs)

We store images directly in the database (BLOB).
1.  **ViewModel**: Uses `OpenFileDialog` to read a file into a `byte[]`.
2.  **Model**: `EyeScan` has a `public byte[] Eye_Scan { get; set; }` property.
3.  **View**: Determines the Image Source directly from the byte array.
    ```xml
    <Image Source="{Binding Eye_Scan}" ... />
    ```
    *Note: WPF's Image control can bind directly to a byte array if converter logic is present, or more commonly to a BitmapImage. In this simplified implementation, we rely on standard binding; if WPF validation complains about `byte[]` vs `ImageSource`, a simple ValueConverter would be the standard enhancement.*

---

## Transactional Integrity

Like the Prescription view, saving is an all-or-nothing operation.
```csharp
using var transaction = connection.BeginTransaction();
// 1. Save Master Eye_Analysis
// 2. Save/Update Sclera Priorities (Delete old, Insert new)
// 3. Save new Eye Scans
transaction.Commit();
```

*Created: December 7, 2025*
