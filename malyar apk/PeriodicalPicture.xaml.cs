using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace malyar_apk
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PeriodicalPicture : ContentView
    {
        //private Label[] labels_with_time;
        //private byte num_of_spans_filled_in = 0, num_of_time_picks = 0;
        
        /// <summary>
        /// Value bigger than one, because this can have 16:9, 20:9, 1280:720 etc, literally solved
        /// </summary>
        static private double ScreenDimensionsRatio;

        public PeriodicalPicture()
        {
            InitializeComponent();
            //wallpaper.Source = ImageSource.FromFile(@"images\plus_add.png");
        }

        public PeriodicalPicture(ImageSource source)
        {
            InitializeComponent();
            wallpaper.Source = source;
        }

        public static void InitializeImageBoundsRatio()
        {
            double ratio = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Height;
            ScreenDimensionsRatio = ratio >= 1 ? ratio : 1 / ratio;
        }

        private async void Wallpaper_Tapped(object sender, EventArgs e)
        {
            FileResult result = await FilePicker.PickAsync(PickOptions.Images);
            if (result == null)
                return;

            (sender as Image).Source = ImageSource.FromFile(result.FullPath);
 
        }


        public static int etalon_width { get; private set; }
        public static int etalon_height { get; private set; }

        public void PreserveShape(bool horizontal)
        {

            if (horizontal)
            {
                if (etalon_width <= 0 && wallpaper.Width > 0)
                    etalon_width = (int)wallpaper.Width;

                wallpaper.MinimumHeightRequest = 0;
                wallpaper.WidthRequest = (etalon_height <= 0? wallpaper.Height : etalon_height) * (1/ScreenDimensionsRatio);
          
            }
            else
            {
                if (etalon_height <= 0 && wallpaper.Height > 0)
                    etalon_height = (int)wallpaper.Height;

                wallpaper.MinimumWidthRequest = 0;
                wallpaper.HeightRequest = (etalon_width <= 0? wallpaper.Width : etalon_width)  * ScreenDimensionsRatio;

            }

            HorizontalOptions = horizontal ? LayoutOptions.Start : LayoutOptions.Fill;
        }
    }
}