using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xamarin.Forms;
using Xamarin.Essentials;
using Xamarin.Forms.Internals;
using System.Reflection;
using System.Text.Json.Serialization;

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

        public static uint CountLinesOfTypeSerialization(Type type)
        {
            uint output = 2; //lines with StartObject and EndObject semicolons
            PropertyInfo[] properties = type.GetProperties();
            for(int i = 0; i<properties.Length; ++i)
            {
                if (properties[i].GetCustomAttribute(typeof(JsonIgnoreAttribute)) != null){
                    continue;
                }
                output++;
            }
            return output;
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
                //Sharing the EndTime_s between the deserialized models
                for(int i = 0; i<result.Count-1; ++i)
                {
                    result[i].end_time = TimeSpan.FromMinutes(result[i+1].start_time.TotalMinutes);
                }
                result[result.Count - 1].end_time = TimeSpan.FromDays(1);
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
