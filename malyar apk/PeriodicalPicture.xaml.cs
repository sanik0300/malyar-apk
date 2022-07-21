using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace malyar_apk
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PeriodicalPicture : ContentView, ICloneable, IComparable
    {    
        /// <summary>
        /// Value bigger than one, because this can have 16:9, 20:9, 1280:720 etc, literally solved
        /// </summary>
        static private double ScreenDimensionsRatio;

        public event EventHandler FilledIn;
        
        private TimedPictureModel actual_schedule_piece = new TimedPictureModel();

        public PeriodicalPicture()
        {
            InitializeComponent();
        }

        public PeriodicalPicture(ImageSource source)
        {
            InitializeComponent();
            wallpaper.Source = source;
            this.actual_schedule_piece.path_to_wallpaper = TimedPictureModel.just_original;
            this.when_end.Time = this.actual_schedule_piece.end_time = new TimeSpan(23, 59, 0);
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
            
            this.actual_schedule_piece.path_to_wallpaper = result.FullPath;
            
            wallpaper.Aspect = Aspect.AspectFill;
            this.times.Opacity = 1;//это можно вынести в стиль
            this.times.IsEnabled = true;

            if (FilledIn != null && actual_schedule_piece.Complete)
                FilledIn.Invoke(sender, e);
        }

        public void PreserveShape(bool horizontal)
        {
            if (horizontal)
            {         
                wallpaper.MinimumHeightRequest = 0;
                wallpaper.WidthRequest = wallpaper.Height * (1/ScreenDimensionsRatio);
            }
            else
            {            
                wallpaper.MinimumWidthRequest = 0;
                wallpaper.HeightRequest = wallpaper.Width * ScreenDimensionsRatio;
            }

            HorizontalOptions = horizontal ? LayoutOptions.Start : LayoutOptions.Fill;
        }

        private void time_picker_time_changed(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Time")
                return;

            if (sender == when_begin) { this.actual_schedule_piece.start_time = (sender as TimePicker).Time; }
            else { this.actual_schedule_piece.end_time = (sender as TimePicker).Time; }

            if (FilledIn != null && actual_schedule_piece.Complete)
                FilledIn.Invoke(sender, e);
        }

        public void Swap(PeriodicalPicture other)
        {
            string temp_for_img_path = this.actual_schedule_piece.path_to_wallpaper;
            ImageSource temp_for_source = this.wallpaper.Source;
            TimeSpan temp_for_start=this.actual_schedule_piece.start_time, temp_for_end=this.actual_schedule_piece.end_time;

            this.actual_schedule_piece.path_to_wallpaper = other.actual_schedule_piece.path_to_wallpaper;
            other.actual_schedule_piece.path_to_wallpaper = temp_for_img_path;

            this.wallpaper.Source = other.wallpaper.Source;
            other.wallpaper.Source = temp_for_source;

            this.when_begin.Time = other.when_begin.Time;
            this.when_end.Time = other.when_end.Time;
            other.when_begin.Time = temp_for_start;
            other.when_end.Time = temp_for_end;
        }
        public static void Join(PeriodicalPicture prev, PeriodicalPicture current, PeriodicalPicture residual)
        {
            //после swap`a prev всегда длиннее
            residual.when_begin.Time = current.when_end.Time;
            prev.when_end.Time = current.when_begin.Time;
        }
        public static void Join(PeriodicalPicture prev, PeriodicalPicture current, TimespanCompareResult relation)
        {
            switch (relation)
            {
                case TimespanCompareResult.EARLIER:
                    current.when_begin.Time = prev.when_end.Time;
                    break;
                case TimespanCompareResult.LATER:
                    prev.when_end.Time = current.when_begin.Time;
                    break;
                case TimespanCompareResult.EQUAL:
                    current.when_begin.Time = prev.when_end.Time = TimeSpan.FromMinutes((prev.when_begin.Time.Minutes + current.when_end.Time.Minutes) / 2);
                    break;
            }
        }

        public object Clone()
        {
            PeriodicalPicture copy = new PeriodicalPicture();
            copy.wallpaper.Source = this.wallpaper.Source;
            copy.when_begin.Time = this.when_begin.Time;
            copy.when_end.Time = this.when_end.Time;
            return copy;
        }

        public int CompareTo(object obj)
        {
            PeriodicalPicture that = obj as PeriodicalPicture;
            if (that is null) { throw new TypeAccessException("опять-таки ты чё пёс шо ты сюда вложил?"); }

            return this.actual_schedule_piece.CompareTo(that.actual_schedule_piece);
        }
    }
}