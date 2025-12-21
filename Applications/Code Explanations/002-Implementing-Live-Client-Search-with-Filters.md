# Implementing Live Client Search with Filters

This document details how we implemented the real-time (live) search functionality in the Clients view, including advanced filtering by Gender and Age.

## Table of Contents
1. [ViewModel Logic (ClientVM.cs)](#viewmodel-logic-clientvmcs)
2. [Data Filtering Strategy](#data-filtering-strategy)
3. [User Interface Design (Clients.xaml)](#user-interface-design-clientsxaml)
4. [Command Implementation](#command-implementation)

---

## 1. ViewModel Logic (ClientVM.cs)
The core of the live search lies in the `SearchText` property. Instead of waiting for a button click, we trigger the search every time the text changes.

```csharp
public string SearchText
{
    get => _searchText;
    set
    {
        if (_searchText != value)
        {
            _searchText = value;
            OnPropertyChanged();
            _ = SearchClientsAsync(); // Trigger live search
        }
    }
}
```
**Explanation**:
- `_ = SearchClientsAsync()`: This "fires and forgets" the asynchronous search task so the UI remains responsive while the database is queried.

---

## 2. Data Filtering Strategy
The `SearchClientsAsync` method handles both the database search and subsequent in-memory filtering.

```csharp
private async Task SearchClientsAsync()
{
    // 1. Fetch base data from database
    IEnumerable<Client> clients;
    if (string.IsNullOrWhiteSpace(SearchText))
        clients = await _repository.GetAllAsync();
    else
        clients = await _repository.SearchAsync(SearchText);

    // 2. Apply Gender Filter (In-Memory)
    if (SelectedGenderFilter == "Male")
        clients = clients.Where(c => c.Gender == 1);
    else if (SelectedGenderFilter == "Female")
        clients = clients.Where(c => c.Gender == 0);

    // 3. Apply Age Filter (In-Memory)
    if (FilterByAge)
    {
        clients = clients.Where(c => c.Age.HasValue && 
                               c.Age.Value >= MinAge && 
                               c.Age.Value <= MaxAge);
    }

    // 4. Update the UI Collection
    Clients = new ObservableCollection<Client>(clients);
}
```
**Explanation**:
- **Step 1**: Gets the starting list of clients based on the search string.
- **Step 2 & 3**: Filters the results using LINQ (`.Where`) based on the selected gender and age range.
- **Step 4**: Updates the `Clients` collection, which is bound to the DataGrid, making the results appear instantly.

---

## 3. User Interface Design (Clients.xaml)
The search bar area was redesigned to accommodate the new filter controls while maintaining a sleek look.

### Search Bar Structure
```xml
<Grid>
    <TextBox Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}" 
             Style="{StaticResource SearchTextBoxStyle}"
             Tag="Search for name, email or mobile..."/>
</Grid>
```
**Explanation**:
- `UpdateSourceTrigger=PropertyChanged`: Crucial for "live" search; it tells WPF to update the ViewModel property on every keystroke.

### Advanced Filters
A combination of a `ComboBox` for Gender and `TextBox`es for Age range allow for granular control.
- **Age Filter Visibility**: The age inputs are hidden unless the `FilterByAge` checkbox is checked, using the `BoolToVisibilityConverter`.

---

## 4. Command Implementation
We added a "Show All" command to provide a quick way to reset the search while keeping specific filters active.

```csharp
ShowAllCommand = new RelayCommand(async _ => 
{
    SearchText = string.Empty; // Clear text
    await SearchClientsAsync(); // Refresh list with current filters
});
```
**Explanation**:
- This command simplifies the user experience by allowing them to quickly go back to the full list (subject to active filters) with a single click.
