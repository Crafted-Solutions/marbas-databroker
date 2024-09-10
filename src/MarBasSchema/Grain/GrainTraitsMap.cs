using System.Collections;
using System.Diagnostics.CodeAnalysis;
using MarBasCommon;

namespace MarBasSchema.Grain
{
    public class GrainTraitsMap : IDictionary<string, IList<ITraitBase>?>
    {
        private readonly IDictionary<string, IList<ITraitBase>?> _map = new Dictionary<string, IList<ITraitBase>?>();

        public void Set(ITraitBase trait, string? key = null)
        {
            var propName = key ?? (trait.PropDef as INamed)?.Name;
            if (string.IsNullOrEmpty(propName))
            {
                throw new ArgumentException($"Either {nameof(trait)} is named or {nameof(key)} is required to be non-empty string");
            }
            propName = propName.Trim().Replace(' ', '_');
            List<ITraitBase>? vals = null;
            if (_map.ContainsKey(propName))
            {
                vals = (List<ITraitBase>?)_map[propName];
            }
            if (null == vals)
            {
                if (!trait.IsNull)
                {
                    _map[propName] = new List<ITraitBase>() { trait };
                }
                return;
            }

            var ind = vals.FindIndex((t) => t.Id == trait.Id);
            if (-1 < ind)
            {
                vals[ind] = trait;
                return;
            }

            ind = vals.FindIndex((t) => t.Ord > trait.Ord);
            if (0 > ind)
            {
                vals.Add(trait);
            }
            else
            {
                vals.Insert(ind, trait);
            }
        }

        public object?[]? GetValues(string key)
        {
            if (ContainsKey(key))
            {
                return null;
            }

            var vals = _map[key];
            if (null == vals)
            {
                return null;
            }
            return vals.Select((t) => t.Value).ToArray();
        }

        public T?[]? GetValues<T>(string key)
        {
            if (ContainsKey(key))
            {
                return null;
            }

            var vals = _map[key];
            if (null == vals)
            {
                return null;
            }
            return vals.Where(t => t is ITraitValue<T>).Select((t) => ((ITraitValue<T>)t).Value).ToArray();
        }

        public IList<ITraitBase>? this[string key] { get => _map[key]; set => _map[key] = value; }

        public ICollection<string> Keys => _map.Keys;

        public ICollection<IList<ITraitBase>?> Values => _map.Values;

        public int Count => _map.Count;

        public bool IsReadOnly => false;

        public void Add(string key, IList<ITraitBase>? value) => _map.Add(key, value);

        public void Add(KeyValuePair<string, IList<ITraitBase>?> item) => _map.Add(item);

        public void Clear() => _map.Clear();

        public bool Contains(KeyValuePair<string, IList<ITraitBase>?> item) => _map.Contains(item);

        public bool ContainsKey(string key) => _map.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, IList<ITraitBase>?>[] array, int arrayIndex) => _map.CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<string, IList<ITraitBase>?>> GetEnumerator() => _map.GetEnumerator();

        public bool Remove(string key) => _map.Remove(key);

        public bool Remove(KeyValuePair<string, IList<ITraitBase>?> item) => _map.Remove(item);

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out IList<ITraitBase>? value) => _map.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_map).GetEnumerator();
    }
}
