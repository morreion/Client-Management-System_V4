# Enhancing Health Report Data Completeness

This document explains the technical implementation of the enhancements made to the Health Report generation system. These changes ensure that all clinical data points, from medical history to advanced eye analysis, are accurately captured in the final PDF report.

## Table of Contents
1. [Data Model Updates](#data-model-updates)
2. [Repository Enhancements](#repository-enhancements)
3. [PdfService logic](#pdfservice-logic)
4. [User Interface Changes](#user-interface-changes)

---

## 1. Data Model Updates
To ensure completeness, we utilized several models that store critical patient data. 

### BodySystemsOverview.cs
This model captures a high-level view of various biological systems. 
- **Purpose**: Acts as a summary for systems like Immune, Allergy, Sleep, etc.
- **Fields**: Each property (e.g., `Immune`, `Sleep`) is a string representing the clinical assessment of that system.

### Anthropometrics.cs
This model was expanded in the report to include more specific metrics:
- `SpO2_Percent`: Oxygen saturation levels.
- `Zinc_Status`: Assessment of zinc levels.
- `NOX_Status`: Nitric Oxide assessment.

---

## 2. Repository Enhancements
To fetch the new "Body Systems Overview" data for a specific client, we added a retrieval method to the repository.

### BodySystemsOverviewRepository.cs
```csharp
public async Task<BodySystemsOverview?> GetByClientIdAsync(int clientId)
{
    using var connection = _dbConnection.GetConnection();
    return await connection.QueryFirstOrDefaultAsync<BodySystemsOverview>(
        "SELECT * FROM Body_Systems_Overview WHERE ClientID = @clientId", 
        new { clientId });
}
```
**Explanation**:
- `@clientId`: A parameter used to prevent SQL injection.
- `QueryFirstOrDefaultAsync`: Fetches the first record matching the client ID or returns null if none exists.

---

## 3. PdfService logic
The `PdfService.cs` is the "brain" of the report generation. It coordinates data fetching and layout composition.

### GenerateHealthReportAsync
We updated this method to include the new data sources:
```csharp
IEnumerable<BodySystemsOverview> bodySystemsList = options.IncludeBodySystems 
    ? await _bodySystemsRepo.GetByClientIdAsync(client.ClientID) 
    : Enumerable.Empty<BodySystemsOverview>();
```
**Explanation**:
- **Conditional Fetching**: Data is only retrieved from the database if the user has checked the corresponding option in the UI.
- `client.ClientID`: Ensures we only get data belonging to the specific patient.

### ComposeBodySystems (New Section)
A new method was created to draw the grid of body systems in the PDF:
```csharp
private void ComposeBodySystems(IContainer container, IEnumerable<BodySystemsOverview> list)
{
    container.Table(table => {
        // ... Table definition ...
        table.Cell().Text("System").Bold();
        table.Cell().Text("Status").Bold();
        // ... Drawing rows ...
    });
}
```
**Explanation**:
- **Structured Layout**: Uses a two-column table to list the system name and its associated evaluation.

---

## 4. User Interface Changes
The user selects which sections to include via the `Reports.xaml` view.

### Reports.xaml
```xml
<CheckBox Content="Body Systems Overview" 
          IsChecked="{Binding ReportOptions.IncludeBodySystems}" 
          Style="{StaticResource ReportsCheckBoxStyle}"/>
```
**Explanation**:
- `IsChecked`: Bound to a property in the `ReportOptions` model. When checked, the `PdfService` knows to include this section in the next generated report.
