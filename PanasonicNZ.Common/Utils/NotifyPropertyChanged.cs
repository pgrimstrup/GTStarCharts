using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PanasonicNZ.Common
{
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        protected readonly Dictionary<string, object> PropertyValues = new Dictionary<string, object>();

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler StateChanged;

        bool _changed;

        public virtual bool HasChanged
        {
            get => _changed;
            protected set
            {
                if(_changed != value)
                {
                    _changed = value;
                    OnStateChanged();
                }
            }
        }

        public virtual void UnChanged()
        {
            HasChanged = false;
        }

        public void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        protected T Get<T>([CallerMemberName] string propertyName = null)
        {
            if (String.IsNullOrEmpty(propertyName)) return default(T);
            if (!PropertyValues.TryGetValue(propertyName, out object currentValue))
                return default(T);
            return (T)currentValue;
        }

        protected bool Set<T>(T newValue, [CallerMemberName] string propertyName = null)
        {
            if (String.IsNullOrEmpty(propertyName)) return false;

            var currentValue = Get<T>(propertyName);
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue)) return false;

            PropertyValues[propertyName] = newValue;

            HasChanged = true;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
