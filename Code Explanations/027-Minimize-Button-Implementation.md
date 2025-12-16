# 027 - Minimize Button Implementation

## Overview
Added a **Minimize** button to the main application window, styled to match the existing **Exit** button.

## 1. Icon Generation
We generated two custom assets for the button:
- `minimize_def.png`: Default state (Dark circle with white dash).
- `minimize_mo.png`: MouseOver state (Lighter circle).

## 2. Style Definition
We defined a new style `MinimizeApp` in `Styles/Button.xaml`. This style inherits the visual properties of the `ExitApp` style but positions the button to the left of the Exit button.

```xml
<Style x:Key="MinimizeApp" TargetType="{x:Type Button}">
    <!-- Size matches Exit Button -->
    <Setter Property="Height" Value="40" />
    <Setter Property="Width" Value="40" />
    
    <!-- Alignment -->
    <Setter Property="HorizontalAlignment" Value="Right" />
    <Setter Property="VerticalAlignment" Value="Top" />
    
    <!-- Margin Calculation -->
    <!-- Exit Button Margin: Right=15 -->
    <!-- Minimize Button Margin: Right = 15 (Gap) + 40 (Exit Width) + 10 (Gap) = 65 -->
    <Setter Property="Margin" Value="0,15,65,0" />

    <Setter Property="Template">
        <!-- Uses the new ImageBrushes -->
        <ControlTemplate TargetType="{x:Type Button}">
             <Border Background="{StaticResource minimize_app_def}" ... />
             <!-- Triggers verify MouseOver changes background to minimize_app_mo -->
        </ControlTemplate>
    </Setter>
</Style>
```

## 3. Logic Implementation
In `MainWindow.xaml.cs`, we implemented the logic to minimize the window using `WindowState`.

```csharp
private void MinimizeApp_Click(object sender, RoutedEventArgs e)
{
    // Sets the window state to localized in the taskbar
    this.WindowState = WindowState.Minimized;
}
```

## 4. UI Placement
In `MainWindow.xaml`, the button was added to the main grid, sharing the same space as the Close button but positioned via its Style's margin.

```xml
<Button x:Name="MinimizeApp"
        Style="{StaticResource MinimizeApp}"
        Click="MinimizeApp_Click" />
```
