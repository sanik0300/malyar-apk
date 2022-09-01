using Java.Lang;
using Android.OS;
using malyar_apk.Shared;

namespace malyar_apk.Droid
{
    class TPModelParcelableCreator : Object, IParcelableCreator
    {
        public Object CreateFromParcel(Parcel source)
        {
            return new TPModelParcelable(
                new TimedPictureModel(
                    source.ReadString(),
                    System.TimeSpan.FromMinutes(source.ReadInt()),
                    System.TimeSpan.FromMinutes(source.ReadInt())
                    ));
        }

        public Object[] NewArray(int size)
        {
            return new Object[size];
        }
    }
}