using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace malyar_apk.Droid
{
    public class ErrorDialogFragment : DialogFragment
    {
        Timer _timer;
        Context _context_for_builder;

        public ErrorDialogFragment(Context context_for_builder)
        {
            this._context_for_builder = context_for_builder;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _timer = new Timer(3000) { Enabled = true, AutoReset = false };
        }

        public override Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            AlertDialog atdlg = new AlertDialog.Builder(this._context_for_builder).SetTitle("Ошибка соединения")
                                                            .SetMessage("не удалось скачать изображение из удаленного источника. Возможно, это потому что нет сети")
                                                            .SetCancelable(true)
                                                            .SetNeutralButton("OK", (sender, args) =>
                                                            {
                                                                _timer.Stop();
                                                                (sender as Dialog).Dismiss();                                                                
                                                            })
                                                            .Create();
            
            _timer.Elapsed += (sender, args) => { atdlg.Dismiss(); };

            _timer.Start();
            return atdlg;
        }

        public override void OnCancel(IDialogInterface dialog)
        {
            _timer.Stop();
            base.OnCancel(dialog);
        }

        public override void OnDestroyView()
        {
            _context_for_builder = null;
            _timer.Dispose();
            base.OnDestroyView();
        }
    }
}