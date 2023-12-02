using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KtSubs.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KtSubs.Wpf.ViewModels
{
    public class LayersSettingsViewModel : Dialog
    {
        private readonly LayersSettingsManager layersSettingsManager;

        public List<LayerSettingEntryViewModel> LayerSettings { get; } = new();
        public IRelayCommand SaveSettingsCommand { get; }

        public LayersSettingsViewModel(LayersSettingsManager layersSettingsManager)
        {
            this.layersSettingsManager = layersSettingsManager;
            SaveSettingsCommand = new RelayCommand(SaveSettings, () => LayerSettings.Any((entry) => entry.IsActive));

            LayerSettings = layersSettingsManager.LayerNameIsActivePair.
                                      Values.Select(entry => new LayerSettingEntryViewModel(entry, SaveSettingsCommand.NotifyCanExecuteChanged))
                                      .ToList();
        }

        private void SaveSettings()
        {
            var values = LayerSettings.Select(layerSettingsVm => layerSettingsVm.GetLayerSettingsEntry());
            layersSettingsManager.ReplaceEntries(values);
            Close(true);
        }

        public class LayerSettingEntryViewModel : ObservableObject
        {
            private string layerName;

            public string LayerName
            {
                get { return layerName; }
                set { SetProperty(ref layerName, value); }
            }

            private bool isActive;
            private readonly Action notifyCanExecuteChanged;

            public bool IsActive
            {
                get { return isActive; }
                set
                {
                    if (SetProperty(ref isActive, value))
                    {
                        notifyCanExecuteChanged();
                    }
                }
            }

            public LayerSettingEntryViewModel(LayerSettingsEntry entry, Action NotifyCanExecuteChanged)
            {
                layerName = entry.Name;
                isActive = entry.IsActive;
                notifyCanExecuteChanged = NotifyCanExecuteChanged;
            }

            public LayerSettingsEntry GetLayerSettingsEntry()
            {
                return new LayerSettingsEntry(layerName, isActive);
            }
        }
    }
}