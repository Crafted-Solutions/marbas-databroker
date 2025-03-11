using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CraftedSolutions.MarBasSchema
{
    public class UpdateableTracker : INotifyPropertyChanged
    {
        private readonly ISet<string> _dirtyFields = new HashSet<string>();

        private Func<Type, ISet<string>>? _scopeGetter;
        private bool _acceptAlways = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void TrackPropertyChange<TScope>([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            GetScope<TScope>().Add(propertyName);
        }

        public bool IsChangeAccepted<T>(T? oldValue, T? newValue, [CallerMemberName] string propertyName = "")
        {
            return _acceptAlways || !EqualityComparer<T>.Default.Equals(oldValue, newValue);
        }

        public bool AcceptAllChanges { get => _acceptAlways; set => _acceptAlways = value; }

        public void AddScope<TScope>()
        {
            var df = new HashSet<string>();
            var prevGetter = _scopeGetter;
            _scopeGetter = (t) =>
            {
                if (typeof(TScope).IsAssignableFrom(t))
                {
                    //Console.WriteLine($"UpdateableTracker.GetScope<{typeof(TScope)}>");
                    return df;
                }
                //if (null == prevGetter)
                //    Console.WriteLine($"UpdateableTracker.GetScope<default>");
                return null == prevGetter ? _dirtyFields : prevGetter(t);
            };
        }

        public ISet<string> GetScope<TScope>()
        {
            //if (null == _scopeGetter)
            //    Console.WriteLine($"UpdateableTracker.GetScope<default>");
            return null == _scopeGetter ? _dirtyFields : _scopeGetter(typeof(TScope));
        }
    }
}
