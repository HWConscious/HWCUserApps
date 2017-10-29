using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using OpenNETCF.IoC;
using UniversalBeacon.Library;
using UniversalBeacon.Library.Core.Interfaces;

namespace HWC_UserApp.Droid
{
    [Activity(Label = "HWC_UserApp", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            // Create the bluetooth provider if it doesn't yet exist and add to the service collection
            var provider = RootWorkItem.Services.Get<IBluetoothPacketProvider>();
            if (provider == null)
            {
                provider = new AndroidBluetoothPacketProvider(this);
                RootWorkItem.Services.Add<IBluetoothPacketProvider>(provider);
            }

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new HWC_UserApp.App());
        }
    }
}

