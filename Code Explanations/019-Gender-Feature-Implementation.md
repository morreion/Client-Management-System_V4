# 019-Gender-Feature-Implementation

## Overview
Added a Gender field (Male/Female) to the Client management system. This involved a full-stack update from the database schema to the UI.

## 1. Database & Repository (ClientRepository.cs)
### Schema Migration
The `EnsureGenderColumnExists` method runs at startup to check if the `Gender` column exists in the `Client` table. If not, it executes an `ALTER TABLE` command to add it essentially performing an auto-migration.
```csharp
private void EnsureGenderColumnExists()
{
    // ...
    // Check if column exists
    bool hasGender = columns.Any(c => c.name == "Gender");

    if (!hasGender)
    {
        // Add column if missing (Default 0 = Female)
        var alterSql = "ALTER TABLE Client ADD COLUMN Gender INTEGER NOT NULL DEFAULT 0";
        connection.Execute(alterSql);
    }
}
```

### CRUD Operations
Updated `AddAsync` and `UpdateAsync` SQL queries to include the `@Gender` parameter, ensuring the data is persisted.

## 2. Model (Client.cs)
Added the `Gender` property as an integer.
*   **0**: Female
*   **1**: Male

Added a `GenderDisplay` helper property for the DataGrid to show text ("Male"/"Female") instead of numbers.
```csharp
public int Gender { get; set; } // 0=Female, 1=Male
public string GenderDisplay => Gender == 1 ? "Male" : "Female";
```

## 3. Value Converter (IntToBoolConverter.cs)
Created a converter to bind the integer `Gender` property to the boolean `IsChecked` property of the RadioButtons.
*   **Convert**: Checks if the `Gender` value matches the `ConverterParameter` (0 or 1).
*   **ConvertBack**: Returns the `ConverterParameter` as the new integer value when the RadioButton is checked.

## 4. UI (Clients.xaml)
### DataGrid
Added a new column to display the gender.
```xml
<DataGridTextColumn Header="Gender" Binding="{Binding GenderDisplay}" Width="80"/>
```

### Input Form
Added two RadioButtons bound to the same `Gender` property but with different `ConverterParameter` values.
```xml
<RadioButton Content="Female" IsChecked="{Binding SelectedClient.Gender, Converter={StaticResource IntToBoolConverter}, ConverterParameter=0}" ... />
<RadioButton Content="Male" IsChecked="{Binding SelectedClient.Gender, Converter={StaticResource IntToBoolConverter}, ConverterParameter=1}" ... />
```
