namespace KtSubs.Core.Services
{
    public class LayersSettings
    {
        public event EventHandler LayersSettingsChanged;

        private readonly ISubtitlesStore subtitlesStore;
        public SortedDictionary<string, LayerSettingsEntry> LayerNameIsActivePair { get; private set; } = new();

        public LayersSettings(ISubtitlesStore subtitlesStore)
        {
            this.subtitlesStore = subtitlesStore;
            ReplaceEntries();
            subtitlesStore.SubtitlesLoaded += HandleNewSubtitlesLoaded;
        }

        public bool IsLayerActive(string layerName)
        {
            if (LayerNameIsActivePair.TryGetValue(layerName, out LayerSettingsEntry? value))
            {
                return value.IsActive;
            }

            return false;
        }

        public void ReplaceEntries(IEnumerable<LayerSettingsEntry> entries)
        {
            LayerNameIsActivePair = new SortedDictionary<string, LayerSettingsEntry>();
            foreach (var entry in entries)
            {
                LayerNameIsActivePair.Add(entry.Name, entry);
            }
            LayersSettingsChanged?.Invoke(this, new EventArgs());
        }

        private void ReplaceEntries()
        {
            LayerNameIsActivePair = new SortedDictionary<string, LayerSettingsEntry>();
            foreach (var layer in subtitlesStore.Layers)
            {
                LayerNameIsActivePair.Add(layer, new LayerSettingsEntry(layer, true));
            }
        }

        private void HandleNewSubtitlesLoaded(object? sender, EventArgs e)
        {
            ReplaceEntries();
        }
    }

    public class LayerSettingsEntry
    {
        public string Name { get; private set; }
        public bool IsActive { get; set; }

        public LayerSettingsEntry(string layerName, bool isActive)
        {
            Name = layerName;
            IsActive = isActive;
        }
    }
}