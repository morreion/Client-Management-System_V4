# 034-Window-Maximize-and-Resize-Implementation

This document explains the implementation of window maximization and responsive resizing for the WPF application.

## Table of Contents
1. [XAML Layout Changes](#1-xaml-layout-changes)
2. [Window Control Styles](#2-window-control-styles)
3. [Code-Behind Logic](#3-code-behind-logic)
4. [Responsive Design Principles](#4-responsive-design-principles)

---

## 1. XAML Layout Changes

### Responsive Grid
To allow the application to expand, we changed the second column definition from a fixed width to a proportional width (`*`).

```xml
<Grid.ColumnDefinitions>
    <ColumnDefinition Width="250" /> <!-- Fixed sidebar -->
    <ColumnDefinition Width="*" />   <!-- Flexible content area -->
</Grid.ColumnDefinitions>
```

| Property | Value | Description |
| :--- | :--- | :--- |
| `ResizeMode` | `CanResizeWithGrip` | Enables window borders to be draggable for resizing. |
| `MinWidth` | `950` | Ensures the layout doesn't break at very small sizes. |
| `MinHeight` | `650` | Ensures vertical content remains visible. |

---

## 2. Window Control Styles

We added a new button for maximization in `Button.xaml`.

```xml
<Style x:Key="MaximizeApp" TargetType="{x:Type Button}">
    <Setter Property="Margin" Value="0,15,115,0" /> <!-- Positioned between Min and Close -->
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type Button}">
                <Border x:Name="border" Background="{StaticResource maximize_app_def}" />
                <ControlTemplate.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter TargetName="border" Property="Background" Value="{StaticResource maximize_app_mo}" />
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

---

## 3. Code-Behind Logic

The code-behind handles the interaction between the UI and the Windows OS.

### Maximize Toggle
```csharp
private void MaximizeApp_Click(object sender, RoutedEventArgs e)
{
    if (WindowState == WindowState.Maximized)
        WindowState = WindowState.Normal;
    else
        WindowState = WindowState.Maximized;
}
```

### Drag to Restore
When a window is maximized, simply calling `DragMove()` won't work correctly. We calculate the relative position of the mouse and restore the window to that spot.

```csharp
if (WindowState == WindowState.Maximized)
{
    var mousePos = e.GetPosition(this);
    var screenPos = PointToScreen(mousePos);
    var percentX = mousePos.X / ActualWidth;
    
    WindowState = WindowState.Normal;
    
    Left = screenPos.X - (Width * percentX);
    Top = screenPos.Y - mousePos.Y;
}
DragMove();
```

---

## 4. Responsive Design Principles

1.  **Sidebar Consistency**: In professional applications (like VS Code or Outlook), the sidebar remains a constant width to provide a stable navigation experience.
2.  **Content Fluidity**: The central area where the user works should always take up as much space as possible.
3.  **Visual Polish**: Removing corner radiuses when maximized aligns the window flush with the screen edges, which is a key trait of modern Windows 10/11 applications.
