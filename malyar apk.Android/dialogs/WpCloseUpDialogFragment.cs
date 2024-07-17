using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.ConstraintLayout.Widget;
using Android.Content;

using Xamarin.Essentials;
using System;
using malyar_apk.Shared;

namespace malyar_apk.Droid
{
    internal class DismissableOnTouchDialog : Dialog
    {
        public DismissableOnTouchDialog(Context context) : base(context) { }

        public override bool OnTouchEvent(MotionEvent e)
        {
            this.Dismiss();
            return base.OnTouchEvent(e);
        }
    }
    public class WpCloseUpDialogFragment : AndroidX.Fragment.App.DialogFragment
    {
        double screenRatio;
        string pathToWP;
        //float[] lastOrientationChangeValues;

        public WpCloseUpDialogFragment(string pathToWP, double screenRatio=0)
        {
            this.pathToWP = pathToWP;
            this.screenRatio = screenRatio;
        }

        private void ViewTreeObserver_GlobalLayout(object sender, EventArgs e)
        {
            if (this.Resources.Configuration.Orientation != Android.Content.Res.Orientation.Landscape
                || !Convert.ToBoolean(Preferences.Get(Constants.HRZ_WP_DIALOG_MODE, 0))) { return; }

            SurfaceOrientation sfo = this.Context.GetSystemService(Android.Content.Context.WindowService).JavaCast<IWindowManager>().DefaultDisplay.Rotation;
            rotationDependentLandscapeScaling(this.Dialog.FindViewById(Resource.Id.wp_view) as ImageView, sfo);
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState) => new DismissableOnTouchDialog(this.Context);
        
       public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
       {
            return inflater.Inflate(Resource.Layout.WallpaperModal, container, false);
       }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            this.View.ViewTreeObserver.GlobalLayout += ViewTreeObserver_GlobalLayout;

            (this.Dialog.FindViewById(Resource.Id.wp_view) as ImageView).SetImageBitmap(BitmapFactory.DecodeFile(pathToWP));

            fixPadding(this.Context.Resources.Configuration.Orientation);
        }

        private void fixPadding(Android.Content.Res.Orientation or)
        {
            View modalView = this.Dialog.FindViewById(Resource.Id.wp_linear);
            int paddingBigger = or == Android.Content.Res.Orientation.Landscape ? modalView.PaddingTop : modalView.PaddingLeft;
            int paddingSmaller = (int)(paddingBigger * screenRatio);

            if (or == Android.Content.Res.Orientation.Landscape)
            {
                modalView.SetPadding(paddingBigger, paddingSmaller, paddingBigger, paddingSmaller);
            }
            else
            {
                modalView.SetPadding(paddingSmaller, paddingBigger, paddingSmaller, paddingBigger);
            }
        }

        private void scaleImageView(ImageView imgv, Android.Content.Res.Orientation or)
        {
            if (or == Android.Content.Res.Orientation.Landscape && Convert.ToBoolean(Preferences.Get(Constants.HRZ_WP_DIALOG_MODE, 0)))
            {
                Matrix matrix = new Matrix();
                imgv.SetScaleType(ImageView.ScaleType.Matrix);
                Bitmap bmp = (imgv.Drawable as BitmapDrawable).Bitmap;

                matrix.PostRotate(90, bmp.Width / 2, bmp.Height / 2);
                imgv.ImageMatrix = matrix;

                /*if (lastOrientationChangeValues == null)
                {
                    lastOrientationChangeValues = new float[9];
                }
                matrix.GetValues(lastOrientationChangeValues);*/

                return;
            }

            imgv.SetScaleType(ImageView.ScaleType.CenterCrop);
        }

        public override void OnResume()
        {
            base.OnResume();
            Window wnd = this.Dialog.Window;
            wnd.SetBackgroundDrawable(new ColorDrawable(Color.White) { Alpha = 128 });
            wnd.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            wnd.SetGravity(GravityFlags.Center);

            if (this.Resources.Configuration.Orientation != Android.Content.Res.Orientation.Landscape) { return; }

            ImageView imageView = this.Dialog.FindViewById(Resource.Id.wp_view) as ImageView;

            if (Convert.ToBoolean(Preferences.Get(Constants.HRZ_WP_DIALOG_MODE, 0)))
            {
                 scaleImageView(imageView, this.Context.Resources.Configuration.Orientation);               
            }
            else {
                var lprms = imageView.LayoutParameters as ConstraintLayout.LayoutParams;
                lprms.MatchConstraintPercentWidth = (float)Math.Pow(screenRatio, 2);
                imageView.LayoutParameters = lprms;
            }
        }

        private void rotationDependentLandscapeScaling(ImageView imgv, SurfaceOrientation sfo)
        {
            Matrix matrix = imgv.ImageMatrix;

            Bitmap bmp = (imgv.Drawable as BitmapDrawable).Bitmap;

            float[] matrixValues = new float[9];
            matrix.GetValues(matrixValues);
            matrix.PostTranslate(0, matrixValues[Matrix.MtransY] * (sfo == SurfaceOrientation.Rotation90 ? 1 : -1));
            if (sfo == SurfaceOrientation.Rotation90)
            {
                matrix.PostScale(-1, -1, bmp.Width / 2, bmp.Height / 2);
            }

            bool isImgNarrow = (float)bmp.Width / bmp.Height < screenRatio;
            float scaleCoef = isImgNarrow ? (float)imgv.Height / bmp.Width : (float)imgv.Width / bmp.Height;

            matrix.PostScale(scaleCoef, scaleCoef, 0, 0);

            matrix.GetValues(matrixValues);
            //align bmp to top of imageview - in both cases 
            if (sfo == SurfaceOrientation.Rotation90)
            {
                matrix.PostTranslate(matrixValues[Matrix.MtransX] * -1, 0);
            }
            else
            { //rotation 270
                float xMove = imgv.Width - matrixValues[Matrix.MtransX];
                matrix.PostTranslate(xMove, 0);
            }

            if (isImgNarrow)
            { //center by x
                float prfH = (float)(bmp.Width / screenRatio);

                int mult = sfo == SurfaceOrientation.Rotation90 ? 1 : -1;
                matrix.PostTranslate((prfH - bmp.Height) / 2 * scaleCoef * mult, 0);
            }
            else { //center by y
                float prfW = (float)(bmp.Height * screenRatio);
                matrix.PostTranslate(0, (prfW - bmp.Width) / 2 * scaleCoef);
            }
            imgv.ImageMatrix = matrix;
        }

        public override void OnStop()
        {
            this.View.ViewTreeObserver.GlobalLayout -= ViewTreeObserver_GlobalLayout;
            base.OnStop();
        }

        /*public override void OnDestroyView()
        {
            if (this.Dialog != null && this.RetainInstance)
                Dialog.SetDismissMessage(null);
            base.OnDestroyView();
        }*/
    }
}