using malyar_apk.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace malyar_apk
{
    public interface ISchedulingMediator
    {
        void AssignActionsOfChange(IList<TimedPictureModel> tpms);
        void SetWallpaperConstant(TimedPictureModel tpm);
        void CleanChangesSchedule(int num_of_actions_to_remove);
    }
}
