using System;
using System.Collections.Generic;
using System.IO;
using Vintasoft.Primitives;
using Vintasoft.WiaImageScanning;
using Vintasoft.WiaImageScanning.ImageEncoders;

namespace Vintasoft.UniversalImageScanner
{
    /// <summary>
    /// The universal WIA image scanner.
    /// </summary>
    public class UniversalWiaImageScanner : IUniversalImageScanner
    {

        WiaDevice _wiaDevice;
        List<WiaAcquiredImage> _acquiredImages;



        public UniversalWiaImageScanner(WiaDevice wiaDevice)
        {
            _wiaDevice = wiaDevice;
            _acquiredImages = new List<WiaAcquiredImage>();
        }



        /// <summary>
        /// Gets the name of image scanner.
        /// </summary>
        public string Name
        {
            get { return _wiaDevice.Name; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether image scanner should show the UI.
        /// </summary>
        public bool ShowUI
        {
            get { return _wiaDevice.ShowUI; }
            set { _wiaDevice.ShowUI = value; }
        }

        /// <summary>
        /// Gets a value indicating whether image scanner has the flated.
        /// </summary>
        public bool HasFlatbed
        {
            get { return _wiaDevice.HasFlatbed; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether flatbed is enabled.
        /// </summary>
        public bool IsFlatbedEnabled
        {
            get { return _wiaDevice.IsFlatbedEnabled; }
            set { _wiaDevice.IsFlatbedEnabled = value; }
        }

        /// <summary>
        /// Gets a value indicating whether image scanner has the automatic document feeder.
        /// </summary>
        public bool HasFeeder
        {
            get { return _wiaDevice.HasFeeder; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the automatic document feeder is enabled.
        /// </summary>
        public bool IsFeederEnabled
        {
            get { return _wiaDevice.IsFeederEnabled; }
            set { _wiaDevice.IsFeederEnabled = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the automatic document feeder supports duplex scanning.
        /// </summary>
        public bool HasDuplex
        {
            get { return _wiaDevice.HasDuplex; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the duplex scanning is enabled.
        /// </summary>
        public bool IsDuplexEnabled
        {
            get { return _wiaDevice.IsDuplexEnabled; }
            set { _wiaDevice.IsDuplexEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the pixel type for scanned images.
        /// </summary>
        public ImagePixelType ImagePixelType
        {
            get
            {
                WiaImagePixelType pixelType = _wiaDevice.ScanImagePixelType;
                switch (pixelType)
                {
                    case WiaImagePixelType.BlackWhite:
                        return ImagePixelType.BlackWhite;
                    case WiaImagePixelType.Grayscale:
                        return ImagePixelType.Grayscale;
                }
                return ImagePixelType.RGB;
            }
            set
            {
                switch (value)
                {
                    case ImagePixelType.BlackWhite:
                        _wiaDevice.ScanImagePixelType = WiaImagePixelType.BlackWhite;
                        break;
                    case ImagePixelType.Grayscale:
                        _wiaDevice.ScanImagePixelType = WiaImagePixelType.Grayscale;
                        break;
                    case ImagePixelType.RGB:
                        _wiaDevice.ScanImagePixelType = WiaImagePixelType.RGB;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the horizontal resolution for scanned images.
        /// </summary>
        public int ScanXResolution
        {
            get { return _wiaDevice.ScanXResolution; }
            set { _wiaDevice.ScanXResolution = value; }
        }

        /// <summary>
        /// Gets or sets the vertical resolution for scanned images.
        /// </summary>
        public int ScanYResolution
        {
            get { return _wiaDevice.ScanYResolution; }
            set { _wiaDevice.ScanYResolution = value; }
        }

        /// <summary>
        /// Gets the count of scanned images.
        /// </summary>
        public int ScannedImageCount
        {
            get { return _acquiredImages.Count; }
        }



        /// <summary>
        /// Opens the image scanner.
        /// </summary>
        public void Open()
        {
            _wiaDevice.Open();
        }

        /// <summary>
        /// Returns the image layout, in inches, of image scanner.
        /// </summary>
        /// <returns>The image layout, in inches</returns>
        public VintasoftRect GetImageLayout()
        {
            VintasoftRect imageLayoutInPixels = _wiaDevice.GetImageLayout();
            VintasoftRect imageLayoutInInches = new VintasoftRect(
                imageLayoutInPixels.X / ScanXResolution, imageLayoutInPixels.Y / ScanYResolution,
                imageLayoutInPixels.Width / ScanXResolution, imageLayoutInPixels.Height / ScanYResolution);
            return imageLayoutInInches;
        }

        /// <summary>
        /// Sets the image layout for image scanner.
        /// </summary>
        /// <param name="imageLayout">The image layout in inches.</param>
        public void SetImageLayout(VintasoftRect imageLayoutInInches)
        {
            VintasoftRect imageLayoutInPixels = new VintasoftRect(
                imageLayoutInInches.X * ScanXResolution, imageLayoutInInches.Y * ScanYResolution,
                imageLayoutInInches.Width * ScanXResolution, imageLayoutInInches.Height * ScanYResolution);
            _wiaDevice.SetImageLayout(imageLayoutInPixels);
        }

        /// <summary>
        /// Acquires one image from image scanner.
        /// </summary>
        /// <returns>True - image is acquired; False - image is not acquired.</returns>
        public bool AcquireImageSync()
        {
            WiaAcquiredImage acquiredImage = _wiaDevice.AcquireImageSync();
            if (acquiredImage == null)
                return false;

            _acquiredImages.Add(acquiredImage);
            return true;
        }

        /// <summary>
        /// Saves image to a file.
        /// </summary>
        /// <param name="imageIndex">The index of image in image buffer.</param>
        /// <param name="filename">The name of file, where image must be saved.</param>
        public void SaveImage(int imageIndex, string filename)
        {
            if (imageIndex < 0 || imageIndex >= _acquiredImages.Count)
                throw new ArgumentOutOfRangeException();
            if (filename == null)
                throw new ArgumentNullException();

            WiaImageEncoderSettings encoderSettings = null;
            switch (Path.GetExtension(filename).ToLowerInvariant())
            {
                case ".jpg":
                    encoderSettings = new WiaJpegEncoderSettings();
                    break;
                case ".tif":
                    encoderSettings = new WiaTiffEncoderSettings();
                    ((WiaTiffEncoderSettings)encoderSettings).TiffMultiPage = true;
                    break;
                case ".pdf":
                    encoderSettings = new WiaPdfEncoderSettings();
                    ((WiaPdfEncoderSettings)encoderSettings).PdfMultiPage = true;
                    break;
                case ".bmp":
                    encoderSettings = new WiaBmpEncoderSettings();
                    break;
                default:
                    encoderSettings = new WiaPngEncoderSettings();
                    break;
            }

            _acquiredImages[imageIndex].Save(filename, encoderSettings);
        }

        /// <summary>
        /// Returns scanned image as a stream.
        /// </summary>
        /// <param name="imageIndex">The index of image in image buffer.</param>
        /// <returns>The memory stream that contains scanned image.</returns>
        public Stream GetImageAsStream(int imageIndex)
        {
            if (imageIndex < 0 || imageIndex >= _acquiredImages.Count)
                throw new ArgumentOutOfRangeException();

            MemoryStream mem = new MemoryStream();
            _acquiredImages[imageIndex].Save(mem, new WiaPngEncoderSettings());
            return mem;
        }

        /// <summary>
        /// Clears the scanned images.
        /// </summary>
        public void ClearImages()
        {
            if (_acquiredImages != null)
            {
                _acquiredImages.Clear();
            }
        }

        /// <summary>
        /// Closes the image scanner.
        /// </summary>
        public void Close()
        {
            _wiaDevice.Close();
        }

        /// <summary>
        /// Disposes used resources.
        /// </summary>
        public void Dispose()
        {
            ClearImages();
        }

    }
}
