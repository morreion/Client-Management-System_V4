# 036 - Implementing Live Client Search with Filters

Welcome! In this guide, we'll dive into the mechanics of "Live Search." This is when your list updates instantly as you type, without needing to click a "Search" button.

## Table of Contents
1. [The "Live" Trigger](#the-live-trigger)
2. [The Filtering Algorithm](#the-filtering-algorithm)
3. [MVVM Property Chaining](#mvvm-property-chaining)
4. [Responsive UI Layout](#responsive-ui-layout)

---

## 1. The "Live" Trigger
The secret to live search is in the `UpdateSourceTrigger` within your XAML.

### Clients.xaml
```xml
<TextBox Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}" 
         Style="{StaticResource SearchTextBoxStyle}"
         Tag="Search for name, email or mobile..."/>
```
* **How it works**: By default, a TextBox only updates the ViewModel when you "lose focus" (click away). By setting it to `PropertyChanged`, the `SearchText` property in our ViewModel is updated on **every single keystroke**.

---

## 2. The Filtering Algorithm
The filtering happens in two stages: **Database Search** followed by **In-Memory Filtering**.

### ClientVM.cs
```csharp
private async Task SearchClientsAsync()
{
    // Stage 1: Database Search
    IEnumerable<Client> results;
    if (string.IsNullOrWhiteSpace(SearchText))
        results = await _repository.GetAllAsync();
    else
        results = await _repository.SearchAsync(SearchText);

    // Stage 2: Memory-Based Filtering (LINQ)
    if (SelectedGenderFilter != "All Genders")
    {
        int targetGender = SelectedGenderFilter == "Male" ? 1 : 0;
        results = results.Where(c => c.Gender == targetGender);
    }

    if (FilterByAge)
    {
        results = results.Where(c => c.Age >= MinAge && c.Age <= MaxAge);
    }

    // Update the UI collection
    Clients = new ObservableCollection<Client>(results);
}
```
* **Why Stage 2?**: Some filters (like Age) are calculated properties in C# and aren't stored directly in the database columns. Filtering them in memory using **LINQ** (`.Where`) is faster and more flexible.

---

## 3. MVVM Property Chaining
When you change a filter (like checking a "Filter by Age" box), the list needs to refresh. We do this by calling the search method inside the property setter.

```csharp
public bool FilterByAge
{
    get => _filterByAge;
    set 
    { 
        _filterByAge = value; 
        OnPropertyChanged(); 
        _ = SearchClientsAsync(); // Trigger the refresh!
    }
}
```
* **What is `_ = ...`?**: This is called a "discard." It tells C# to start the asynchronous search task without waiting for it to finish right here. This prevents the UI from "freezing" while the search runs.

---

## 4. Responsive UI Layout
To make the search bar look premium, we used a `Grid` with flexible columns.

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>    <!-- Search Box (Flexible) -->
        <ColumnDefinition Width="160"/>  <!-- Gender Filter (Fixed) -->
        <ColumnDefinition Width="Auto"/> <!-- Age Filter (Fixed) -->
    </Grid.ColumnDefinitions>
</Grid>
```
* **Aesthetic Tip**: By using `Width="*"` for the search box, it will expand to fill all available space, ensuring the UI looks great on any screen size or resolution.
