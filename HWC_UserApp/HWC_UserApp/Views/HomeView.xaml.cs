using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using HWC_UserApp.UserApp.Models;
using HWC_UserApp.ViewModels;

namespace HWC_UserApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HomeView : ContentPage
	{
        #region Initialize

        public HomeView()
		{
			InitializeComponent();
            
            ShowDefaultGrid();

            var viewModel = new HomeViewModel();

            BindingContext = viewModel;
            viewModel.NewCouponReceived += ViewModel_NewCouponReceived;
        }

        #endregion

        #region Private methods

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