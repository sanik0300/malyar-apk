using System;
using System.Collections.Generic;
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

        private TimedPictureModel actual_schedule_part = new TimedPictureModel();
        
        private double previous_count_of_minutes = HoursPerWallpaperBydefault * 60;
    
        public SchedulePiece()
        {
            InitializeComponent();
            choose_end.Time = TimeSpan.FromHours(HoursPerWallpaperBydefault);
        }

        public SchedulePiece(string path_to_img, TimeSpan start, TimeSpan end)
        {
            InitializeComponent();
            if (path_to_img == TimedPictureModel.just_original)
            {
                IMagesMediator mediator = DependencyService.Get<IMagesMediator>();
                wallpaper.Source = ImageSource.FromStream(() => new MemoryStream(mediator.GetOriginalWP()));
            }
            this.choose_start.Time = start;
            
            if (end.Days >= 1)//24:00 and maybe more
            {
                timespan_panel.Children[0].IsEnabled = false;

                timespan_panel.Children[1] = new Label()
                {
                    Text =$"{end.TotalHours}:{end.Minutes}",
                    HorizontalTextAlignment = TextAlignment.Center,
                    FontSize = 20,
                    TextColor = choose_start.TextColor,
                    Background = Brush.Transparent,
                    WidthRequest = choose_start.Width,
                    HeightRequest = choose_start.Height,
                    BackgroundColor = Color.White
                };
            }
            else {
                this.choose_end.Time = end;
                AdjustSizeToMinutes(); 
            }

        }

        private void AdjustSizeToMinutes()
        {
            int default_minutes = HoursPerWallpaperBydefault * 60;
            bool horizontal = DeviceDisplay.MainDisplayInfo.Width > DeviceDisplay.MainDisplayInfo.Height;
            int current_min_minutes = horizontal ? MinMinutesToShowHRZ : MinMinutesToShowVERT;       

            if (previous_count_of_minutes < default_minutes || actual_schedule_part.DurationInMinutes < default_minutes)
            {
                //Увеличиваем/уменьшаем высоту/ширину во столько раз, во сколько новое кол-во минут больше старого!
                double multiplier = Math.Max(Math.Min(actual_schedule_part.DurationInMinutes, default_minutes), current_min_minutes) 
                                    / Math.Max(Math.Min(previous_count_of_minutes, default_minutes), current_min_minutes);

                if (horizontal)//то есть если горизонтально
                {
                    WidthRequest = this.Width * multiplier;
                }
                else
                {
                    HeightRequest = this.Height * multiplier + 20 * (1 - multiplier);

                    timespan_panel.Orientation = actual_schedule_part.DurationInMinutes < HRZ_Switch_Minutes ? StackOrientation.Horizontal : StackOrientation.Vertical;              
                    
                    if (actual_schedule_part.DurationInMinutes < HRZ_Switch_Minutes && previous_count_of_minutes >= HRZ_Switch_Minutes)
                    {
                        grid.Children.Remove(del_button);
                        to_store_del_swiftly.Children.Add(del_button);
                    }
                    else if(actual_schedule_part.DurationInMinutes >= HRZ_Switch_Minutes && previous_count_of_minutes < HRZ_Switch_Minutes)
                    {
                        to_store_del_swiftly.Children.Remove(del_button);
                        grid.Children.Add(del_button, 1, 1);
                    }

                    if(actual_schedule_part.DurationInMinutes <= ROWSP_Switch_Minutes && previous_count_of_minutes > ROWSP_Switch_Minutes)
                    {
                        grid.RowDefinitions[0].Height = 15;
                        
                        Grid.SetColumn(filepath_place, 1);
                        Grid.SetColumnSpan(filepath_place, 1);

                        Grid.SetRow(wallpaper, 0);
                        Grid.SetRowSpan(wallpaper, 3);

                        Grid.SetRow(to_store_del_swiftly, 1);
                        Grid.SetRowSpan(to_store_del_swiftly, 2);
                    }
                    else if(actual_schedule_part.DurationInMinutes > ROWSP_Switch_Minutes && previous_count_of_minutes <= ROWSP_Switch_Minutes)
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
            }
            InvalidateMeasure();
        }
        
        private void TimePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Time")
                return;
           
            if(sender == choose_end) {
                actual_schedule_part.end_time = choose_end.Time;
            }
            else {
                actual_schedule_part.start_time = choose_start.Time;
            }

            AdjustSizeToMinutes();

            previous_count_of_minutes = actual_schedule_part.DurationInMinutes;
        }
    }
}