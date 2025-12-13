# 025-Extended-Reporting.md

## Overview
This document explains the implementation of extended reporting features, including **Prescription Reports** and **Client Contact Lists**.

## 1. PdfService Updates (`PdfService.cs`)
We updated the `PdfService` to support new report types.

### Prescription Report
Added `GeneratePrescriptionReportAsync` to create a dedicated PDF for a client's prescriptions.
- **Data Fetching**: Retrieves prescriptions (`_prescriptionRepo.GetByClientIdAsync`) and then fetches supplements for each (`GetSupplementsByPrescriptionIdAsync`).
- **Composition**: Displays each prescription with its date, recommendations, and a nested table of supplements (Breakfast, Lunch, Dinner, Bedtime).

### Contact List Report
Added `GenerateContactListReportAsync` to create a list of all clients.
- **Features**: Supports dynamic column selection. Use a `switch` expression to render only the columns selected by the user.
- **Dynamic Table**: The QuestPDF table definition iterates over the `selectedColumns` list to create headers and cells dynamically.

## 2. ViewModel Updates (`ReportsVM.cs`)
We extended the `ReportsVM` to handle the new user interactions.

- **Commands**: 
    - `GeneratePrescriptionReportCommand`: Calls the new service method for the selected client.
    - `GenerateContactListCommand`: Collects selected columns and calls the service for all clients.
- **Properties**: Added boolean properties (`IncludeName`, `IncludeMobile`, etc.) bound to checkboxes in the UI to let the user customize the Contact List.

## 3. UI Refactoring (`Reports.xaml`)
The `Reports` view was redesigned to accommodate the new features.

- **Dual-Column Layout**: 
    - **Left Column**: "Individual Client Reports". Contains Client Selector and the Health Report configuration.
    - **Right Column**: "General Reports". Contains the Contact List configuration.
- **Reporting Options**: Added a "Prescriptions" checkbox to the Health Report sections, allowing it to be included/excluded from the main report.
- **Styles**: Reused `ReportsCheckBoxStyle` and `MyCustomComboBoxStyle` for consistency.

## Usage
1.  **Prescription Report**: Select a client -> Click "Generate Prescription Report".
2.  **Contact List**: Check desired columns (e.g., Name, Mobile) -> Click "Generate Contact List".
