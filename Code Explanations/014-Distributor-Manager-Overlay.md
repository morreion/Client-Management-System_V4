# 014 - Distributor Manager Overlay

**Topic:** Implementing the Distributor Manager Overlay within the Supplements View.

This document details how we refactored the standalone Distributor page into an integrated overlay within the Supplements view. This improves workflow, allowing users to add/manage distributors while in the middle of creating a supplement record.

## Table of Contents
1.  [Overview](#overview)
2.  [ViewModel Implementation (SupplementsVM.cs)](#viewmodel-implementation)
3.  [View Implementation (Supplements.xaml)](#view-implementation)
4.  [Integration & Navigation](#integration--navigation)

---

## Overview

The goal was to remove the separate "Distributor" navigation screen and integrate its functionality directly into the "Supplements" screen using the "Overlay" pattern.

---

## ViewModel Implementation

We extended `SupplementsVM.cs` to handle Distributor operations. This is a partial refactor where we merged logic from `DistributorVM` into `SupplementsVM`.

### 1. Manager Properties

```csharp
// Controls the visibility of the overlay
private bool _isDistributorManagerOpen;
public bool IsDistributorManagerOpen { ... }

// The Distributor currently being added or edited
private Distributor? _selectedManagerDistributor;
public Distributor? SelectedManagerDistributor { ... }

// The list of distributors shown IN THE MANAGER grid
private ObservableCollection<Distributor> _distributorManagerList;
public ObservableCollection<Distributor> DistributorManagerList { ... }
```

### 2. Opening the Manager

When the manager opens, we must do two things:
1.  Fetch the fresh list of distributors (in case changes happened elsewhere).
2.  Clear the "Edit" form so it's ready for a "New" entry.

```csharp
private async Task OpenDistributorManager()
{
    IsDistributorManagerOpen = true;
    SelectedManagerDistributor = new Distributor(); // Ready for 'Add'
    await LoadDistributorManagerList();
}
```

### 3. Closing the Manager

When closing, it is **critical** to refresh the main dropdown list. If the user added a new distributor in the manager, the main Supplement form needs to see it immediately.

```csharp
private void CloseDistributorManager()
{
    IsDistributorManagerOpen = false;
    // Refresh the dropdown list in the main view
    _ = LoadDataAsync(); 
}
```

---

## View Implementation

In `Supplements.xaml`, we added the trigger button and the overlay grid.

### 1. The Trigger Button

We placed a small "Manage" button directly inside the Form layout, next to the "Distributor" label.

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>
    
    <TextBlock Text="Distributor *" ... />
    
    <!-- The Button -->
    <Button Grid.Column="1" Content="⚙️ Manage Distributors" 
            Command="{Binding OpenDistributorManagerCommand}" ... />
</Grid>
```

### 2. The Overlay UI

Just like the Sclera Priority overlay, we use a high Z-Index Grid.

```xml
<Grid Grid.Row="0" Grid.RowSpan="4" Panel.ZIndex="100" 
      Visibility="{Binding IsDistributorManagerOpen, Converter={StaticResource BoolToVis}}"
      Background="#AA000000">

    <Border HorizontalAlignment="Right" Width="500" Background="#212529" ...>
        <Grid Margin="20">
             <!-- Header -->
             <StackPanel ...><Button Content="Back" .../></StackPanel>
             
             <!-- Edit Form -->
             <Border ...>
                 <StackPanel>
                     <TextBlock Text="Name *" .../>
                     <TextBox Text="{Binding SelectedManagerDistributor.Name}" .../>
                     <!-- other fields -->
                 </StackPanel>
             </Border>
             
             <!-- List -->
             <DataGrid ItemsSource="{Binding DistributorManagerList}" ... />
        </Grid>
    </Border>
</Grid>
```

### 3. Fixing XML Structure

**Important Note during Implementation:**
When implementing overlays, care must be taken with XAML structure.
*   **Wrong:** Putting the Overlay Grid *after* the `</Grid>` of the main layout but *inside* `UserControl`. (`UserControl` can only have one child).
*   **Correct:** Putting the Overlay Grid *inside* the main layout `Grid`, usually as the last element, setting `Grid.RowSpan` to cover all rows.

```xml
<!-- Main Layout Grid -->
<Grid>
    <!-- Row Defs -->
    <!-- Content logic -->

    <!-- Overlay Grid (Inside Main Grid) -->
    <Grid Panel.ZIndex="100" ... >
    </Grid>
</Grid>
```

---

## Integration & Navigation

Finally, we cleaned up `MainWindow.xaml` by removing the redundant "Distributors" navigation button.

```xml
<!-- Removed this block -->
<!--
<Menu:Btn Command="{Binding DistributorViewCommand}" ... >
    ...
</Menu:Btn>
-->
```

This results in a cleaner navigation menu, focusing on the core functional areas (Clients, Health, Diet, etc.) while keeping lookup management contextual.
