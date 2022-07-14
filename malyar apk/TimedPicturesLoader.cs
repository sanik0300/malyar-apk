using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace malyar_apk
{
    class TimedPicturesLoader
    {
        static internal ArrayList InitExistingOnes()
        {
            IMagesMediator mediator = DependencyService.Get<IMagesMediator>();
            if(File.Exists(mediator.GetPathToSchedule()))
            {
                throw new NotImplementedException("здесь пока ремонт");
            }
            else { return null; }
        }
    }
}
