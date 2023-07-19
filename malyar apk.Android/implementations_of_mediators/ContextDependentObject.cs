using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using malyar_apk.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace malyar_apk.Droid
{
    //don't confuse this with context_bound_object ;)
    internal abstract class ContextDependentObject
    {
        static internal MainActivity BaseContext;

        static internal TPModelParcelable[] IlistToParcelables(IList<TimedPictureModel> collection)
        {
            var result = new TPModelParcelable[collection.Count];
            for (int i = 0; i < collection.Count; ++i)
            {
                result[i] = new TPModelParcelable(collection[i]);
            }
            return result;
        }

        static internal List<TimedPictureModel> ParcelablesToList(IParcelable[] collection)
        {
            var result = new List<TimedPictureModel>(collection.Length);
            foreach (IParcelable p in collection)
            {
                result.Add((p as TPModelParcelable).source);
            }
            return result;
        }
    }
}