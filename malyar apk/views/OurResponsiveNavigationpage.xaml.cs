using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using malyar_apk.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace malyar_apk
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OurResponsiveNavigationpage : NavigationPage
    {
        public OurResponsiveNavigationpage()
        {
            InitializeComponent();
            jsonIOmanager.ExceptionOccured += ReportException;
        }

        public OurResponsiveNavigationpage(Page page) : base(page) 
        {
            jsonIOmanager.ExceptionOccured += ReportException;
        }

        private async void ReportException(object sender, EventArgs e)
        {
            string[] excp_texts = sender as string[];
            StringBuilder join_info = new StringBuilder(excp_texts[1].Length + excp_texts[2].Length + 2);
            join_info.AppendLine(excp_texts[1]);
            join_info.AppendLine("---------------");
            join_info.AppendLine(excp_texts[2]);

            await DisplayAlert(excp_texts[0], join_info.ToString(), "ok :(");
        }
    }
}