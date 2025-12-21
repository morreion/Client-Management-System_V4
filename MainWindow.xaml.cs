using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;

namespace Client_Management_System_V4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Closes the application
        /// </summary>
        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Minimizes the window to taskbar
        /// </summary>
        private void MinimizeApp_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Toggles between maximized and normal window state
        /// </summary>
        private void MaximizeApp_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// Allows dragging the window by clicking and holding on empty areas.
        /// Also handles double-click to maximize/restore.
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                // Double-click toggles maximize
                if (e.ClickCount == 2)
                {
                    MaximizeApp_Click(sender, e);
                }
                else
                {
                    // If maximized, restore before dragging
                    if (WindowState == WindowState.Maximized)
                    {
                        // Get mouse position relative to window and screen
                        var mousePos = e.GetPosition(this);
                        var screenPos = PointToScreen(mousePos);
                        var percentX = mousePos.X / ActualWidth;
                        
                        // Restore window
                        WindowState = WindowState.Normal;
                        
                        // Position window so mouse is at same relative X position
                        Left = screenPos.X - (Width * percentX);
                        Top = screenPos.Y - mousePos.Y;
                    }
                    
                    DragMove();
                }
            }
        }

        /// <summary>
        /// Handles window state changes to adjust corner radius
        /// </summary>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                // Remove corner radius when maximized (looks cleaner at screen edges)
                MainBorder.CornerRadius = new CornerRadius(0);
                NavBorder.CornerRadius = new CornerRadius(0);
                
                // Add padding to prevent content from going under taskbar
                BorderThickness = new Thickness(7);
            }
            else
            {
                // Restore corner radius when normal
                MainBorder.CornerRadius = new CornerRadius(20);
                NavBorder.CornerRadius = new CornerRadius(20, 0, 0, 20);
                BorderThickness = new Thickness(0);
            }
        }
    }
}