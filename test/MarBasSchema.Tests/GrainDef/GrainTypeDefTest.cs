using AwesomeAssertions;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;
using CraftedSolutions.MarBasSchema.GrainDef;
using CraftedSolutions.MarBasSchema.Tests.Grain;
using System.Globalization;

namespace CraftedSolutions.MarBasSchema.Tests.GrainDef
{
    [TestClass]
    public class GrainTypeDefTest
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        class TypeDefMock : GrainPlain, ITypeDef
        {
            private readonly IEnumerable<Guid> _mixInIds = [Guid.NewGuid(), Guid.NewGuid()];

            public string? Impl { get; set; }

            public IEnumerable<Guid> MixInIds => _mixInIds;
        }

        [TestMethod]
        public void CTor_setting_defaults_given_no_parameters()
        {
            var reference = new GrainLocalized();
            var typeDef = new GrainTypeDef();

            typeDef.Should().BeEquivalentTo<IGrainLocalized>(reference,
               options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                   nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            AssertDefaults(typeDef);
        }

        [TestMethod]
        public void CTor_setting_Name()
        {
            var name = "Type42";
            var reference = new GrainLocalized(name);
            var typeDef = new GrainTypeDef(name);

            typeDef.Should().BeEquivalentTo<IGrainLocalized>(reference,
                options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                    nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            AssertDefaults(typeDef);
        }

        [TestMethod]
        public void CTor_setting_Name_and_Parent()
        {
            var name = "Type42";
            var parent = new Identifiable();
            var reference = new GrainLocalized(name, parent);
            var typeDef = new GrainTypeDef(name, parent);

            typeDef.Should().BeEquivalentTo<IGrainLocalized>(reference,
                options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                    nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            AssertDefaults(typeDef);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("tester")]
        public void CTor_setting_Name_Parent_and_Owner(string ownerName)
        {
            var name = "Type42";
            var parent = new Identifiable();
            var owner = GrainBaseTest.MakePrincipal(ownerName);
            var reference = new GrainLocalized(name, parent, owner);
            var typeDef = new GrainTypeDef(name, parent, creator: owner);

            typeDef.Should().BeEquivalentTo<IGrainLocalized>(reference,
                options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                    nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            AssertDefaults(typeDef);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("tester")]
        public void CTor_setting_Name_Parent_Owner_Culture_and_CultureInfo(string ownerName)
        {
            var name = "Type42";
            var parent = new Identifiable();
            var owner = GrainBaseTest.MakePrincipal(ownerName);
            var culture = CultureInfo.GetCultureInfo("en-US");
            var reference = new GrainLocalized(name, parent, owner, culture);
            var typeDef = new GrainTypeDef(name, parent, creator: owner, culture: culture);

            typeDef.Should().BeEquivalentTo<IGrainLocalized>(reference,
                options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                    nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            AssertDefaults(typeDef);
        }

        [TestMethod]
        public void CTor_setting_Name_Parent_and_MixIns()
        {
            var name = "Type42";
            var parent = new Identifiable();
            var mixins = new HashSet<IIdentifiable> { new Identifiable(), new Identifiable() };
            var reference = new GrainLocalized(name, parent);
            var typeDef = new GrainTypeDef(name, parent, mixins);

            typeDef.Should().BeEquivalentTo<IGrainLocalized>(reference,
                options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                    nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            typeDef.MixIns.Should().BeEquivalentTo(mixins);
            typeDef.MixInIds.Should().BeEquivalentTo(mixins.Select(x => x.Id));
        }

        [TestMethod]
        public void CTor_copying_IGrain()
        {
            var source = new GrainPlain()
            {
                Name = "Type42",
                Path = "marbas/Schema/Type42",
                Owner = "tester",
                ParentId = Guid.NewGuid()
            };
            var typeDef = new GrainTypeDef(source);

            typeDef.Should().BeEquivalentTo<IGrain>(source,
                options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                    nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            AssertDefaults(typeDef);
            typeDef.TypeDef.Should().BeNull();
            typeDef.TypeDefId.Should().BeNull();
            typeDef.TypeName.Should().BeNull();
        }

        [TestMethod]
        public void CTor_copying_ITypeDef()
        {
            var source = new TypeDefMock()
            {
                Name = "Type42",
                Path = "marbas/Schema/Type42",
                Owner = "tester",
                ParentId = Guid.NewGuid(),
                Impl = "MarBasSchema.TypeDef"
            };
            var typeDef = new GrainTypeDef(source);

            typeDef.Should().BeEquivalentTo<IGrain>(source,
                options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                    nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            typeDef.Should().BeEquivalentTo<ITypeDef>(source);
            typeDef.TypeDef.Should().BeNull();
            typeDef.TypeDefId.Should().BeNull();
            typeDef.TypeName.Should().BeNull();
        }

        [TestMethod]
        public void CTor_copying_IGrainTypeDef()
        {
            var source = new GrainTypeDef("Type42", new Identifiable(), [new Identifiable(), new Identifiable()], GrainBaseTest.MakePrincipal("tester"), CultureInfo.GetCultureInfo("en-US"))
            {
                Impl = "MarBasSchema.TypeDef",
                DefaultInstance = new Identifiable()
            };
            var typeDef = new GrainTypeDef(source);
            typeDef.Should().BeEquivalentTo(source);
            typeDef.GetDirtyFields<IGrainTypeDef>().Should()
                .NotBeEmpty().And
                .BeEquivalentTo(source.GetDirtyFields<IGrainTypeDef>());
            typeDef.TypeDef.Should().BeNull();
            typeDef.TypeDefId.Should().BeNull();
            typeDef.TypeName.Should().BeNull();
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Impl_changing_Impl(bool acceptAllChanges)
        {
            var typeDef = new GrainTypeDef();
            typeDef.FieldTracker.AcceptAllChanges = acceptAllChanges;

            typeDef.Impl.Should().BeNull();

            typeDef.Impl = null;
            UpdateableTrackerTest.AssertFieldUpdates<IGrainTypeDef>(typeDef, acceptAllChanges, nameof(ITypeDef.Impl));

            typeDef.Impl = "MarBasSchema.TypeDef";
            typeDef.Impl.Should().NotBeNull();
            typeDef.GetDirtyFields<IGrainTypeDef>().Should().Satisfy(proprety => nameof(ITypeDef.Impl) == proprety);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_DefaultInstance_changing_DefaultInstance_and_DefaultInstanceId(bool acceptAllChanges)
        {
            var typeDef = new GrainTypeDef();
            typeDef.FieldTracker.AcceptAllChanges = acceptAllChanges;

            typeDef.DefaultInstance.Should().BeNull();
            typeDef.DefaultInstanceId.Should().BeNull();

            typeDef.DefaultInstance = null;
            typeDef.DefaultInstanceId.Should().BeNull();
            UpdateableTrackerTest.AssertFieldUpdates<IGrainTypeDef>(typeDef, acceptAllChanges, nameof(IGrainTypeDef.DefaultInstance));

            typeDef.DefaultInstance = new Identifiable();
            typeDef.DefaultInstance.Should().NotBeNull();
            typeDef.DefaultInstanceId.Should().Be(typeDef.DefaultInstance.Id);
            typeDef.GetDirtyFields<IGrainTypeDef>().Should().Satisfy(property => nameof(IGrainTypeDef.DefaultInstance) == property);
        }

        [TestMethod]
        public void Set_TypeDef_throwing_NotSupportedException()
        {
            var typeDef = new GrainTypeDef();
            
            typeDef.TypeDef.Should().BeNull();
            typeDef.TypeDef = null;
            typeDef.GetDirtyFields<IGrainTypeDef>().Should().BeEmpty();
            typeDef.Invoking(typeDef => typeDef.TypeDef = new Identifiable()).Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void AddMixIn_appending_IIdentifiable_to_MixIns()
        {
            var mixins = new HashSet<IIdentifiable> { new Identifiable(), new Identifiable() };
            var mixin = new Identifiable();
            var typeDef = new GrainTypeDef("Type42", mixins: mixins);

            typeDef.AddMixIn(mixin);
            typeDef.MixIns.Should().BeEquivalentTo(mixins.Append(mixin));
            typeDef.MixInIds.Should().BeEquivalentTo(mixins.Select(x => x.Id).Append(mixin.Id));
            typeDef.GetDirtyFields<IGrainTypeDef>().Should().Satisfy(property => nameof(IGrainTypeDef.MixIns) == property);

            typeDef.GetDirtyFields<IGrainTypeDef>().Clear();
            typeDef.AddMixIn(mixin);
            typeDef.MixIns.Should().HaveCount(3);
            typeDef.GetDirtyFields<IGrainTypeDef>().Should().BeEmpty();
        }

        [TestMethod]
        public void RemoveMixIn_deleting_IIdentifiable_from_MixIns()
        {
            var mixin = new Identifiable();
            var mixins = new HashSet<IIdentifiable> { new Identifiable(), mixin };
            var typeDef = new GrainTypeDef("Type42", mixins: mixins);

            mixins.Remove(mixin);
            typeDef.RemoveMixIn(mixin);
            typeDef.MixIns.Should().BeEquivalentTo(mixins);
            typeDef.MixInIds.Should().BeEquivalentTo(mixins.Select(x => x.Id));
            typeDef.GetDirtyFields<IGrainTypeDef>().Should().Satisfy(property => nameof(IGrainTypeDef.MixIns) == property);

            typeDef.GetDirtyFields<IGrainTypeDef>().Clear();
            typeDef.RemoveMixIn(mixin);
            typeDef.MixIns.Should().BeEquivalentTo(mixins);
            typeDef.GetDirtyFields<IGrainTypeDef>().Should().BeEmpty();
        }

        [TestMethod]
        public void ClearMixIns_emptying_MixIns()
        {
            var typeDef = new GrainTypeDef("Type42", mixins: [new Identifiable(), new Identifiable()]);

            typeDef.ClearMixIns();
            typeDef.MixIns.Should().BeEmpty();
            typeDef.MixInIds.Should().BeEmpty();
            typeDef.GetDirtyFields<IGrainTypeDef>().Should().Satisfy(property => nameof(IGrainTypeDef.MixIns) == property);

            typeDef.GetDirtyFields<IGrainTypeDef>().Clear();
            typeDef.ClearMixIns();
            typeDef.MixInIds.Should().BeEmpty();
            typeDef.GetDirtyFields<IGrainTypeDef>().Should().BeEmpty();
        }

        [TestMethod]
        public void ReplaceMixIns_changing_MixIns()
        {
            var mixins = new HashSet<IIdentifiable> { new Identifiable(), new Identifiable() };
            var typeDef = new GrainTypeDef("Type42", mixins: [new Identifiable()]);

            typeDef.ReplaceMixIns(mixins);
            typeDef.MixIns.Should().BeEquivalentTo(mixins);
            typeDef.MixInIds.Should().BeEquivalentTo(mixins.Select(x => x.Id));
            typeDef.GetDirtyFields<IGrainTypeDef>().Should().Satisfy(property => nameof(IGrainTypeDef.MixIns) == property);

            typeDef.GetDirtyFields<IGrainTypeDef>().Clear();
            typeDef.ReplaceMixIns(null);
            typeDef.MixIns.Should().BeEmpty();
            typeDef.MixInIds.Should().BeEmpty();
            typeDef.GetDirtyFields<IGrainTypeDef>().Should().Satisfy(property => nameof(IGrainTypeDef.MixIns) == property);
        }

        public static void AssertDefaults(GrainTypeDef typeDef)
        {
            typeDef.DefaultInstance.Should().BeNull();
            typeDef.DefaultInstanceId.Should().BeNull();
            typeDef.MixIns.Should().BeEmpty();
            typeDef.MixInIds.Should().BeEmpty();
            typeDef.Impl.Should().BeNull();
        }
    }
}
