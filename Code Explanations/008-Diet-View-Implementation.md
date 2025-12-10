# Diet View Implementation - Code Explanation

## Table of Contents
1. [Overview](#overview)
2. [Model Implementation](#model-implementation)
3. [Repository Pattern](#repository-pattern)
4. [ViewModel Logic](#viewmodel-logic)
5. [UI Design](#ui-design)

---

## Overview

The Diet View allows practitioners to record meal plans (Breakfast, Lunch, Dinner, Snacks) for clients on specific dates. It is designed to handle potentially long text entries for detailed dietary tracking.

**Files Created:**
- [Diet.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Models/Diet.cs)
- [DietRepository.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Repositories/DietRepository.cs)
- [DietVM.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/ViewModel/DietVM.cs)
- [Diet.xaml](file:///C:/Users/admin/source/repos/Client-Management-System_V4/View/Diet.xaml)

---

## Model Implementation

### Diet.cs
Matches the `Diet` table schema strictly:
```csharp
public class Diet
{
    public int? DietID { get; set; }
    public DateTime? Diet_Date { get; set; } = DateTime.Now;
    public string? Breakfast { get; set; }
    public string? Lunch { get; set; }
    public string? Dinner { get; set; }
    public string? Snacks { get; set; }
    public int ClientID { get; set; } // Foreign Key
    
    // Computed (for DataGrid display)
    public string? ClientName { get; set; }
}
```

---

## Repository Pattern

### DietRepository.cs
Implements `IRepository<Diet>` with `JOIN` logic.

#### Search Functionality
Searches not just by client name, but also inside the meal fields.
```csharp
var sql = @"
    SELECT d.*, c.Name as ClientName 
    FROM Diet d 
    JOIN Client c ON d.ClientID = c.ClientID 
    WHERE c.Name LIKE @Search 
       OR d.Breakfast LIKE @Search 
       OR d.Lunch LIKE @Search ...";
```
This allows finding "Who was eating gluten?" or "Who had the Keto breakfast?".

---

## ViewModel Logic

### DietVM.cs
Follows the standard CRUD pattern:
1. **Load**: Fetches `Clients` for the dropdown and `DietList` for the grid.
2. **Add**: Creates new `Diet` object.
3. **Save**: Validates ClientID, inserts or updates, and updates the UI.
4. **Delete**: Removes record after confirmation.

---

## UI Design

### Diet.xaml
Designed for text-heavy input.

1. **Multiline TextBoxes**:
   Each meal input handles wrapping and multiple lines.
   ```xml
   <TextBox Text="{Binding ...}"
            TextWrapping="Wrap" 
            AcceptsReturn="True" 
            MinHeight="60"/>
   ```

2. **Date Handling**:
   Uses the styled `DatePicker`.
   ```xml
   <DatePicker SelectedDate="{Binding SelectedDiet.Diet_Date}" ... />
   ```

3. **Master-Detail Layout**:
   - Top: Filterable DataGrid showing Date and summary.
   - Bottom: Detailed editing form.

---

*Created: December 7, 2025*
