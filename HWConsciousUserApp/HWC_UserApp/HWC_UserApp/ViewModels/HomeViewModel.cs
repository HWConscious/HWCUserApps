using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using HWC_UserApp.UserApp.Models;

using Newtonsoft.Json;
using OpenNETCF.IoC;
using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Library.Core.Interop;

namespace HWC_UserApp.ViewModels
{
    public class HomeViewModel
    {
        #region Data members

        private long? _userID { get; set; }
        private RestClient _userLocationRestClient { get; set; }
        private BeaconService _beaconService { get; set; }
        private IBluetoothPacketProvider _bluetoothPacketProvider { get; set; }
        private readonly bool _isSimulateBeaconData = false;
        private readonly bool _isSimulateCouponData = false;
        
        public ObservableCollection<Beacon> Beacons => _beaconService?.Beacons;
        public ObservableCollection<Coupon> ReceivedCoupons { get; set; } = new ObservableCollection<Coupon>();
        public event EventHandler<NewCouponReceivedEventArgs> NewCouponReceived;

        #endregion

        #region Initialize

        public HomeViewModel()
        {    
            if (FetchUserID() && RestClientInitialize() && BeaconServiceInitialize())
            {
                PingTimerInitializeAsync();
            }
            else
            {
                Utility.DebugLog("HomeViewModel not initialized");
            }
        }
        
        // Fetch UserID for this device
        private bool FetchUserID()
        {
            _userID = 1;    // TODO: Update to it's real value for the app instance
            return true;
        }

        // Initialize client for REST call
        private bool RestClientInitialize()
        {
            if (_userID != null)
            {
                try
                {
                    _userLocationRestClient = new RestClient(
                         RestClient.HttpVerb.POST,
                         UserApp.Models.Constants.RestApiEndpoint + "/users/" + _userID + "/locations",
                         new Dictionary<string, string>() { { "x-api-key", UserApp.Models.Constants.XApiKeyValue } },
                         null,
                         null,
                         60 * 1000);  // 1 minute timeout
                    return true;
                }
                catch (Exception ex)
                {
                    Utility.DebugLog("Error in initializing REST client for sending user location. EXCEPTION: " + ex.Message);
                }
            }
            return false;
        }

        // Initialize Beacon service
        private bool BeaconServiceInitialize()
        {
            // Construct the Beacon service
            _beaconService = RootWorkItem.Services.Get<BeaconService>();
            if (_beaconService == null)
            {
                try
                {
                    _beaconService = RootWorkItem.Services.AddNew<BeaconService>();
                    _beaconService.Beacons.CollectionChanged += Beacons_CollectionChanged;

                    _bluetoothPacketProvider = RootWorkItem.Services.Get<IBluetoothPacketProvider>();

                    Utility.DebugLog("Beacon service started...");
                    return true;
                }
                catch (Exception ex)
                {
                    Utility.DebugLog("Error in initializing Beacon service. EXCEPTION: " + ex.Message);
                }
            }
            else
            {
                return true;
            }
            return false;
        }

        // Initialize ping timer
        private async void PingTimerInitializeAsync()
        {
            // Delay 1 second before the first ping
            await Task.Delay(1000);

            while (true)
            {
                // Add a sample Beacon to Beacon list (for debugging purpose; when real Beacon device not available)
                if (_isSimulateBeaconData) { Beacons?.Add(GetAMockProximityBeacon()); }

                // Invoke the streaming action
                StreamUserLocation();

                // Start the ping timer
                await Task.Delay(UserApp.Models.Constants.UserLocationPingFrequencyInMs);
            }
        }

        #endregion

        #region Private methods
        
        // Stream user location
        private void StreamUserLocation()
        {
            _bluetoothPacketProvider.Start();

            Utility.DebugLog("Streaming location...");
            try
            {
                if (Beacons?.Any() ?? false)
                {
                    foreach (var beacon in Beacons)
                    {
                        if (beacon?.BeaconType == Beacon.BeaconTypeEnum.iBeacon)
                        {
                            if (beacon?.BeaconFrames?[0] is ProximityBeaconFrame beaconFrame)
                            {
                                if (!string.IsNullOrEmpty(beaconFrame.UuidAsString))
                                {
                                    Utility.DebugLog("Sending UUID: " + beaconFrame.UuidAsString + " (detected at: " + beacon.Timestamp + ")");
                                    PushUserLocationAsync(beaconFrame.UuidAsString);
                                }
                                else
                                {
                                    Utility.DebugLog("Sending cancelled: Invalid iBeacon UUID value");
                                }
                            }
                            else
                            {
                                Utility.DebugLog("Sending cancelled: Proximity beacon-frame not found");
                            }
                        }
                        else
                        {
                            Utility.DebugLog("Sending cancelled: Device is not a iBeacon");
                        }
                    }
                }
                else
                {
                    Utility.DebugLog("Sending cancelled: No Beacon found");
                }
            }
            catch (Exception ex)
            {
                Utility.DebugLog("Beacons streaming error. EXCEPTION: " + ex.Message);
            }

            try
            {
                if (Beacons?.Count > 0)
                {
                    Beacons?.Clear();
                }
            }
            catch (Exception ex)
            {
                Utility.DebugLog("Beacons clearing error. EXCEPTION: " + ex.Message);
            }
        }

        // Push user location to cloud
        private async void PushUserLocationAsync(string uuid)
        {
            if (_userID != null && !string.IsNullOrEmpty(uuid))
            {
                // Prepare content for REST request
                if (_userLocationRestClient.UpdateContent("{\"Type\": \"IBeacon\", \"DeviceID\": \"" + uuid + "\"}"))
                {
                    try
                    {
                        // Make REST call to push user location
                        string responseValue = await _userLocationRestClient.MakeRequestAsync();
                        Utility.DebugLog("Response [coupons(s)]: " + responseValue);

                        // Deserialize the response value into coupon(s)
                        List<Coupon> coupons = JsonConvert.DeserializeObject<List<Coupon>>(responseValue);

                        // Add sample Coupons to received Coupon list (for debugging purpose; when real Coupons not available)
                        if (_isSimulateCouponData) { coupons = GetMockCoupons(); }

                        // Add received Coupons to list
                        AddCouponsToReceivedList(coupons);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error in REST call for pushing user location. EXCEPTION: " + ex.Message);
                    }
                }
            }
        }

        // Add Coupons to received coupon-list
        private void AddCouponsToReceivedList(List<Coupon> coupons)
        {
            // Add only the Coupons which are not received already
            var couponsToAdd = coupons?.Where(c => !ReceivedCoupons.Any(rC => rC.CouponID == c.CouponID));
            if (couponsToAdd?.Any() ?? false)
            {
                var couponList = couponsToAdd.ToList();
                couponList?.ForEach(ReceivedCoupons.Add);
                OnNewCouponReceived(couponList);
            }
        }
        
        // Trigger event when new Coupons received
        private void OnNewCouponReceived(List<Coupon> newCoupons)
        {
            EventHandler<NewCouponReceivedEventArgs> handler = NewCouponReceived;
            if (handler != null && newCoupons != null)
            {
                handler(this, new NewCouponReceivedEventArgs(newCoupons));
            }
        }

        #endregion

        #region Helper methods
        
        private Beacon GetAMockProximityBeacon()
        {
            // Create a mock iBeacon
            BLEAdvertisementPacket packet = new BLEAdvertisementPacket()
            {
                BluetoothAddress = 73699147514185,
                RawSignalStrengthInDBm = -62,
                Timestamp = DateTime.Now
            };
            Beacon beacon = new Beacon(packet);
            beacon.BeaconType = Beacon.BeaconTypeEnum.iBeacon;
            beacon.BeaconFrames = new ObservableCollection<BeaconFrameBase>()
            {
                new ProximityBeaconFrame(new byte[] { 2, 21, 91, 240, 232, 154, 87, 96, 74, 157, 187, 138, 122, 137, 92, 156, 153, 178, 0, 1, 0, 0, 191 })
            };

            return beacon;
        }

        private List<Coupon> GetMockCoupons()
        {
            // Return a list of mock Coupons
            return new List<Coupon>()
            {
                new Coupon() { CouponID = 2, ClientSpotID = 1, NotificationID = 2, Name = "Doughnut Coupon", CouponCode = "09876543210", Description = "SAVE $1.99", DiscountCents = 199.0 },
                new Coupon() { CouponID = 3, ClientSpotID = 1, NotificationID = 3, Name = "Croissant Coupon", CouponCode = "92186293264", Description = "SAVE $0.49", DiscountCents = 49.0 }
            };
        }

        private void Beacons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //switch (e.Action)
            //{
            //    case NotifyCollectionChangedAction.Add:
            //        //DebugLog("Beacons CollectionChanged: Added");
            //        break;
            //    case NotifyCollectionChangedAction.Remove:
            //        //DebugLog("Beacons CollectionChanged: Removed");
            //        break;
            //    case NotifyCollectionChangedAction.Reset:
            //        //DebugLog("Beacons CollectionChanged: Reset");
            //        break;
            //    default:
            //        DebugLog("Beacons CollectionChanged: Other");
            //        break;
            //}
        }

        #endregion
    }

    public class NewCouponReceivedEventArgs : EventArgs
    {
        private readonly List<Coupon> _newCoupons;

        public NewCouponReceivedEventArgs(List<Coupon> newCoupons)
        {
            _newCoupons = newCoupons;
        }

        public List<Coupon> NewCoupons
        {
            get { return _newCoupons; }
        }
    }
}
