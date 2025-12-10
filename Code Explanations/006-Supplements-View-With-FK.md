# Supplements View Implementation - Code Explanation

## Table of Contents
1. [Overview](#overview)
2. [Model Class & Schema](#model-class--schema)
3. [Repository Pattern with JOIN](#repository-pattern-with-join)
4. [ViewModel Layer](#viewmodel-layer)
5. [View UI & ComboBox](#view-ui--combobox)

---

## Overview

The Supplements view manages the list of herbal remedies and supplements. A key feature is linking each supplement to a **Distributor**, requiring a foreign key relationship and a user-friendly dropdown selection.

**Files Created:**
- [Supplement.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Models/Supplement.cs)
- [SupplementRepository.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Repositories/SupplementRepository.cs)
- [SupplementsVM.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/ViewModel/SupplementsVM.cs)
- [Supplements.xaml](file:///C:/Users/admin/source/repos/Client-Management-System_V4/View/Supplements.xaml)

---

## Model Class & Schema

### Supplement.cs

Based on the verified database schema (no price/cost columns):

```csharp
public class Supplement
{
    public int? SupplementID { get; set; }
    public string Name { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public string? Usage { get; set; }
    
    // Foreign Key
    public int DistributorID { get; set; }
    
    // Computed property for display (filled by Repository JOIN)
    public string? DistributorName { get; set; }
}
```

**Note:** `DistributorName` is not in the `Supplements` table but is essential for displaying "Metagenics" instead of "1" in the DataGrid.

---

## Repository Pattern with JOIN

### SupplementRepository.cs

The repository handles the complexity of fetching related distributor data.

#### GetAllAsync (SQL JOIN)
```csharp
var sql = @"
    SELECT s.*, d.Name as DistributorName 
    FROM Supplements s 
    LEFT JOIN Distributor d ON s.DistributorID = d.DistributorID 
    ORDER BY s.Name";
return await connection.QueryAsync<Supplement>(sql);
```
- **LEFT JOIN**: Ensures supplements are loaded even if the distributor ID is invalid or missing (though schema enforces FK).
- **d.Name as DistributorName**: Maps the distributor's name to the computed property in our `Supplement` model.

---

## ViewModel Layer

### SupplementsVM.cs

This ViewModel manages two collections:
1. `Supplements`: The list of items to display.
2. `Distributors`: The list of options for the dropdown.

#### Loading Data
```csharp
private async Task LoadDataAsync()
{
    // 1. Load Distributors first (for ComboBox)
    var distributors = await _distributorRepository.GetAllAsync();
    Distributors = new ObservableCollection<Distributor>(distributors);

    // 2. Load Supplements
    var supplements = await _repository.GetAllAsync();
    Supplements = new ObservableCollection<Supplement>(supplements);
}
```

#### Validation
```csharp
if (SelectedSupplement.DistributorID <= 0)
{
    MessageBox.Show("Please select a distributor.");
    return;
}
```
- Ensures a valid distributor is selected before saving.

---

## View UI & ComboBox

### Supplements.xaml

#### Distinct Features
1. **Slide-up Animation**: inherited from `Page_Style`.
2. **Custom ComboBox**: Uses `MyCustomComboBoxStyle` for consistent dark theming.

```xml
<ComboBox ItemsSource="{Binding Distributors}"
          SelectedValue="{Binding SelectedSupplement.DistributorID}"
          SelectedValuePath="DistributorID"
          DisplayMemberPath="Name"
          Style="{StaticResource MyCustomComboBoxStyle}" />
```

- **ItemsSource**: Binds to the list of loaded distributors.
- **DisplayMemberPath**: Shows the `Name` property in the list.
- **SelectedValuePath**: Uses `DistributorID` as the underlying value.
- **SelectedValue**: Binds that ID to the `Supplement.DistributorID` model property.

 This setup automatically handles the lookup:
 - When user picks "Metagenics", `DistributorID` (e.g., 5) is saved.
 - When loading a supplement with ID=5, "Metagenics" is automatically selected.

---

## Conclusion

The Supplements view successfully implements a **Many-to-One relationship** (Many Supplements -> One Distributor) using standard MVVM practices and Dapper JOINs.

*Created: December 7, 2025*
