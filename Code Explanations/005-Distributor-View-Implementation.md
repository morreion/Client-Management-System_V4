# Distributor View Implementation - Code Explanation

## Table of Contents
1. [Overview](#overview)
2. [Model Class](#model-class)
3. [Repository Pattern](#repository-pattern)
4. [ViewModel Layer](#viewmodel-layer)
5. [View UI](#view-ui)
6. [Key Features](#key-features)

---

## Overview

The Distributor view manages supplement distributors/suppliers in the system. It follows the MVVM pattern established in Phase 1 with the Client view.

**Files Created:**
- [Distributor.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Models/Distributor.cs) - Model
- [DistributorRepository.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Repositories/DistributorRepository.cs) - Data  access
- [DistributorVM.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/ViewModel/DistributorVM.cs) - ViewModel
- [Distributor.xaml](file:///C:/Users/admin/source/repos/Client-Management-System_V4/View/Distributor.xaml) - UI

---

## Model Class

### Distributor.cs

```csharp
public class Distributor
{
    public int? DistributorID { get; set; }      // Primary key, nullable before insert
    public string Name { get; set; } = string.Empty;  // Required
    public string? Contact_Info { get; set; }    // Optional contact details
    public string? Website { get; set; }         // Optional website URL
}
```

**Key Points:**
- Maps directly to `Distributor` table in database
- `DistributorID` is nullable (`int?`) because it's only assigned after database insert
- `Name` defaults to empty string to avoid null warnings
- Properties match database column names exactly (Dapper convention)

---

## Repository Pattern

### DistributorRepository.cs

Implements `IRepository<Distributor>` interface with full CRUD operations:

#### GetAllAsync()
```csharp
return await connection.QueryAsync<Distributor>(
    "SELECT * FROM Distributor ORDER BY Name");
```
- Returns all distributors sorted alphabetically
- Uses Dapper's `QueryAsync` for async database access

#### Search Async(string searchTerm)
```csharp
return await connection.QueryAsync<Distributor>(
    @"SELECT * FROM Distributor 
      WHERE Name LIKE @Search 
         OR Contact_Info LIKE @Search 
         OR Website LIKE @Search
      ORDER BY Name",
    new { Search = $"%{searchTerm}%" });
```
- Searches across Name, Contact_Info, and Website fields
- Uses SQL `LIKE` with `%` wildcards for partial matching
- Parameterized query prevents SQL injection

#### AddAsync(Distributor entity)
```csharp
var sql = @"INSERT INTO Distributor (Name, Contact_Info, Website) 
            VALUES (@Name, @Contact_Info, @Website);
            SELECT last_insert_rowid();";
return await connection.ExecuteScalarAsync<int>(sql, entity);
```
- Inserts new distributor and returns the auto-generated ID
- `last_insert_rowid()` is SQLite's function to get the last inserted ID
- Returns `int` which is then assigned to the model's `DistributorID`

---

## ViewModel Layer

### DistributorVM.cs

#### Properties

**ObservableCollection:**
```csharp
public ObservableCollection<Distributor> Distributors { get; set; }
```
- Automatically notifies UI when collection changes
- Bound to DataGrid's `ItemsSource`

**SelectedDistributor:**
```csharp
public Distributor? SelectedDistributor
{
    get => _selectedDistributor;
    set
    {
        _selectedDistributor = value;
        OnPropertyChanged(nameof(SelectedDistributor));
        OnPropertyChanged(nameof(IsDistributorSelected));  // Updates button states
    }
}
```
- Two-way bound to DataGrid's `SelectedItem`
- Triggers `IsDistributorSelected` update to enable/disable buttons

#### Commands

**LoadedCommand:**
```csharp
LoadedCommand = new RelayCommand(async _ => await InitializeAsync());
```
- Executes when view loads (triggered by `EventTrigger` in XAML)
- Calls `InitializeAsync()` which loads all distributors

**SaveCommand:**
```csharp
SaveCommand = new RelayCommand(
    async _ => await SaveDistributor(), 
    _ => IsDistributorSelected  // Command only enabled when distributor selected
);
```
- Determines Insert vs Update based on `DistributorID == null`
- Validates required fields before saving
- Shows success/error MessageBox

**DeleteCommand:**
```csharp
var result = MessageBox.Show(
    $"Are you sure you want to delete '{SelectedDistributor.Name}'?",
    "Confirm Delete",
    MessageBoxButton.YesNo,
    MessageBoxImage.Question);
```
- Prompts for confirmation before deletion
- Only enabled when a distributor is selected

---

## View UI

### Distributor.xaml

#### Animation Effect
```xml
<UserControl Style="{StaticResource Page_Style}">
```
- `Page_Style` includes slide-up animation from [Page.xaml](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Styles/Page.xaml)
- Animates from `Y=50` to `Y=0` over 0.8 seconds
- Fades from `Opacity=0` to `Opacity=1` over 1 second

#### Layout Structure
```
Grid (4 rows)
 ├─ Row 0: Search Bar (TextBox + Button)
 ├─ Row 1: DataGrid (Distributor List)
 ├─ Row 2: Action Buttons (Add, Save, Delete, Cancel)
 └─ Row 3: Detail Form (Name, Contact, Website)
```

#### Search Functionality
```xml
<TextBox Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}">
    <TextBox.InputBindings>
        <KeyBinding Key="Enter" Command="{Binding SearchCommand}"/>
    </TextBox.InputBindings>
</TextBox>
```
- `UpdateSourceTrigger=PropertyChanged` updates ViewModel immediately
- Enter key triggers search command
- Search button also available for mouse users

#### DataGrid Styling
```xml
<DataGrid Background="#2D3035"
          Foreground="#DBDBDB"
          RowBackground="#2D3035"
          AlternatingRowBackground="#333740">
```
- Dark theme colors matching application style
- Alternating row colors for better readability
- No grid lines for cleaner appearance

#### Form Fields
All TextBoxes use consistent dark theme:
```xml
<TextBox Background="#3F4347" 
         Foreground="#DBDBDB" 
         BorderBrush="#555"/>
```
- Contact Info uses `TextWrapping="Wrap"` and `AcceptsReturn="True"` for multiline input
- All fields bound with `UpdateSourceTrigger=PropertyChanged` for instant updates

---

## Key Features

### 1. Master-Detail Pattern
- Top: DataGrid shows all distributors
- Bottom: Form shows selected distributor details
- Click a row to edit, click "Add New" to create

### 2. Real-Time Search
- Search across Name, Contact Info, and Website
- Results update as you type
- Clear search box to show all distributors

### 3. Validation
```csharp
if (string.IsNullOrWhiteSpace(SelectedDistributor.Name))
{
    MessageBox.Show("Please enter a name...");
    return;
}
```
- Name is required field
- Prevents saving incomplete records

### 4. User Feedback
- Success messages after Add/Update/Delete
- Confirmation dialog before deletion
- Error messages if operations fail
- Loading indicator during async operations

### 5. Responsive UI
- Buttons enable/disable based on selection state
- `IsDistributorSelected` computed property controls button `IsEnabled`
- Cancel button clears selection (`SelectedDistributor = null`)

---

## Usage Flow

1. **View Distributors**: DataGrid loads all distributors on startup
2. **Search**: Type in search box, press Enter or click Search button
3. **Add New**: Click "Add New" → Form clears → Fill details → Click "Save"
4. **Edit**: Click row in DataGrid → Edit fields → Click "Save"
5. **Delete**: Select row → Click "Delete" → Confirm → Record removed
6. **Cancel**: Click "Cancel" to deselect and clear form

---

*Created: December 7, 2025*  
*Distributor View: WORKING ✅*
