using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

using OpenNETCF.IoC;
using UniversalBeacon.Library;
using UniversalBeacon.Library.Core.Interfaces;

namespace HWC_UserApp.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            //var provider = RootWorkItem.Services.Get<IBluetoothPacketProvider>();
            //if (provider == null)
            //{
            //    provider = new IosBluetoothPacketProvider(this);
            //    RootWorkItem.Services.Add<IBluetoothPacketProvider>(provider);
            //}

            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }
}
