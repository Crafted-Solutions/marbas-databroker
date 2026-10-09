using AwesomeAssertions;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;
using CraftedSolutions.MarBasSchema.GrainTier;
using CraftedSolutions.MarBasSchema.IO;
using CraftedSolutions.MarBasSchema.Tests.Grain;
using System.Globalization;
using System.Net.Mime;
using System.Text;

namespace CraftedSolutions.MarBasSchema.Tests.GrainTier
{
    [TestClass]
    public class GrainFileTest
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        class FileMock : GrainPlain, IFile
        {
            public string MimeType { get; set; } = MediaTypeNames.Text.Plain;

            public long Size => Content?.Length ?? 0;

            public IStreamableContent? Content { get; set; } = new StreamableContent(Encoding.UTF8.GetBytes("lorem impsum"));

#pragma warning disable CA1822 // Mark members as static
            public new Guid? TypeDefId => SchemaDefaults.FileTypeDefID;
#pragma warning restore CA1822 // Mark members as static
        }

        [TestMethod]
        public void CTor_setting_defaults_given_no_parameters()
        {
            var reference = new GrainLocalized();
            var file = new GrainFile();

            file.Should().BeEquivalentTo<IGrainLocalized>(reference,
               options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                   nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            AssertDefaults(file);
        }


        [TestMethod]
        public void CTor_setting_Name()
        {
            var name = "File42";
            var reference = new GrainLocalized(name);
            var file = new GrainFile(name);

            file.Should().BeEquivalentTo<IGrainLocalized>(reference,
               options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                   nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            AssertDefaults(file);
        }

        [TestMethod]
        public void CTor_setting_Name_and_Parent()
        {
            var name = "File42";
            var parent = new Identifiable();
            var reference = new GrainLocalized(name, parent);
            var file = new GrainFile(name, parent);

            file.Should().BeEquivalentTo<IGrainLocalized>(reference,
               options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                   nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            AssertDefaults(file);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("tester")]
        public void CTor_setting_Name_Parent_and_Owner(string ownerName)
        {
            var name = "File42";
            var parent = new Identifiable();
            var owner = GrainBaseTest.MakePrincipal(ownerName);
            var reference = new GrainLocalized(name, parent, owner);
            var file = new GrainFile(name, parent, owner);

            file.Should().BeEquivalentTo<IGrainLocalized>(reference,
               options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                   nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            AssertDefaults(file);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("tester")]
        public void CTor_setting_Name_Parent_Owner_Culture_and_CultureInfo(string ownerName)
        {
            var name = "File42";
            var parent = new Identifiable();
            var owner = GrainBaseTest.MakePrincipal(ownerName);
            var culture = CultureInfo.GetCultureInfo("en-US");
            var reference = new GrainLocalized(name, parent, owner, culture);
            var file = new GrainFile(name, parent, owner, culture);

            file.Should().BeEquivalentTo<IGrainLocalized>(reference,
               options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                   nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            AssertDefaults(file);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void CTor_copying_IGrain(bool withType)
        {
            var source = new GrainPlain()
            {
                Name = "File42",
                Path = "marbas/Files/File42",
                Owner = "tester",
                ParentId = Guid.NewGuid()
            };
            if (withType)
            {
                source.TypeDefId = Guid.NewGuid();
            }
            var file = new GrainFile(source);
            file.Should().BeEquivalentTo<IGrain>(source,
                options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                    nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            if (!withType)
            {
                AssertDefaults(file);
            }
        }

        [TestMethod]
        public void CTor_copying_IFile()
        {
            var source = new FileMock()
            {
                Name = "File42",
                Path = "marbas/Files/File42",
                Owner = "tester",
                ParentId = Guid.NewGuid()
            };
            var file = new GrainFile(source);
            file.Should().BeEquivalentTo<IGrain>(source,
                options => options.ExcludingMembersNamed(nameof(IGrain.CTime), nameof(IGrain.MTime),
                    nameof(ITypeConstraint.TypeDef), nameof(ITypeConstraint.TypeName), nameof(ITypeConstraint.TypeDefId)));
            file.Should().BeEquivalentTo<IFile>(source);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_MimeType_changing_MimeType(bool acceptAllChanges)
        {
            var file = new GrainFile();
            file.FieldTracker.AcceptAllChanges = acceptAllChanges;

            file.MimeType.Should().Be(MediaTypeNames.Application.Octet);

            file.MimeType = string.Empty;
            file.MimeType.Should().Be(MediaTypeNames.Application.Octet);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainFile>(file, acceptAllChanges, nameof(IFile.MimeType));

            file.MimeType = MediaTypeNames.Application.Octet;
            file.MimeType.Should().Be(MediaTypeNames.Application.Octet);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainFile>(file, acceptAllChanges, nameof(IFile.MimeType));

            file.MimeType = MediaTypeNames.Image.Jpeg;
            file.MimeType.Should().Be(MediaTypeNames.Image.Jpeg);
            file.GetDirtyFields<IGrainFile>().Should().Satisfy(property => nameof(IFile.MimeType) == property);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Size_changing_Size(bool acceptAllChanges)
        {
            var file = new GrainFile();
            file.FieldTracker.AcceptAllChanges = acceptAllChanges;

            file.Size.Should().Be(0);

            file.Size = 0;
            file.Size.Should().Be(0);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainFile>(file, acceptAllChanges, nameof(IFile.Size));

            file.Size = 42;
            file.Size.Should().Be(42);
            file.GetDirtyFields<IGrainFile>().Should().Satisfy(property => nameof(IFile.Size) == property);

            file.Content = new StreamableContent(new byte[100]);
            file.GetDirtyFields<IGrainFile>().Clear();

            file.Size = 42;
            file.Size.Should().Be(100);
            file.GetDirtyFields<IGrainFile>().Should().BeEmpty();
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Content_changing_Content_and_Size(bool acceptAllChanges)
        {
            var content = new StreamableContent(new byte[100]);
            var file = new GrainFile();
            file.FieldTracker.AcceptAllChanges = acceptAllChanges;

            file.Size.Should().Be(0);
            file.Content.Should().BeNull();

            file.Content = null;
            file.Content.Should().BeNull();
            file.Size.Should().Be(0);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainFile>(file, acceptAllChanges, nameof(IFile.Content), nameof(IFile.Size));

            file.Content = content;
            file.Content.Should()
                .NotBeNull().And
                .Satisfy<IStreamableContent>(content => content.Length.Should().Be(100));
            file.Size.Should().Be(100);
            file.GetDirtyFields<IGrainFile>().Should().Satisfy(
                property => nameof(IFile.Content) == property,
                property => nameof(IFile.Size) == property
                );

            file.GetDirtyFields<IGrainFile>().Clear();
            file.Content = content;
            file.Size.Should().Be(100);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainFile>(file, acceptAllChanges, nameof(IFile.Content), nameof(IFile.Size));

            file.GetDirtyFields<IGrainFile>().Clear();
            file.Content = new StreamableContent(new byte[100]);
            file.GetDirtyFields<IGrainFile>().Remove(nameof(IFile.Content));
            file.Size.Should().Be(100);
            UpdateableTrackerTest.AssertFieldUpdates<IGrainFile>(file, acceptAllChanges, nameof(IFile.Size));
        }

        private static void AssertDefaults(GrainFile file)
        {
            file.TypeDefId.Should().Be(SchemaDefaults.FileTypeDefID);
            file.TypeName.Should().Be(SchemaDefaults.FileTypeName);
            file.MimeType.Should().Be(MediaTypeNames.Application.Octet);
            file.Content.Should().BeNull();
            file.Size.Should().Be(0);
        }
    }
}
