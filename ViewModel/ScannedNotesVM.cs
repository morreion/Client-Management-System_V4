using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Printing;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using Client_Management_System_V4.Models;
using Client_Management_System_V4.Repositories;
using Client_Management_System_V4.Utilities;

namespace Client_Management_System_V4.ViewModel
{
    /// <summary>
    /// ViewModel for Scanned Notes management with PDF/image viewing,
    /// document navigation, editing, and printing functionality.
    /// </summary>
    public class ScannedNotesVM : ViewModelBase
    {
        #region Private Fields

        private readonly ScannedNotesRepository _repository;
        private readonly ClientRepository _clientRepository;
        
        private ObservableCollection<Client> _clients = new();
        private ObservableCollection<ScannedNote> _scannedNotes = new();
        
        private Client? _selectedClient;
        private ScannedNote? _selectedNote;
        
        private string _searchText = string.Empty;
        private bool _isLoading;
        private bool _isEditMode;
        
        private int _currentNoteIndex;
        private BitmapImage? _currentImage;
        private string? _tempPdfPath;
        private bool _isViewerOverlayOpen;
        private double _currentZoom = 1.0;

        #endregion

        #region Properties

        /// <summary>
        /// Collection of all clients for dropdown selection
        /// </summary>
        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set { _clients = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Collection of scanned notes for selected client
        /// </summary>
        public ObservableCollection<ScannedNote> ScannedNotes
        {
            get => _scannedNotes;
            set { _scannedNotes = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Currently selected client from dropdown
        /// </summary>
        public Client? SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsClientSelected));
                _ = LoadNotesForClientAsync();
            }
        }

        /// <summary>
        /// Currently selected/displayed scanned note
        /// </summary>
        public ScannedNote? SelectedNote
        {
            get => _selectedNote;
            set
            {
                _selectedNote = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNoteSelected));
                OnPropertyChanged(nameof(IsPdfDocument));
                OnPropertyChanged(nameof(IsImageDocument));
                UpdateCurrentIndex();
                LoadDocumentForDisplay();
            }
        }

        /// <summary>
        /// Search text for filtering notes
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    _ = SearchNotesAsync();
                }
            }
        }

        /// <summary>
        /// Loading indicator
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Edit mode flag
        /// </summary>
        public bool IsEditMode
        {
            get => _isEditMode;
            set { _isEditMode = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Current 1-based index in document list
        /// </summary>
        public int CurrentNoteIndex
        {
            get => _currentNoteIndex;
            set { _currentNoteIndex = value; OnPropertyChanged(); OnPropertyChanged(nameof(NavigationDisplay)); }
        }

        /// <summary>
        /// Total number of documents for current client
        /// </summary>
        public int TotalNotes => ScannedNotes.Count;

        /// <summary>
        /// Display string for navigation (e.g., "2 of 5")
        /// </summary>
        public string NavigationDisplay => TotalNotes > 0 ? $"{CurrentNoteIndex} of {TotalNotes}" : "No documents";

        /// <summary>
        /// Current image to display (for image documents)
        /// </summary>
        public BitmapImage? CurrentImage
        {
            get => _currentImage;
            set { _currentImage = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Temporary path for PDF file (for WebView2 display)
        /// </summary>
        public string? TempPdfPath
        {
            get => _tempPdfPath;
            set { _tempPdfPath = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// True if a client is selected
        /// </summary>
        public bool IsClientSelected => SelectedClient != null;

        /// <summary>
        /// True if a note is selected
        /// </summary>
        public bool IsNoteSelected => SelectedNote != null;

        /// <summary>
        /// True if the selected note is a PDF
        /// </summary>
        public bool IsPdfDocument => SelectedNote?.IsPdf ?? false;

        /// <summary>
        /// True if the selected note is an image
        /// </summary>
        public bool IsImageDocument => SelectedNote?.IsImage ?? false;

        /// <summary>
        /// True if the document viewer overlay is open
        /// </summary>
        public bool IsViewerOverlayOpen
        {
            get => _isViewerOverlayOpen;
            set { _isViewerOverlayOpen = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Current zoom level for the document viewer (1.0 = 100%)
        /// </summary>
        public double CurrentZoom
        {
            get => _currentZoom;
            set 
            { 
                _currentZoom = Math.Clamp(value, 0.5, 3.0); 
                OnPropertyChanged();
                OnPropertyChanged(nameof(ZoomPercentageDisplay));
            }
        }

        /// <summary>
        /// Display string for the current zoom level
        /// </summary>
        public string ZoomPercentageDisplay => $"{Math.Round(CurrentZoom * 100)}%";

        #endregion

        #region Commands

        public ICommand LoadedCommand { get; }
        public ICommand UploadDocumentCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand PreviousDocumentCommand { get; }
        public ICommand NextDocumentCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddNewCommand { get; }
        public ICommand OpenViewerOverlayCommand { get; }
        public ICommand CloseViewerOverlayCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand ResetZoomCommand { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of ScannedNotesVM
        /// </summary>
        public ScannedNotesVM()
        {
            _repository = new ScannedNotesRepository();
            _clientRepository = new ClientRepository();

            // Initialize commands
            LoadedCommand = new RelayCommand(async _ => await InitializeAsync());
            UploadDocumentCommand = new RelayCommand(_ => UploadDocument(), _ => IsClientSelected);
            SaveCommand = new RelayCommand(async _ => await SaveNoteAsync(), _ => IsNoteSelected);
            DeleteCommand = new RelayCommand(async _ => await DeleteNoteAsync(), _ => IsNoteSelected);
            PreviousDocumentCommand = new RelayCommand(_ => NavigatePrevious(), _ => CanNavigatePrevious());
            NextDocumentCommand = new RelayCommand(_ => NavigateNext(), _ => CanNavigateNext());
            PrintCommand = new RelayCommand(_ => PrintDocument(), _ => IsNoteSelected);
            CancelCommand = new RelayCommand(_ => CancelEdit());
            AddNewCommand = new RelayCommand(_ => AddNewNote(), _ => IsClientSelected);
            OpenViewerOverlayCommand = new RelayCommand(_ => OpenViewerOverlay(), _ => IsNoteSelected);
            CloseViewerOverlayCommand = new RelayCommand(_ => CloseViewerOverlay());
            ZoomInCommand = new RelayCommand(_ => ZoomIn(), _ => IsNoteSelected);
            ZoomOutCommand = new RelayCommand(_ => ZoomOut(), _ => IsNoteSelected);
            ResetZoomCommand = new RelayCommand(_ => ResetZoom(), _ => IsNoteSelected);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the view - loads clients
        /// </summary>
        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                var clients = await _clientRepository.GetAllAsync();
                Clients = new ObservableCollection<Client>(clients);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading clients: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Loads scanned notes for the selected client
        /// </summary>
        private async Task LoadNotesForClientAsync()
        {
            if (SelectedClient == null)
            {
                ScannedNotes.Clear();
                SelectedNote = null;
                return;
            }

            try
            {
                IsLoading = true;
                var notes = await _repository.GetByClientIdAsync(SelectedClient.ClientID);
                ScannedNotes = new ObservableCollection<ScannedNote>(notes);
                OnPropertyChanged(nameof(TotalNotes));

                // Select first note if available
                if (ScannedNotes.Any())
                {
                    SelectedNote = ScannedNotes.First();
                }
                else
                {
                    SelectedNote = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading scanned notes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Searches notes by document name for the current client
        /// </summary>
        private async Task SearchNotesAsync()
        {
            if (SelectedClient == null) return;

            try
            {
                IsLoading = true;

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    await LoadNotesForClientAsync();
                }
                else
                {
                    var notes = await _repository.SearchByClientAsync(SelectedClient.ClientID, SearchText);
                    ScannedNotes = new ObservableCollection<ScannedNote>(notes);
                    OnPropertyChanged(nameof(TotalNotes));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching notes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Updates the current index based on selected note
        /// </summary>
        private void UpdateCurrentIndex()
        {
            if (SelectedNote != null && ScannedNotes.Contains(SelectedNote))
            {
                CurrentNoteIndex = ScannedNotes.IndexOf(SelectedNote) + 1;
            }
            else
            {
                CurrentNoteIndex = 0;
            }
        }

        /// <summary>
        /// Loads the document for display (image or PDF)
        /// </summary>
        private void LoadDocumentForDisplay()
        {
            // Clear previous
            CurrentImage = null;
            CleanupTempPdf();

            if (SelectedNote?.Scanned_Document == null || SelectedNote.Scanned_Document.Length == 0)
            {
                return;
            }

            if (SelectedNote.IsImage)
            {
                // Load as image
                try
                {
                    var image = new BitmapImage();
                    using (var stream = new MemoryStream(SelectedNote.Scanned_Document))
                    {
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = stream;
                        image.EndInit();
                        image.Freeze();
                    }
                    CurrentImage = image;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (SelectedNote.IsPdf)
            {
                // Save to temp file for WebView2
                try
                {
                    var appDataPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "ClientManagementSystemV4",
                        "TempDocuments");
                        
                    Directory.CreateDirectory(appDataPath);
                    _tempPdfPath = Path.Combine(appDataPath, $"temp_{SelectedNote.ScannedNotesID}.pdf");
                    File.WriteAllBytes(_tempPdfPath, SelectedNote.Scanned_Document);
                    TempPdfPath = _tempPdfPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error preparing PDF: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Cleans up temporary PDF file
        /// </summary>
        private void CleanupTempPdf()
        {
            if (!string.IsNullOrEmpty(_tempPdfPath) && File.Exists(_tempPdfPath))
            {
                try
                {
                    File.Delete(_tempPdfPath);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
            _tempPdfPath = null;
        }

        /// <summary>
        /// Opens file dialog to upload a new document
        /// </summary>
        private void UploadDocument()
        {
            if (SelectedClient == null)
            {
                MessageBox.Show("Please select a client first.", "No Client Selected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new OpenFileDialog
            {
                Title = "Select Document to Upload",
                Filter = "Supported Files (*.pdf;*.png;*.jpg;*.jpeg)|*.pdf;*.png;*.jpg;*.jpeg|" +
                         "PDF Files (*.pdf)|*.pdf|" +
                         "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|" +
                         "All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var extension = Path.GetExtension(dialog.FileName).TrimStart('.').ToUpperInvariant();
                    var fileData = File.ReadAllBytes(dialog.FileName);

                    // Create new note with uploaded document
                    var newNote = new ScannedNote
                    {
                        Document_Name = Path.GetFileNameWithoutExtension(dialog.FileName),
                        Document_Date = DateTime.Today,
                        Document_Type = extension,
                        Scanned_Document = fileData,
                        ClientID = SelectedClient.ClientID
                    };

                    ScannedNotes.Add(newNote);
                    SelectedNote = newNote;
                    IsEditMode = true;
                    OnPropertyChanged(nameof(TotalNotes));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading file: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Creates a new empty note for the current client
        /// </summary>
        private void AddNewNote()
        {
            if (SelectedClient == null) return;

            SelectedNote = new ScannedNote
            {
                Document_Date = DateTime.Today,
                ClientID = SelectedClient.ClientID
            };
            IsEditMode = true;
        }

        /// <summary>
        /// Saves the current note (add or update)
        /// </summary>
        private async Task SaveNoteAsync()
        {
            if (SelectedNote == null || SelectedClient == null) return;

            // Validation
            if (string.IsNullOrWhiteSpace(SelectedNote.Document_Name))
            {
                MessageBox.Show("Document name is required.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;

                if (SelectedNote.ScannedNotesID == 0)
                {
                    // Add new
                    var newId = await _repository.AddAsync(SelectedNote);
                    SelectedNote.ScannedNotesID = newId;

                    if (!ScannedNotes.Contains(SelectedNote))
                    {
                        ScannedNotes.Add(SelectedNote);
                    }

                    MessageBox.Show("Document saved successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Update existing
                    var success = await _repository.UpdateAsync(SelectedNote);
                    if (success)
                    {
                        MessageBox.Show("Document updated successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                OnPropertyChanged(nameof(TotalNotes));
                IsEditMode = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving document: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Deletes the selected note
        /// </summary>
        private async Task DeleteNoteAsync()
        {
            if (SelectedNote == null || SelectedNote.ScannedNotesID == 0) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{SelectedNote.Document_Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                var success = await _repository.DeleteAsync(SelectedNote.ScannedNotesID);

                if (success)
                {
                    var index = ScannedNotes.IndexOf(SelectedNote);
                    ScannedNotes.Remove(SelectedNote);
                    OnPropertyChanged(nameof(TotalNotes));

                    // Select next or previous
                    if (ScannedNotes.Any())
                    {
                        SelectedNote = ScannedNotes.ElementAtOrDefault(Math.Min(index, ScannedNotes.Count - 1));
                    }
                    else
                    {
                        SelectedNote = null;
                    }

                    MessageBox.Show("Document deleted successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting document: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Navigates to the previous document
        /// </summary>
        private void NavigatePrevious()
        {
            if (!CanNavigatePrevious()) return;

            var currentIndex = ScannedNotes.IndexOf(SelectedNote!);
            SelectedNote = ScannedNotes[currentIndex - 1];
        }

        /// <summary>
        /// Navigates to the next document
        /// </summary>
        private void NavigateNext()
        {
            if (!CanNavigateNext()) return;

            var currentIndex = ScannedNotes.IndexOf(SelectedNote!);
            SelectedNote = ScannedNotes[currentIndex + 1];
        }

        private bool CanNavigatePrevious()
        {
            if (SelectedNote == null || ScannedNotes.Count <= 1) return false;
            return ScannedNotes.IndexOf(SelectedNote) > 0;
        }

        private bool CanNavigateNext()
        {
            if (SelectedNote == null || ScannedNotes.Count <= 1) return false;
            return ScannedNotes.IndexOf(SelectedNote) < ScannedNotes.Count - 1;
        }

        /// <summary>
        /// Prints the current document
        /// </summary>
        private void PrintDocument()
        {
            if (SelectedNote?.Scanned_Document == null) return;

            try
            {
                if (SelectedNote.IsImage && CurrentImage != null)
                {
                    // Print image
                    var printDialog = new PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        var visual = new DrawingVisual();
                        using (var dc = visual.RenderOpen())
                        {
                            var pageWidth = printDialog.PrintableAreaWidth;
                            var pageHeight = printDialog.PrintableAreaHeight;
                            var imageWidth = CurrentImage.Width;
                            var imageHeight = CurrentImage.Height;

                            // Scale to fit
                            var scale = Math.Min(pageWidth / imageWidth, pageHeight / imageHeight);
                            var scaledWidth = imageWidth * scale;
                            var scaledHeight = imageHeight * scale;

                            // Center on page
                            var x = (pageWidth - scaledWidth) / 2;
                            var y = (pageHeight - scaledHeight) / 2;

                            dc.DrawImage(CurrentImage, new Rect(x, y, scaledWidth, scaledHeight));
                        }

                        printDialog.PrintVisual(visual, SelectedNote.Document_Name ?? "Scanned Document");
                    }
                }
                else if (SelectedNote.IsPdf && !string.IsNullOrEmpty(TempPdfPath) && File.Exists(TempPdfPath))
                {
                    // For PDF, open in default viewer which has print capability
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = TempPdfPath,
                        UseShellExecute = true,
                        Verb = "print"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing document: {ex.Message}", "Print Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Cancels the current edit
        /// </summary>
        private void CancelEdit()
        {
            if (SelectedNote?.ScannedNotesID == 0)
            {
                // Remove unsaved note
                ScannedNotes.Remove(SelectedNote);
                SelectedNote = ScannedNotes.FirstOrDefault();
                OnPropertyChanged(nameof(TotalNotes));
            }
            else
            {
                // Reload to discard changes
                _ = LoadNotesForClientAsync();
            }
            IsEditMode = false;
        }

        /// <summary>
        /// Opens the document viewer overlay
        /// </summary>
        private void OpenViewerOverlay()
        {
            if (SelectedNote != null)
            {
                IsViewerOverlayOpen = true;
            }
        }

        /// <summary>
        /// Closes the document viewer overlay
        /// </summary>
        private void CloseViewerOverlay()
        {
            IsViewerOverlayOpen = false;
        }

        /// <summary>
        /// Increases the zoom level
        /// </summary>
        private void ZoomIn()
        {
            CurrentZoom += 0.1;
        }

        /// <summary>
        /// Decreases the zoom level
        /// </summary>
        private void ZoomOut()
        {
            CurrentZoom -= 0.1;
        }

        /// <summary>
        /// Resets the zoom level to 100%
        /// </summary>
        private void ResetZoom()
        {
            CurrentZoom = 1.0;
        }

        #endregion
    }
}
