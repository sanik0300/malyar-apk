using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using malyar_apk.Shared;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace malyar_apk
{
    public partial class MainPage : ContentPage
    {
        private double width = 0, height = 0;    
        bool horizontal;
        private bool user_went_to_other_page = false;

        private static IOMediator memdiator;
        public MainPage()
        {          
            InitializeComponent();
            TimedPicturesLoader.IntervalDeleted += Interval_WasDeleted;
            TimedPicturesLoader.IntervalInserted += Interval_WasInserted;
            GeneralIO.ProgressChanged += Serialization_ProgressChanged;
            memdiator = DependencyService.Get<IOMediator>();
            memdiator.ScheduleLoaded += ScheduleLoaded;
        }

        private void ScheduleLoaded(object sender, ValuePassedEventArgs<List<TimedPictureModel>> args)
        {
            var IUM = DependencyService.Get<IUXMediator>();
            var TPMs = args.value as List<TimedPictureModel>;

            if (TPMs == null)
            {
                TimedPicturesLoader.FitIntervalIn(ChangeDirection.InsertNew, TimedPictureModel.OriginalForTheWholeDay());
                IUM.OuchError("Не удалось расшифровать расписание обоев", schedule_container.Children[0]);
                return;
            }

            for (int i = 0; i < TPMs.Count; ++i)
            {
                var SP = new SchedulePiece(TPMs[i]);

                SP.SaveableChangeWasDone += OnSaveableChangeWasDone;
                schedule_container.Children.Add(SP);
            }
            IUM.DeliverToast("Загружено успешно");

            if (schedule_container.Children.Count == 0)
                return;
            (schedule_container.Children[0] as SchedulePiece).ProtectFromClickingDel(schedule_container.Children.Count > 1);
        }

        private void OnSaveableChangeWasDone(object sender, EventArgs e) { save.IsEnabled = true; }

        private void Serialization_ProgressChanged(object sender, ValueChangedEventArgs e)
        {
            if(!horizontal) {
                snek.Progress = e.NewValue;
            }     
        }

        private async void Interval_WasInserted(object sender, TPModelAddedEventArgs args)
        {
            var SP = new SchedulePiece(sender as TimedPictureModel);
            SP.SaveableChangeWasDone += OnSaveableChangeWasDone;

            switch (args.howtoadd)
            {
                case AdditionMode.Insert:
                    schedule_container.Children.Insert(args.PositionIndex, SP);
                    break;
                case AdditionMode.Replace:
                    schedule_container.Children[args.PositionIndex] = SP;
                    break;
            }

            (schedule_container.Children[0] as SchedulePiece).ProtectFromClickingDel(schedule_container.Children.Count > 1);
            if (schedule_container.Children.Count > 1)
            {
                OnSaveableChangeWasDone(SP, EventArgs.Empty);
            }
            addnew.IsEnabled = false;

            //Scroll to the center of newly added SP------------------
            double scrollpos = GetScrollPosition(ref schedule_container, args.PositionIndex, horizontal);
            await scrollview.ScrollToAsync(horizontal ? scrollpos : 0, horizontal ? 0 : scrollpos, true);
        }
        private double GetScrollPosition(ref StackLayout layout, int target_view_position, bool horizontal)
        {
            double result=0;
            if(target_view_position <= layout.Children.Count / 2)
            {
                if (horizontal)
                {
                    for (int i = 0; i < target_view_position; ++i)
                        result += layout.Children[i].Width;

                    result += layout.Children[target_view_position].Width / 2; 
                }
                else {
                    for (int i = 0; i < target_view_position; ++i)
                        result += layout.Children[i].Height;

                    result += layout.Children[target_view_position].Height / 2;
                }   
            }
            else {              
                if (horizontal)
                {
                    result = layout.Width;
                    
                    for (int i = layout.Children.Count - 1; i > target_view_position; i--)
                        result += layout.Children[i].Width;
                    
                    result -= layout.Children[target_view_position].Width / 2;
                }
                else {
                    result = layout.Height;

                    for (int i = layout.Children.Count - 1; i > target_view_position; i--)
                        result += layout.Children[i].Height;

                    result -= layout.Children[target_view_position].Height / 2;
                }
            }
            return result;
        }

        private void Interval_WasDeleted(object sender, TPModelDeletedEventArgs args)
        {
            for (byte i = 0; i < args.Range; ++i)
            {
                schedule_container.Children.RemoveAt(args.OldIndex);
            }          
            (schedule_container.Children[0] as SchedulePiece).ProtectFromClickingDel(schedule_container.Children.Count > 1);
            OnSaveableChangeWasDone(this, EventArgs.Empty);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (user_went_to_other_page) {
                user_went_to_other_page = false;
                return;
            }
            if (memdiator.WasInitialized) {return;}               

            if (TimedPicturesLoader.CheckOutExistingOnes())
                return;

            TimedPicturesLoader.FitIntervalIn(ChangeDirection.InsertNew, TimedPictureModel.OriginalForTheWholeDay());
            (schedule_container.Children[0] as SchedulePiece).ProtectFromClickingDel(schedule_container.Children.Count > 1);
            
            //else - wait for the event of ~deserialization~
        }

        private async void scroll_up_Clicked(object sender, EventArgs e)
        {
            await scrollview.ScrollToAsync(0, 0, true);
        }

        private async void scroll_down_Clicked(object sender, EventArgs e)
        {
            if (horizontal)
                await scrollview.ScrollToAsync(scrollview.ContentSize.Width - scrollview.Width, 0, true);
            else
                await scrollview.ScrollToAsync(0, scrollview.ContentSize.Height - scrollview.Height, true);           
        }

        private void scrollview_Scrolled(object sender, ScrolledEventArgs e)
        {
            if(schedule_container.Children.Count == 0) { return;  }
            if (horizontal) {
                scroll_up.IsEnabled = scrollview.ScrollX > schedule_container.Children[0].Width / 2;
                scroll_down.IsEnabled = (scrollview.Width - scrollview.ScrollX) > schedule_container.Children.Last().Width / 2;
            }
            else {
                scroll_up.IsEnabled = scrollview.ScrollY > schedule_container.Children[0].Height / 2;
                scroll_down.IsEnabled = (scrollview.Height - scrollview.ScrollY) > schedule_container.Children.Last().Height * (double)(schedule_container.Children.Count+1)/4;
            }
        }

        private string filepath_on_the_way = null;
        private void preview_Tapped(object sender, EventArgs e)
        {
            memdiator.AskForFileInPicker();
            memdiator.FilePathDelivered += OnFilePathDelivered;
        }
        private void OnFilePathDelivered(object sender, ValuePassedEventArgs<string> e)
        {
            if (e.value != null)
            {
                filepath_on_the_way = e.value;
                preview.Source = ImageSource.FromFile(filepath_on_the_way);
            }
            addnew.IsEnabled = CheckConstructorCompleteness() && end_new.Time > begin_new.Time;
            memdiator.FilePathDelivered -= OnFilePathDelivered;
        }

        private void addnew_Clicked(object sender, EventArgs e)
        {
            var model = new TimedPictureModel(filepath_on_the_way, begin_new.Time, whole_day.IsChecked && checkbox_pair.IsVisible ? TimeSpan.FromDays(1) : end_new.Time);
            TimedPicturesLoader.FitIntervalIn(ChangeDirection.InsertNew, model);         
        }

        private void time_picker_for_new_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Time")
                return;
            TimePicker[] pickers_up = new TimePicker[2] { begin_new, end_new };

            checkbox_pair.IsVisible = pickers_up[1].Time == TimeSpan.Zero;
            if (checkbox_pair.IsVisible) {
                addnew.IsEnabled = CheckConstructorCompleteness();
                return; 
            }

            addnew.IsEnabled = CheckConstructorCompleteness() && pickers_up[0].Time < pickers_up[1].Time;
        }
        private void whole_day_CheckedChanged(object sender, CheckedChangedEventArgs e) 
        {
            addnew.IsEnabled = CheckConstructorCompleteness();
        }

        private bool CheckConstructorCompleteness()
        {
            end_new.Opacity = whole_day.IsChecked && checkbox_pair.IsVisible ? 0 : 1;

            return filepath_on_the_way != null && (whole_day.IsChecked || !checkbox_pair.IsVisible);
        }

        private void save_Clicked(object sender, EventArgs e)
        {
            TimedPicturesLoader.TryToSaveExistingOnes();
            save.IsEnabled = false;
        }

        private async void to_settings_Clicked(object sender, EventArgs e)
        {
            user_went_to_other_page = true;
            await Navigation.PushAsync(new SettingsPage());
        }

        protected override void OnSizeAllocated(double width, double height)
        {                                            
            if (this.width == width && this.height == height) { return; }
            horizontal = width > height;

            base.OnSizeAllocated(width, height);

            this.width = width;
            this.height = height;

            scrollview.Orientation = horizontal ? ScrollOrientation.Horizontal : ScrollOrientation.Vertical;
            stack_layout.Orientation = horizontal ? StackOrientation.Horizontal : StackOrientation.Vertical;//большая панель где содержится вообще всё
            buttons_strip.Orientation = horizontal ? StackOrientation.Vertical : StackOrientation.Horizontal;//панель где кнопка с настройками
            add_another.Orientation = horizontal ? StackOrientation.Vertical : StackOrientation.Horizontal;//панель где кнопка добавления
            schedule_container.Orientation = horizontal ? StackOrientation.Horizontal : StackOrientation.Vertical;//панель которая содержит сами обои
            snek.IsVisible = !horizontal;
            //loading.IsVisible = horizontal;

            scroll_up.Rotation = horizontal ? -90 : 0;
            scroll_down.Rotation = horizontal ? 90 : 180;
        }
    }
}
