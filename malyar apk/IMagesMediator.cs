using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace malyar_apk
{
    public interface IMagesMediator
    {
        string GetPathToSchedule();
        byte[] GetOriginalWP();
    }
}
