using System;
using System.Collections.Generic;
using System.Text;

namespace malyar_apk
{
    public enum InvolvedPermissions { StorageRead, StorageWrite, ExactAlarm }
    public interface IPermitMediator
    {
        void AskPermission(InvolvedPermissions perm);
        bool IsPermitted(InvolvedPermissions perm);

        event EventHandler FilesReadUnblocked;
        void OnFilesReadUnblocked();
    }
}
