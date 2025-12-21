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
            {
                WindowState = WindowState.Normal;
                CenterWindowOnScreen();
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        /// <summary>
        /// Prevents mouse clicks in the content area from bubbling up to the window dragging logic.
        /// </summary>
        private void ContentArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Centers the window on the primary screen's work area.
        /// </summary>
        private void CenterWindowOnScreen()
        {
            this.Width = 950;
            this.Height = 650;

            var workArea = SystemParameters.WorkArea;
            this.Left = (workArea.Width - this.Width) / 2 + workArea.Left;
            this.Top = (workArea.Height - this.Height) / 2 + workArea.Top;
        }

        /// <summary>
        /// Allows dragging the window by clicking and holding on empty areas.
        /// Also handles double-click to maximize/restore.
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var mousePos = e.GetPosition(this);
                
                // Draggable areas:
                // 1. Sidebar (X < 250)
                // 2. Title Bar area (Y < 60)
                bool isSidebar = mousePos.X < 250;
                bool isHeader = mousePos.Y < 60;

                if (!isSidebar && !isHeader) return;

                // Double-click toggles maximize/centered-restore
                if (e.ClickCount == 2)
                {
                    MaximizeApp_Click(sender, e);
                }
                else
                {
                    // If maximized, restore to center before starting drag
                    if (WindowState == WindowState.Maximized)
                    {
                        WindowState = WindowState.Normal;
                        CenterWindowOnScreen();
                    }
                    
                    try
                    {
                        DragMove();
                    }
                    catch (Exception)
                    {
                        // Ignore if drag move cannot be initiated
                    }
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