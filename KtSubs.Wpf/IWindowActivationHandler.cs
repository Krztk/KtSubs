using System.Collections.Generic;

namespace KtSubs.Wpf
{
    public interface IWindowActivationHandler
    {
        void OnWindowActivated(WindowParameters parameters);
    }

    public class WindowParameters
    {
        private Dictionary<string, object> parameters = new();

        public void Add(string name, object value)
        {
            parameters[name] = value;
        }

        public object Get(string name)
        {
            return parameters[name];
        }
    }
}