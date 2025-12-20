using System.Windows.Controls;

namespace Client_Management_System_V4.View
{
    /// <summary>
    /// Interaction logic for ScannedNotes.xaml
    /// View for managing scanned documents (PDFs and images) associated with clients.
    /// </summary>
    public partial class ScannedNotes : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the ScannedNotes view.
        /// Note: WebView2 does not work well with TranslateTransform animations,
        /// so this overlay uses a simple fade-in effect (Overlay_Grid_Style) 
        /// without the slide-in animation (Drawer_Style).
        /// </summary>
        public ScannedNotes()
        {
            InitializeComponent();
        }
    }
}
