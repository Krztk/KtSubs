namespace KtSubs.Core.Settings
{
    public class Hotkey
    {
        public int Key { get; private set; }
        public int Modifiers { get; private set; }

        public Hotkey(int key, int modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }

        public static Hotkey FromString(string hotkey, Hotkey fallback)
        {
            try 
            {
                var parts = hotkey.Split('_');
                var modifiers = int.Parse(parts[0]);
                var key = int.Parse(parts[1]);

                return new Hotkey(key, modifiers);
            } catch (Exception)
            {
                return fallback;
            }
        }

        public static Hotkey Default()
        {
            int F11 = 100;
            return new Hotkey(F11, 0);
        }

        public override string ToString()
        {
            return $"{Modifiers}_{Key}";
        }
    }
}