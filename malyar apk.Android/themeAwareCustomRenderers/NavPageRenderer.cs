using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using malyar_apk.Droid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppCompat;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(ThemeAwareNavPageRenderer))]
namespace malyar_apk.Droid
{
    internal class ThemeAwareNavPageRenderer : NavigationPageRenderer
    {
        public ThemeAwareNavPageRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<NavigationPage> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null) { return; }

            NavigationPage renderedPage = e.NewElement as NavigationPage;
            (App.Current as App).UserAppThemeChanged += (sender, a) =>
            {
                renderedPage.BarBackgroundColor = ColorExtensions.ToColor(Resources.GetColor(Resource.Color.colorPrimary));
            };
        }
    }
}