﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace malyar_apk
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SchedulePiece : ContentView
    {
        public const int HoursPerWallpaperBydefault = 4;
        /// <summary>
        /// Количество минут на таймере при котором кнопка делете переходит вплотную к таймеру (в вертикальной ориентации)
        /// </summary>
        private static int HRZ_Switch_Minutes = HoursPerWallpaperBydefault * 60 / 2;
        /// <summary>
        /// Количество минут на таймере, при котором панель с таймером и кнопкой делете растягивается на 2 строки сетки
        /// </summary>
        private static int ROWSP_Switch_Minutes = HoursPerWallpaperBydefault*60 / 4;
        
        private static int MinMinutesToShowHRZ=HoursPerWallpaperBydefault*60/4*3/2, MinMinutesToShowVERT = HoursPerWallpaperBydefault*60/4/4*3;

        private static IMagesMediator mediator = DependencyService.Get<IMagesMediator>();
        private TimedPictureModel actual_schedule_part;

        private double previous_count_of_minutes = HoursPerWallpaperBydefault * 60;
        private bool has_initialized = false, property_changing_from_inside = false;
        
        public double DimensionMultiplier { get {

                if (actual_schedule_part == null)
                    return 1;
                int default_minutes = HoursPerWallpaperBydefault * 60;
                bool horizontal = DeviceDisplay.MainDisplayInfo.Width > DeviceDisplay.MainDisplayInfo.Height;
                int current_min_minutes = horizontal ? MinMinutesToShowHRZ : MinMinutesToShowVERT;
               
                return Math.Max(Math.Min(actual_schedule_part.DurationInMinutes, default_minutes), current_min_minutes)
                                    / Math.Max(Math.Min(previous_count_of_minutes, default_minutes), current_min_minutes);
            }
        }

        public SchedulePiece(TimedPictureModel model)
        {                
            this.actual_schedule_part = model;
            model.PropertyChanged += schedule_part_PropertyChanged;
            
            InitializeComponent();
     
            this.choose_start.Time = model.start_time;
            this.choose_end.Time = TimedPictureModel.ClampTimespan(model.end_time);

            if (model.path_to_wallpaper == TimedPictureModel.just_original || model.path_to_wallpaper == null)
            {
                wallpaper.Source = ImageSource.FromStream(() => new MemoryStream(mediator.GetOriginalWP()));
                model.path_to_wallpaper = TimedPictureModel.just_original;//на всякий случай, а то эта переменная чё-то может пропадать в процессе
            }
            else { 
                wallpaper.Source = ImageSource.FromFile(model.path_to_wallpaper);     
            }
            this.filepath_here.Text = model.path_to_wallpaper.ToLower(); 
  
            has_initialized = true;
        }

        private void schedule_part_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            property_changing_from_inside = true;
            switch (e.PropertyName)
            {
                case nameof(actual_schedule_part.start_time):
                    this.choose_start.Time = TimedPictureModel.ClampTimespan(actual_schedule_part.start_time);
                    break;
                case nameof(actual_schedule_part.end_time):
                    this.choose_end.Time = TimedPictureModel.ClampTimespan(actual_schedule_part.end_time);
                    break;
            }
            property_changing_from_inside = false;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            
            if (width < 0 || height < 0) { return; }

            if (previous_count_of_minutes >= HoursPerWallpaperBydefault * 60 && actual_schedule_part.DurationInMinutes >= HoursPerWallpaperBydefault * 60)
                return;

            bool horizontal = DeviceDisplay.MainDisplayInfo.Width > DeviceDisplay.MainDisplayInfo.Height;

            timespan_panel.Orientation = !horizontal && actual_schedule_part.DurationInMinutes < HRZ_Switch_Minutes ? StackOrientation.Horizontal : StackOrientation.Vertical;

            if (!horizontal && actual_schedule_part.DurationInMinutes < HRZ_Switch_Minutes && previous_count_of_minutes >= HRZ_Switch_Minutes)
            {
                grid.Children.Remove(del_almost_button);
                to_store_del_swiftly.Children.Add(del_almost_button);
            }

            if (horizontal || (actual_schedule_part.DurationInMinutes >= HRZ_Switch_Minutes && previous_count_of_minutes < HRZ_Switch_Minutes))
            {
                to_store_del_swiftly.Children.Remove(del_almost_button);
                grid.Children.Add(del_almost_button, 1, 1);
            }

            if (!horizontal && actual_schedule_part.DurationInMinutes <= ROWSP_Switch_Minutes && previous_count_of_minutes > ROWSP_Switch_Minutes)
            {
                grid.RowDefinitions[0].Height = 15;

                Grid.SetColumn(filepath_place, 1);
                Grid.SetColumnSpan(filepath_place, 1);

                Grid.SetRow(wallpaper, 0);
                Grid.SetRowSpan(wallpaper, 3);

                Grid.SetRow(to_store_del_swiftly, 1);
                Grid.SetRowSpan(to_store_del_swiftly, 2);
            }
            else if (horizontal || (actual_schedule_part.DurationInMinutes > ROWSP_Switch_Minutes && previous_count_of_minutes <= ROWSP_Switch_Minutes))
            {
                grid.RowDefinitions[0].Height = 20;

                Grid.SetColumn(filepath_place, 0);
                Grid.SetColumnSpan(filepath_place, 2);

                Grid.SetRow(wallpaper, 1);
                Grid.SetRowSpan(wallpaper, 2);

                Grid.SetRow(to_store_del_swiftly, 2);
                Grid.SetRowSpan(to_store_del_swiftly, 1);
            }
        }
       

        private void TimePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Time")
                return;

            (sender as View).IsEnabled = (sender as TimePicker).Time > TimeSpan.Zero;

            if (!has_initialized) { return; }
               
            if (!property_changing_from_inside)
            {
                if (choose_start.Time >= choose_end.Time)
                {
                    Vibration.Vibrate(200);
                    mediator.OuchBadTimespan("введён невозможный отрезок времени", this);

                    property_changing_from_inside = true; //защита от рекурсии просто отпад :))
                    (sender as TimePicker).Time = (sender == choose_start) ? actual_schedule_part.start_time : actual_schedule_part.end_time;
                    property_changing_from_inside = false;

                    return;
                }

                if (sender == choose_end) {
                    if(choose_end.Time == TimeSpan.Zero) {
                        actual_schedule_part.end_time = TimeSpan.FromDays(1);
                        choose_end.IsEnabled = false;
                    }
                    else { actual_schedule_part.end_time = choose_end.Time; }                   
                }
                else {
                    actual_schedule_part.start_time = choose_start.Time;
                }
                TimedPicturesLoader.FitIntervalIn((sender == choose_end)? ChangeDirection.AffectDownwards : ChangeDirection.AffectUpwards, actual_schedule_part);
            }
            
            previous_count_of_minutes = actual_schedule_part.DurationInMinutes;
        }

        private void wallpaper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Source" || this.actual_schedule_part == null)
                return;

            this.actual_schedule_part.path_to_wallpaper = this.filepath_here.Text = ((sender as Image).Source as FileImageSource)?.File;     
        }

        private void img_almost_Tapped(object sender, EventArgs e) { mediator.DeliverToast("Жмите 2 раза, чтобы поменять картинку"); }
        private async void source_img_really_Tapped(object sender, EventArgs e)
        {
            FileResult result = await FilePicker.PickAsync(PickOptions.Images);
            if (result == null)
                return;
            (sender as Image).Source = ImageSource.FromFile(result.FullPath);
        }

        private void delete_Touched(object sender, EventArgs e) { mediator.DeliverToast("Жмите 2 раза, чтобы удалить"); }
        private void delete_Tapped(object sender, EventArgs e)
        {
            TimedPicturesLoader.DeleteInterval(this.actual_schedule_part);
        }
    }
}