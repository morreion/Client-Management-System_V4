# Interactive Links & Fixes

## 1. Crash Fix: Clients DataGrid
**Problem**: The `Hyperlink` in the Clients DataGrid was crashing the application when clicked.
**Cause**: The `RequestNavigate` event with a `NavigateUri` binding can cause exceptions if the URI isn't perfectly formatted (e.g. missing `mailto:`).
**Fix**:
*   Removed `NavigateUri` binding in XAML.
*   Switched to `Click` event handler (`Hyperlink_Click`).
*   In code-behind, we retrieve the `Client` object from the `DataContext` and safely pass the `Email` string to `LinkHelper`.

```csharp
private void Hyperlink_Click(object sender, RoutedEventArgs e)
{
    if (sender is Hyperlink link && link.DataContext is Client client)
    {
        Utilities.LinkHelper.OpenEmail(client.Email);
    }
}
```

## 2. Supplements: Distributor Overlay
**Problem**: The **Distributor Manager** overlay inside the Supplements view was missing the "Email" and "Website" launch buttons.
**Fix**:
*   Added the 📧 (Email) and 🌐 (Website) buttons to the overlay form.
*   Wired them to `OpenEmail_Click` and `OpenWebsite_Click` events in `Supplements.xaml.cs`.

Now, managing distributors from the Supplements view provides the same functionality as the main Distributor view.
