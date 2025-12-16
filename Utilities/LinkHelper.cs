using System;
using System.Diagnostics;
using System.Windows;
using System.Runtime.InteropServices;

namespace Client_Management_System_V4.Utilities
{
    public static class LinkHelper
    {
        public static void OpenLink(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            try
            {
                // Hack for .NET Core to use default browser
                // https://github.com/dotnet/runtime/issues/17938
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void OpenEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return;
            OpenLink($"mailto:{email}");
        }
    }
}
