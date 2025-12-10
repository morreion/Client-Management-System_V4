# Anthropometrics View Implementation - Code Explanation

## Table of Contents
1. [Overview](#overview)
2. [Real-Time BMI Calculation](#real-time-bmi-calculation)
3. [Model Implementation](#model-implementation)
4. [Repository & JOINs](#repository--joins)
5. [ViewModel Logic](#viewmodel-logic)
6. [UI Features](#ui-features)

---

## Overview

The Anthropometrics view tracks physical measurements (Weight, Height, BP, Pulse, etc.) for each client visit. A unique feature is the **Link to Client** and **Real-Time BMI Calculation**.

**Files Created:**
- [Anthropometrics.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Models/Anthropometrics.cs)
- [AnthropometricsRepository.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Repositories/AnthropometricsRepository.cs)
- [AntropometricsVM.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/ViewModel/AntropometricsVM.cs)
- [Antropometrics.xaml](file:///C:/Users/admin/source/repos/Client-Management-System_V4/View/Antropometrics.xaml)

---

## Real-Time BMI Calculation

To calculate BMI automatically as the user types Weight or Height, the **Model** must notify the UI of changes.

### why INotifyPropertyChanged?
Standard POCO models don't notify the UI when properties change. By implementing `INotifyPropertyChanged` in the Model itself, we enable:
1. User types "80" in Weight.
2. `Weight` setter fires `OnPropertyChanged("Weight")` AND `OnPropertyChanged("BMI")`.
3. UI updates both the Weight textbox and the BMI read-only textbox instantly.

```csharp
public double? Weight
{
    get => _weight;
    set 
    { 
        _weight = value; 
        OnPropertyChanged(); 
        OnPropertyChanged(nameof(BMI)); // Crucial for auto-update
    }
}
```

---

## Model Implementation

### BMI Logic
```csharp
public double? BMI
{
    get
    {
        if (Weight.HasValue && Height.HasValue && Height.Value > 0)
        {
            // Formula: kg / m^2
            // Convert cm to m: Height / 100
            double heightInMeters = Height.Value / 100.0;
            return Math.Round(Weight.Value / (heightInMeters * heightInMeters), 1);
        }
        return null; // Return null if invalid data
    }
}
```

---

## Repository & JOINs

### AnthropometricsRepository.cs
We need to display the **Client Name** in the list, but the table only stores `ClientID`.

```csharp
var sql = @"
    SELECT a.*, c.Name as ClientName 
    FROM Anthropometrics a 
    JOIN Client c ON a.ClientID = c.ClientID 
    ORDER BY a.Assessment_Date DESC";
```
- **JOIN Client**: Fetches the name corresponding to the ID.
- **ClientName**: Mapped to the computed property in the model.

---

## ViewModel Logic

### AntropometricsVM.cs
Manages two data sources:
1. `AnthropometricsList`: The records to inspect.
2. `Clients`: The list of clients for the dropdown.

#### Event Subscription (Optional but robust)
The VM also subscribes to model changes, though the direct Model->UI binding handles the BMI display. The subscription allows the VM to react if we wanted to trigger validation or other side effects.

```csharp
clients = await _clientRepository.GetAllAsync();
records = await _repository.GetAllAsync();
```
Loads both asynchronously on startup.

---

## UI Features

### Antropometrics.xaml

1. **Client Selection**:
   ```xml
   <ComboBox ItemsSource="{Binding Clients}"
             SelectedValue="{Binding SelectedAnthropometrics.ClientID}"
             DisplayMemberPath="Name" ... />
   ```
   - User picks "John Doe", `ClientID` (e.g., 42) is saved.

2. **Read-Only BMI Field**:
   ```xml
   <TextBox Text="{Binding SelectedAnthropometrics.BMI, Mode=OneWay}" 
            IsReadOnly="True" ... />
   ```
   - `Mode=OneWay`: User cannot edit BMI directly.
   - Updates automatically when Weight/Height changes.

3. **Date Picker Styling**:
   Custom style applied explicitly to match dark theme.
   ```xml
   <Style TargetType="DatePickerTextBox">
       <Setter Property="Background" Value="#3F4347"/> ...
   </Style>
   ```

---

*Created: December 7, 2025*
