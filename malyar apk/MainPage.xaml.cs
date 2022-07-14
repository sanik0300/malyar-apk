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

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {          
            base.OnAppearing();
            PeriodicalPicture.InitializeImageBoundsRatio();

            ArrayList ArL = TimedPicturesLoader.InitExistingOnes();
            if(ArL == null)
            {
                grid.Children.Add(new PeriodicalPicture(ImageSource.FromStream( () => new MemoryStream(mediator.GetOriginalWP()) )), 0, 0);//with the original wallpaper
                grid.Children.Add(new PeriodicalPicture(), 1, 0);//aka a button to add new ones

                grid.Children.Add(new PeriodicalPicture(), 0, 1);
                grid.Children.Add(new PeriodicalPicture(), 1, 1);
                grid.Children.Add(new PeriodicalPicture(), 0, 2);
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
            if(horizontal)
            {

            }
            else
            {

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
            snek.Rotation = horizontal ? -90 : 0;
            loading.IsVisible = horizontal;

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
