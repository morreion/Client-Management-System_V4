# 023-PDF-Reporting-Implementation.md

## Overview
This document explains the implementation of the new Reporting feature, which allows users to generate professional PDF health reports for clients. We leveraged the **QuestPDF** library for its modern, fluent API.

## 1. Dependencies
- **Package**: `QuestPDF` (Version 2024.3.0 or similar)
- **License**: Community License (set in `PdfService` constructor).

## 2. Architecture
The reporting feature follows the MVVM pattern with a dedicated Service layer for PDF generation.

### A. Repositories (Data Layer)
We updated the following repositories to include a `GetByClientIdAsync(int clientId)` method, allowing efficient fetching of a specific client's records:
- `MedHxRepository`
- `AnthropometricsRepository`
- `DietRepository`
- `TreatmentRepository`
- `EyeAnalysisRepository`
- `PrescriptionRepository`

### B. Service Layer (`PdfService.cs`)
This service is responsible for fetching data and composing the PDF document.
- **Data Fetching**: It instantiates repositories and fetches data based on the `ReportOptions` selected by the user.
- **Composition**: uses `QuestPDF.Fluent` to build the document structure (Header, Content, Footer).
- **Patient Details**: Renders client info (Name, DOB, Contact, etc.).
- **Sections**: Renders tables and lists for Medical History, Diet, Treatment, etc., only if they contain data and are selected.

```csharp
// Example: Composing a Table for Anthropometrics
private void ComposeAnthropometrics(IContainer container, IEnumerable<Anthropometrics> list)
{
    container.Table(table =>
    {
        table.ColumnsDefinition(columns => { /* Define widths */ });
        table.Header(header => { /* Define Headers */ });
        
        foreach (var item in list)
        {
            table.Cell().Text(item.Weight.ToString());
            // ...
        }
    });
}
```

### C. ViewModel (`ReportsVM.cs`)
- **State**:
    - `SelectedClient`: The client to report on.
    - `Options`: An instance of `ReportOptions` (booleans for "Include Diet", "Include Treatment", etc.).
- **Commands**:
    - `GenerateReportCommand`: Opens a `SaveFileDialog` to choose the output location, then calls `PdfService.GenerateHealthReportAsync`.

### D. View (`Reports.xaml`)
- Provides a user-friendly interface:
    1.  **Select Client**: Using a searchable ComboBox.
    2.  **Customize Report**: Using CheckBoxes bound to `Options`.
    3.  **Generate**: A large button to trigger the creation.

## 3. Key Features
- **Null Safety**: The PDF generation handles missing data gracefully (e.g., nullable BMI, missing fields) by displaying "-" or "No records found".
- **Modularity**: The PDF content is broken down into small private methods (`ComposeHeader`, `ComposePatientDetails`, etc.) for maintainability.
- **Design**: Uses a clean, professional layout with a blue color scheme (`Colors.Blue.Medium`), styled fonts, and consistent padding.
