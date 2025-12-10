# 013 - Eye Analysis Manager Overlay

**Topic:** Implementing a Manager Overlay for Sclera Priority Types functionality in the Eye Analysis View.

This document explains the implementation of a "manager" overlay that allows users to Add, Edit, and Delete Sclera Priority Types directly from the Eye Analysis view without navigating away.

## Table of Contents
1.  [Overview](#overview)
2.  [ViewModel Implementation (EyeAnalysisVM.cs)](#viewmodel-implementation)
3.  [View Implementation (EyeAnalysis.xaml)](#view-implementation)
4.  [Key Concepts](#key-concepts)

---

## Overview

Previously, the Sclera Priorities were a static list of checkboxes. We needed a way to manage these types (CRUD operations). Instead of creating a whole new page, we implemented a sliding "Drawer" or "Overlay" that appears on top of the current view.

---

## ViewModel Implementation

In `EyeAnalysisVM.cs`, we added the logic to control the overlay state and handle the CRUD operations for the priority types.

### 1. Overlay State Management

We added a boolean property to control if the overlay is visible.

```csharp
private bool _isManagerOpen;
public bool IsManagerOpen
{
    get => _isManagerOpen;
    set
    {
        _isManagerOpen = value;
        OnPropertyChanged(nameof(IsManagerOpen));
    }
}
```

### 2. Commands

New commands were introduced to handle the user interactions for opening/closing the manager and saving/deleting records.

```csharp
// Commands definition
public ICommand OpenManagerCommand { get; }
public ICommand CloseManagerCommand { get; }
public ICommand SavePriorityCommand { get; }
public ICommand DeletePriorityCommand { get; }
public ICommand ClearPriorityCommand { get; }

// Initialization in Constructor
OpenManagerCommand = new RelayCommand(async _ => await OpenManager());
CloseManagerCommand = new RelayCommand(_ => CloseManager());
SavePriorityCommand = new RelayCommand(async _ => await SavePriorityAsync());
DeletePriorityCommand = new RelayCommand(async _ => await DeletePriorityAsync());
ClearPriorityCommand = new RelayCommand(_ => SelectedManagerPriority = new ScleraPriorityType());
```

### 3. Logic Methods

**Opening the Manager:**
When opening, we ensure there is a blank object ready for input (`SelectedManagerPriority`) so the user can type immediately.

```csharp
private async Task OpenManager()
{
     IsManagerOpen = true;
     // Initialize a new object so the 'Add' form is ready
     SelectedManagerPriority = new ScleraPriorityType(); 
     await LoadManagerList();
}
```

**Saving Logic:**
We handle both "Add" (ID is 0) and "Update" (ID > 0) scenarios in one method.

```csharp
private async Task SavePriorityAsync()
{
     if (SelectedManagerPriority == null) return;
     
     // Validation
     if (string.IsNullOrWhiteSpace(SelectedManagerPriority.Priority_Name))
     {
         MessageBox.Show("Name is required.");
         return;
     }

     try
     {
         if (SelectedManagerPriority.ScleraPriorityTypeID == 0)
             await _repository.AddPriorityTypeAsync(SelectedManagerPriority);
         else
             await _repository.UpdatePriorityTypeAsync(SelectedManagerPriority);
         
         await LoadManagerList(); // Refresh the grid
         SelectedManagerPriority = new ScleraPriorityType(); // Reset form
     }
     catch(Exception ex)
     {
         MessageBox.Show("Error: " + ex.Message);
     }
}
```

---

## View Implementation

In `EyeAnalysis.xaml`, we added the overlay UI structure.

### 1. The Overlay Grid

We use a `Grid` with a high `Panel.ZIndex` to ensure it sits on top of everything else. The `Visibility` is bound to the `IsManagerOpen` property using a Converter.

```xml
<Grid Grid.Row="0" Grid.RowSpan="4" Panel.ZIndex="100" 
      Visibility="{Binding IsManagerOpen, Converter={StaticResource BoolToVis}}"
      Background="#AA000000"> 
      <!-- Semi-transparent black background dimming -->

    <!-- Drawer Content -->
    <Border HorizontalAlignment="Right" Width="450" Background="#212529">
        <!-- Content goes here -->
    </Border>

</Grid>
```

*   **`Panel.ZIndex="100"`**: Forces this element to be drawn on top of other elements in the Grid.
*   **`Background="#AA000000"`**: Creates a 'modal' effect by dimming the background content.
*   **`HorizontalAlignment="Right"`**: Makes the panel appear attached to the right side of the screen.

### 2. The Edit Form

The form binds directly to the properties of `SelectedManagerPriority`.

```xml
<TextBlock Text="Name *" Style="{StaticResource FieldLabel}"/>
<TextBox Text="{Binding SelectedManagerPriority.Priority_Name, UpdateSourceTrigger=PropertyChanged}" ... />

<TextBlock Text="Description" Style="{StaticResource FieldLabel}"/>
<TextBox Text="{Binding SelectedManagerPriority.Description, UpdateSourceTrigger=PropertyChanged}" ... />
```

### 3. The DataGrid (List)

Allows selecting an item to edit it.

```xml
<DataGrid ItemsSource="{Binding ManagerPriorityList}"
          SelectedItem="{Binding SelectedManagerPriority}" ... >
```

When a user clicks a row in this `DataGrid`, `SelectedManagerPriority` updates, filling the "Edit Form" with that item's data automatically due to Two-Way binding.

---

## Key Concepts

*   **Overlay / Drawer Pattern**: Using a high Z-Index Grid with a semi-transparent background to create focus without leaving the page.
*   **Single Responsibility ViewModel**: The `EyeAnalysisVM` handles both the main analysis logic AND the management logic. For larger apps, this manager logic might be moved to a separate ViewModel, but for this size, keeping it together simplifies state sharing (updating the dropdowns when the manager closes).
*   **State Resetting**: Crucial step is resetting the `SelectedManagerPriority` to `new()` after a save command, so the user can immediately add another item without manually clearing fields.
