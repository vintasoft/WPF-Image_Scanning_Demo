using System;
using System.Collections.Generic;
using Vintasoft.EsclImageScanning;
using Vintasoft.WpfTwain;
using Vintasoft.WiaImageScanning;

namespace Vintasoft.UniversalImageScanner
{
    /// <summary>
    /// The WPF manager for universal image scanners.
    /// </summary>
    public class WpfUniversalImageScannerManager : IDisposable
    {

        DeviceManager _twainDeviceManager32;
        DeviceManager _twainDeviceManager64;
        WiaDeviceManager _wiaDeviceManager;
        EsclDeviceManager _esclDeviceManager;
        Dictionary<string, IUniversalImageScanner> _nameToImageScanner = new Dictionary<string, IUniversalImageScanner>();



        /// <summary>
        /// Creates instance of WindowsUniversalImageScannerManager class.
        /// </summary>
        public WpfUniversalImageScannerManager()
        {
            try
            {
                _twainDeviceManager32 = new DeviceManager();
                _twainDeviceManager32.IsTwain2Compatible = true;
                _twainDeviceManager32.Use32BitDevices();
            }
            catch
            {
            }

            try
            {
                _twainDeviceManager64 = new DeviceManager();
                _twainDeviceManager64.IsTwain2Compatible = true;
            }
            catch
            {
            }

            try
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    _wiaDeviceManager = new WiaDeviceManager();
                }
            }
            catch
            {
            }

            try
            {
                _esclDeviceManager = new EsclDeviceManager();
            }
            catch
            {
            }
        }



        /// <summary>
        /// Disposes resources, which are used by this manager.
        /// </summary>
        public void Dispose()
        {
            if (_twainDeviceManager32 != null)
            {
                _twainDeviceManager32.Dispose();
                _twainDeviceManager32 = null;
            }

            if (_twainDeviceManager64 != null)
            {
                _twainDeviceManager64.Dispose();
                _twainDeviceManager64 = null;
            }

            if (_wiaDeviceManager != null)
            {
                _wiaDeviceManager.Dispose();
                _wiaDeviceManager = null;
            }

            if (_esclDeviceManager != null)
            {
                _esclDeviceManager.Dispose();
                _esclDeviceManager = null;
            }
        }

        /// <summary>
        /// Opens this manager.
        /// </summary>
        public void Open()
        {
            try
            {
                if (_twainDeviceManager32 != null)
                    _twainDeviceManager32.Open();
            }
            catch
            {
            }

            try
            {
                if (_twainDeviceManager64 != null)
                    _twainDeviceManager64.Open();
            }
            catch
            {
            }

            try
            {
                if (_wiaDeviceManager != null)
                    _wiaDeviceManager.Open();
            }
            catch
            {
            }

            try
            {
                if (_esclDeviceManager != null)
                    _esclDeviceManager.Open();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Returns the names of available image scanner.
        /// </summary>
        /// <returns>The names of available image scanner.</returns>
        public string[] GetUniversalImageScannerNames()
        {
            List<string> deviceNames = new List<string>();

            if (_twainDeviceManager32 != null)
            {
                if (_twainDeviceManager32.State == DeviceManagerState.Opened)
                {
                    for (int i = 0; i < _twainDeviceManager32.Devices.Count; i++)
                    {
                        deviceNames.Add(string.Format("TWAIN 32-bit: {0}", _twainDeviceManager32.Devices[i].Info.ProductName));
                    }
                }
            }

            if (_twainDeviceManager64 != null)
            {
                if (_twainDeviceManager64.State == DeviceManagerState.Opened)
                {
                    for (int i = 0; i < _twainDeviceManager64.Devices.Count; i++)
                    {
                        deviceNames.Add(string.Format("TWAIN 64-bit: {0}", _twainDeviceManager64.Devices[i].Info.ProductName));
                    }
                }
            }

            if (_wiaDeviceManager != null)
            {
                for (int i = 0; i < _wiaDeviceManager.Devices.Count; i++)
                {
                    deviceNames.Add(string.Format("WIA: {0}", _wiaDeviceManager.Devices[i].Name));
                }
            }

            if (_esclDeviceManager != null)
            {
                for (int i = 0; i < _esclDeviceManager.Devices.Count; i++)
                {
                    deviceNames.Add(string.Format("eSCL: {0}", _esclDeviceManager.Devices[i].Name));
                }
            }

            return deviceNames.ToArray();
        }

        /// <summary>
        /// Finds the image scanner by the name.
        /// </summary>
        /// <param name="name">The name of image scanner.</param>
        /// <returns>Image scanner if image scanner is found; otherwise, null.</returns>
        public IUniversalImageScanner Find(string name)
        {
            if (name.StartsWith("TWAIN 32-bit: "))
            {
                if (_nameToImageScanner.ContainsKey(name))
                {
                    return _nameToImageScanner[name];
                }
                else
                {
                    Device twainDevice = _twainDeviceManager32.Devices.Find(name.Substring("TWAIN 32-bit: ".Length));
                    if (twainDevice != null)
                    {
                        _nameToImageScanner[name] = new UniversalWpfTwainImageScanner(twainDevice);
                        return _nameToImageScanner[name];
                    }
                }
            }
            else if (name.StartsWith("TWAIN 64-bit: "))
            {
                if (_nameToImageScanner.ContainsKey(name))
                {
                    return _nameToImageScanner[name];
                }
                else
                {
                    Device twainDevice = _twainDeviceManager64.Devices.Find(name.Substring("TWAIN 64-bit: ".Length));
                    if (twainDevice != null)
                    {
                        _nameToImageScanner[name] = new UniversalWpfTwainImageScanner(twainDevice);
                        return _nameToImageScanner[name];
                    }
                }
            }
            else if (name.StartsWith("WIA: "))
            {
                if (_nameToImageScanner.ContainsKey(name))
                {
                    return _nameToImageScanner[name];
                }
                else
                {
                    WiaDevice wiaDevice = _wiaDeviceManager.Devices.FindByName(name.Substring("WIA: ".Length));
                    if (wiaDevice != null)
                    {
                        _nameToImageScanner[name] = new UniversalWiaImageScanner(wiaDevice);
                        return _nameToImageScanner[name];
                    }
                }
            }
            else if (name.StartsWith("eSCL: "))
            {
                if (_nameToImageScanner.ContainsKey(name))
                {
                    return _nameToImageScanner[name];
                }
                else
                {
                    EsclDevice esclDevice = _esclDeviceManager.Devices.FindByName(name.Substring("eSCL: ".Length));
                    if (esclDevice != null)
                    {
                        _nameToImageScanner[name] = new UniversalEsclImageScanner(esclDevice);
                        return _nameToImageScanner[name];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Closes this manager.
        /// </summary>
        public void Close()
        {
            try
            {
                if (_twainDeviceManager32 != null && _twainDeviceManager32.State != DeviceManagerState.Closed)
                    _twainDeviceManager32.Close();
            }
            catch
            {
            }

            try
            {
                if (_twainDeviceManager64 != null && _twainDeviceManager64.State != DeviceManagerState.Closed)
                    _twainDeviceManager64.Close();
            }
            catch
            {
            }

            try
            {
                if (_wiaDeviceManager != null && _wiaDeviceManager.IsOpened)
                    _wiaDeviceManager.Close();
            }
            catch
            {
            }

            try
            {
                if (_esclDeviceManager != null && _esclDeviceManager.IsOpened)
                    _esclDeviceManager.Close();
            }
            catch
            {
            }
        }

    }
}
