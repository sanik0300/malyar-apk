using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xamarin.Forms;
using Xamarin.Essentials;
using Xamarin.Forms.Internals;

namespace malyar_apk.Shared
{
    public class jsonIOmanager
    {
        public static event EventHandler<ValueChangedEventArgs> ProgressChanged;
        public static jsonIOmanager CurrentInstance { get; private set; }
        
        private float MaxOfProgress, processed_so_far;
        public jsonIOmanager() {
            CurrentInstance = this;
        }

        public void OnProgressChanged()
        {
            ++processed_so_far;
            ProgressChanged.Invoke(this, new ValueChangedEventArgs(default, processed_so_far / MaxOfProgress));
        }

        public List<TimedPictureModel> GetScheduleFromFile(string where_from)
        {
            MaxOfProgress = Preferences.Get(Constants.SAVED_WALLPAPERS_COUNT_KEY, 24 / (Constants.MinutesPerWallpaperByDefault/60));
            List<TimedPictureModel> result = null;
            try
            {
                using (var Fs = new FileStream(where_from, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var sr = new StreamReader(Fs))
                    {
                        if (MaxOfProgress == 1) {
                            result = new List<TimedPictureModel>(1) { new TimedPictureModel(sr.ReadToEnd(), TimeSpan.Zero, TimeSpan.FromDays(1)) };
                        }
                        else {
                            result = JsonSerializer.Deserialize<List<TimedPictureModel>>(sr.ReadToEnd(), new JsonSerializerOptions() { WriteIndented = true });
                        }
                    }   
                } 
            }
            catch(Exception e) {
                Log.Warning("exception", $"{e.GetType().Name}: {e.Message}");
            }
            finally
            {
                ProgressChanged.Invoke(this, new ValueChangedEventArgs(MaxOfProgress, 0));
                
            }
            return result;
        }

        public bool SaveScheduleToFile(IList<TimedPictureModel> what, string where_to)
        {
            MaxOfProgress = (uint)what.Count;
            try
            {
                using (var Fs = new FileStream(where_to, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    Fs.SetLength(0);
                    using (var sw = new StreamWriter(Fs))
                    {
                        if (MaxOfProgress == 1) {
                            sw.Write(where_to);
                        }
                        else {
                            string output = JsonSerializer.Serialize(what, what.GetType(), new JsonSerializerOptions() { WriteIndented = true });
                            sw.Write(output);
                        }
                    }
                }
                Preferences.Set(Constants.SAVED_WALLPAPERS_COUNT_KEY, what.Count);
                return true;
            }
            catch(Exception e) {
                Log.Warning("exception", $"{e.GetType().Name}: {e.Message}");
                return false; 
            }
            finally
            {
                ProgressChanged.Invoke(this, new ValueChangedEventArgs(MaxOfProgress, 0));
            }
        }
    }
}
