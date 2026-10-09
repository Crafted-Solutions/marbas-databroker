using System.Collections;
using System.Diagnostics.CodeAnalysis;
using CraftedSolutions.MarBasCommon;

namespace CraftedSolutions.MarBasSchema.Grain
{
    public class GrainTraitsMap : IDictionary<string, IList<ITrait>?>
    {
        private readonly IDictionary<string, IList<ITrait>?> _map = new Dictionary<string, IList<ITrait>?>();

        public void Emplace(ITrait trait, string? key = null)
        {
            var propName = key ?? (trait.PropDef as INamed)?.Name;
            if (string.IsNullOrEmpty(propName))
            {
                throw new ArgumentException($"Either {nameof(trait)}.{nameof(ITraitRef.PropDef)} is named or {nameof(key)} is required to be non-empty string");
            }
            propName = propName.Trim().Replace(' ', '_');
            List<ITrait>? vals = null;
            if (_map.TryGetValue(propName, out var value))
            {
                vals = (List<ITrait>?)value;
            }
            if (null == vals)
            {
                if (!trait.IsNull)
                {
                    _map[propName] = [trait];
                }
                return;
            }

            if (0 < vals.Count && vals[0].ValueType != trait.ValueType)
            {
                throw new ArgumentException($"{nameof(trait)}[{propName}].{nameof(ITrait.ValueType)} of {trait.ValueType} is incompatible with exiting {vals[0].ValueType}");
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
            if (_map.TryGetValue(key, out var vals) && null != vals)
            {
                return vals.Select((t) => t.Value).ToArray();
            }
            return null;
        }

        public T?[]? GetValues<T>(string key)
        {
            if (_map.TryGetValue(key, out var vals) && null != vals)
            {
                return vals.Where(t => t is ITraitValue<T>).Select((t) => ((ITraitValue<T>)t).Value).ToArray();
            }
            return null;
        }

        public IList<ITrait>? this[string key] { get => _map[key]; set => _map[key] = value; }

        public ICollection<string> Keys => _map.Keys;

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public ICollection<IList<ITrait>?> Values => _map.Values;

        public int Count => _map.Count;

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public bool IsReadOnly => false;

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void Add(string key, IList<ITrait>? value) => _map.Add(key, value);

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void Add(KeyValuePair<string, IList<ITrait>?> item) => _map.Add(item);

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void Clear() => _map.Clear();

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public bool Contains(KeyValuePair<string, IList<ITrait>?> item) => _map.Contains(item);

        public bool ContainsKey(string key) => _map.ContainsKey(key);

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void CopyTo(KeyValuePair<string, IList<ITrait>?>[] array, int arrayIndex) => _map.CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<string, IList<ITrait>?>> GetEnumerator() => _map.GetEnumerator();

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public bool Remove(string key) => _map.Remove(key);

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public bool Remove(KeyValuePair<string, IList<ITrait>?> item) => _map.Remove(item);

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out IList<ITrait>? value) => _map.TryGetValue(key, out value);

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_map).GetEnumerator();
    }
}
