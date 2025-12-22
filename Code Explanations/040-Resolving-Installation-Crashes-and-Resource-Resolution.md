# Resolving Installation Crashes and Resource Resolution

| Concept | Description |
| :--- | :--- |
| **Problem** | Application startup failure (`StaticResourceExtension` exception) after installation in `Program Files`. |
| **Root Cause 1** | Path resolution ambiguity in single-file published EXE for relative resource Uris. |
| **Root Cause 2** | Read-only permissions in `Program Files` preventing SQLite database modification. |
| **Solution 1** | Absolute **Pack URIs** for all resource dictionaries and assets. |
| **Solution 2** | Redirecting database storage to `%LocalAppData%`. |

## Table of Contents
1. [Absolute Pack URIs for Resources](#absolute-pack-uris-for-resources)
2. [AppData Redirection for SQLite](#appdata-redirection-for-sqlite)
3. [Cleaner App Startup & Diagnostics](#cleaner-app-startup--diagnostics)

---

## Absolute Pack URIs for Resources

In standard WPF development, relative paths like `Source="Styles/Button.xaml"` usually work. However, when an application is published as a **Single-File EXE**, the internal structure of the assembly can become complex. The WPF resource engine sometimes fails to resolve relative paths correctly in this "pre-jitted" or "self-contained" state.

### Implementation in App.xaml
We updated [App.xaml](file:///c:/Users/admin/source/repos/Client-Management-System_V4/App.xaml) to use the formal **Pack URI** syntax.

```xml
<ResourceDictionary.MergedDictionaries>
    <!-- Use absolute pack URIs for maximum reliability in single-file published apps -->
    <ResourceDictionary Source="pack://application:,,,/Styles/GlobalConverters.xaml"/>
    <ResourceDictionary Source="pack://application:,,,/Styles/Button.xaml"/>
    <ResourceDictionary Source="pack://application:,,,/Styles/Text.xaml"/>
    <!-- ... other dictionaries ... -->
</ResourceDictionary.MergedDictionaries>
```

**What it means:**
- `pack://application:,,,/`: This tells WPF to look specifically inside the application's own compiled resources (the assembly manifest).
- `/Styles/Button.xaml`: The absolute path within the project.

### Implementation in Styles
We applied the same logic to images and fonts in [Button.xaml](file:///c:/Users/admin/source/repos/Client-Management-System_V4/Styles/Button.xaml) and [Text.xaml](file:///c:/Users/admin/source/repos/Client-Management-System_V4/Styles/Text.xaml).

```xml
<!-- In Button.xaml -->
<ImageBrush x:Key="close_app_def"
            ImageSource="pack://application:,,,/Images/shutdown_def.png"
            Stretch="None" />

<!-- In Text.xaml -->
<Setter Property="FontFamily"
        Value="pack://application:,,,/Fonts/#Rubik" />
```

---

## AppData Redirection for SQLite

When an application is installed in `C:\Program Files`, it runs in a highly restricted security context. Standard users do not have permission to write to this folder. Since SQLite needs to modify its `.db` file, the application would crash silently if it tried to access a database in the installation folder.

### Implementation in DatabaseManager.cs
We updated the connection string to use `Environment.SpecialFolder.LocalApplicationData`.

```csharp
// Inside DatabaseManager.cs
private static readonly string DbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
    "ClientManagementSystemV4", 
    "client_management.db");
```

**Why this works:**
- Windows guarantees that every user has full write access to their own `AppData\Local` folder.
- This ensures the database is "portable" across different windows installations and survives app updates without permission issues.

---

## Cleaner App Startup & Diagnostics

To ensure a smooth launch, we removed the "Smoke Test" message boxes once we verified the fix. We also kept a robust global exception handler in [App.xaml.cs](file:///c:/Users/admin/source/repos/Client-Management-System_V4/App.xaml.cs) to catch any future issues.

```csharp
private void LogFatalError(Exception? ex, string context)
{
    // ...
    // Mirror to desktop
    var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    File.WriteAllText(Path.Combine(desktopPath, "CMS_CRITICAL_ERROR.txt"), message);
}
```

**Key Benefit:**
- If the app ever fails again, the user will find a `CMS_CRITICAL_ERROR.txt` file on their desktop, making debugging trivial for us!
