using System;
using System.ComponentModel;
using System.IO;
using malyar_apk.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace malyar_apk
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SchedulePiece : ContentView
    {
        /// <summary>
        /// Количество минут на таймере при котором кнопка делете переходит вплотную к таймеру (в вертикальной ориентации)
        /// </summary>
        private static int HRZ_Switch_Minutes = Constants.MinutesPerWallpaperByDefault / 2;
        /// <summary>
        /// Количество минут на таймере, при котором панель с таймером и кнопкой делете растягивается на 2 строки сетки
        /// </summary>
        private static int ROWSP_Switch_Minutes = Constants.MinutesPerWallpaperByDefault / 4;  
        private static int MinMinutesToShowHRZ= 90, MinMinutesToShowVERT = 60;

        private static IMagesMediator mediator = DependencyService.Get<IMagesMediator>();
        private TimedPictureModel actual_schedule_part;
        private double previous_count_of_minutes = Constants.MinutesPerWallpaperByDefault;
        private bool has_initialized = false,
                     property_changing_from_inside = false;
        private byte shrink_rate = 0;

        public event EventHandler SaveableChangeWasDone;

        public double DimensionMultiplier { 
            get {
                if (actual_schedule_part == null) { return 1; }          

                bool horizontal = DeviceDisplay.MainDisplayInfo.Width > DeviceDisplay.MainDisplayInfo.Height;
                int current_min_minutes = horizontal ? MinMinutesToShowHRZ : MinMinutesToShowVERT;

                return Math.Max(Math.Min(actual_schedule_part.DurationInMinutes, Constants.MinutesPerWallpaperByDefault), current_min_minutes) / Constants.MinutesPerWallpaperByDefault;
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

       static private double parent_layout_smaller_side_cached = 0;
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if(DeviceDisplay.MainDisplayInfo.Width > DeviceDisplay.MainDisplayInfo.Height)
            {
                if (parent_layout_smaller_side_cached == 0) {
                    parent_layout_smaller_side_cached = heightConstraint;       
                }
                else {
                    heightConstraint = parent_layout_smaller_side_cached;
                }
                return new SizeRequest(new Size(heightConstraint * DimensionMultiplier * Constants.UiHorizontalRatio, heightConstraint));
            }
            else {
                if (parent_layout_smaller_side_cached == 0)  {
                    parent_layout_smaller_side_cached = widthConstraint;
                }
                else  {
                    widthConstraint = parent_layout_smaller_side_cached;
                }
                return new SizeRequest(new Size(widthConstraint, widthConstraint * DimensionMultiplier * Constants.UiVerticalRatio));
            }
        }

        private double width = 0, height = 0;
        protected override void OnSizeAllocated(double width, double height)
        {
            if (this.width == width && this.height == height) { return; }
            bool horizontal = DeviceDisplay.MainDisplayInfo.Width > DeviceDisplay.MainDisplayInfo.Height;

            base.OnSizeAllocated(width, height);

            this.width = width;
            this.height = height;

            double padding_coef = Math.Max(2, 10 * DimensionMultiplier);
            akaphone.CornerRadius = (float)padding_coef;
            akaphone.Padding = new Thickness(padding_coef);
            
            if (previous_count_of_minutes >= Constants.MinutesPerWallpaperByDefault && actual_schedule_part.DurationInMinutes >= Constants.MinutesPerWallpaperByDefault)
                return;

            timespan_panel.Orientation = !horizontal && actual_schedule_part.DurationInMinutes < HRZ_Switch_Minutes ? StackOrientation.Horizontal : StackOrientation.Vertical;           

            double change = actual_schedule_part.DurationInMinutes / previous_count_of_minutes;
            if (change > 1)//if this' interval became longer
            {
                if (horizontal || (actual_schedule_part.DurationInMinutes > ROWSP_Switch_Minutes && shrink_rate > 1))
                {
                    grid.RowDefinitions[0].Height = 20;

                    Grid.SetColumn(filepath_place, 0);
                    Grid.SetColumnSpan(filepath_place, 2);

                    Grid.SetRow(akaphone, 1);
                    Grid.SetRowSpan(akaphone, 2);

                    Grid.SetRow(to_store_del_swiftly, 2);
                    Grid.SetRowSpan(to_store_del_swiftly, 1);

                    shrink_rate--;
                }
                
                if (horizontal || (actual_schedule_part.DurationInMinutes >= HRZ_Switch_Minutes && shrink_rate > 0))
                {
                    to_store_del_swiftly.Children.Remove(del_button);
                    grid.Children.Add(del_button, 1, 1);
                    shrink_rate--;
                }
            }
            else {
                if (!horizontal && actual_schedule_part.DurationInMinutes < HRZ_Switch_Minutes && shrink_rate < 1)
                {
                    grid.Children.Remove(del_button);
                    to_store_del_swiftly.Children.Add(del_button);
                    shrink_rate++;
                }
                if (!horizontal && actual_schedule_part.DurationInMinutes <= ROWSP_Switch_Minutes && shrink_rate < 2)
                {
                    grid.RowDefinitions[0].Height = 15;

                    Grid.SetColumn(filepath_place, 1);
                    Grid.SetColumnSpan(filepath_place, 1);

                    Grid.SetRow(akaphone, 0);
                    Grid.SetRowSpan(akaphone, 3);

                    Grid.SetRow(to_store_del_swiftly, 1);
                    Grid.SetRowSpan(to_store_del_swiftly, 2);

                    shrink_rate++;
                }
            } 
        }

        private void TimePicker_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this==null || e.PropertyName != "Time")
                return;

            (sender as View).IsEnabled = (sender as TimePicker).Time > TimeSpan.Zero;

            if (!has_initialized) { return; }
               
            previous_count_of_minutes = actual_schedule_part.DurationInMinutes;
            if (!property_changing_from_inside)
            {
                if (choose_start.Time >= choose_end.Time && actual_schedule_part.end_time.Days == 0)
                {
                    Vibration.Vibrate(200);
                    mediator.OuchError("введён невозможный отрезок времени", this);

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

                SaveableChangeWasDone.Invoke(this, null);
                TimedPicturesLoader.FitIntervalIn((sender == choose_end)? ChangeDirection.AffectDownwards : ChangeDirection.AffectUpwards, actual_schedule_part);
            }
        }

        private void wallpaper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Source" || this.actual_schedule_part == null)
                return;

            this.actual_schedule_part.path_to_wallpaper = this.filepath_here.Text = ((sender as Image).Source as FileImageSource)?.File;          

            if (SaveableChangeWasDone!=null)
                SaveableChangeWasDone.Invoke(this, null);
        }

        private void img_almost_Tapped(object sender, EventArgs e) { mediator.DeliverToast("Жмите 2 раза, чтобы поменять картинку"); }
        private async void source_img_really_Tapped(object sender, EventArgs e)
        {
            FileResult result = await FilePicker.PickAsync(PickOptions.Images);
            if (result == null)
                return;
            (sender as Image).Source = ImageSource.FromFile(result.FullPath);
        }

        private byte ClicksCount = 0;
        private void del_button_Clicked(object sender, EventArgs e)
        {
            TimeSpan tt = new TimeSpan(0, 0, 0, 0, 500);
            Device.StartTimer(tt, PotentialDeleteCallback);
            ClicksCount++;
        }
        private bool PotentialDeleteCallback()
        {
            switch (ClicksCount) {
                case 1:
                    mediator.DeliverToast("Жмите 2 раза, чтобы удалить");
                    break;
                case 2:
                    TimedPicturesLoader.DeleteInterval(this.actual_schedule_part);
                    break;
            }
            ClicksCount = 0;
            return false;
        }

        public void ProtectFromClickingDel(bool can_we_del)
        {
            this.del_button.IsEnabled = this.del_button.IsVisible = can_we_del;
        }
    }
}