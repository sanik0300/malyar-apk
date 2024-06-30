using System;
using System.Collections.Generic;
using System.Text;

namespace malyar_apk.Shared
{
    //Generalised args class for former ScheduleAdded event and newly added FilePathDelivered event
    public  class ValuePassedEventArgs<T>
    {
        public readonly T value;
        public ValuePassedEventArgs(T value) {  this.value = value; }
    }
}
