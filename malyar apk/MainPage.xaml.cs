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
        IMagesMediator mediator = DependencyService.Get<IMagesMediator>();
        bool horizontal;
        int scrollable_unit_X, scrollable_unit_Y;

        public MainPage()
        {
            InitializeComponent();
            //grid.SizeChanged += Grid_SizeChanged;
        }

        private void InsertNewWallpaper(PeriodicalPicture PP)
        {
            if (horizontal)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.Children.Add(PP, grid.ColumnDefinitions.Count - 1, 0);
            }
            else
            {
                if (grid.Children.Count % 2 == 0)
                {
                    grid.RowDefinitions.Add(new RowDefinition());
                    grid.Children.Add(PP, 0, grid.RowDefinitions.Count - 1);
                }
                else
                {
                    grid.Children.Add(PP, 1, grid.RowDefinitions.Count - 1);
                }
            }
        }

        private void AlignTimespanOfNewWallpaper(object sender, EventArgs e)
        {
            var current = grid.Children[grid.Children.Count - 1] as PeriodicalPicture;
            var previous = grid.Children[grid.Children.Count - 2] as PeriodicalPicture;
            int compare_result = current.CompareTo(previous);

            if(compare_result == -1 || compare_result == 2)
            {
                current.Swap(previous);
            }

            current.FilledIn -= AlignTimespanOfNewWallpaper;

            if (Math.Abs(compare_result) == 2)//То есть один полностью закрывает другой, но они не равны
            {
                var third = previous.Clone() as PeriodicalPicture;
                InsertNewWallpaper(third);
                PeriodicalPicture.Join(previous, current, third);
                
                ///third.FilledIn += AlignTimespanOfNewWallpaper;
            }
            else { PeriodicalPicture.Join(previous, current, (TimespanCompareResult)compare_result); }
          
            var final_tail = new PeriodicalPicture();
            final_tail.FilledIn += AlignTimespanOfNewWallpaper;
            InsertNewWallpaper(final_tail);
        }

        /*private void Grid_SizeChanged(object sender, EventArgs e)
        {
            if (grid.ColumnDefinitions.Count != 0)
            {
                scrollable_unit_X = (int)grid.Width / grid.ColumnDefinitions.Count;
            }
            else if (grid.RowDefinitions.Count != 0)
            {
                scrollable_unit_Y = (int)grid.Height / grid.RowDefinitions.Count;              
            }
        }*/
         /*private void grid_ChildAdded(object sender, ElementEventArgs e)
         {
            var last_PP = grid.Children[grid.Children.Count - 1] as PeriodicalPicture;
            if(last_PP != null)
            {
                last_PP.FilledIn += AlignTimespanOfNewWallpaper;
                if (!last_PP.Empty)
                {
                    InsertNewWallpaper(new PeriodicalPicture());
                }
            }
        }*/

        protected override void OnAppearing()
        {          
            base.OnAppearing();
            PeriodicalPicture.InitializeImageBoundsRatio();

            ArrayList ArL = TimedPicturesLoader.InitExistingOnes();
            if(ArL == null)
            {
                grid.Children.Add(new PeriodicalPicture(ImageSource.FromStream( () => new MemoryStream(mediator.GetOriginalWP()) )), 0, 0);//with the original wallpaper
                grid.Children.Add(new PeriodicalPicture(), 1, 0);//aka a button to add new ones
            }
            else
            {
                throw new NotImplementedException("ты как вообще сюда добрался");
            }
            
            PeriodicalPicture[] pictures = grid.Children.OfType<PeriodicalPicture>().ToArray();
            pictures[pictures.Length - 1].FilledIn += AlignTimespanOfNewWallpaper;

            /*for(int i = 1; i<pictures.Length; i++)
            {
                pictures[i].FilledIn += AlignTimespanOfNewWallpaper;
            }*/
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
                scroll_up.IsEnabled = scrollview.ScrollX > scrollable_unit_X / 2;
                scroll_down.IsEnabled = (scrollview.Width - scrollview.ScrollX) > scrollable_unit_X / 2;
            }
            else
            {
                //snek.Text = (sender as ScrollView).ScrollY.ToString();
                scroll_up.IsEnabled = scrollview.ScrollY > scrollable_unit_Y / 2;
                scroll_down.IsEnabled = (scrollview.Height - scrollview.ScrollY) > scrollable_unit_Y/2; 
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {                                            
            if (this.width == width && this.height == height) { return; }
            horizontal = width > height;

            PeriodicalPicture[] pictures = grid.Children.OfType<PeriodicalPicture>().ToArray();
            foreach (PeriodicalPicture pp in pictures)
            {
                pp.PreserveShape(horizontal);
            } 
           
            base.OnSizeAllocated(width, height);

            this.width = width;
            this.height = height;
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            scrollview.Orientation = horizontal ? ScrollOrientation.Horizontal : ScrollOrientation.Vertical;
            stack_layout.Orientation = horizontal ? StackOrientation.Horizontal : StackOrientation.Vertical;
            buttons_strip.Orientation = horizontal ? StackOrientation.Vertical : StackOrientation.Horizontal;
            snek.IsVisible = !horizontal;
            loading.IsVisible = horizontal;
            
            snek.Rotation = scroll_up.Rotation = scroll_down.Rotation = horizontal ? -90 : 0;

            if (horizontal)
            {
                for (int i = 0; i < pictures.Length; i++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.Children.Add(pictures[i], i, 0);
                }
            }
            else
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());

                for (int i = 0; i < pictures.Length; i++)
                {
                    if(i%2==0) { grid.RowDefinitions.Add(new RowDefinition()); }
                        
                    grid.Children.Add(pictures[i], i % 2, i / 2);
                }
            }
        }
    }
}
