using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;


namespace malyar_apk.Droid
{
    internal class GestureRecognizerWithLongPress : GestureDetector.SimpleOnGestureListener
    {
        private InteractiveImage img;
        public GestureRecognizerWithLongPress(InteractiveImage img) { this.img = img; }

        public override void OnLongPress(MotionEvent e)
        { 
            img.OnLongPressed();
            base.OnLongPress(e);
        }

        public override bool OnSingleTapConfirmed(MotionEvent e)
        {
            img.OnTapped();
            return base.OnSingleTapConfirmed(e);

        }

        public override bool OnDoubleTap(MotionEvent e)
        {
            img.OnTappedTwice();
            return base.OnDoubleTap(e);
        }
    }
}