using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Vintasoft.Primitives;
using Vintasoft.UniversalImageScanner;

namespace WpfImageScanningDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// The image scanner manager.
        /// </summary>
        WpfUniversalImageScannerManager _imageScannerManager;
        /// <summary>
        /// The selected image scanner.
        /// </summary>
        IUniversalImageScanner _imageScanner;
        /// <summary>
        /// The index of scanned image that displays in picture box.
        /// </summary>
        int _scannedImageIndex = -1;



        public MainWindow()
        {
            // register the evaluation license for VintaSoft Twain .NET SDK
            Vintasoft.WinTwain.TwainGlobalSettings.Register("REG_USER", "REG_EMAIL", "EXPIRATION_DATE", "REG_CODE");

            InitializeComponent();

            this.Title = string.Format("VintaSoft WPF Image Scanning Demo v{0}", Vintasoft.WpfTwain.TwainEnvironment.ProductVersion);

            imagePixelTypesComboBox.Items.Add("Black-white");
            imagePixelTypesComboBox.Items.Add("Grayscale");
            imagePixelTypesComboBox.Items.Add("RGB");
            imagePixelTypesComboBox.SelectedIndex = 0;

            imageResolutionsComboBox.Items.Add("200 dpi");
            imageResolutionsComboBox.Items.Add("300 dpi");
            imageResolutionsComboBox.Items.Add("600 dpi");
            imageResolutionsComboBox.SelectedIndex = 0;

            pageSizesComboBox.Items.Add("US Letter (8.5 x 11 inches)");
            pageSizesComboBox.Items.Add("A5 (5.8 x 8.2 inches)");
            pageSizesComboBox.Items.Add("A4 (8.2 x 11.6 inches)");
            pageSizesComboBox.Items.Add("B5 dpi (6.9 x 9.8 inches)");
            pageSizesComboBox.SelectedIndex = 0;

            imageEncodersComboBox.Items.Add("PNG");
            imageEncodersComboBox.Items.Add("JPG");
            imageEncodersComboBox.Items.Add("TIF");
            imageEncodersComboBox.Items.Add("PDF");
            imageEncodersComboBox.Items.Add("BMP");
            imageEncodersComboBox.SelectedIndex = 0;

            scanImagesButton.IsEnabled = false;

            DisableImageManipulationButtons();

            devicesComboBox.Items.Add("Device list is loading...");
            devicesComboBox.SelectedIndex = 0;
            Cursor = Cursors.Wait;
        }



        /// <summary>
        /// Window content is rendered.
        /// </summary>
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                // create the universal image scanner manager
                _imageScannerManager = new WpfUniversalImageScannerManager();

                // open the image scanner manager
                _imageScannerManager.Open();

                // get the names of available image scanners
                string[] deviceNames = _imageScannerManager.GetUniversalImageScannerNames();
                devicesComboBox.Items.Clear();
                // if image scanners are not found in the system
                if (deviceNames.Length == 0)
                {
                    MessageBox.Show("Devices are not found.");
                    return;
                }

                // for each image scanner
                for (int i = 0; i < deviceNames.Length; i++)
                {
                    // add the name of image scanner to the device list
                    devicesComboBox.Items.Add(string.Format("{0}", deviceNames[i]));
                }
                // select the first device in the device list
                devicesComboBox.SelectedIndex = 0;

                scanImagesButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(GetFullExceptionMessage(ex));
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// Window is closing.
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DisposeImageInPictureBox();

            if (_imageScannerManager != null)
            {
                _imageScannerManager.Dispose();
                _imageScannerManager = null;
            }
        }

        /// <summary>
        /// Selected device is changed in the device list.
        /// </summary>
        private void devicesComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // if image scanner was previously selected
            if (_imageScanner != null)
            {
                // clear scanned images
                _imageScanner.ClearImages();
                _scannedImageIndex = -1;

                // clear reference to the selected image scanner
                _imageScanner = null;

                DisableImageManipulationButtons();
            }
        }

        /// <summary>
        /// "Scan images" button is clicked.
        /// </summary>
        private void scanImagesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string imageScannerName = (string)devicesComboBox.SelectedItem;
                // select the image scanner
                _imageScanner = _imageScannerManager.Find(imageScannerName);
                // if image scanner is not selected
                if (_imageScanner == null)
                    return;

                // open the image scanner
                _imageScanner.Open();

                // specify that device UI should be used
                _imageScanner.ShowUI = showDeviceUiCheckBox.IsChecked == true;

                // if image scanner has the automatic document feeder
                if (_imageScanner.HasFeeder)
                    // set the state of the automatic document feeder
                    _imageScanner.IsFeederEnabled = useDocumentFeederCheckBox.IsChecked == true;
                // image scanner supports the duplex scanning
                if (_imageScanner.HasDuplex)
                    // set the state of duplex scanning
                    _imageScanner.IsDuplexEnabled = useDuplexScanningCheckBox.IsChecked == true;

                // set the image pixel type for scanned images
                switch (imagePixelTypesComboBox.SelectedItem)
                {
                    case "Black-white":
                        _imageScanner.ImagePixelType = ImagePixelType.BlackWhite;
                        break;
                    case "Grayscale":
                        _imageScanner.ImagePixelType = ImagePixelType.Grayscale;
                        break;
                    default:
                        _imageScanner.ImagePixelType = ImagePixelType.RGB;
                        break;
                }

                // set the image resolution for scanned images
                switch (imageResolutionsComboBox.SelectedItem)
                {
                    case "200 dpi":
                        _imageScanner.ScanXResolution = 200;
                        _imageScanner.ScanYResolution = 200;
                        break;
                    case "300 dpi":
                        _imageScanner.ScanXResolution = 300;
                        _imageScanner.ScanYResolution = 300;
                        break;
                    case "600 dpi":
                        _imageScanner.ScanXResolution = 600;
                        _imageScanner.ScanYResolution = 600;
                        break;
                }

                // set the page size for scanned images
                switch (pageSizesComboBox.SelectedItem)
                {
                    case "US Letter (8.5 x 11 inches)":
                        _imageScanner.SetImageLayout(new VintasoftRect(0, 0, 8.5, 11));
                        break;
                    case "A5 (5.8 x 8.2 inches)":
                        _imageScanner.SetImageLayout(new VintasoftRect(0, 0, 5.8, 8.2));
                        break;
                    case "A4 (8.2 x 11.6 inches)":
                        _imageScanner.SetImageLayout(new VintasoftRect(0, 0, 8.2, 11.6));
                        break;
                    case "B5 dpi (6.9 x 9.8 inches)":
                        _imageScanner.SetImageLayout(new VintasoftRect(0, 0, 6.9, 9.8));
                        break;
                }

                do
                {
                    try
                    {
                        // if image is acquired from image scanner
                        if (_imageScanner.AcquireImageSync())
                        {
                            int scannedImageIndex = _imageScanner.ScannedImageCount - 1;

                            // get scanned image as a stream
                            Stream imageAsStream = _imageScanner.GetImageAsStream(scannedImageIndex);

                            DisposeImageInPictureBox();

                            // show scanned image in picture box
                            scannedImage.Source = BitmapFrame.Create(imageAsStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                            _scannedImageIndex = scannedImageIndex;

                            EnableImageManipulationButtons();

                            UpdateScannedImagesInfoLabel();
                        }
                        // if image is not acquired
                        else
                        {
                            Console.WriteLine("Scan is finished.");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format("Scan is failed: {0}", ex.Message));
                        break;
                    }
                }
                while (true);

                // close the image scanner
                _imageScanner.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(GetFullExceptionMessage(ex));
            }
        }

        /// <summary>
        /// "Previous image" button is clicked.
        /// </summary>
        private void previousImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_scannedImageIndex <= 0)
                return;

            // get image as a stream
            Stream imageAsStream = _imageScanner.GetImageAsStream(_scannedImageIndex - 1);

            DisposeImageInPictureBox();

            // set image to the picture box
            scannedImage.Source = BitmapFrame.Create(imageAsStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            _scannedImageIndex--;

            UpdatePreviousAndNextImageButton();
            UpdateScannedImagesInfoLabel();
        }

        /// <summary>
        /// "Next image" button is clicked.
        /// </summary>
        private void nextImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_scannedImageIndex >= (_imageScanner.ScannedImageCount - 1))
                return;

            // get image as a stream
            Stream imageAsStream = _imageScanner.GetImageAsStream(_scannedImageIndex + 1);

            DisposeImageInPictureBox();

            // set image to the picture box
            scannedImage.Source = BitmapFrame.Create(imageAsStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            _scannedImageIndex++;

            UpdatePreviousAndNextImageButton();
            UpdateScannedImagesInfoLabel();
        }

        /// <summary>
        /// "Save image" button is clicked.
        /// </summary>
        private void saveImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // get the name of image encoder
                string imageEncoderName = ((string)imageEncodersComboBox.SelectedItem).ToLowerInvariant();
                // get the name of file, where image should be saved
                string filename = string.Format("scannedImage.{0}", imageEncoderName);
                // save the scanned image to a file
                _imageScanner.SaveImage(_scannedImageIndex, filename);
                MessageBox.Show("Image is saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Image is not saved: " + ex.Message);
            }
        }

        /// <summary>
        /// "Save images" button is clicked.
        /// </summary>
        private void saveImagesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // get the name of image encoder
                string imageEncoderName = ((string)imageEncodersComboBox.SelectedItem).ToLowerInvariant();
                if (imageEncoderName != "tif" && imageEncoderName != "pdf")
                {
                    MessageBox.Show("All images can be saved to a multipage TIFF or PDF file only.");
                    return;
                }

                // get the name of file, where image should be saved
                string filename = string.Format("scannedImage.{0}", imageEncoderName);
                // for each scanned image
                for (int i = 0; i < _imageScanner.ScannedImageCount; i++)
                {
                    // add scanned image to a file
                    _imageScanner.SaveImage(i, filename);
                }
                MessageBox.Show("All images are saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Images are not saved: " + ex.Message);
            }
        }

        /// <summary>
        /// "Delete images" button is clicked.
        /// </summary>
        private void deleteImagesButton_Click(object sender, RoutedEventArgs e)
        {
            // clear scanned images
            _imageScanner.ClearImages();
            _scannedImageIndex = -1;

            DisposeImageInPictureBox();

            DisableImageManipulationButtons();
        }

        /// <summary>
        /// Disposes the image in picture box.
        /// </summary>
        private void DisposeImageInPictureBox()
        {
            // if picture box contains previously scanned image
            if (scannedImage.Source != null)
            {
                // clear previously scanned image
                scannedImage.Source = null;
            }
        }

        /// <summary>
        /// Enables the image manipulation buttons.
        /// </summary>
        private void EnableImageManipulationButtons()
        {
            previousImageButton.IsEnabled = true;
            nextImageButton.IsEnabled = true;
            saveImageButton.IsEnabled = true;
            saveAllImagesButton.IsEnabled = true;
            deleteAllImagesButton.IsEnabled = true;
        }

        /// <summary>
        /// Disables the image manipulation buttons.
        /// </summary>
        private void DisableImageManipulationButtons()
        {
            previousImageButton.IsEnabled = false;
            nextImageButton.IsEnabled = false;
            saveImageButton.IsEnabled = false;
            saveAllImagesButton.IsEnabled = false;
            deleteAllImagesButton.IsEnabled = false;
        }

        /// <summary>
        /// Updates the "Previous image" and "Next image" buttons.
        /// </summary>
        private void UpdatePreviousAndNextImageButton()
        {
            previousImageButton.IsEnabled = _scannedImageIndex != 0;
            nextImageButton.IsEnabled = _scannedImageIndex != _imageScanner.ScannedImageCount - 1;
        }

        /// <summary>
        /// Updates the label with information about scanned images.
        /// </summary>
        private void UpdateScannedImagesInfoLabel()
        {
            scannedImagesInfoLabel.Content = string.Format("{0} from {1}", _scannedImageIndex + 1, _imageScanner.ScannedImageCount);
        }

        /// <summary>
        /// Returns message that contains information about exception and inner exceptions.
        /// </summary>
        private static string GetFullExceptionMessage(Exception ex)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine(ex.Message);

            Exception innerException = ex.InnerException;
            while (innerException != null)
            {
                if (ex.Message != innerException.Message)
                    sb.AppendLine(string.Format("Inner exception: {0}", innerException.Message));
                innerException = innerException.InnerException;
            }

            return sb.ToString();
        }

    }
}
