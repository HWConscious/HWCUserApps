using System;
using System.Diagnostics;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using HWC_UserApp.ViewCells;

using UniversalBeacon.Library.Core.Entities;

namespace HWC_UserApp.UserApp.Models
{
    #region Utility models

    /// <summary>
    /// Utility methods
    /// </summary>
    public class Utility
    {
        #region Public methods

        /// <summary>
        /// Writes log string in the output window when debugging.
        /// </summary>
        /// <param name="logString">Log string</param>
        public static void DebugLog(string logString)
        {
            if (!string.IsNullOrEmpty(logString))
            {
                Debug.WriteLine("[" + DateTime.Now.ToString() + "] " + logString);
            }
        }

        #endregion
    }

    /// <summary>
    /// XAML extension to provide image-source from embedded resource in PCL.
    /// </summary>
    [ContentProperty("Source")]
    public class ImageResourceExtension : IMarkupExtension
    {
        public string Source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source == null) { return null; }
            return ImageSource.FromResource(Source);
        }
    }

    /// <summary>
    /// XAML extension to provide image-source from Uri in PCL.
    /// </summary>
    [ContentProperty("Source")]
    public class ImageUriExtension : IMarkupExtension
    {
        public string Source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source == null) { return null; }
            return ImageSource.FromUri(new Uri(Source));
        }
    }

    /// <summary>
    /// Provides dedicated DataTemplate based on object type.
    /// </summary>
    internal class ViewCellTemplateSelector : DataTemplateSelector
    {
        private readonly DataTemplate _couponTemplate;
        private readonly DataTemplate _genericBeaconTemplate;
        private readonly DataTemplate _iBeaconTemplate;

        public ViewCellTemplateSelector()
        {
            _couponTemplate = new DataTemplate(typeof(CouponViewCell));
            _genericBeaconTemplate = new DataTemplate(typeof(GenericBeaconViewCell));
            _iBeaconTemplate = new DataTemplate(typeof(IBeaconViewCell));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is Coupon)
            {
                return _couponTemplate;
            }
            else if (item is Beacon beacon)
            {
                if (beacon.BeaconType == Beacon.BeaconTypeEnum.iBeacon)
                {
                    return _iBeaconTemplate;
                }
                else
                {
                    return _genericBeaconTemplate;
                }
            }
            return null;
        }
    }

    #endregion

    #region Representational models

    /// <summary>
    /// Representation of Coupon.
    /// </summary>
    public class Coupon
    {
        public long CouponID { get; set; }
        public long ClientSpotID { get; set; }
        public long? NotificationID { get; set; }
        public string Name { get; set; }
        public string CouponCode { get; set; }
        public string Description { get; set; }
        public double DiscountCents { get; set; }
        public ImageSource BarcodeImageResource => ImageSource.FromResource("HWC_UserApp.UserApp.MediaFiles.barcodes.barcode_" + CouponCode + ".png");
    }

    #endregion
}
