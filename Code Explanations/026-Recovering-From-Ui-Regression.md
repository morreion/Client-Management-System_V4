# 026 - Troubleshooting & Recovering from UI Regressions

## Overview
This session focused on recovering from a "regression" (bugs introduced by reverting code) caused by a git operation. We fixed two main issues:
1.  **Horizontal Scrollbars** not appearing in the DataGrid.
2.  **Gender Column & Radio Buttons** disappearing.

This document explains the technical reasons why these issues occurred and the logic behind the fixes.

## 1. The Git Cause
When you ran `git checkout master` followed by `git pull origin master`, you replaced your **local, working code** with the code from the **remote server**, which did not have your latest changes.

*   **Result**: The file `Clients.xaml` reverted to an older version (before we added the Gender column), and `IntToBoolConverter.cs` was deleted (because it didn't exist in the remote version).

## 2. Horizontal Scrollbar Logic

### The Problem: `Width="*"`
Originally, the **Name** column was set to `Width="*"`.
*   **What it means**: "Take up all remaining space *visible on the screen*."
*   **The Check**: The DataGrid tries to squeeze all columns to fit the window width. Since they fit (by checking the Name column), the DataGrid decides "No scrollbar needed."

### The Fix: Fixed Widths
We changed the **Name** column to `Width="250"`.
*   **New Logic**: The DataGrid calculates the total width: `50 (ID) + 250 (Name) + 80 (Gender) + 120 (Mobile) + 200 (Email) + 100 (DOB) + 50 (Age) = 850px`.
*   **The Result**: If your application window is smaller than **850px**, the content physically cannot fit, forcing the **Horizontal Scrollbar** to appear.

```xml
<!-- Before: Tries to fit in window -->
<DataGridTextColumn Header="Name" Binding="{Binding Name}" Width="*"/>

<!-- After: Forces total width to be larger than window -->
<DataGridTextColumn Header="Name" Binding="{Binding Name}" Width="250"/>
```

## 3. Fixing the ScrollBar Style
The original scrollbar code used a "hack" to rotate a vertical scrollbar by -90 degrees to make it horizontal. This is fragile and often causes layout issues.

```xml
<!-- Old, Fragile Way -->
<RotateTransform Angle="-90"/>
```

We refactored this to use a proper **ControlTemplate** with `Grid.ColumnDefinitions`. This is the correct, robust way to style a horizontal scrollbar.

```xml
<!-- New, Robust Way -->
<ControlTemplate TargetType="{x:Type ScrollBar}">
    <Grid>
        <!-- Horizontal bars grow by Column, not Row -->
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
        <!-- IsDirectionReversed="False" is standard for horizontal -->
        <Track x:Name="PART_Track" Grid.Column="0" IsDirectionReversed="False">
            ...
        </Track>
    </Grid>
</ControlTemplate>
```

## 4. Recovering Radio Buttons (IntToBoolConverter)
Radio buttons work with `boolean` (True/False) values (Checked/Unchecked). However, our Gender data is stored as an `integer` (0 or 1).

We need a "Converter" to bridge this gap.

### How it works:
*   **Radio Button 1 (Male)**: `ConverterParameter=1`
    *   *Question*: "Is the Gender value equal to 1?" -> If Yes, `IsChecked = True`.
*   **Radio Button 2 (Female)**: `ConverterParameter=0`
    *   *Question*: "Is the Gender value equal to 0?" -> If Yes, `IsChecked = True`.

```csharp
public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
{
    // Check if the integer value (from Database) matches the parameter (from XAML)
    if (value is int intValue && int.TryParse(parameter?.ToString(), out int targetValue))
    {
        return intValue == targetValue;
    }
    return false;
}
```

Since this file was deleted by the git revert, the Radio Buttons broke. Re-creating it fixed the binding.
