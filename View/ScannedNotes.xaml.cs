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
