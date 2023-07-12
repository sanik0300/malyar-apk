using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xamarin.Forms;
using Xamarin.Essentials;
using Xamarin.Forms.Internals;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Globalization;
using System.Linq;

namespace malyar_apk.Shared
{
    public static class GeneralIO
    {
        public static event EventHandler<ValueChangedEventArgs> ProgressChanged;
        
        private static float MaxOfProgress, processed_so_far;

        public static void OnProgressChanged()
        {
            ++processed_so_far;
            ProgressChanged.Invoke(null, new ValueChangedEventArgs(default, processed_so_far / MaxOfProgress));
        }
        

        public static List<TimedPictureModel> GetScheduleFromFile(string where_from)
        {
            MaxOfProgress = Preferences.Get(Constants.SAVED_WALLPAPERS_COUNT_KEY, 24 / (Constants.MinutesPerWallpaperByDefault/60));
            List<TimedPictureModel> result = null;

            bool legacy_file = !File.Exists(where_from);
            if(legacy_file) {
                where_from += ".json";
            }

            try
            {
                using (var Fs = new FileStream(where_from, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var sr = new StreamReader(Fs))
                    {
                        if (MaxOfProgress == 1) {
                            result = new List<TimedPictureModel>(1) { new TimedPictureModel(sr.ReadToEnd(), TimeSpan.Zero, TimeSpan.FromDays(1)) };
                        }
                        else if (legacy_file) {                           
                            result = JsonSerializer.Deserialize<List<TimedPictureModel>>(sr.ReadToEnd(), new JsonSerializerOptions() { WriteIndented = true });
                        }
                        else {
                            result = new List<TimedPictureModel>((int)MaxOfProgress);
                            
                            while(!sr.EndOfStream)
                            {
                                ReadOnlySpan<char> tpmodel_entry = sr.ReadLine().AsSpan();
                                result.Add(new TimedPictureModel(tpmodel_entry.Slice(6).ToString(),
                                                                TimeSpan.ParseExact(tpmodel_entry.Slice(0, 5).ToString(), Constants.TimeFormat, CultureInfo.InvariantCulture)));
                            }
                        }
                    }   
                }
                //Sharing the EndTime_s between the deserialized models
                for(int i = 0; i<result.Count-1; ++i)
                {
                    result[i].end_time = result[i+1].start_time;
                }
                result[result.Count - 1].end_time = TimeSpan.FromDays(1);
            }
            catch(Exception e) { ReactToException(e); }
            finally 
            {           
                ProgressChanged.Invoke(null, new ValueChangedEventArgs(MaxOfProgress, 0));     
            }
            return result;
        }

        public static bool SaveScheduleToFile(IList<TimedPictureModel> list_to_save, string where_to)
        {
            MaxOfProgress = (uint)list_to_save.Count;
            
            if(!File.Exists(where_to))//there can exist only either schedule file without extension (new), or .json (legacy)
            {
                File.Delete(where_to + ".json");
            }

            try {
                using (var Fs = new FileStream(where_to, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    Fs.SetLength(0);
                    using (var sw = new StreamWriter(Fs))
                    {
                        if (list_to_save.Count == 1)
                        {
                            sw.Write(list_to_save[0].path_to_wallpaper);
                        }
                        else {
                            foreach (var tpmodel in list_to_save)
                            {
                                sw.WriteLine($"{tpmodel.StartTime.ToString(Constants.TimeFormat)} {tpmodel.path_to_wallpaper}");
                            }
                        }
                    }
                }
                Preferences.Set(Constants.SAVED_WALLPAPERS_COUNT_KEY, list_to_save.Count);
                return true;
            }
            catch(Exception e) 
            {
                ReactToException(e);
                return false; 
            }
            finally {
                ProgressChanged.Invoke(null, new ValueChangedEventArgs(MaxOfProgress, 0));
            }
        }

        public static void HandleMissingImage(NoSuchImgHandling imgHandling, int index_of_missing_img)
        {
            switch ((NoSuchImgHandling)Enum.ToObject(typeof(NoSuchImgHandling), Preferences.Get(Constants.MISSING_IMG_HANDLING, 0)))
            {
                case NoSuchImgHandling.IgnoreAndWaitNext: break;

                case NoSuchImgHandling.PutDefault:
                    DependencyService.Get<ISchedulingMediator>().SetWallpaperConstant(DependencyService.Get<IOMediator>().PathToOriginalWP);
                    break;

                case NoSuchImgHandling.PutNext:
                    string crt_path_to_schedule = DependencyService.Get<IOMediator>().PathToSchedule;
                    bool legacy_mode = !File.Exists(crt_path_to_schedule);
                    if (legacy_mode)
                    {
                        crt_path_to_schedule += ".json";
                    }

                    string closest_ok_path = null, earliest_ok_path_next_day = null;   
                    uint crt_entry = 0;

                    using (FileStream fs = new FileStream(crt_path_to_schedule, FileMode.Open, FileAccess.Read, FileShare.Read)) 
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            if (legacy_mode)
                            {
                                int lines_per_instance = 2 + typeof(TimedPictureModel).GetProperties().Where(x => x.GetCustomAttribute(typeof(JsonIgnoreAttribute)) != null).Count();

                                sr.ReadLine();
                                while (!sr.EndOfStream)
                                {
                                    sr.ReadLine();//skip the start object token
                                    string some_filepath= sr.ReadLine().Split(new char[] { '\"' }, StringSplitOptions.RemoveEmptyEntries)[3];
                                        
                                    check_fpath_validity(ref some_filepath, ref closest_ok_path, ref earliest_ok_path_next_day, ref crt_entry, ref index_of_missing_img);
                                    
                                    if (closest_ok_path != null) { break; }

                                    for (byte i = 0; i < lines_per_instance - 1; ++i) { sr.ReadLine(); }
                                    crt_entry++;
                                }
                            }
                            else {
                                string some_fpth = string.Empty;
                                while(some_fpth != null)
                                {
                                    some_fpth = sr.ReadLine().Substring(6);

                                    check_fpath_validity(ref some_fpth, ref closest_ok_path, ref earliest_ok_path_next_day, ref crt_entry, ref index_of_missing_img);
                                    
                                    if (closest_ok_path != null) { break; }
                                    
                                    crt_entry++;
                                }
                            }
                        }
                    }
                    if(closest_ok_path==null && earliest_ok_path_next_day==null) { return; }

                    DependencyService.Get<ISchedulingMediator>().SetWallpaperConstant(closest_ok_path != null ? closest_ok_path : earliest_ok_path_next_day);
                    break;
            }
        }

        private static void check_fpath_validity(ref string fpath, ref string path1, ref string path2, ref uint crtentry, ref int indx)
        {
            if(!File.Exists(fpath)) { return; }

            if(path2 == null && crtentry < indx) {
                path2 = fpath;
            }
            else if(path1 == null && crtentry > indx) { path1 = fpath; }
        }

        private static async void ReactToException(Exception e)
        {
            #if DEBUG
            Log.Warning("exception", $"{e.GetType().Name}: {e.Message}");
            #endif

            Vibration.Vibrate(500);
            await Application.Current.MainPage.DisplayAlert(e.GetType().Name, e.Message, "ok :(");
        }
    }
}
