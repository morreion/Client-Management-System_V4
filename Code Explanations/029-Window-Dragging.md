# Enabling Window Dragging (Custom Chrome)

## The Issue
When you set `WindowStyle="None"` on a WPF Window to create a custom UI, you remove the standard operating system "chrome" (the title bar with close/minimize/maximize buttons and the draggable area). This causes the window to be stuck in place.

## The Solution
To restore the ability to move the window, we must manually handle mouse events and tell the Window to initiate a drag operation.

### 1. The Code Behind (`MainWindow.xaml.cs`)
We added a `Window_MouseDown` event handler. The critical method is `DragMove()`, which handles the complex logic of following the mouse cursor while moving the window.

```csharp
private void Window_MouseDown(object sender, MouseButtonEventArgs e)
{
    // Check if it's the Left Mouse Button (standard drag behavior)
    if (e.ChangedButton == MouseButton.Left)
    {
        // Built-in WPF method to handle window dragging
        this.DragMove();
    }
}
```

### 2. The XAML (`MainWindow.xaml`)
We connected this event handler to the `Window` itself. This ensures that clicking and dragging *anywhere* on the window (that isn't capturing the mouse, like a button) will trigger the move.

```xml
<Window ...
    WindowStyle="None"
    MouseDown="Window_MouseDown">
```

Now, your custom border and background act as the "handle" for moving the application.
