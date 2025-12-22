using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Client_Management_System_V4.ViewModel;

namespace Client_Management_System_V4.View
{
    /// <summary>
    /// Interaction logic for ScannedNotes.xaml
    /// View for managing scanned documents (PDFs and images) associated with clients.
    /// </summary>
    public partial class ScannedNotes : UserControl
    {
        private ScannedNotesVM? _viewModel;

        /// <summary>
        /// Initializes a new instance of the ScannedNotes view.
        /// </summary>
        public ScannedNotes()
        {
            InitializeComponent();
            this.DataContextChanged += ScannedNotes_DataContextChanged;
            this.Loaded += ScannedNotes_Loaded;
        }

        private async void ScannedNotes_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ensure WebView2 has a writable user data folder
                // Default is next to EXE, which fails in C:\Program Files
                var appDataPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ClientManagementSystemV4",
                    "WebView2_Data");
                
                System.IO.Directory.CreateDirectory(appDataPath);
                
                var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, appDataPath);
                await PdfWebView.EnsureCoreWebView2Async(env);
            }
            catch (Exception ex)
            {
                // If it fails, the PDF viewer simply won't work, but we should log it
                // We reuse the existing App crash logging if possible, but here we just ignore or show a warning
            }
        }

        /// <summary>
        /// Handles DataContext changes to manage PropertyChanged subscriptions.
        /// </summary>
        private void ScannedNotes_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            _viewModel = e.NewValue as ScannedNotesVM;

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                
                // Initialize zoom level
                UpdateWebViewZoom();
            }
        }

        /// <summary>
        /// Handles property changes in the ViewModel.
        /// </summary>
        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ScannedNotesVM.CurrentZoom))
            {
                UpdateWebViewZoom();
            }
            else if (e.PropertyName == nameof(ScannedNotesVM.IsViewerOverlayOpen) && _viewModel?.IsViewerOverlayOpen == true)
            {
                // Reset zoom when opening overlay (optional, but consistent)
                UpdateWebViewZoom();
            }
        }

        /// <summary>
        /// Synchronizes the WebView2 zoom level with the ViewModel's CurrentZoom.
        /// </summary>
        private void UpdateWebViewZoom()
        {
            if (PdfWebView != null && _viewModel != null)
            {
                try
                {
                    // WebView2 ZoomFactor is 1.0 based, just like our CurrentZoom
                    PdfWebView.ZoomFactor = _viewModel.CurrentZoom;
                }
                catch (Exception)
                {
                    // CoreWebView2 might not be initialized yet.
                    // Note: If initialization happens later, the zoom will be 1.0 by default.
                }
            }
        }
    }
}
