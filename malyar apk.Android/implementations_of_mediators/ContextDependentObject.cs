using Android.App;
using Android.Content;
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
    //don't confuse this with context_bound_object ;)
    internal abstract class ContextDependentObject
    {
        static public MainActivity BaseContext;
        static public string path_to_schedule;
        
    }
}