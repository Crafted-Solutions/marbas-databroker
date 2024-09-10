using MarBasCommon;

namespace MarBasSchema.Event
{
    public enum SchemaModificationType
    {
        Update = 1, Create = 2, Delete = 3
    }

    public class SchemaModifiedEventArgs<TSubject>: EventArgs
        where TSubject: IIdentifiable
    {
        protected readonly IList<TSubject> _subjects;
        protected readonly SchemaModificationType _type;
        protected readonly Type _subjectType;

        public SchemaModifiedEventArgs(SchemaModificationType changeType, IEnumerable<TSubject>? subjects = null, Type? concreteSubjectType = null)
        {
            _type = changeType;
            _subjects = subjects?.ToList() ?? new List<TSubject>();
            _subjectType = concreteSubjectType ?? typeof(TSubject);
        }

        public void AddSubject(TSubject subject)
        {
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
