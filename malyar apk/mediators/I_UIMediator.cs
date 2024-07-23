using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace malyar_apk
{
    public interface I_UIMediator
    {

        int CountBackgrounds();
        void FixBackgroundTiling(VisualElement visualElement);
        void AdjustOpacity(VisualElement visualElement, float opacity);

        void ConfirmThemeChangeOnPlatform(int themeId);
        /// <summary>
        /// </summary>
        /// <returns>true - dark, false - light, null - system themes not supported in this version</returns>
        bool? IsSystemDarkTheme();

        //void SubscribeRendererToThemeChange(NavigationPage nPage);
        //void SubscribeRendererToThemeChange(SwitchCell cell);
    }
}
