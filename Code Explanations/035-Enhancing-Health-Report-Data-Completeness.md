# 035 - Enhancing Health Report Data Completeness

Hello! In this lesson, we will explore how we ensured the patient's Health Report is 100% complete by pulling in data from every clinical section.

## Table of Contents
1. [Introduction](#introduction)
2. [Data Schema Expansion](#data-schema-expansion)
3. [The Repository Layer](#the-repository-layer)
4. [PDF Service Logic](#pdf-service-logic)
5. [The UI Trigger](#the-ui-trigger)

---

## 1. Introduction
A health report is only useful if it's complete. We previously had some fields missing, such as **Body Systems Overview**, detailed **Eye Analysis**, and specific **Anthropometrics** (like Zinc it NOX status). We updated the code to bridge these gaps.

---

## 2. Data Schema Expansion
To include more data, we had to look at our data models.

### BodySystemsOverview.cs
This class holds the assessment for various body systems.
```csharp
public class BodySystemsOverview
{
    public int ClientID { get; set; }
    public string Immune { get; set; } = string.Empty;
    public string Sleep { get; set; } = string.Empty;
    // ... and 19 other systems
}
```
* **What it means**: Each property represents a specific system being analyzed. By having this model, we can pass a single object to the PDF generator instead of 21 separate strings.

---

## 3. The Repository Layer
Before we can print the data, we must fetch it from the SQLite database.

### BodySystemsOverviewRepository.cs
We added a specific method to fetch data by the Client's ID:
```csharp
public async Task<BodySystemsOverview?> GetByClientIdAsync(int clientId)
{
    using var connection = _dbConnection.GetConnection();
    return await connection.QueryFirstOrDefaultAsync<BodySystemsOverview>(
        "SELECT * FROM Body_Systems_Overview WHERE ClientID = @clientId", 
        new { clientId });
}
```
* **Code Breakdown**:
    * `using var connection`: Ensures the database connection is closed automatically after use.
    * `@clientId`: A parameter that protects our database from "SQL Injection" attacks.
    * `QueryFirstOrDefaultAsync`: Dapper method that runs the SQL and maps the result directly into our `BodySystemsOverview` object.

---

## 4. PDF Service Logic
The `PdfService.cs` uses the **QuestPDF** library to build the document.

### Composing the Body Systems Section
```csharp
private void ComposeBodySystems(IContainer container, IEnumerable<BodySystemsOverview> list)
{
    container.Table(table => {
        table.ColumnsDefinition(columns => {
            columns.RelativeColumn(); // System Name
            columns.RelativeColumn(); // Assessment
        });
        
        foreach (var item in list) {
            table.Cell().Text("Immune System");
            table.Cell().Text(item.Immune);
            // ... repeats for other systems
        }
    });
}
```
* **Explanation**: 
    * `IContainer`: The "box" in the PDF where the table will live.
    * `table.ColumnsDefinition`: We define two equal-width columns.
    * `foreach`: We iterate through any records found for that client to fill the table rows.

---

## 5. The UI Trigger
Finally, the user needs a way to include this in the report.

### Reports.xaml
```xml
<CheckBox Content="Body Systems Overview" 
          IsChecked="{Binding ReportOptions.IncludeBodySystems}" 
          Style="{StaticResource ReportsCheckBoxStyle}"/>
```
* **Binding**: The `IsChecked` property is "two-way" bound to `ReportOptions`. When you check this box, the `IncludeBodySystems` boolean becomes `true`, and the `PdfService` will now execute the layout logic we wrote in step 4.
