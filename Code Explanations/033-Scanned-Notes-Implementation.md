# Scanned Notes Feature - Code Explanation

This document provides a detailed explanation of all the code created for the Scanned Notes feature, which allows users to view and manage scanned documents (PDFs and images) associated with clients.

---

## Table of Contents

1. [ScannedNote Model](#1-scannednote-model)
2. [ScannedNotesRepository](#2-scannednotesrepository)
3. [ByteArrayToImageConverter](#3-bytearraytoimageconverter)
4. [ScannedNotesVM (ViewModel)](#4-scannednotesvm-viewmodel)
5. [ScannedNotes View (XAML)](#5-scannednotes-view-xaml)
6. [Navigation Integration](#6-navigation-integration)

---

## 1. ScannedNote Model

**File:** `Models/ScannedNote.cs`

The model class represents a row in the `Scanned_Notes` database table.

```csharp
using System;

namespace Client_Management_System_V4.Models
{
    /// <summary>
    /// Represents a scanned document/note associated with a client.
    /// Stores PDF files or images as binary data (BLOB).
    /// </summary>
    public class ScannedNote
    {
        /// <summary>
        /// Primary key - auto-incremented ID
        /// </summary>
        public int ScannedNotesID { get; set; }

        /// <summary>
        /// Name/title of the document
        /// </summary>
        public string? Document_Name { get; set; }

        /// <summary>
        /// Date the document was created or scanned
        /// </summary>
        public DateTime? Document_Date { get; set; }

        /// <summary>
        /// Type of document: "PDF", "PNG", "JPG", "JPEG", etc.
        /// </summary>
        public string? Document_Type { get; set; }

        /// <summary>
        /// Description or notes about the document
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Binary data of the scanned document (BLOB)
        /// </summary>
        public byte[]? Scanned_Document { get; set; }

        /// <summary>
        /// Foreign key to Client table
        /// </summary>
        public int ClientID { get; set; }

        #region Display Helpers

        /// <summary>
        /// Returns true if the document is a PDF file
        /// </summary>
        public bool IsPdf => Document_Type?.ToUpperInvariant() == "PDF";

        /// <summary>
        /// Returns true if the document is an image file (not PDF)
        /// </summary>
        public bool IsImage => !IsPdf && !string.IsNullOrEmpty(Document_Type);

        /// <summary>
        /// Display string combining name and date
        /// </summary>
        public string DisplayName => $"{Document_Name ?? "Untitled"} ({Document_Date?.ToString("dd/MM/yyyy") ?? "No date"})";

        #endregion
    }
}
```

### Explanation

| Property | Type | Purpose |
|----------|------|---------|
| `ScannedNotesID` | `int` | Primary key matching the database `ScannedNotesID` column |
| `Document_Name` | `string?` | User-friendly name for the document |
| `Document_Date` | `DateTime?` | When the document was created/scanned |
| `Document_Type` | `string?` | File extension: "PDF", "PNG", "JPG", etc. |
| `Description` | `string?` | Notes about the document content |
| `Scanned_Document` | `byte[]?` | The actual file data as binary (BLOB) |
| `ClientID` | `int` | Foreign key linking to the Client |

### Computed Properties

- **`IsPdf`**: Uses `ToUpperInvariant()` for case-insensitive comparison to check if the document is a PDF
- **`IsImage`**: Returns true if it's NOT a PDF and has a document type (meaning it's an image)
- **`DisplayName`**: Combines name and date for display in lists, with fallbacks for null values

---

## 2. ScannedNotesRepository

**File:** `Repositories/ScannedNotesRepository.cs`

The repository handles all database operations using Dapper ORM.

```csharp
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using Client_Management_System_V4.Data;
using Client_Management_System_V4.Models;
using Dapper;

namespace Client_Management_System_V4.Repositories
{
    public class ScannedNotesRepository : IRepository<ScannedNote>
    {
        private readonly string _connectionString;

        public ScannedNotesRepository()
        {
            _connectionString = DatabaseManager.ConnectionString;
        }
```

### Key Methods Explained

#### GetByClientIdAsync

```csharp
public async Task<IEnumerable<ScannedNote>> GetByClientIdAsync(int clientId)
{
    using var connection = new SQLiteConnection(_connectionString);
    const string sql = @"
        SELECT ScannedNotesID, Document_Name, Document_Date, Document_Type, 
               Description, Scanned_Document, ClientID 
        FROM Scanned_Notes 
        WHERE ClientID = @ClientId 
        ORDER BY Document_Date DESC";
    return await connection.QueryAsync<ScannedNote>(sql, new { ClientId = clientId });
}
```

- **Purpose**: Retrieves all scanned notes for a specific client
- **`using var connection`**: Ensures the connection is disposed after use
- **`QueryAsync<ScannedNote>`**: Dapper maps SQL results directly to `ScannedNote` objects
- **`ORDER BY Document_Date DESC`**: Most recent documents appear first

#### AddAsync

```csharp
public async Task<int> AddAsync(ScannedNote note)
{
    using var connection = new SQLiteConnection(_connectionString);
    const string sql = @"
        INSERT INTO Scanned_Notes (
            Document_Name, Document_Date, Document_Type, 
            Description, Scanned_Document, ClientID
        )
        VALUES (
            @Document_Name, @Document_Date, @Document_Type, 
            @Description, @Scanned_Document, @ClientID
        );
        SELECT last_insert_rowid();";
    
    return await connection.ExecuteScalarAsync<int>(sql, note);
}
```

- **Purpose**: Inserts a new scanned note and returns the new ID
- **`@Document_Name, @Document_Date...`**: Dapper automatically maps properties from the `note` object
- **`last_insert_rowid()`**: SQLite function to get the auto-generated ID

---

## 3. ByteArrayToImageConverter

**File:** `Utilities/ByteArrayToImageConverter.cs`

This converter transforms `byte[]` data into a WPF `BitmapImage` for display.

```csharp
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Client_Management_System_V4.Utilities
{
    public class ByteArrayToImageConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If the value is null or not a byte array, return null
            if (value is not byte[] bytes || bytes.Length == 0)
            {
                return null;
            }

            try
            {
                // Create a BitmapImage from the byte array
                var image = new BitmapImage();
                using (var stream = new MemoryStream(bytes))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze(); // Freeze for cross-thread access
                }
                return image;
            }
            catch
            {
                // If image creation fails (e.g., PDF data), return null
                return null;
            }
        }
```

### Key Concepts

| Code | Explanation |
|------|-------------|
| `IValueConverter` | WPF interface for data binding value conversion |
| `value is not byte[] bytes` | Pattern matching: checks type AND extracts value |
| `MemoryStream(bytes)` | Wraps byte array as a stream for BitmapImage |
| `BeginInit()/EndInit()` | Required bracketing for BitmapImage initialization |
| `CacheOption = BitmapCacheOption.OnLoad` | Loads image data into memory immediately |
| `image.Freeze()` | Makes the image immutable for cross-thread safety |

---

## 4. ScannedNotesVM (ViewModel)

**File:** `ViewModel/ScannedNotesVM.cs`

The ViewModel contains all the logic for the Scanned Notes view.

### Properties

```csharp
public class ScannedNotesVM : ViewModelBase
{
    private readonly ScannedNotesRepository _repository;
    private readonly ClientRepository _clientRepository;
    
    private ObservableCollection<Client> _clients = new();
    private ObservableCollection<ScannedNote> _scannedNotes = new();
    private Client? _selectedClient;
    private ScannedNote? _selectedNote;
    private BitmapImage? _currentImage;
    private string? _tempPdfPath;
```

| Property | Purpose |
|----------|---------|
| `_clients` | List of all clients for the dropdown |
| `_scannedNotes` | Documents for the currently selected client |
| `_selectedClient` | The client selected in the dropdown |
| `_selectedNote` | The document currently being viewed |
| `_currentImage` | BitmapImage for image documents |
| `_tempPdfPath` | Path to temporary PDF file for WebView2 |

### Document Loading Logic

```csharp
private void LoadDocumentForDisplay()
{
    // Clear previous
    CurrentImage = null;
    CleanupTempPdf();

    if (SelectedNote?.Scanned_Document == null || SelectedNote.Scanned_Document.Length == 0)
    {
        return;
    }

    if (SelectedNote.IsImage)
    {
        // Load as image using MemoryStream
        var image = new BitmapImage();
        using (var stream = new MemoryStream(SelectedNote.Scanned_Document))
        {
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
        }
        CurrentImage = image;
    }
    else if (SelectedNote.IsPdf)
    {
        // Save to temp file for WebView2
        var tempFolder = Path.Combine(Path.GetTempPath(), "ClientManagement");
        Directory.CreateDirectory(tempFolder);
        _tempPdfPath = Path.Combine(tempFolder, $"temp_{SelectedNote.ScannedNotesID}.pdf");
        File.WriteAllBytes(_tempPdfPath, SelectedNote.Scanned_Document);
        TempPdfPath = _tempPdfPath;
    }
}
```

### Key Concepts

1. **For Images**: Convert `byte[]` directly to `BitmapImage` in memory
2. **For PDFs**: Write to temp file because WebView2 can only navigate to file paths/URLs
3. **CleanupTempPdf()**: Deletes old temp files to prevent buildup

### File Upload Logic

```csharp
private void UploadDocument()
{
    var dialog = new OpenFileDialog
    {
        Title = "Select Document to Upload",
        Filter = "Supported Files (*.pdf;*.png;*.jpg;*.jpeg)|*.pdf;*.png;*.jpg;*.jpeg|" +
                 "PDF Files (*.pdf)|*.pdf|" +
                 "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|" +
                 "All Files (*.*)|*.*"
    };

    if (dialog.ShowDialog() == true)
    {
        var extension = Path.GetExtension(dialog.FileName).TrimStart('.').ToUpperInvariant();
        var fileData = File.ReadAllBytes(dialog.FileName);

        var newNote = new ScannedNote
        {
            Document_Name = Path.GetFileNameWithoutExtension(dialog.FileName),
            Document_Date = DateTime.Today,
            Document_Type = extension,
            Scanned_Document = fileData,
            ClientID = SelectedClient.ClientID
        };

        ScannedNotes.Add(newNote);
        SelectedNote = newNote;
        IsEditMode = true;
    }
}
```

| Code | Explanation |
|------|-------------|
| `OpenFileDialog` | Windows file picker dialog |
| `Filter` | Limits visible files to supported types |
| `Path.GetExtension()` | Extracts ".pdf" from filename |
| `TrimStart('.')` | Removes leading dot to get "PDF" |
| `File.ReadAllBytes()` | Reads entire file into `byte[]` |

---

## 5. ScannedNotes View (XAML)

**File:** `View/ScannedNotes.xaml`

### WebView2 PDF Viewer

```xml
<!-- PDF Viewer using WebView2 -->
<Border Visibility="{Binding IsPdfDocument, Converter={StaticResource BoolToVis}}"
        Background="#1a1a1a" CornerRadius="4">
    <wv2:WebView2 x:Name="PdfWebView"
                  Source="{Binding TempPdfPath}"/>
</Border>
```

| Attribute | Explanation |
|-----------|-------------|
| `Visibility="{Binding IsPdfDocument...}"` | Only shows when a PDF is selected |
| `wv2:WebView2` | Microsoft's WebView2 control (based on Edge/Chromium) |
| `Source="{Binding TempPdfPath}"` | Navigates to the temp file path |

### Image Viewer

```xml
<!-- Image Viewer (for image documents) -->
<Border Visibility="{Binding IsImageDocument, Converter={StaticResource BoolToVis}}"
        Background="#1a1a1a" CornerRadius="4">
    <Image Source="{Binding CurrentImage}" 
           Stretch="Uniform" 
           RenderOptions.BitmapScalingMode="HighQuality"/>
</Border>
```

| Attribute | Explanation |
|-----------|-------------|
| `Source="{Binding CurrentImage}"` | Binds to the BitmapImage from ViewModel |
| `Stretch="Uniform"` | Scales proportionally to fit container |
| `RenderOptions.BitmapScalingMode="HighQuality"` | Uses high-quality image scaling |

### Document Navigation Bar

```xml
<StackPanel Orientation="Horizontal" VerticalAlignment="Center">
    <Button Content="◀ Prev" 
            Command="{Binding PreviousDocumentCommand}"/>
    <TextBlock Text="{Binding NavigationDisplay}" 
               Foreground="#DBDBDB" FontWeight="SemiBold"
               VerticalAlignment="Center" Margin="15,0"/>
    <Button Content="Next ▶" 
            Command="{Binding NextDocumentCommand}"/>
</StackPanel>
```

- **Previous/Next buttons**: Bound to commands that change `SelectedNote`
- **NavigationDisplay**: Shows "X of Y" format from a computed property

---

## 6. Navigation Integration

### NavigationVM.cs Changes

```csharp
// Added property
public ICommand ScannedNotesCommand { get; }

// Added in constructor
ScannedNotesCommand = new RelayCommand(_ => SafeNavigate(new ScannedNotesVM()));
```

### MainWindow.xaml Changes

```xml
<!--// Scanned Notes //-->
<Menu:Btn Style="{StaticResource BtnStyle}"
          Command="{Binding ScannedNotesCommand}">
    <Grid>
        <Image Source="Images/icons8-file-prescription-96.png"
               Style="{StaticResource Image_Style}" />
        <TextBlock Text="Scanned Notes"
                   Style="{StaticResource Text_Style}" />
    </Grid>
</Menu:Btn>
```

### DataTemplate.xaml Changes

```xml
<DataTemplate DataType="{x:Type vm:ScannedNotesVM}">
    <view:ScannedNotes />
</DataTemplate>
```

This DataTemplate tells WPF: "When the `CurrentView` property contains a `ScannedNotesVM` object, render it using the `ScannedNotes` view."

---

## 7. Document Viewer Overlay

The document viewer uses a full-screen overlay with fade animation.

> [!WARNING]
> **WebView2 does NOT work well with `TranslateTransform` animations!**  
> If you use the `Drawer_Style` (slide-in animation) with WebView2, the PDF will only partially render until mouse interaction occurs. The fix is to use only `Overlay_Grid_Style` (fade effect) without any translate transforms.

### XAML Overlay Structure

```xml
<!-- Overlay uses Panel.ZIndex to appear above other content -->
<Grid Grid.Row="0" Grid.RowSpan="4" Panel.ZIndex="100" 
      Tag="{Binding IsViewerOverlayOpen}"
      Style="{StaticResource Overlay_Grid_Style}">
    
    <!-- NO Drawer_Style here - it breaks WebView2 rendering! -->
    <Border HorizontalAlignment="Stretch" 
            Background="#212529" 
            CornerRadius="8">
        <!-- WebView2 content -->
    </Border>
</Grid>
```

| Element | Purpose |
|---------|---------|
| `Panel.ZIndex="100"` | Ensures overlay appears above all other content |
| `Tag="{Binding IsViewerOverlayOpen}"` | Passes boolean to style for animation |
| `Overlay_Grid_Style` | Provides fade-in/out + visibility toggle |
| **NO** `Drawer_Style` | Omitted because TranslateTransform breaks WebView2 |

```csharp
// Property to control overlay visibility
private bool _isViewerOverlayOpen;
public bool IsViewerOverlayOpen
{
    get => _isViewerOverlayOpen;
    set { _isViewerOverlayOpen = value; OnPropertyChanged(); }
}

// Commands
public ICommand OpenViewerOverlayCommand { get; }
public ICommand CloseViewerOverlayCommand { get; }

// Methods
private void OpenViewerOverlay()
{
    if (SelectedNote != null)
    {
        IsViewerOverlayOpen = true;
    }
}

private void CloseViewerOverlay()
{
    IsViewerOverlayOpen = false;
}
```

### XAML Overlay Structure

```xml
<!-- Overlay uses Panel.ZIndex to appear above other content -->
<Grid Grid.Row="0" Grid.RowSpan="4" Panel.ZIndex="100" 
      Tag="{Binding IsViewerOverlayOpen}"
      Style="{StaticResource Overlay_Grid_Style}">
    
    <Border Tag="{Binding IsViewerOverlayOpen}" 
            Style="{StaticResource Drawer_Style}" 
            HorizontalAlignment="Stretch">
        <!-- Full document viewer content here -->
    </Border>
</Grid>
```

| Element | Purpose |
|---------|---------|
| `Panel.ZIndex="100"` | Ensures overlay appears above all other content |
| `Tag="{Binding IsViewerOverlayOpen}"` | Passes boolean to style for animation |
| `Overlay_Grid_Style` | Predefined style that shows/hides based on Tag |
| `Drawer_Style` | Provides slide-in animation for the overlay |

---

## Summary

The Scanned Notes feature follows the existing MVVM pattern:

1. **Model** (`ScannedNote.cs`) - Mirrors database structure
2. **Repository** (`ScannedNotesRepository.cs`) - Handles data access with Dapper
3. **ViewModel** (`ScannedNotesVM.cs`) - Contains all business logic
4. **View** (`ScannedNotes.xaml`) - Presentation with WebView2 for PDFs
5. **Navigation** - Integrated via NavigationVM and DataTemplate
