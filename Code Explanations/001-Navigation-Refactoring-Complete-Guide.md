# 001-Navigation-Refactoring-Complete-Guide

## Table of Contents
1. [Overview](#overview)
2. [Phase 1: Customer to Client Rename](#phase-1-customer-to-client-rename)
3. [Phase 2: ViewModels and Commands](#phase-2-viewmodels-and-commands)
4. [Phase 3: DataTemplate Mappings](#phase-3-datatemplate-mappings)
5. [Phase 4: MainWindow Navigation UI](#phase-4-mainwindow-navigation-ui)
6. [How Navigation Works](#how-navigation-works)

---

## Overview

This implementation refactored the entire navigation system from a generic management application to a **health-focused client management system** with 9 navigation items:

**New Navigation Structure:**
1. **Client** (renamed from Customer)
2. **Antropometrics** (new)
3. **Body Systems Overview** (new)
4. **Eye Analysis** (new)
5. **Diet** (new)
6. **Treatment** (new)
7. **Prescription** (new)
8. **Supplements** (new)
9. **Distributor** (new)

---

## Phase 1: Customer to Client Rename

### File: `View/Clients.xaml`

```xml
<UserControl x:Class="Client_Management_System_V4.View.Clients"
```
**Explanation:**
- `x:Class` attribute defines the **fully qualified class name** that connects this XAML file to its code-behind
- Changed from `Customers` to `Clients` to reflect the new naming
- This MUST match the class name in the `.cs` file

```xml
<UserControl.DataContext>
    <vm:ClientVM />
</UserControl.DataContext>
```
**Explanation:**
- `DataContext` sets the **ViewModel** for this view
- `vm:ClientVM` creates an instance of the ClientVM class
- This allows XAML to bind to properties exposed by the ViewModel
- The `vm:` prefix refers to the namespace declared at the top: `xmlns:vm="clr-namespace:Client_Management_System_V4.ViewModel"`

---

### File: `View/Clients.xaml.cs`

```csharp
public partial class Clients : UserControl
{
    public Clients()
    {
        InitializeComponent();
    }
}
```
**Explanation:**
- `partial class` - The class is split between this file and the XAML-generated code
- `Clients` - Class name MUST match the `x:Class` in the XAML file
- `: UserControl` - Inherits from WPF's UserControl base class
- `InitializeComponent()` - **Auto-generated method** that parses the XAML and creates the UI elements
- This method is called in the constructor to build the visual tree

--- 

### File: `ViewModel/ClientVM.cs`

```csharp
class ClientVM : Utilities.ViewModelBase
{
    private readonly PageModel _pageModel;
```
**Explanation:**
- `ClientVM` - The ViewModel class for the Client view
- `: Utilities.ViewModelBase` - Inherits from ViewModelBase which implements `INotifyPropertyChanged`
- `_pageModel` - **Private field** that holds the data model
- `readonly` - Can only be set in the constructor, ensuring data integrity

```csharp
public int CustomerID
{
    get { return _pageModel.CustomerCount; }
    set { _pageModel.CustomerCount = value; OnPropertyChanged(); }
}
```
**Explanation:**
- **Property** that exposes CustomerID to the view
- `get` - Returns the value from the underlying model
- `set` - Updates the model AND calls `OnPropertyChanged()`
- `OnPropertyChanged()` - Notifies the UI that this property changed, triggering a re-render
- This is the **MVVM data binding mechanism**

```csharp
public ClientVM()
{
    _pageModel = new PageModel();
    CustomerID = 100528;
}
```
**Explanation:**
- **Constructor** - Initializes the ViewModel when created
- Creates a new `PageModel` instance
- Sets default `CustomerID` to 100528
- This code runs when `<vm:ClientVM />` is instantiated in XAML

---

## Phase 2: ViewModels and Commands

### File: `ViewModel/NavigationVM.cs`

```csharp
class NavigationVM : ViewModelBase
{
    private object? _currentView = null;
```
**Explanation:**
- `NavigationVM` - The **main navigation controller**
- `_currentView` - Holds the **currently displayed ViewModel**
- `object?` - Nullable type (can be any ViewModel type)
- `= null` - Initial value before any view is loaded

```csharp
public object? CurrentView
{
    get { return _currentView; }
    set { _currentView = value; OnPropertyChanged(); }
}
```
**Explanation:**
- Public property bound to MainWindow's `ContentControl`
- When this property changes, the UI automatically switches views
- `OnPropertyChanged()` triggers WPF's binding system to update the display

```csharp
public ICommand ClientCommand { get; set; }
public ICommand AntropometricsCommand { get; set; }
// ... more commands
```
**Explanation:**
- `ICommand` - WPF interface for **command pattern**
- Each property represents a navigation button
- These are bound to the navigation menu buttons in MainWindow.xaml
- `{ get; set; }` - Auto-implemented properties

```csharp
private void Client(object obj) => CurrentView = new ClientVM();
private void Antropometrics(object obj) => CurrentView = new AntropometricsVM();
```
**Explanation:**
- **Command handler methods** - Called when user clicks navigation button
- `object obj` - Parameter from the command (not used here, but required by ICommand interface)
- `=>` - Expression-bodied method (shorthand for single-line methods)
- `new ClientVM()` - Creates a NEW instance of the ViewModel
- Setting `CurrentView` triggers the UI to display that view

```csharp
public NavigationVM()
{
    ClientCommand = new RelayCommand(Client);
    AntropometricsCommand = new RelayCommand(Antropometrics);
    // ... more commands
    
    CurrentView = new ClientVM();
}
```
**Explanation:**
- **Constructor** - Runs when NavigationVM is created
- `new RelayCommand(Client)` - Wraps the `Client` method in an `ICommand` implementation
- The `RelayCommand` class connects UI buttons to the handler methods
- Last line sets the **startup view** to Client

---

### File: `Utilities/RelayCommand.cs`

```csharp
private readonly Action<object> _execute;
private readonly Func<object?, bool>? _canExecute;
```
**Explanation:**
- `Action<object>` - Delegate that takes an object parameter and returns void
- `_execute` - Stores the method to call when command is executed (e.g., the `Client` method)
- `Func<object?, bool>?` - Delegate that determines if command can execute (optional)
- `_canExecute` - Can be null if the command is always enabled

```csharp
public RelayCommand(Action<object> execute, Func<object?, bool>? canExecute = null)
{
    _execute = execute;
    _canExecute = canExecute;
}
```
**Explanation:**
- Constructor accepts the **execute method** (required) and **canExecute method** (optional)
- `= null` makes `canExecute` parameter optional
- Stores these delegates in private fields

```csharp
public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);
```
**Explanation:**
- Implements `ICommand.CanExecute`
- If `_canExecute` is null, command is **always enabled** (returns true)
- Otherwise, calls the `_canExecute` delegate to determine if button should be enabled
- Used to disable navigation buttons based on application state

```csharp
public void Execute(object? parameter) => _execute(parameter!);
```
**Explanation:**
- Implements `ICommand.Execute`
- Called when user clicks the button
- `parameter!` - Null-forgiving operator (tells compiler we're sure it won't be null)
- Calls the stored delegate (e.g., the `Client` method from NavigationVM)

---

## Phase 3: DataTemplate Mappings

### File: `Utilities/DataTemplate.xaml`

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:vm="clr-namespace:Client_Management_System_V4.ViewModel"
                    xmlns:view="clr-namespace:Client_Management_System_V4.View">
```
**Explanation:**
- `ResourceDictionary` - Container for reusable resources
- `xmlns:vm` - Imports the ViewModel namespace with prefix `vm`
- `xmlns:view` - Imports the View namespace with prefix `view`
- These prefixes allow us to reference ViewModels and Views in XAML

```xml
<DataTemplate DataType="{x:Type vm:ClientVM}">
    <view:Clients />
</DataTemplate>
```
**Explanation:**
- `DataTemplate` - Defines **how to display** a specific type
- `DataType="{x:Type vm:ClientVM}"` - This template applies to `ClientVM` objects
- `<view:Clients />` - When WPF needs to display a `ClientVM`, it will create a `Clients` UserControl
- This is the **automatic ViewModel-to-View mapping**
- When `CurrentView` is set to a `ClientVM`, WPF automatically uses this template

**How it works:**
1. User clicks "Client" button
2. `ClientCommand` executes → calls `Client` method
3. `Client` method sets `CurrentView = new ClientVM()`
4. WPF's `ContentControl` sees the new `CurrentView` value
5. WPF looks for a `DataTemplate` with `DataType=ClientVM`
6. Finds this template and creates a `Clients` UserControl to display

---

## Phase 4: MainWindow Navigation UI

### File: `MainWindow.xaml`

```xml
<StackPanel Height="520" Width="228">
```
**Explanation:**
- `StackPanel` - Layout container that stacks children vertically
- `Height="520"` - Increased from 400 to fit 9 navigation items
- `Width="228"` - Fixed width for navigation sidebar

```xml
<Menu:Btn Style="{StaticResource BtnStyle}"
          Command="{Binding ClientCommand}"
          IsChecked="True">
```
**Explanation:**
- `Menu:Btn` - Custom `RadioButton` defined in `Utilities/Btn.cs`
- `Menu:` prefix refers to `xmlns:Menu="clr-namespace:Client_Management_System_V4.Utilities"`
- `Style="{StaticResource BtnStyle}"` - Applies the button style from `Styles/Button.xaml`
- `Command="{Binding ClientCommand}"` - **Data binding** to NavigationVM's ClientCommand property
- `IsChecked="True"` - First button is selected by default

```xml
<Image Source="Images/img_customer.png"
       Style="{StaticResource Image_Style}" />
```
**Explanation:**
- `Image` - Displays the navigation icon
- `Source="Images/img_customer.png"` - **Relative path** to the icon image
- The image is loaded as an **embedded resource** (configured in .csproj)
- `Style="{StaticResource Image_Style}"` - Applies image styling from `Styles/Image.xaml`

```xml
<TextBlock Text="Client"
           Style="{StaticResource Text_Style}" />
```
**Explanation:**
- `TextBlock` - Displays the navigation label
- `Text="Client"` - The text shown to user
- `Style="{StaticResource Text_Style}"` - Applies text styling from `Styles/Text.xaml`
- Includes font family, size, color, margins

---

## How Navigation Works

### Complete Flow:

1. **User clicks "Antropometrics" button**
   - Button is a `Menu:Btn` (custom RadioButton)
   - Button's `Command` property is bound to `{Binding AntropometricsCommand}`

2. **WPF invokes the command**
   - WPF finds `AntropometricsCommand` in the DataContext (NavigationVM)
   - Calls `RelayCommand.Execute()`
   - RelayCommand calls the stored delegate → `Antropometrics` method

3. **NavigationVM.Antropometrics method executes**
   ```csharp
   private void Antropometrics(object obj) => CurrentView = new AntropometricsVM();
   ```
   - Creates new instance of `AntropometricsVM`
   - Sets `CurrentView` property to this new ViewModel
   - `OnPropertyChanged()` is called (in the property setter)

4. **Property change notification**
   - `OnPropertyChanged()` raises the `PropertyChanged` event
   - WPF's binding system listens for this event
   - Binding system knows `CurrentView` changed

5. **ContentControl updates**
   ```xml
   <ContentControl Content="{Binding CurrentView}" />
   ```
   - ContentControl in MainWindow is bound to `CurrentView`
   - Receives notification of change
   - Gets new value (AntropometricsVM instance)

6. **DataTemplate lookup**
   - ContentControl needs to display an `AntropometricsVM` object
   - Searches for a `DataTemplate` with `DataType="{x:Type vm:AntropometricsVM}"`
   - Finds template in `DataTemplate.xaml`

7. **View creation**
   ```xml
   <DataTemplate DataType="{x:Type vm:AntropometricsVM}">
       <view:Antropometrics />
   </DataTemplate>
   ```
   - WPF creates an `Antropometrics` UserControl
   - Sets its `DataContext` to the `AntropometricsVM` instance
   - Renders the view

8. **Page transition animation**
   - `Page_Style` is applied (from `Styles/Page.xaml`)
   - Fade-in and slide-up animation plays
   - New view appears smoothly

---

## Key Concepts Explained

### MVVM Pattern
- **Model** - Data (PageModel.cs) 
- **View** - UI (*.xaml files)
- **ViewModel** - Logic and data for views (*VM.cs files)
- Views don't directly interact with Models - they go through ViewModels

### Data Binding
- `{Binding PropertyName}` - Connects XAML to C# properties
- Automatically updates UI when properties change
- Requires `INotifyPropertyChanged` implementation

### Commands
- Alternative to event handlers in MVVM
- Allows buttons to be bound to methods
- Supports enable/disable logic via `CanExecute`

### DataTemplates  
- Define how WPF should display specific types
- Enable automatic ViewModel-to-View mapping
- Eliminate need for manual view creation code

---

## Summary

This refactoring successfully transformed the application from a generic management system to a specialized **health client management system** with 9 focused navigation items. All components follow WPF best practices and the MVVM pattern, ensuring maintainable and testable code.
