using System;
using System.Collections;
using System.IO;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace malyar_apk
{
    public partial class MainPage : ContentPage
    {
        private double width = 0, height = 0;
       
        bool horizontal;

        public MainPage()
        {
            InitializeComponent();
        }

       private void InsertNewWallpaper()
       {
            schedule_container.Children.Add(new SeparatorCustom());
            schedule_container.Children.Add(new SchedulePiece());         
       }

        protected override void OnAppearing()
        {          
            base.OnAppearing();

            ArrayList ArL = TimedPicturesLoader.InitExistingOnes();
            if(ArL == null)
            {
                schedule_container.Children.Add(new SchedulePiece(TimedPictureModel.just_original, TimeSpan.Zero, TimeSpan.FromDays(1)));//original wallpaper during the whole day
                //for(int i =0; i<2; i++) { InsertNewWallpaper(); }
            }
            else
            {
                throw new NotImplementedException("ты как вообще сюда добрался");
            }          
        }

        private async void scroll_up_Clicked(object sender, EventArgs e)
        {
            await scrollview.ScrollToAsync(0, 0, true);
        }

        private async void scroll_down_Clicked(object sender, EventArgs e)
        {
            await scrollview.ScrollToAsync(0, scrollview.ContentSize.Height - scrollview.Height, true);
        }

        private void scrollview_Scrolled(object sender, ScrolledEventArgs e)
        {
            if (horizontal)
            {
                scroll_up.IsEnabled = scrollview.ScrollX > schedule_container.Children[0].Width / 2;
                scroll_down.IsEnabled = (scrollview.Width - scrollview.ScrollX) > schedule_container.Children.Last().Width / 2;
            }
            else
            {
                scroll_up.IsEnabled = scrollview.ScrollY > schedule_container.Children[0].Height / 2;
                scroll_down.IsEnabled = (scrollview.Height - scrollview.ScrollY) > schedule_container.Children.Last().Height * (double)(schedule_container.Children.Count+1)/4;
            }
        }

        private void addnew_Clicked(object sender, EventArgs e)
        {

        }

        private bool time_change_recursion = false;
        private void time_picker_for_new_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Time")
                return;
            TimePicker[] pickers_up = new TimePicker[2] { begin_new, end_new };

            checkbox_pair.IsVisible = pickers_up[1].Time == TimeSpan.Zero;
            if (checkbox_pair.IsVisible) {
                whole_day_CheckedChanged(whole_day, new CheckedChangedEventArgs(whole_day.IsChecked));
                return; 
            }//Отстать от юзера и дать шанс включить поддержку 24 часов

            if (pickers_up[0].Time == pickers_up[1].Time &&!time_change_recursion)
            {
                if (pickers_up[0].Time.TotalMinutes < TimedPictureModel.MinLegitMinutesDelta)//если время выбрано от 00:00 до где-то 00:04
                {
                    pickers_up[1].Time = TimeSpan.FromMinutes(pickers_up[1].Time.TotalMinutes + TimedPictureModel.MinLegitMinutesDelta);
                }
                else
                {
                    pickers_up[0].Time = TimeSpan.FromMinutes(pickers_up[0].Time.TotalMinutes - TimedPictureModel.MinLegitMinutesDelta);
                }
            }
            time_change_recursion = false;

            if (pickers_up[1].Time < pickers_up[0].Time)
            {
                TimeSpan temp = pickers_up[0].Time;
                time_change_recursion = true;
                pickers_up[0].Time = pickers_up[1].Time;
                pickers_up[1].Time = temp;
            }
        }

        private void whole_day_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            addnew.IsEnabled = e.Value && (sender as CheckBox).IsVisible;
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
            loading.IsVisible = horizontal;
            
            snek.Rotation = scroll_up.Rotation = scroll_down.Rotation = horizontal ? -90 : 0;

            foreach(View view in schedule_container.Children)
            {
                view.HorizontalOptions = horizontal ? LayoutOptions.Start : LayoutOptions.FillAndExpand;
                view.VerticalOptions = horizontal ? LayoutOptions.FillAndExpand : LayoutOptions.Start;
            }
        }
    }
}
