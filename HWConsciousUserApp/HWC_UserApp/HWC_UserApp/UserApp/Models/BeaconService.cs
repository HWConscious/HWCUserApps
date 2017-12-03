using System;
using System.Collections.ObjectModel;

using Xamarin.Forms;

using OpenNETCF.IoC;
using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Library.Core.Interop;

namespace HWC_UserApp.UserApp.Models
{
    internal class BeaconService : IDisposable
    {
        #region Data members

        private readonly BeaconManager _beaconManager;

        public ObservableCollection<Beacon> Beacons => _beaconManager?.BluetoothBeacons;
        public event EventHandler BeaconScanningStarted;
        public event EventHandler<BTError> BeaconScanningStopped;

        #endregion

        #region Public methods

        public BeaconService()
        {
            // Get the platform-specific provider
            var provider = RootWorkItem.Services.Get<IBluetoothPacketProvider>();

            if (provider != null)
            {
                // Construct & start the bluetooth beacon manager,
                // giving it an invoker to marshal collection changes to the UI thread
                _beaconManager = new BeaconManager(provider, Device.BeginInvokeOnMainThread);
                provider.WatcherStopped += Provider_WatcherStopped;
            }
            else
            {
                throw new Exception("Bluetooth packet provider is unavailable");
            }
        }

        private void Provider_WatcherStopped(object sender, BTError e)
        {
            BeaconScanningStopped?.Invoke(this, e);
        }

        public void WatcherStart()
        {
            _beaconManager?.Start();
            BeaconScanningStarted?.Invoke(this, null);
        }

        public void WatcherStop()
        {
            _beaconManager?.Stop();
        }

        public void Dispose()
        {
            WatcherStop();
        }

        #endregion
    }
}
