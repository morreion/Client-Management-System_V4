# 017 - Prescription State & Persistence Fix

**Topic:** fixing issues where Prescription details were not saving correctly, and input fields contained "stale" data from other clients.

## The Problems

1.  **"Values Not Saving"**: Users were entering dosage information (Breakfast, Lunch, etc.) but finding it gone after saving.
    *   **Root Cause**: The "Add" button was positioned *before* the dosage inputs visually. Users were likely clicking "Add" (adding an empty item to the list) and *then* typing the dosage into the form fields, expecting those fields to belong to the saved record. The text in the form fields was never actually added to the object in the list.
2.  **"Stale Data"**: When switching from Client A to Client B, the text typed in the "Add Item" form (e.g., "1 pill" for Breakfast) remained there.
    *   **Root Cause**: The properties bound to these textboxes (`ItemToAddBreakfast`, etc.) were not being reset when the `SelectedPrescription` changed.

## The Solutions

### 1. UX Improvement (Prescription.xaml)
We moved the **"Add Item To List"** button to the **bottom** of the form.
This enforces the correct workflow:
1.  Select Supplement
2.  Enter Dosages
3.  **Click Add** (Capture all data into the list)

### 2. State Management (PrescriptionVM.cs)
We created a `ResetAddForm()` method and called it whenever the `SelectedPrescription` changes.

```csharp
public Prescription? SelectedPrescription
{
    get => _selectedPrescription;
    set
    {
        _selectedPrescription = value;
        OnPropertyChanged();
        
        // FIX: Clear the staging inputs so they don't persist across clients
        ResetAddForm(); 

        // Load details...
        if (_selectedPrescription != null) ...
    }
}
```

This ensures that every time you navigate to a new prescription, the "Add New Item" form starts fresh.
