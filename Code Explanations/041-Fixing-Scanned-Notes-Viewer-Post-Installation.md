# Fixing Scanned Notes Viewer Post-Installation

| Feature | Fix Implemented |
| :--- | :--- |
| **Document Viewer** | Explicitly initialized **WebView2** with a writable `UserDataFolder` in `AppData`. |
| **PDF Visibility** | Fixed **BoolToVisibilityConverter** to support the `Inverted` parameter. |
| **Temp Storage** | Redirected temporary PDF extraction to a dedicated folder in `AppData`. |

## Table of Contents
1. [WebView2 Initialization & Permissions](#webview2-initialization--permissions)
2. [Fixing the Visibility Converter](#fixing-the-visibility-converter)
3. [Safe Temp Document Storage](#safe-temp-document-storage)

---

## WebView2 Initialization & Permissions

By default, the WebView2 control tries to create a "User Data Folder" (to store cache, cookies, etc.) in the same directory as the executable. When the application is installed in `C:\Program Files`, it does not have write permissions to that folder, causing WebView2 to fail silently.

### The Fix in ScannedNotes.xaml.cs
We updated the [ScannedNotes](file:///c:/Users/admin/source/repos/Client-Management-System_V4/View/ScannedNotes.xaml.cs) view's code-behind to explicitly point this folder to a safe location in `AppData`.

```csharp
private async void ScannedNotes_Loaded(object sender, RoutedEventArgs e)
{
    // Define a writable path in LocalAppData
    var appDataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ClientManagementSystemV4",
        "WebView2_Data");
    
    Directory.CreateDirectory(appDataPath);
    
    // Create the environment with the custom data folder
    var env = await CoreWebView2Environment.CreateAsync(null, appDataPath);
    await PdfWebView.EnsureCoreWebView2Async(env);
}
```

---

## Fixing the Visibility Converter

In the `ScannedNotes` view, the "No Document Selected" message is supposed to disappear when a document is selected. This was handled using a `ConverterParameter="Inverted"`. However, our custom converter was ignoring this parameter, keeping the message visible at all times.

### The Fix in BoolToVisibilityConverter.cs
We updated the [BoolToVisibilityConverter](file:///c:/Users/admin/source/repos/Client-Management-System_V4/Utilities/BoolToVisibilityConverter.cs) to recognize and handle the `Inverted` instruction.

```csharp
public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
{
    if (value is bool boolValue)
    {
        string? paramStr = parameter as string;
        if (paramStr != null && paramStr.Equals("Inverted", StringComparison.OrdinalIgnoreCase))
        {
            // If Inverted is true, hide (Collapsed) when the value is true
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }
    return Visibility.Collapsed;
}
```

---

## Safe Temp Document Storage

Previously, PDFs were saved to a generic temp folder. For better security and reliability, we moved this to the application's dedicated data folder.

### The Fix in ScannedNotesVM.cs
Updated [ScannedNotesVM](file:///c:/Users/admin/source/repos/Client-Management-System_V4/ViewModel/ScannedNotesVM.cs) to use the app-specific `AppData` path for temporary document extraction.

```csharp
var appDataPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "ClientManagementSystemV4",
    "TempDocuments");
    
Directory.CreateDirectory(appDataPath);
_tempPdfPath = Path.Combine(appDataPath, $"temp_{SelectedNote.ScannedNotesID}.pdf");
```

**Why this matters:**
This ensures that the OS doesn't randomly delete our files while the viewer is open and guarantees the application always has full write access to the folder.
