using Android.OS;
using Android.Runtime;
using malyar_apk.Shared;
using Java.Interop;

namespace malyar_apk.Droid
{
    class TPModelParcelable : Java.Lang.Object, IParcelable
    {
        public TimedPictureModel source { get; private set; }
        public int DescribeContents()
        {
            return 2;
        }

        public TPModelParcelable(TimedPictureModel model) { source = model; }
      
        public void WriteToParcel(Parcel dest, [GeneratedEnum] ParcelableWriteFlags flags)
        {
            dest.WriteString(source.path_to_wallpaper);
            dest.WriteInt((int)source.StartTime.TotalMinutes);
            dest.WriteInt((int)source.EndTime.TotalMinutes);
        }

        [ExportField("CREATOR")]
        public static TPModelParcelableCreator InitializeCreator()
        {
            return new TPModelParcelableCreator();
        }
    }
}