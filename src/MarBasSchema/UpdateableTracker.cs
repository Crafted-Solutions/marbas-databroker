using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CraftedSolutions.MarBasSchema
{
    public sealed class UpdateableTracker: INotifyPropertyChanged, ICloneable
    {
        private readonly Dictionary<Type, HashSet<string>> _dirtyFields = new () { { typeof(object), [] } };

        public event PropertyChangedEventHandler? PropertyChanged;

        public UpdateableTracker() { }

        private UpdateableTracker(UpdateableTracker other)
        {
            _dirtyFields = new (other._dirtyFields);
        }

        public void TrackPropertyChange<TScope>([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            GetScope<TScope>().Add(propertyName);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public bool IsChangeAccepted<T>(T? oldValue, T? newValue, [CallerMemberName] string propertyName = "")
#pragma warning restore IDE0060 // Remove unused parameter
        {
            return AcceptAllChanges || !EqualityComparer<T>.Default.Equals(oldValue, newValue);
        }

        public bool AcceptAllChanges { get; set; }

        public void AddScope<TScope>()
        {
            var type = typeof(TScope);
            if (_dirtyFields.ContainsKey(type))
            {
                return;
            }
            _dirtyFields[type] = [];
        }

        public ISet<string> GetScope<TScope>()
        {
            if (_dirtyFields.TryGetValue(typeof(TScope), out var value))
            {
                return value;
            }
            return DefaultScope;
        }

        public ISet<string> DefaultScope => _dirtyFields[typeof(object)];

        public IEnumerable<string> AllChanges => _dirtyFields.SelectMany(x => x.Value);

        public object Clone()
        {
            return new UpdateableTracker(this);
        }
    }
}
