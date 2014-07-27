﻿using Catrobat.IDE.Core.ExtensionMethods;
using Catrobat.IDE.Core.Models;
using Catrobat.IDE.Core.Resources.Localization;
using Catrobat.IDE.Core.Services;
using Catrobat.IDE.Core.Utilities.Helpers;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Catrobat.IDE.Core.ViewModels.Editor.Sprites
{
    public class SpritesViewModel : ViewModelBase
    {
        #region Private Members

        private Program _currentProject;
        private Sprite _selectedSprite;
        private int _numberOfObjectsSelected;
        private bool _isSpriteSelecting;

        #endregion

        #region Properties

        public Program CurrentProject
        {
            get { return _currentProject; }
            private set 
            { 
                _currentProject = value;

                if (_currentProject != null)
                {
                    Sprites.CollectionChanged -= SpritesCollectionChanged;
                    Sprites.CollectionChanged += SpritesCollectionChanged;
                }

                ServiceLocator.DispatcherService.RunOnMainThread(() =>
                {
                    RaisePropertyChanged(() => Sprites);
                                    ServiceLocator.DispatcherService.RunOnMainThread(() => RaisePropertyChanged(() => CurrentProject));
                });

            }
        }

        public ObservableCollection<Sprite> Sprites
        {
            get
            {
                return CurrentProject.Sprites;
            }
        }

        public bool IsSpritesEmpty
        {
            get
            {
                if (Sprites == null)
                    return true;
                return Sprites.Count <= 0;
            }
        }

        public Sprite SelectedSprite
        {
            get
            {
                return _selectedSprite;
            }
            set
            {
                if (ReferenceEquals(_selectedSprite, value))
                    return;

                _selectedSprite = value;

                RaisePropertyChanged(() => SelectedSprite);

                EditSpriteCommand.RaiseCanExecuteChanged();
                CopySpriteCommand.RaiseCanExecuteChanged();
                DeleteSpriteCommand.RaiseCanExecuteChanged();

                var spriteChangedMessage = new GenericMessage<Sprite>(SelectedSprite);
                Messenger.Default.Send(spriteChangedMessage, ViewModelMessagingToken.CurrentSpriteChangedListener);
            }
        }


        public bool IsSpriteSelecting
        {
            get { return _isSpriteSelecting; }
            set
            {
                _isSpriteSelecting = value;
                RaisePropertyChanged(() => IsSpriteSelecting);
            }
        }

        public int NumberOfObjectsSelected
        {
            get { return _numberOfObjectsSelected; }
            set
            {
                if (value == _numberOfObjectsSelected) return;
                _numberOfObjectsSelected = value;
                RaisePropertyChanged(() => NumberOfObjectsSelected);
            }
        }

        # endregion

        #region Commands

        public RelayCommand AddNewSpriteCommand
        {
            get;
            private set;
        }

        public RelayCommand EditSpriteCommand
        {
            get;
            private set;
        }

        public RelayCommand CopySpriteCommand
        {
            get;
            private set;
        }

        public RelayCommand DeleteSpriteCommand
        {
            get;
            private set;
        }

        public RelayCommand UndoCommand
        {
            get;
            private set;
        }

        public RelayCommand RedoCommand
        {
            get;
            private set;
        }

        public RelayCommand ClearObjectsSelectionCommand
        {
            get;
            private set;
        }

        public RelayCommand StartPlayerCommand
        {
            get;
            private set;
        }

        # endregion

        #region CanCommandsExecute
        private bool CanExecuteDeleteSpriteCommand()
        {
            return SelectedSprite != null;
        }
        private bool CanExecuteCopySpriteCommand()
        {
            return SelectedSprite != null;
        }
        private bool CanExecuteEditSpriteCommand()
        {
            return SelectedSprite != null;
        }

        #endregion

        #region Actions

        private static void AddNewSpriteAction()
        {
            ServiceLocator.NavigationService.NavigateTo<AddNewSpriteViewModel>();
        }

        private static void EditSpriteAction()
        {
            ServiceLocator.NavigationService.NavigateTo<SpriteEditorViewModel>();
        }

        private async void CopySpriteAction()
        {
            var originalIndex = Sprites.IndexOf(SelectedSprite);

            var newSprite = await SelectedSprite.CloneAsync(CurrentProject);
            var newIndex = originalIndex + 1;

            Sprites.Insert(newIndex, newSprite);
        }

        private void DeleteSpriteAction()
        {
            var sprite = AppResources.Editor_ObjectSingular;
            var messageContent = String.Format(AppResources.Editor_MessageBoxDeleteText, "1", sprite);
            var messageHeader = String.Format(AppResources.Editor_MessageBoxDeleteHeader, sprite);

            ServiceLocator.NotifictionService.ShowMessageBox(messageHeader,
                messageContent, DeleteSpriteMessageBoxResult, MessageBoxOptions.OkCancel);
        }

        private void ClearObjectSelectionAction()
        {
            SelectedSprite = null;
        }

        private void StartPlayerAction()
        {
           ServiceLocator.PlayerLauncherService.LaunchPlayer(CurrentProject);
        }

        private void UndoAction()
        {
            CurrentProject.Undo();
        }

        private void RedoAction()
        {
            CurrentProject.Redo();
        }

        protected override void GoBackAction()
        {
            SelectedSprite = null;
            base.GoBackAction();
        }

        #endregion

        #region MessageActions

        private void CurrentProjectChangedAction(GenericMessage<Program> message)
        {
            CurrentProject = message.Content;
            SelectedSprite = null;
        }

        private void CurrentSpriteChangedMessageAction(GenericMessage<Sprite> message)
        {
            SelectedSprite = message.Content;
        }

        #endregion

        #region MessageBoxResult

        private void DeleteSpriteMessageBoxResult(MessageboxResult result)
        {
            if (result == MessageboxResult.Ok)
            {
                // SelectedSprite.LocalVariables.Clear();

                ReferenceHelper.CleanUpSpriteReferences(SelectedSprite, CurrentProject);

                SelectedSprite.Delete(CurrentProject);
                Sprites.Remove(SelectedSprite);

                SelectedSprite = null;
            }
        }

        #endregion

        public SpritesViewModel()
        {
            AddNewSpriteCommand = new RelayCommand(AddNewSpriteAction);
            EditSpriteCommand = new RelayCommand(EditSpriteAction, CanExecuteEditSpriteCommand);
            CopySpriteCommand = new RelayCommand(CopySpriteAction, CanExecuteCopySpriteCommand);
            DeleteSpriteCommand = new RelayCommand(DeleteSpriteAction, CanExecuteDeleteSpriteCommand);

            StartPlayerCommand = new RelayCommand(StartPlayerAction);

            UndoCommand = new RelayCommand(UndoAction);
            RedoCommand = new RelayCommand(RedoAction);

            ClearObjectsSelectionCommand = new RelayCommand(ClearObjectSelectionAction);

            Messenger.Default.Register<GenericMessage<Program>>(this,
                 ViewModelMessagingToken.CurrentProjectChangedListener, CurrentProjectChangedAction);

            Messenger.Default.Register<GenericMessage<Sprite>>(this,
                ViewModelMessagingToken.CurrentSpriteChangedListener, CurrentSpriteChangedMessageAction);
        }

        private void SpritesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(() => IsSpritesEmpty);
        }
    }
}
