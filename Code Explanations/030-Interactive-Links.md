# Interactive Links (Email & Website)

## Overview
We've added functionality to open the default email client or web browser when interacting with specific fields in the application.

## 1. Helper Class (`LinkHelper.cs`)
A new utility class `LinkHelper` was created to handle the `Process.Start` logic safely. It handles the `UseShellExecute=true` requirement for .NET Core apps to open the default browser.

```csharp
public static void OpenLink(string url)
{
    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
}

public static void OpenEmail(string email)
{
    OpenLink($"mailto:{email}");
}
```

## 2. Clients View
### DataGrid
The Email column was converted from a simple text column to a `DataGridTemplateColumn` containing a `Hyperlink`.
*   **Interaction**: Click the link in the grid cell.
*   **Behavior**: Opens default mail app (e.g., Outlook) with the `To:` field pre-filled.

### properties Edit Form
Added a small "Email" icon button (📧) next to the Email TextBox.
*   **Interaction**: Click the icon.
*   **Behavior**: Opens default mail app.

## 3. Distributor View
### Edit Overlay
Added icon buttons for both Email and Website fields.
*   **Email**: 📧 icon opens mailto link.
*   **Website**: 🌐 icon opens the URL in the default browser (Chrome/Edge/etc.).

## How it works
The buttons invoke code-behind methods (`OpenEmail_Click`, `OpenWebsite_Click`) which then call the static `LinkHelper` methods.
