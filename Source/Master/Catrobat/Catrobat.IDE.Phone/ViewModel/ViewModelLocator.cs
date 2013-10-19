﻿using System.Diagnostics;
using System.Globalization;
using System.Windows;
using Catrobat.IDE.Core;
using Catrobat.IDE.Core.CatrobatObjects;
using Catrobat.IDE.Core.Resources;
using Catrobat.IDE.Core.Services;
using Catrobat.IDE.Core.UI;
using Catrobat.IDE.Phone.ViewModel.Editor;
using Catrobat.IDE.Phone.ViewModel.Editor.Costumes;
using Catrobat.IDE.Phone.ViewModel.Editor.Formula;
using Catrobat.IDE.Phone.ViewModel.Editor.Scripts;
using Catrobat.IDE.Phone.ViewModel.Editor.Sounds;
using Catrobat.IDE.Phone.ViewModel.Editor.Sprites;
using Catrobat.IDE.Phone.ViewModel.Main;
using Catrobat.IDE.Phone.ViewModel.Service;
using Catrobat.IDE.Phone.ViewModel.Settings;
using Catrobat.IDE.Phone.ViewModel.Share;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using ServiceLocator = Microsoft.Practices.ServiceLocation.ServiceLocator;

namespace Catrobat.IDE.Phone.ViewModel
{
    public class ViewModelLocator
    {
        private static CatrobatContextBase _context;

        static ViewModelLocator()
        {
            Core.Services.ServiceLocator.Register<MainViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<AddNewProjectViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<UploadProjectViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<UploadProjectLoadingViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<UploadProjectLoginViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<SoundRecorderViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<SettingsViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<SettingsBrickViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<SettingsLanguageViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<SettingsThemeViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<CostumeNameChooserViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<ChangeCostumeViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<NewSoundSourceSelectionViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<ChangeSoundViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<SoundNameChooserViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<AddNewSpriteViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<ChangeSpriteViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<ProjectSettingsViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<ProjectImportViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<OnlineProjectViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<NewBroadcastMessageViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<ScriptBrickCategoryViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<AddNewScriptBrickViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<FormulaEditorViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<PlayerLauncherViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<TileGeneratorViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<VariableSelectionViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<AddNewGlobalVariableViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<AddNewLocalVariableViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<ChangeVariableViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<SpritesViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<SpriteEditorViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<ShareProjectServiceSelectionViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<UploadToSkyDriveViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<NewCostumeSourceSelectionViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<CostumeSavingViewModel>(TypeCreationMode.Normal);
            Core.Services.ServiceLocator.Register<EditorLoadingViewModel>(TypeCreationMode.Normal);


            if (ViewModelBase.IsInDesignModeStatic)
            {
                var context = new CatrobatContextDesign();
  
                var messageContext = new GenericMessage<CatrobatContextBase>(context);
                Messenger.Default.Send(messageContext, ViewModelMessagingToken.ContextListener);

                var messageCurrentSprite = new GenericMessage<Sprite>(context.CurrentProject.SpriteList.Sprites[0]);
                Messenger.Default.Send(messageCurrentSprite, ViewModelMessagingToken.CurrentSpriteChangedListener);
            }
        }

        private static Project InitializeFirstTimeUse(CatrobatContextBase context)
        {
            Project currentProject = null;
            var localSettings = CatrobatContext.RestoreLocalSettingsStatic();

            if (localSettings == null)
            {
                if (Debugger.IsAttached)
                {
                    var loader = new SampleProjectLoader();
                    loader.LoadSampleProjects();
                }

                currentProject = CatrobatContext.RestoreDefaultProjectStatic(CatrobatContextBase.DefaultProjectName);
                currentProject.Save();
                context.LocalSettings = new LocalSettings { CurrentProjectName = currentProject.ProjectHeader.ProgramName };
            }
            else
            {
                context.LocalSettings = localSettings;
                currentProject = CatrobatContext.LoadNewProjectByNameStatic(context.LocalSettings.CurrentProjectName);
            }

            return currentProject;
        }

        public static void LoadContext()
        {
            _context = new CatrobatContext();
            var currentProject = InitializeFirstTimeUse(_context) ??
                                 CatrobatContext.RestoreDefaultProjectStatic(CatrobatContextBase.DefaultProjectName);

            if (_context.LocalSettings.CurrentLanguageString == null)
                _context.LocalSettings.CurrentLanguageString =
                    Core.Services.ServiceLocator.CulureService.GetCulture().TwoLetterISOLanguageName;

            var themeChooser = (ThemeChooser)Core.Services.ServiceLocator.ThemeChooser;
            if (_context.LocalSettings.CurrentThemeIndex != -1)
                themeChooser.SelectedThemeIndex = _context.LocalSettings.CurrentThemeIndex;

            if (_context.LocalSettings.CurrentLanguageString != null)
                Core.Services.ServiceLocator.GetInstance<SettingsViewModel>().CurrentCulture =
                    new CultureInfo(_context.LocalSettings.CurrentLanguageString);

            var message1 = new GenericMessage<ThemeChooser>(themeChooser);
            Messenger.Default.Send(message1, ViewModelMessagingToken.ThemeChooserListener);

            var message2 = new GenericMessage<CatrobatContextBase>(_context);
            Messenger.Default.Send(message2, ViewModelMessagingToken.ContextListener);

            var message = new GenericMessage<Project>(currentProject);
            Messenger.Default.Send(message, ViewModelMessagingToken.CurrentProjectChangedListener);
        }

        public static void SaveContext(Project currentProject)
        {
            if (currentProject == null || _context == null)
                return;

            var themeChooser = (ThemeChooser)Core.Services.ServiceLocator.ThemeChooser;
            var settingsViewModel = Core.Services.ServiceLocator.GetInstance<SettingsViewModel>();

            if (themeChooser.SelectedTheme != null)
            {
                _context.LocalSettings.CurrentThemeIndex = themeChooser.SelectedThemeIndex;
            }

            if (settingsViewModel.CurrentCulture != null)
            {
                _context.LocalSettings.CurrentLanguageString = settingsViewModel.CurrentCulture.Name;
            }

            _context.LocalSettings.CurrentProjectName = currentProject.ProjectHeader.ProgramName;
            CatrobatContext.StoreLocalSettingsStatic(_context.LocalSettings);

            currentProject.Save();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel MainViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<MainViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public AddNewProjectViewModel AddNewProjectViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<AddNewProjectViewModel>();
            }
        }



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public ProjectSettingsViewModel ProjectSettingsViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<ProjectSettingsViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public UploadProjectViewModel UploadProjectViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<UploadProjectViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public UploadProjectLoadingViewModel UploadProjectLoadingViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<UploadProjectLoadingViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public UploadProjectLoginViewModel UploadProjectLoginViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<UploadProjectLoginViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public SoundRecorderViewModel SoundRecorderViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<SoundRecorderViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public SettingsViewModel SettingsViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<SettingsViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public SettingsBrickViewModel SettingsBrickViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<SettingsBrickViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public SettingsLanguageViewModel SettingsLanguageViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<SettingsLanguageViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public SettingsThemeViewModel SettingsThemeViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<SettingsThemeViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public CostumeNameChooserViewModel CostumeNameChooserViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<CostumeNameChooserViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public ChangeCostumeViewModel ChangeCostumeViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<ChangeCostumeViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public ChangeSoundViewModel ChangeSoundViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<ChangeSoundViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public NewSoundSourceSelectionViewModel NewSoundSourceSelectionViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<NewSoundSourceSelectionViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public SoundNameChooserViewModel SoundNameChooserViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<SoundNameChooserViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public ChangeSpriteViewModel ChangeSpriteViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<ChangeSpriteViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public AddNewSpriteViewModel AddNewSpriteViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<AddNewSpriteViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public ProjectImportViewModel ProjectImportViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<ProjectImportViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public OnlineProjectViewModel OnlineProjectViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<OnlineProjectViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public NewBroadcastMessageViewModel NewBroadcastMessageViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<NewBroadcastMessageViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public ScriptBrickCategoryViewModel ScriptBrickCategoryViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<ScriptBrickCategoryViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public AddNewScriptBrickViewModel AddNewScriptBrickViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<AddNewScriptBrickViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public FormulaEditorViewModel FormulaEditorViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<FormulaEditorViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public PlayerLauncherViewModel PlayerLauncherViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<PlayerLauncherViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public TileGeneratorViewModel TileGeneratorViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<TileGeneratorViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public VariableSelectionViewModel VariableSelectionViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<VariableSelectionViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public AddNewGlobalVariableViewModel AddNewGlobalVariableViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<AddNewGlobalVariableViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public AddNewLocalVariableViewModel AddNewLocalVariableViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<AddNewLocalVariableViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public ChangeVariableViewModel ChangeVariableViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<ChangeVariableViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public SpritesViewModel SpritesViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<SpritesViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public SpriteEditorViewModel SpriteEditorViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<SpriteEditorViewModel>();
            }
        }

        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public ShareProjectServiceSelectionViewModel ShareProjectServiceSelectionViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<ShareProjectServiceSelectionViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public UploadToSkyDriveViewModel UploadToSkyDriveViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<UploadToSkyDriveViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
           "CA1822:MarkMembersAsStatic",
           Justification = "This non-static member is needed for data binding purposes.")]
        public NewCostumeSourceSelectionViewModel NewCostumeSourceSelectionViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<NewCostumeSourceSelectionViewModel>();
            }
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
           "CA1822:MarkMembersAsStatic",
           Justification = "This non-static member is needed for data binding purposes.")]
        public CostumeSavingViewModel CostumeSavingViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<CostumeSavingViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
           "CA1822:MarkMembersAsStatic",
           Justification = "This non-static member is needed for data binding purposes.")]
        public EditorLoadingViewModel EditorLoadingViewModel
        {
            get
            {
                return Core.Services.ServiceLocator.GetInstance<EditorLoadingViewModel>();
            }
        }

        
        public static void Cleanup()
        {
        }
    }
}