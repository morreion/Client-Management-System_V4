# Inventory & Distributor Reporting Implementation

This document explains the implementation of two new PDF reports: **Supplements Inventory** and **Distributor List**.

## 1. Overview
The goal was to provide printable PDF reports for the entire Supplements inventory and the list of Distributors. Unlike the Contact List, these reports include all columns by default and do not require user selection.

## 2. PdfService Updates
We extended the `PdfService` class to include two new methods:

### `GenerateSupplementsReportAsync`
*   **Input**: `IEnumerable<Supplement>`
*   **Layout**: Landscape A4.
*   **Columns**: Name, Type, Distributor, Description, Usage.
*   **Logic**: Iterates through the collection and renders a table row for each supplement.

### `GenerateDistributorReportAsync`
*   **Input**: `IEnumerable<Distributor>`
*   **Layout**: Landscape A4.
*   **Columns**: Name, Email, Mobile, Work Phone, Website, Address.
*   **Logic**: Iterates through the collection and renders a table row for each distributor.

## 3. ViewModel Logic (`ReportsVM`)
*   **Repositories**: Injected `SupplementRepository` and `DistributorRepository` to fetch the data.
*   **Commands**:
    *   `GenerateSupplementsReportCommand`: Fetches all supplements and calls the service.
    *   `GenerateDistributorReportCommand`: Fetches all distributors and calls the service.
*   **File Handling**: Uses `SaveFileDialog` to let the user choose where to save the PDF, and automatically opens it after generation.

## 4. UI Updates (`Reports.xaml`)
A new section **"Inventory & Distributors"** was added to the Reports view.
*   Contains two buttons styled with distinct colors (Green for Supplements, Indigo for Distributors) to differentiate them from Client reports.
*   Each button binds to its respective command in the ViewModel.

## 5. Key Code Snippets

### PdfService Table Generation (Supplements)
```csharp
table.ColumnsDefinition(columns =>
{
    columns.RelativeColumn(2); // Name
    columns.RelativeColumn(2); // Type
    columns.RelativeColumn(2); // Distributor
    columns.RelativeColumn(3); // Description
    columns.RelativeColumn(3); // Usage
});

foreach (var item in supplements)
{
    table.Cell().Text(item.Name ?? "-");
    table.Cell().Text(item.Type ?? "-");
    // ... other cells
}
```

### ReportsVM Command Handler
```csharp
private async Task GenerateSupplementsReport()
{
    var saveFileDialog = new SaveFileDialog { Filter = "PDF Details|*.pdf" };
    if (saveFileDialog.ShowDialog() == true)
    {
        var supplements = await _supplementRepository.GetAllAsync();
        await _pdfService.GenerateSupplementsReportAsync(supplements, saveFileDialog.FileName);
        OpenPdf(saveFileDialog.FileName);
    }
}
```
