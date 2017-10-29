using System;
using System.Collections.ObjectModel;

using Xamarin.Forms;

using OpenNETCF.IoC;
using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Library.Core.Interfaces;

namespace HWC_UserApp.UserApp.Models
{
    internal class BeaconService : IDisposable
    {
        #region Data members

        private readonly BeaconManager _beaconManager;

        public ObservableCollection<Beacon> Beacons => _beaconManager?.BluetoothBeacons;

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
                _beaconManager.Start();
            }
            else
            {
                throw new Exception("Bluetooth packet provider is unavailable");
            }
        }

        public void Dispose()
        {
            _beaconManager?.Stop();
        }

        #endregion
    }
}
