using System;
using System.ComponentModel;
using System.IO;
using malyar_apk.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.Xaml;

namespace malyar_apk
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SchedulePiece : ContentView
    {  
        private const int MaxIntervalWhenShrink = Constants.MinutesPerWallpaperByDefault / 2,
                           MaxIntervalWhenChangeGrid = Constants.MinutesPerWallpaperByDefault/4*3;

        private static IUXMediator mediator = DependencyService.Get<IUXMediator>();
        private static IOMediator iomdtr = DependencyService.Get<IOMediator>();
        
        private TimedPictureModel actual_schedule_part;
        private double previous_count_of_minutes = Constants.MinutesPerWallpaperByDefault;
        private bool has_initialized = false,
                     property_changing_from_inside = false,
                     image_defect = false;

        public event EventHandler SaveableChangeWasDone;

        private static int MinMinutesToShowHRZ = 120, MinMinutesToShowVERT = 105;
        public double DimensionMultiplier { 
            get {
                if (actual_schedule_part == null) { return 1; }          

                bool horizontal = DeviceDisplay.MainDisplayInfo.Width > DeviceDisplay.MainDisplayInfo.Height;
                int current_min_minutes = horizontal ? MinMinutesToShowHRZ : MinMinutesToShowVERT;

                return Math.Max(Math.Min(actual_schedule_part.DurationInMinutes, Constants.MinutesPerWallpaperByDefault), current_min_minutes) / Constants.MinutesPerWallpaperByDefault;
            }
        }

        static internal bool worth_asking_for_file = true;//first launch значит разрешений ещё не дали
        public SchedulePiece(TimedPictureModel model)
        {
            this.actual_schedule_part = model;
            model.PropertyChanged += schedule_part_PropertyChanged;

            InitializeComponent();
            this.choose_start.Time = model.start_time;
            this.choose_end.Time = TimedPictureModel.ClampTimespan(model.end_time);
            time_text_label.Text = model.GetTimeIntervalString();

            if (worth_asking_for_file) {
                UpdateImgRepresentation();
            }
            else {
                image_defect = true;
                MessagingCenter.Subscribe<IOMediator>(this, Constants.UpdateImg, OnMessageReceived);
                this.filepath_here.Text = model.path_to_wallpaper.Insert(0, "(НЕТ ДОСТУПА) ");
            }
           has_initialized = true;
        }
        private void OnMessageReceived(IOMediator sender)
        {
            UpdateImgRepresentation();
            MessagingCenter.Unsubscribe<IOMediator>(this, Constants.UpdateImg);
        }
        private void UpdateImgRepresentation()
        {
            bool fileexists = File.Exists(actual_schedule_part.path_to_wallpaper);
            image_defect = !fileexists;
            this.filepath_here.Text = fileexists ? actual_schedule_part.path_to_wallpaper : actual_schedule_part.path_to_wallpaper.Insert(0, "(НЕ НАЙДЕН) ");
            wallpaper.Source = fileexists ? ImageSource.FromFile(actual_schedule_part.path_to_wallpaper) : ImageSource.FromResource(Constants.FileNotFoundResourcePath);
        }
        public void PingImageRepresentation()
        {
            if(File.Exists(actual_schedule_part.path_to_wallpaper) != image_defect) { return; }
            UpdateImgRepresentation();
            //do ONLY if image-defect is "true" but the image file is actually ok 
            //or vice versa if file is actually cant be found but image-defect is "false"
        }

        private void schedule_part_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            property_changing_from_inside = true;
            switch (e.PropertyName)
            {
                case nameof(actual_schedule_part.path_to_wallpaper):
                    filepath_here.Text = actual_schedule_part.path_to_wallpaper;
                    wallpaper.Source = ImageSource.FromFile(actual_schedule_part.path_to_wallpaper);
                    set_orig_button.IsEnabled = true;

                    SaveableChangeWasDone?.Invoke(this, null);
                    break;
                case nameof(actual_schedule_part.start_time):
                    this.choose_start.Time = TimedPictureModel.ClampTimespan(actual_schedule_part.start_time);
                    time_text_label.Text = actual_schedule_part.GetTimeIntervalString();
                    break;
                case nameof(actual_schedule_part.end_time):
                    this.choose_end.Time = TimedPictureModel.ClampTimespan(actual_schedule_part.end_time);
                    time_text_label.Text = actual_schedule_part.GetTimeIntervalString();
                    break;
            }
            property_changing_from_inside = false;
        }

        static private double parent_bigger_side_cached, parent_smaller_side_cached;
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {                
            if(DeviceDisplay.MainDisplayInfo.Width > DeviceDisplay.MainDisplayInfo.Height) 
            {   //Case Horizontal: WConstraint ecть, HConstraint бесконечность
                if (widthConstraint == double.PositiveInfinity)
                {
                    widthConstraint = parent_bigger_side_cached;
                }
                else if(parent_bigger_side_cached==0) {
                    parent_bigger_side_cached = widthConstraint;
                }
                
                heightConstraint = widthConstraint * (DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Width);
                if(parent_smaller_side_cached<=0)
                {
                    parent_smaller_side_cached = heightConstraint;
                }

                return new SizeRequest(new Size(heightConstraint * DimensionMultiplier * Constants.UiHorizontalRatio, heightConstraint));
            }
            else { //Case Vertical: WConstraint ecть, HConstraint бесконечность 
                
                if (widthConstraint == double.PositiveInfinity)
                {
                    widthConstraint = parent_smaller_side_cached;
                }
                else if(parent_smaller_side_cached==0) {
                    parent_smaller_side_cached = widthConstraint;
                }

                heightConstraint = widthConstraint * DimensionMultiplier * Constants.UiVerticalRatio;

                return new SizeRequest(new Size(widthConstraint, heightConstraint));
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
            akaphone.CornerRadius = frameToClipImg.CornerRadius = (float)padding_coef;
            if(filepath_here.Width > 0)
            {
                filepath_place.ScrollToAsync(filepath_here.Width, 0, false);
            }

            bool change_grid_vert = !horizontal && actual_schedule_part.DurationInMinutes <= MaxIntervalWhenChangeGrid;
            double grid_offset = change_grid_vert ? 1 - Math.Pow(DimensionMultiplier, 2) : 0;
            grid.RowDefinitions[1].Height = new GridLength(1 + grid_offset, GridUnitType.Star);
            grid.RowDefinitions[2].Height = new GridLength(4 - grid_offset, GridUnitType.Star);

            if (previous_count_of_minutes >= Constants.MinutesPerWallpaperByDefault && actual_schedule_part.DurationInMinutes >= Constants.MinutesPerWallpaperByDefault)
                return;

            timespans_joined.Orientation = !horizontal && actual_schedule_part.DurationInMinutes <= MaxIntervalWhenShrink*1.25 ? StackOrientation.Horizontal : StackOrientation.Vertical;

            if (horizontal || (actual_schedule_part.DurationInMinutes > MaxIntervalWhenShrink))
            {
                Grid.SetColumn(filepath_place, 0);
                Grid.SetColumnSpan(filepath_place, 2);

                bool change_grid_hrz = horizontal && actual_schedule_part.DurationInMinutes < MaxIntervalWhenChangeGrid;

                Grid.SetColumn(buttons_container, change_grid_hrz ? 0 : 1);
                Grid.SetColumnSpan(buttons_container, change_grid_hrz ? 2 : 1);

                Grid.SetRow(akaphone, change_grid_hrz ? 2 : 1);
                Grid.SetRowSpan(akaphone, change_grid_hrz ? 1 : 2);
            }
            else {
                Grid.SetColumn(filepath_place, 1);
                Grid.SetColumnSpan(filepath_place, 1);

                Grid.SetRow(akaphone, 0);
                Grid.SetRowSpan(akaphone, 3);
            }
        }

        private void TimePicker_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this == null || e.PropertyName != "Time")
                return;

            (sender as View).IsEnabled = (sender as TimePicker).Time > TimeSpan.Zero;

            if (!has_initialized) { return; }
            previous_count_of_minutes = actual_schedule_part.DurationInMinutes;

            if (property_changing_from_inside) { return; }

            if (choose_start.Time < choose_end.Time || choose_end.Time==TimeSpan.Zero)
            {
                if (sender == choose_end)
                {
                    actual_schedule_part.end_time = choose_end.Time != TimeSpan.Zero ? choose_end.Time : TimeSpan.FromDays(1);
                }
                else { actual_schedule_part.start_time = choose_start.Time; }

                SaveableChangeWasDone.Invoke(this, null);
                TimedPicturesLoader.FitIntervalIn((sender == choose_end) ? ChangeDirection.AffectDownwards : ChangeDirection.AffectUpwards, actual_schedule_part);
                return;
            }

            Vibration.Vibrate(200);
            mediator.OuchError("введён невозможный отрезок времени", this);

            property_changing_from_inside = true; //защита от рекурсии просто отпад :))
            (sender as TimePicker).Time = (sender == choose_start) ? actual_schedule_part.start_time : actual_schedule_part.end_time;
            property_changing_from_inside = false;
        }

        private void img_almost_Tapped(object sender, EventArgs e) => mediator.DeliverToast("Жмите 2 раза, чтобы поменять картинку"); 

        private void source_img_really_Tapped(object sender, EventArgs e) 
        {
            IPermitMediator permit = DependencyService.Get<IPermitMediator>();
            if (permit.IsPermitted(InvolvedPermissions.StorageRead))
            {
                iomdtr.AskForFileInPicker(this.actual_schedule_part);
            }
            else {
                permit.AskPermission(InvolvedPermissions.StorageRead);
            }
        }

        private void wallpaper_LongPressed(object sender, EventArgs e)
        {
            if (!File.Exists(actual_schedule_part.path_to_wallpaper)) { return; }

            double ratioParam;
            if (DeviceDisplay.MainDisplayInfo.Width > DeviceDisplay.MainDisplayInfo.Height)
            {
                ratioParam = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Width;
            }
            else {
                ratioParam = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Height;
            }
            DependencyService.Get<IUXMediator>().ShowWallpaperCloseUp(actual_schedule_part.path_to_wallpaper, ratioParam);
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


        private void almost2_Tapped(object sender, EventArgs e)
        {
            mediator.DeliverToast("Жмите 2 раза, чтобы взять поставленные сейчас обои");
        }
        private void set_crt_button_Pressed2(object sender, EventArgs e)
        {
            string pathToOrig = DependencyService.Get<IOMediator>().PathToOriginalWP;
            actual_schedule_part.path_to_wallpaper = pathToOrig;
            wallpaper.Source = FileImageSource.FromFile(pathToOrig);
            filepath_here.Text = pathToOrig;
            (sender as View).IsEnabled = false;
        }
    }
}