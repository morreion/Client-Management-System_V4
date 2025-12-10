# 002-Database-Integration-Client-CRUD

## Table of Contents
1. [Overview](#overview)
2. [Database Infrastructure](#database-infrastructure)
3. [Model Layer](#model-layer)
4. [Repository Pattern](#repository-pattern)
5. [ViewModel Implementation](#viewmodel-implementation)
6. [View Layer (XAML UI)](#view-layer-xaml-ui)
7. [Data Flow Explanation](#data-flow-explanation)

---

## Overview

This implementation adds **SQLite database integration** with **Dapper ORM** to enable full CRUD (Create, Read, Update, Delete) operations for client management.

**Key Technologies:**
- **SQLite** - Lightweight embedded database
- **Dapper** - Micro-ORM for mapping database rows to C# objects
- **Repository Pattern** - Abstraction layer between database and business logic
- **Async/Await** - Non-blocking database operations

---

## Database Infrastructure

### File: `Data/DatabaseManager.cs`

```csharp
public static class DatabaseManager
{
    private static string? _connectionString;
    private const string DatabaseFileName = "HealthManagement.db";
```

**Explanation:**
- `static class` - No instances needed, all methods are static utilities
- `_connectionString` - Cached connection string (initialized once)
- `DatabaseFileName` - Name of the SQLite database file

```csharp
public static string ConnectionString
{
    get
    {
        if (_connectionString == null)
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", DatabaseFileName);
            _connectionString = $"Data Source={dbPath};Version=3;";
        }
        return _connectionString;
    }
}
```

**Explanation:**
- `Property getter` - Returns connection string, initializes if null
- `AppDomain.CurrentDomain.BaseDirectory` - Gets the app's running directory
- `Path.Combine` - Safely combines directory paths (handles \ and /)
- `Data Source={dbPath};Version=3;` - SQLite connection string format
  - `Data Source` - Path to the .db file
  - `Version=3` - SQLite version 3 (latest)

```csharp
public static async Task InitializeDatabaseAsync(string? sqlScriptPath = null)
{
    if (string.IsNullOrEmpty(sqlScriptPath))
    {
        sqlScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "client_mgmt_schema.sql");
        sqlScriptPath = Path.GetFullPath(sqlScriptPath);
    }
```

**Explanation:**
- `async Task` - Asynchronous method that returns no value
- `string?` - Nullable parameter (can be null)
- `"..", "..", ".."` - Navigate up 3 directories from bin/Debug/net8.0-windows/ to project root
- `Path.GetFullPath` - Resolves relative path to absolute path

```csharp
    var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
    Directory.CreateDirectory(dataDir);
```

**Explanation:**
- Creates `Data` folder if it doesn't exist
- `CreateDirectory` is safe to call even if directory exists

```csharp
    using var connection = new SQLiteConnection(ConnectionString);
    await connection.OpenAsync();
```

**Explanation:**
- `using var` - Automatically disposes connection when method ends
- `new SQLiteConnection` - Creates database connection object
- `await connection.OpenAsync()` - Opens connection asynchronously
  - If database file doesn't exist, SQLite creates it

```csharp
    if (!dbExists && File.Exists(sqlScriptPath))
    {
        var schema = await File.ReadAllTextAsync(sqlScriptPath);
        var statements = schema.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var statement in statements)
        {
            var trimmed = statement.Trim();
            if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("--"))
            {
                using var command = new SQLiteCommand(trimmed, connection);
                await command.ExecuteNonQueryAsync();
            }
        }
    }
```

**Explanation:**
- `ReadAllTextAsync` - Reads entire SQL file asynchronously
- `Split(new[] { ';' })` - Splits SQL script into individual statements
- `StringSplitOptions.RemoveEmptyEntries` - Removes empty strings from result
- `Trim()` - Removes leading/trailing whitespace
- `!trimmed.StartsWith("--")` - Skips SQL comments
- `SQLiteCommand` - Executes SQL statement
- `ExecuteNonQueryAsync` - Runs SQL that doesn't return data (CREATE, INSERT, etc.)

**How it works:**
1. App starts → `InitializeDatabaseAsync()` called
2. Checks if database file exists
3. If not, reads `client_mgmt_schema.sql`
4. Splits SQL into individual statements
5. Executes each statement to create tables and insert sample data

---

## Model Layer

### File: `Models/Client.cs`

```csharp
public class Client
{
    public int ClientID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
```

**Explanation:**
- `public class` - Model represents database table
- `ClientID` - Primary key (auto-increment in database)
- `string Name` - Required field, initialized to empty string
- `string?` - Nullable type (can be null)
  - `Address` can be null in database, so property is nullable

```csharp
    public DateTime? DOB { get; set; }
```

**Explanation:**
- `DateTime?` - Nullable DateTime
- Date of birth is optional, so can be null

```csharp
    public string DisplayName => $"{Name} ({Mobile ?? "No phone"})";
```

**Explanation:**
- `=>` - Expression-bodied property (calculated property, no setter)
- `$"{Name} ({Mobile ?? "No phone"})"` - String interpolation
- `Mobile ?? "No phone"` - Null-coalescing operator
  - If `Mobile` is null, use "No phone"
  - Otherwise use `Mobile` value
- **Not stored in database** - Computed on-the-fly

```csharp
    public int? Age
    {
        get
        {
            if (DOB == null) return null;
            var today = DateTime.Today;
            var age = today.Year - DOB.Value.Year;
            if (DOB.Value.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
```

**Explanation:**
- Calculated property (no `set`)
- Returns null if DOB is null
- `today.Year - DOB.Value.Year` - Basic age calculation
- `if (DOB.Value.Date > today.AddYears(-age)) age--;` - Adjusts for birthday not yet reached this year
  - Example: Born Dec 1, 2000. Today is June 1, 2025
  - Basic calc: 2025 - 2000 = 25
  - But birthday hasn't happened yet → age = 24
- **Not stored in database** - Computed from DOB

---

## Repository Pattern

### File: `Repositories/IRepository.cs`

```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<int> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}
```

**Explanation:**
- `interface` - Contract defining required methods
- `<T>` - Generic type parameter (works with any entity)
- `where T : class` - Constraint: T must be a reference type
- `Task<IEnumerable<T>>` - Async method returning collection
- `Task<T?>` - Async method returning single nullable entity
- `Task<int>` - Async method returning integer (new ID)
- `Task<bool>` - Async method returning success/failure

**Why use interface?**
- Abstraction - ViewModels depend on interface, not concrete class
- Testability - Can create mock repositories for unit tests
- Flexibility - Can swap SQLite for SQL Server without changing ViewModels

---

### File: `Repositories/ClientRepository.cs`

```csharp
public class ClientRepository : IRepository<Client>
{
    private readonly string _connectionString;

    public ClientRepository()
    {
        _connectionString = DatabaseManager.ConnectionString;
    }
```

**Explanation:**
- Implements `IRepository<Client>`
- `readonly` - Connection string cannot change after constructor
- Constructor initializes connection string from DatabaseManager

```csharp
    public async Task<IEnumerable<Client>> GetAllAsync()
    {
        using var connection = new SQLiteConnection(_connectionString);
        const string sql = "SELECT * FROM Client ORDER BY Name";
        return await connection.QueryAsync<Client>(sql);
    }
```

**Explanation:**
- `async Task<IEnumerable<Client>>` - Returns list of clients asynchronously
- `using var connection` - Auto-dispose connection when method ends
- `const string sql` - SQL query string (constant)
- `connection.QueryAsync<Client>(sql)` - **Dapper magic!**
  - Executes SQL query
  - Maps each row to a `Client` object automatically
  - Column names match property names
  - Returns list of Client objects

**How Dapper mapping works:**
```
Database Column → C# Property
ClientID        → ClientID
Name            → Name
Address         → Address
DOB             → DOB (converts SQLite DATE to DateTime?)
```

```csharp
    public async Task<int> AddAsync(Client client)
    {
        using var connection = new SQLiteConnection(_connectionString);
        const string sql = @"
            INSERT INTO Client (
                Name, Address, DOB, Mobile, Email, Occupation, 
                Date_First_Consultation, Date_Last_Consultation, 
                Marital_Status, Children, Ref, Alt_Contact
            )
            VALUES (
                @Name, @Address, @DOB, @Mobile, @Email, @Occupation,
                @Date_First_Consultation, @Date_Last_Consultation,
                @Marital_Status, @Children, @Ref, @Alt_Contact
            );
            SELECT last_insert_rowid();";
        
        return await connection.ExecuteScalarAsync<int>(sql, client);
    }
```

**Explanation:**
- `@"..."` - Verbatim string literal (allows multi-line)
- `@Name, @Address` - SQL parameters (prevents SQL injection)
- `SELECT last_insert_rowid()` - SQLite function that returns last inserted ID
- `ExecuteScalarAsync<int>` - Executes SQL and returns single value (the new ID)
- `, client` - Dapper maps `client` properties to `@Name`, `@Address`, etc.

**How parameter mapping works:**
```csharp
client.Name → @Name in SQL
client.Address → @Address in SQL
client.DOB → @DOB in SQL
```

```csharp
    public async Task<IEnumerable<Client>> SearchAsync(string searchTerm)
    {
        using var connection = new SQLiteConnection(_connectionString);
        const string sql = @"
            SELECT * FROM Client 
            WHERE Name LIKE @Search OR Email LIKE @Search
            ORDER BY Name";
        
        return await connection.QueryAsync<Client>(sql, new { Search = $"%{searchTerm}%" });
    }
```

**Explanation:**
- `LIKE @Search` - SQL pattern matching
- `new { Search = $"%{searchTerm}%" }` - Anonymous object for parameters
  - `%` is SQL wildcard (matches any characters)
  - `%john%` matches "John", "Johnny", "Johnson"
- `OR` - Searches both Name and Email columns

---

## ViewModel Implementation

### File: `ViewModel/ClientVM.cs`

```csharp
public class ClientVM : ViewModelBase
{
    private readonly ClientRepository _repository;
    private ObservableCollection<Client> _clients = new();
    private Client? _selectedClient;
```

**Explanation:**
- `ClientRepository _repository` - Database access layer
- `ObservableCollection<Client>` - **Special collection**
  - Implements `INotifyCollectionChanged`
  - When items added/removed, UI automatically updates
  - DataGrid re-renders when collection changes
- `Client? _selectedClient` - Currently selected client (nullable)

```csharp
    public ObservableCollection<Client> Clients
    {
        get => _clients;
        set { _clients = value; OnPropertyChanged(); }
    }
```

**Explanation:**
- Public property for XAML binding
- `set { ... OnPropertyChanged(); }` - Notifies UI when collection reference changes
- **DataGrid binds to this** - `ItemsSource="{Binding Clients}"`

```csharp
    public Client? SelectedClient
    {
        get => _selectedClient;
        set
        {
            _selectedClient = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsClientSelected));
        }
    }
```

**Explanation:**
- Bound to DataGrid's `SelectedItem`
- When user clicks a row, this property updates
- `OnPropertyChanged(nameof(IsClientSelected))` - Also updates computed property
  - This enables/disables Save and Delete buttons

```csharp
    public ICommand SaveCommand { get; }
    
    SaveCommand = new RelayCommand(async _ => await SaveClientAsync());
```

**Explanation:**
- `ICommand` - Interface for commands (button clicks)
- `RelayCommand` - Implementation that wraps a method
- `async _ => await SaveClientAsync()` - Lambda expression
  - `_` - Unused parameter (command parameter)
  - Calls async method when command executes

```csharp
    private async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            await DatabaseManager.InitializeDatabaseAsync();
            await LoadClientsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing database: {ex.Message}", "Database Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
```

**Explanation:**
- Called from constructor
- `try-catch-finally` - Error handling pattern
  - `try` - Attempt operations
  - `catch` - Handle errors
  - `finally` - Always runs (even if error) - turns off loading indicator
- `IsLoading = true` - Shows "Loading..." text
- `MessageBox.Show` - Displays error dialog to user

```csharp
    private async Task SaveClientAsync()
    {
        if (SelectedClient == null) return;

        if (string.IsNullOrWhiteSpace(SelectedClient.Name))
        {
            MessageBox.Show("Client name is required.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
```

**Explanation:**
- Validation before saving
- `string.IsNullOrWhiteSpace` - Checks for null, empty, or whitespace
- Shows warning dialog and exits if validation fails

```csharp
        if (SelectedClient.ClientID == 0)
        {
            // Add new client
            var newId = await _repository.AddAsync(SelectedClient);
            SelectedClient.ClientID = newId;
            Clients.Add(SelectedClient);
        }
        else
        {
            // Update existing client
            SelectedClient.Date_Last_Consultation = DateTime.Today;
            var success = await _repository.UpdateAsync(SelectedClient);
            
            if (success)
            {
                await LoadClientsAsync();
            }
        }
```

**Explanation:**
- `ClientID == 0` - New client (not yet in database)
  - `AddAsync` returns new ID
  - Assign ID to client object
  - Add to ObservableCollection → DataGrid updates automatically
- `ClientID != 0` - Existing client
  - Update `Date_Last_Consultation` to today
  - Call `UpdateAsync`
  - Reload all clients to refresh DataGrid

---

## View Layer (XAML UI)

### File: `View/Clients.xaml`

```xml
<Grid.RowDefinitions>
    <RowDefinition Height="Auto"/>
    <RowDefinition Height="*"/>
    <RowDefinition Height="Auto"/>
    <RowDefinition Height="*"/>
</Grid.RowDefinitions>
```

**Explanation:**
- Defines 4 rows in grid
- `Height="Auto"` - Row height adjusts to content
- `Height="*"` - Row takes remaining space
- Layout: Search (Auto) → DataGrid (*) → Buttons (Auto) → Form (*)

```xml
<TextBox Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"
         ...>
    <TextBox.InputBindings>
        <KeyBinding Key="Enter" Command="{Binding SearchCommand}"/>
    </TextBox.InputBindings>
</TextBox>
```

**Explanation:**
- `{Binding SearchText}` - Two-way binding to ViewModel property
- `UpdateSourceTrigger=PropertyChanged` - Updates ViewModel on every keystroke
  - Default is `LostFocus` (updates when user tabs away)
- `KeyBinding Key="Enter"` - Press Enter to trigger SearchCommand

```xml
<DataGrid ItemsSource="{Binding Clients}"
          SelectedItem="{Binding SelectedClient, Mode=TwoWay}"
          AutoGenerateColumns="False"
          ...>
```

**Explanation:**
- `ItemsSource="{Binding Clients}"` - Binds to ObservableCollection
  - DataGrid displays all clients
  - Automatically updates when collection changes
- `SelectedItem="{Binding SelectedClient, Mode=TwoWay}"` - Two-way binding
  - User clicks row → `SelectedClient` updates in ViewModel
  - ViewModel changes `SelectedClient` → DataGrid selection changes
- `AutoGenerateColumns="False"` - Manual column definition

```xml
<DataGrid.Columns>
    <DataGridTextColumn Header="ID" Binding="{Binding ClientID}" Width="50"/>
    <DataGridTextColumn Header="Name" Binding="{Binding Name}" Width="*"/>
    <DataGridTextColumn Header="Age" Binding="{Binding Age}" Width="50"/>
</DataGrid.Columns>
```

**Explanation:**
- Each column binds to a property
- `{Binding ClientID}` - Shows ClientID for each row
- `{Binding Age}` - Shows computed Age property
- `Width="*"` - Column takes remaining space
- `Width="50"` - Fixed width column

```xml
<Button Content="💾 Save"
        Command="{Binding SaveCommand}"
        IsEnabled="{Binding IsClientSelected}"
        .../>
```

**Explanation:**
- `Command="{Binding SaveCommand}"` - Executes SaveCommand when clicked
- `IsEnabled="{Binding IsClientSelected}"` - Button enabled only if client selected
  - `IsClientSelected` is computed property: `=> SelectedClient != null`

```xml
<TextBox Text="{Binding SelectedClient.Name, UpdateSourceTrigger=PropertyChanged}"
         .../>
```

**Explanation:**
- `{Binding SelectedClient.Name}` - Nested property binding
  - Binds to `Name` property of `SelectedClient` object
- When user types, `SelectedClient.Name` updates immediately
- When different row selected, TextBox shows new client's name

---

## Data Flow Explanation

### Complete Flow: Adding a New Client

1. **User clicks "➕ Add New Client" button**
```xml
<Button Command="{Binding AddNewCommand}" .../>
```

2. **WPF invokes AddNewCommand**
```csharp
AddNewCommand = new RelayCommand(_ => AddNewClient());
```

3. **AddNewClient method executes**
```csharp
private void AddNewClient()
{
    SelectedClient = new Client
    {
        Date_First_Consultation = DateTime.Today
    };
    IsEditMode = true;
}
```
- Creates new Client object (ClientID = 0)
- Sets as SelectedClient
- Property change notification fires

4. **UI updates via data binding**
```xml
<TextBox Text="{Binding SelectedClient.Name}" />
```
- All form fields now bind to new client
- User types name, address, etc.
- Each keystroke updates the Client object

5. **User clicks "💾 Save" button**
```xml
<Button Command="{Binding SaveCommand}" />
```

6. **SaveCommand executes asynchronously**
```csharp
private async Task SaveClientAsync()
{
    // Validation
    if (string.IsNullOrWhiteSpace(SelectedClient.Name))
    {
        MessageBox.Show("Name required");
        return;
    }
    
    // Add to database
    var newId = await _repository.AddAsync(SelectedClient);
    SelectedClient.ClientID = newId;
    Clients.Add(SelectedClient);
}
```

7. **Repository inserts into database**
```csharp
public async Task<int> AddAsync(Client client)
{
    const string sql = "INSERT INTO Client (...) VALUES (...); SELECT last_insert_rowid();";
    return await connection.ExecuteScalarAsync<int>(sql, client);
}
```
- Dapper maps client properties to SQL parameters
- SQLite inserts row
- Returns new ClientID (e.g., 1, 2, 3...)

8. **ViewModel adds to ObservableCollection**
```csharp
Clients.Add(SelectedClient);
```
- ObservableCollection fires `CollectionChanged` event
- DataGrid automatically adds new row
- User sees new client in list immediately

---

## Summary

**Key Concepts:**

1. **Repository Pattern** - Separates data access from business logic
2. **Async/Await** - Non-blocking database operations
3. **ObservableCollection** - Auto-updating UI collection
4. **Data Binding** - XAML connects to ViewModel properties
5. **ICommand** - Button clicks execute ViewModel methods
6. **Dapper ORM** - Automatic mapping between SQL and C# objects

**Data Flow:**
```
User Interaction → Command → ViewModel Method → Repository → 
Database (SQLite) → Repository → ViewModel → ObservableCollection → 
Data Binding → UI Update
```

This architecture ensures:
- ✅ Separation of concerns
- ✅ Testable code
- ✅ Responsive UI (async operations)
- ✅ Automatic UI updates
- ✅ Type-safe database access
