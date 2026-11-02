using AwesomeAssertions;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;
using System.ComponentModel;
using System.Linq.Expressions;

namespace CraftedSolutions.MarBasSchema.Tests
{
    [TestClass]
    public class UpdateableTrackerTest
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        class UpdateableMock
        {
            internal readonly UpdateableTracker tracker = new();
            int intprop;

            public int IntProperty
            {
                get => intprop;
                set
                {
                    if (tracker.IsChangeAccepted(intprop, value))
                    {
                        intprop = value;
                        tracker.TrackPropertyChange<UpdateableMock>();
                    }
                }
            }
        }

        [TestMethod]
        public void Clone_returning_UpdateableTracker()
        {
            var source = new UpdateableTracker();
            source.TrackPropertyChange<IIdentifiable>("Id");
            source.AddScope<INamed>();
            source.TrackPropertyChange<INamed>("Name");
            var tracker = (UpdateableTracker)source.Clone();

            tracker.AllChanges.Should().BeEquivalentTo(source.AllChanges);

            using var monitoredSource = source.Monitor();
            using var monitoredTarget = tracker.Monitor();
            source.TrackPropertyChange<INamed>("Name");
            monitoredSource.Should()
                .Raise("PropertyChanged")
                .WithSender(source)
                .WithArgs<PropertyChangedEventArgs>(args => "Name" == args.PropertyName);
            monitoredTarget.Should().NotRaise("PropertyChanged");
        }

        [TestMethod]
        public void TrackPropertyChange_adding_property_name_to_default_scope()
        {
            var tracker = new UpdateableTracker();
            tracker.TrackPropertyChange<IGrain>("Name");
            tracker.TrackPropertyChange<IGrain>("Title");
            tracker.TrackPropertyChange<IGrain>("Name");

            var defaultScope = tracker.GetScope<IGrain>();
            defaultScope.Should()
                .NotBeEmpty().And
                .Satisfy(
                    changedProperty => "Name" == changedProperty,
                    changedProperty => "Title" == changedProperty
                    );
        }

        [TestMethod]
        public void TrackPropertyChange_adding_property_name_to_registered_scope()
        {
            var propBag = new UpdateableMock();
            propBag.tracker.AddScope<UpdateableMock>();
            propBag.IntProperty = 2;

            var defaultScope = propBag.tracker.GetScope<IGrain>();
            defaultScope.Should().BeEmpty();

            var ownScope = propBag.tracker.GetScope<UpdateableMock>();
            ownScope.Should()
                .NotBeEmpty().And
                .Satisfy(changedProperty => nameof(UpdateableMock.IntProperty) == changedProperty);

            ownScope.Clear();
            propBag.IntProperty = 2;
            ownScope.Should().BeEmpty("duplicate property value was filtered");

            propBag.IntProperty = 3;
            ownScope.Should()
                .NotBeEmpty().And
                .Satisfy(changedProperty => nameof(UpdateableMock.IntProperty) == changedProperty);
        }

        [TestMethod]
        public void TrackPropertyChange_firing_PropertyChanged_event_given_modified_property()
        {
            var eventHandlerCalled = 0;
            var propBag = new UpdateableMock();
            propBag.tracker.PropertyChanged += (source, args) =>
            {
                if (nameof(UpdateableMock.IntProperty) == args.PropertyName)
                {
                    eventHandlerCalled++;
                }
            };
            propBag.IntProperty = 42;
            propBag.IntProperty = 43;

            eventHandlerCalled.Should().Be(2);

            propBag.IntProperty = 43;

            eventHandlerCalled.Should().Be(2, "duplicate property value was filtered");
        }

        [TestMethod]
        public void TrackPropertyChange_awlways_firing_PropertyChanged_event_given_AcceptAllChanges_is_set()
        {
            var eventHandlerCalled = 0;
            var propBag = new UpdateableMock();
            propBag.tracker.AcceptAllChanges = true;
            propBag.tracker.PropertyChanged += (source, args) =>
            {
                if (nameof(UpdateableMock.IntProperty) == args.PropertyName)
                {
                    eventHandlerCalled++;
                }
            };
            propBag.tracker.AcceptAllChanges.Should().BeTrue();

            propBag.IntProperty = 42;
            propBag.IntProperty = 43;

            eventHandlerCalled.Should().Be(2);

            propBag.IntProperty = 43;

            eventHandlerCalled.Should().Be(3);
        }

        [TestMethod]
        public void GetScope_returning_default_scope_for_unregistered_Type()
        {
            var tracker = new UpdateableTracker();
            var defaultScope1 = tracker.GetScope<IGrain>();
            var defaultScope2 = tracker.GetScope<ITrait>();
            defaultScope1.Should()
                .NotBeNull().And
                .BeSameAs(defaultScope2);
        }

        [TestMethod]
        public void GetScope_returning_correct_scope_for_registered_Type()
        {
            var tracker = new UpdateableTracker();
            tracker.AddScope<IGrain>();
            tracker.AddScope<ITrait>();

            var defaultScope1 = tracker.GetScope<string>();
            var defaultScope2 = tracker.GetScope<int>();
            defaultScope1.Should()
                .NotBeNull().And
                .BeSameAs(defaultScope2);

            var grainScope1 = tracker.GetScope<IGrain>();
            var grainScope2 = tracker.GetScope<IGrain>();
            grainScope1.Should()
                .NotBeNull().And
                .NotBeSameAs(defaultScope1).And
                .BeSameAs(grainScope2);

            var traitScope1 = tracker.GetScope<ITrait>();
            var traitScope2 = tracker.GetScope<ITrait>();
            traitScope1.Should()
                .NotBeNull().And
                .NotBeSameAs(defaultScope1).And
                .NotBeSameAs(grainScope1).And
                .BeSameAs(traitScope2);
        }

        public static void AssertFieldUpdates<TScope>(IUpdateable updateable, bool acceptAllChanges, params string[] properties)
        {
            if (acceptAllChanges)
            {
                var expr = properties.Select<string, Expression<Func<string, bool>>>(x => (property) => x == property);
                updateable.GetDirtyFields<TScope>().Should().Satisfy(expr.ToArray());
                updateable.GetDirtyFields<TScope>().Clear();
            }
            else
            {
                updateable.GetDirtyFields<TScope>().Should().BeEmpty();
            }
        }
    }
}
