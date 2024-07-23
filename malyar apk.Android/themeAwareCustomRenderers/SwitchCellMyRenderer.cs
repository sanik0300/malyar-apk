using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using malyar_apk.Droid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(SwitchCell), typeof(SwitchCellMyRenderer))]
namespace malyar_apk.Droid
{
    internal class SwitchCellMyRenderer : SwitchCellRenderer
    {
        TextView targetTextView;
        Android.Widget.Switch targetSwitch;
        Context crtContext;
        protected override Android.Views.View GetCellCore(Cell item, Android.Views.View convertView, ViewGroup parent, Context context)
        {
            ViewGroup topVG = base.GetCellCore(item, convertView, parent, context) as ViewGroup,
                      childVG=null;
            int childCount = topVG.ChildCount;
            for(int i = 0; i < childCount; i++)
            {
                Android.Views.View? crtv = topVG.GetChildAt(i);
                if(crtv is ViewGroup)
                {
                    childVG = crtv as ViewGroup;
                }
                if(crtv is Android.Widget.Switch)
                {
                    targetSwitch = crtv as Android.Widget.Switch;
                }
            }
            targetTextView = childVG.GetChildAt(0) as TextView;
            crtContext = context;

            return topVG;
        }

        protected override void OnCellPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            try {
                base.OnCellPropertyChanged(sender, args);
            }
            catch(ObjectDisposedException) { 
                return; }

            if(args.PropertyName!="OnColor") { return; }

            targetTextView.SetTextColor(crtContext.Resources.GetColor(Resource.Color.textColor));
        }
    }
}