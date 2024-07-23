using Android.App;
using Android.Content;
using Android.Hardware.Display;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace malyar_apk.Droid
{
    internal class RotationChangeListener : Java.Lang.Object, DisplayManager.IDisplayListener
    {
        private Action displayChangeAction;

        private Func<bool> flagGetter;
        private Action<bool> flagSetter;

        public RotationChangeListener(Action displayChangeAction, Func<bool> getter, Action<bool> setter)
        {
            this.displayChangeAction = displayChangeAction;
            this.flagGetter = getter;
            this.flagSetter = setter;
        }

        public void OnDisplayAdded(int displayId) { }

        public void OnDisplayChanged(int displayId)
        {
            if(!flagGetter.Invoke())
            {
                displayChangeAction?.Invoke();
            }
            flagSetter(false);
        }

        public void OnDisplayRemoved(int displayId) { }
    }
}