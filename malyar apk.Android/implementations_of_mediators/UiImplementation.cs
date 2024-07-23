using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content.Res;
using System.Diagnostics;
using AndroidX.AppCompat.App;

[assembly: Dependency(typeof(malyar_apk.Droid.UiImplementation))]
namespace malyar_apk.Droid
{
    internal class UiImplementation : ContextDependentObject, I_UIMediator 
    {
        public void AdjustOpacity(VisualElement visualElement, float opacity)
        {
            Platform.GetRenderer(visualElement).View.Background.Alpha = (int)(255*opacity);
        }

        public void ConfirmThemeChangeOnPlatform(int themeId)
        {
            AppCompatDelegate.DefaultNightMode = themeId;
            BaseContext.Window.SetStatusBarColor(BaseContext.Resources.GetColor(Resource.Color.statusBarColor));
        }

        public int CountBackgrounds()
        {
            int counter = 0, resId;
            while (true)
            {
                string name = $"bg{counter + 1}";
                resId = BaseContext.Resources.GetIdentifier(name, "drawable", BaseContext.PackageName);
                if (resId <= 0)
                {
                    return counter;
                }
                counter++;
            };
        }

        public void FixBackgroundTiling(VisualElement visualElement)
        {
            IVisualElementRenderer renderer = Platform.GetRenderer(visualElement);
            if (renderer == null) { return; }

            Android.Views.View view = renderer.View;
            if (view == null) { return; }

            BitmapDrawable bgBmp = view.Background as BitmapDrawable;
            if (bgBmp == null) { return; }

            bool land = BaseContext.Resources.Configuration.Orientation == Android.Content.Res.Orientation.Landscape;
            bgBmp.Gravity = land ? GravityFlags.FillVertical : GravityFlags.FillHorizontal;
            bgBmp.SetTileModeXY(land ? Shader.TileMode.Repeat : null, land ? null : Shader.TileMode.Mirror);
        }

        public bool? IsSystemDarkTheme() 
        {
            if((int)Build.VERSION.SdkInt < 29) { return null; }
            
            return (Android.App.Application.Context.Resources.Configuration.UiMode & UiMode.NightMask) == UiMode.NightYes;     
        }
    }
}