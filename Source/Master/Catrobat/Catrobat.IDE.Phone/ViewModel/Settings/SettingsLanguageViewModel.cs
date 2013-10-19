﻿using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using Catrobat.IDE.Core.Services;
using Catrobat.IDE.Core.Utilities.Helpers;
using Catrobat.IDE.Core.Resources.Localization;
using GalaSoft.MvvmLight;

namespace Catrobat.IDE.Phone.ViewModel.Settings
{
    public class SettingsLanguageViewModel : ViewModelBase
    {
        #region Private Members

        #endregion

        #region Properties

        public ObservableCollection<CultureInfo> AvailableCultures
        {
            get { return LanguageHelper.SupportedLanguages; }
        }

        public CultureInfo CurrentCulture
        {
            get { return ServiceLocator.CulureService.GetCulture(); }

            set
            {
                if (ServiceLocator.CulureService.GetCulture().Equals(value))
                {
                    return;
                }

                ServiceLocator.CulureService.SetCulture(value);
                //Thread.CurrentThread.CurrentUICulture = value;

                // TODO: portable
                ((LocalizedStrings)ServiceLocator.LocalizedStrings).Reset();
                RaisePropertyChanged(() => CurrentCulture);
            }
        }

        #endregion

        #region Commands

        #endregion

        #region Actions

       

        #endregion

        #region MessageActions


        #endregion

        public SettingsLanguageViewModel()
        {

        }
    }
}