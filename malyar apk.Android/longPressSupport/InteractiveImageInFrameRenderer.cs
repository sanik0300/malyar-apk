using Android.Content;
using Android.Views;
using malyar_apk.Droid;
using malyar_apk;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(InteractiveImage), typeof(InteractiveImageInFrameRenderer))]
namespace malyar_apk.Droid
{
    internal class InteractiveImageInFrameRenderer : ImageRenderer
    {
        InteractiveImage renderedView;
        GestureDetector.SimpleOnGestureListener _listener;
        GestureDetector _detector;

        public InteractiveImageInFrameRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                renderedView = e.NewElement as InteractiveImage;

                _listener = (renderedView.Parent is PhoneFrame) ? new GestureRecognizerWithLongPress(renderedView) : new GestureDetector.SimpleOnGestureListener();
                _detector = new GestureDetector(_listener);

                GenericMotion += (s, a) => _detector.OnTouchEvent(a.Event);
                Touch += (s, a) => _detector.OnTouchEvent(a.Event);
            }
        }

    }
}