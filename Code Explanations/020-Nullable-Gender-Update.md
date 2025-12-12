# 020-Nullable-Gender-Update

## Overview
Updated the Gender feature to allow `null` values (Unknown/Unspecified) instead of enforcing a strict Male/Female binary. This required changing the database column from `NOT NULL` to `NULLABLE` and updating the C# model.

## 1. Database Migration (ClientRepository.cs)
### The Problem
SQLite does not support "altering" a column constraint (e.g., removing `NOT NULL`) directly with `ALTER COLUMN`.

### The Solution: Rename-Add-Copy-Drop
To safely make the column nullable without losing data, we implemented a migration logic that runs at startup:

```csharp
if (genderColumn.notnull == 1) // Check if currently NOT NULL
{
    // Transaction ensures all steps pass or fail together
    using var transaction = connection.BeginTransaction();
    
    // 1. Rename existing column
    connection.Execute("ALTER TABLE Client RENAME COLUMN Gender TO Gender_Old", transaction: transaction);

    // 2. Create NEW nullable column
    connection.Execute("ALTER TABLE Client ADD COLUMN Gender INTEGER", transaction: transaction);

    // 3. Copy data from Old to New
    connection.Execute("UPDATE Client SET Gender = Gender_Old", transaction: transaction);

    // 4. Delete Old column
    connection.Execute("ALTER TABLE Client DROP COLUMN Gender_Old", transaction: transaction);
    
    transaction.Commit();
}
```

## 2. Model Changes (Client.cs)
Changed the `Gender` property to a nullable integer (`int?`).
```csharp
public int? Gender { get; set; } // Null = Unknown, 0 = Female, 1 = Male

public string GenderDisplay
{
    get
    {
        if (!Gender.HasValue) return ""; // Display nothing if unknown
        return Gender.Value == 1 ? "Male" : "Female";
    }
}
```

## 3. UI Handling (IntToBoolConverter.cs)
Updated the converter to gracefully handle `null` values by returning `false` (no radio button selected).
```csharp
// Handle null value (returns false for IsChecked)
if (value == null) return false;
```
Now, if a client has a `null` Gender, neither "Male" nor "Female" radio buttons will be checked.
