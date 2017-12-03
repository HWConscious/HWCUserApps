using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using HWC_UserApp.UserApp.Models;
using HWC_UserApp.ViewModels;

using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Library.Core.Interop;

namespace HWC_UserApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HomeView : ContentPage
	{
        #region Initialize

        public HomeView()
		{
			InitializeComponent();

            // Prepare sources for beacon scanning UI indicator
            var scanningImage = "https://s3.amazonaws.com/hwconscious/media/image_scanning_beacon.gif";
            _defaultScanning.Source = new HtmlWebViewSource() { Html = "<html><body><img style='position:absolute;margin:auto;left:4%;top:0;bottom:0;width:90%;' src='" + scanningImage + "' /></body></html>" };
            _bottomScanning.Source = new HtmlWebViewSource() { Html = "<html><body><img style='position:absolute;margin:auto;left:2%;top:0;bottom:0;width:90%;' src='" + scanningImage + "' /></body></html>" };

            // Show the default UI grid
            ShowDefaultGrid();

            // Construct view model and allocate to context of the page
            var viewModel = new HomeViewModel();
            viewModel.BeaconScanningStarted += ViewModel_BeaconScanningStarted;
            viewModel.BeaconScanningStopped += ViewModel_BeaconScanningStopped;
            viewModel.BeaconDetected += ViewModel_BeaconDetected;
            viewModel.BeaconNotDetected += ViewModel_BeaconNotDetected;
            viewModel.NewCouponReceived += ViewModel_NewCouponReceived;
            BindingContext = viewModel;
        }

        #endregion

        #region Private methods

        private void ViewModel_BeaconScanningStarted(object sender, EventArgs e)
        {
            ShowBeaconScanning();
        }

        private void ViewModel_BeaconScanningStopped(object sender, BTError e)
        {
            HideBeaconScanning();
        }

        private void ViewModel_BeaconDetected(object sender, Beacon e)
        {
            ShowBeaconDetected();
        }

        private void ViewModel_BeaconNotDetected(object sender, EventArgs e)
        {
            HideBeaconDetected();
        }

        private void ViewModel_NewCouponReceived(object sender, NewCouponReceivedEventArgs e)
        {
            HideDefaultGrid();
            ShowCouponListGrid();
            Utility.DebugLog(e.NewCoupons.Count + " new Coupon(s) received");
        }

        #endregion

        #region Helper methods

        private void ShowDefaultGrid()
        {
            _defaultContainerGrid.FadeTo(1, 300);
        }

        private void HideDefaultGrid()
        {
            _defaultContainerGrid.FadeTo(0, 300, Easing.SinOut);
        }

        private void ShowBeaconScanning()
        {
            _defaultScanning.FadeTo(1, 300);
            _bottomScanning.FadeTo(1, 300);
        }

        private void HideBeaconScanning()
        {
            _defaultScanning.FadeTo(0, 300, Easing.SinOut);
            _bottomScanning.FadeTo(0, 300, Easing.SinOut);
        }

        private void ShowBeaconDetected()
        {
            _defaultScanningTransparentLayer.Opacity = 0;
            _bottomScanningTransparentLayer.Opacity = 0;
        }

        private void HideBeaconDetected()
        {
            var transparencyFactor = 0.6;
            _defaultScanningTransparentLayer.Opacity = transparencyFactor;
            _bottomScanningTransparentLayer.Opacity = transparencyFactor;
        }

        private void ShowCouponListGrid()
        {
            _couponListContainerGrid.FadeTo(1, 900, Easing.SinIn);
            _bottomLogoImage.FadeTo(1, 1800, Easing.SinIn);
            _bottomLogoImage.TranslateTo(0, 0, 1200, Easing.SinOut);
        }

        private void HideCouponListGrid()
        {
            _couponListContainerGrid.FadeTo(0, 300, Easing.SinOut);
            _bottomLogoImage.FadeTo(0, 300, Easing.SinOut);
            _bottomLogoImage.TranslateTo(0, 110, 300, Easing.SinIn);
        }

        #endregion
    }
}