using CraftedSolutions.MarBasCommon;

namespace CraftedSolutions.MarBasSchema.Event
{
    public enum SchemaModificationType
    {
        Update = 1, Create = 2, Delete = 3
    }

    public class SchemaModifiedEventArgs<TSubject>(SchemaModificationType changeType, IEnumerable<TSubject>? subjects = null, Type? concreteSubjectType = null)
        : EventArgs
        where TSubject : IIdentifiable
    {
        protected readonly IList<TSubject> _subjects = subjects?.ToList() ?? [];
        protected readonly SchemaModificationType _type = changeType;
        protected readonly Type _subjectType = concreteSubjectType ?? typeof(TSubject);

        public void AddSubject(TSubject subject)
        {
            if (!subject.GetType().IsAssignableTo(_subjectType))
            {
                throw new ArgumentException($"{nameof(subject)} must be instance of {_subjectType.Name}");
            }
            if (!_subjects.Any((x) => x.Id == subject.Id))
            {
                _subjects.Add(subject);
            }
        }

        public bool RemoveSubject(TSubject subject)
        {
            var result = _subjects.Remove(subject);
            if (!result)
            {
                var byId = _subjects.FirstOrDefault((x) => x.Id == subject.Id);
                if (null != byId)
                {
                    result = _subjects.Remove(byId);
                }
            }
            return result;
        }

        public IEnumerable<TSubject> Subjects => _subjects;
        public SchemaModificationType ChangeType => _type;
        public Type ConcreteSubjectType => _subjectType;
    }
}
