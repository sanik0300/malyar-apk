using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using malyar_apk.Shared;
using System.Collections.Generic;
using System.IO;

namespace malyar_apk.Droid
{
    [Service(Enabled = true)]
    public class IO_Service : Service
    {
        public override IBinder OnBind(Intent intent){ return null; }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            var pi = intent.GetParcelableExtra(Intent.ExtraIntent) as PendingIntent;
            Intent back_to_activity = new Intent();

            if (intent.GetBooleanExtra(AndroidConstants.IO_SAVE_KEY, false))//если сохраняем какой-то список
            {
                var TPMPs = intent.GetParcelableArrayExtra(AndroidConstants.LIST_KEY);
                var passed_list = new List<TimedPictureModel>(TPMPs.Length);
                foreach(var parceable in TPMPs)
                {
                    passed_list.Add((parceable as TPModelParcelable).source);
                }
                bool result = GeneralIO.SaveScheduleToFile(passed_list, intent.GetStringExtra(AndroidConstants.FILEPATH_EXTRA_KEY));
                back_to_activity.PutExtra(AndroidConstants.RESULT_SERIALIZED, result);

                pi.Send(this, Result.Ok, back_to_activity);
            }
            else//если выгружаем список из файла
            {
                List<TimedPictureModel> result = GeneralIO.GetScheduleFromFile(intent.GetStringExtra(AndroidConstants.FILEPATH_EXTRA_KEY));
                if (result != null)
                {
                    var parcelables = new TPModelParcelable[result.Count];
                    for (int i = 0; i < result.Count; ++i)
                    {
                        parcelables[i] = new TPModelParcelable(result[i]);
                    }
                    back_to_activity.PutExtra(AndroidConstants.RESULT_DESERIALIZED, parcelables);

                    pi.Send(this, Result.Ok, back_to_activity);
                }
                else {
                    pi.Send(this, Result.Canceled, back_to_activity);
                }
                
            }
            StopSelf();

            return base.OnStartCommand(intent, flags, startId);
        }
        
    }
}