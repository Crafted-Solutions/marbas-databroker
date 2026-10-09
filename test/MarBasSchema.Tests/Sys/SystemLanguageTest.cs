using AwesomeAssertions;
using CraftedSolutions.MarBasSchema.Sys;
using System.Globalization;

namespace CraftedSolutions.MarBasSchema.Tests.Sys
{
    [TestClass]
    public class SystemLanguageTest
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        class SystemLanguageMock() : SystemLanguage("de-DE", "German")
        {
        }

        [TestMethod]
        public void CTor_setting_IsoCode_and_Label()
        {
            var code = "de";
            var label = "German";
            var lang = new SystemLanguage(code, label);

            lang.IsoCode.Should().Be(code);
            lang.Label.Should().Be(label);
            lang.LabelNative.Should().BeNull();
        }

        [TestMethod]
        public void CTor_setting_IsoCode_Label_and_LabelNative()
        {
            var code = "de";
            var label = "German";
            var native = "Deutsch";
            var lang = new SystemLanguage(code, label, native);

            lang.IsoCode.Should().Be(code);
            lang.Label.Should().Be(label);
            lang.LabelNative.Should().Be(native);
        }

        [TestMethod]
        public void CTor_copying_CultureInfo()
        {
            var culture = CultureInfo.GetCultureInfo("fr-FR", true);
            var lang = new SystemLanguage(culture);

            lang.IsoCode.Should().Be(culture.Name);
            lang.Label.Should().Be(culture.EnglishName);
            lang.LabelNative.Should().Be(culture.NativeName);
        }

        [TestMethod]
        public void CTor_copying_ISystemLanguage()
        {
            var source = new SystemLanguageMock()
            {
                LabelNative = "Deutsch (Deutschland)"
            };
            var lang = new SystemLanguage((ISystemLanguage)source);

            lang.Should().BeEquivalentTo(source);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_IsoCode_changing_IsoCode(bool acceptAllChanges)
        {
            var code = "en";
            var lang = new SystemLanguage(code, "English");
            lang.FieldTracker.AcceptAllChanges = acceptAllChanges;

            lang.IsoCode.Should().Be(code);

            lang.IsoCode = code;

            lang.IsoCode.Should().Be(code);
            UpdateableTrackerTest.AssertFieldUpdates<ISystemLanguage>(lang, acceptAllChanges,
                nameof(ISystemLanguageRef.IsoCode), nameof(ISystemLanguage.Label), nameof(ISystemLanguage.LabelNative));

            lang.IsoCode = "fantasy";

            lang.IsoCode.Should().NotBe(code);
            lang.Label.Should().Be("English");
            lang.GetDirtyFields<ISystemLanguage>().Should().Satisfy(
                property => nameof(ISystemLanguage.IsoCode) == property
                );
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_IsoCode_changing_IsoCode_Label_and_LabelNative(bool acceptAllChanges)
        {
            var code = "en";
            var label = "English";
            var lang = new SystemLanguage(code, label);
            lang.FieldTracker.AcceptAllChanges = acceptAllChanges;

            lang.IsoCode.Should().Be(code);
            lang.Label.Should().Be(label);

            lang.IsoCode = code;

            lang.IsoCode.Should().Be(code);
            UpdateableTrackerTest.AssertFieldUpdates<ISystemLanguage>(lang, acceptAllChanges,
                nameof(ISystemLanguageRef.IsoCode), nameof(ISystemLanguage.Label), nameof(ISystemLanguage.LabelNative));

            var culture = CultureInfo.GetCultureInfo("en-US", true);

            lang.IsoCode = culture.Name;

            lang.IsoCode.Should().Be(culture.Name);
            lang.Label.Should().Be(culture.EnglishName);
            lang.LabelNative.Should().Be(culture.NativeName);
            lang.GetDirtyFields<ISystemLanguage>().Should().Satisfy(
                property => nameof(ISystemLanguage.IsoCode) == property,
                property => nameof(ISystemLanguage.Label) == property,
                property => nameof(ISystemLanguage.LabelNative) == property
                );
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_Label_changing_Label(bool acceptAllChanges)
        {
            var code = "en-US";
            var label = "English";
            var lang = new SystemLanguage(code, label);
            lang.FieldTracker.AcceptAllChanges = acceptAllChanges;

            lang.IsoCode.Should().Be(code);
            lang.Label.Should().Be(label);

            lang.Label = label;

            lang.Label.Should().Be(label);
            UpdateableTrackerTest.AssertFieldUpdates<ISystemLanguage>(lang, acceptAllChanges, nameof(ISystemLanguage.Label));

            lang.Label = "English (US)";
            lang.Label.Should().NotBe(label);
            lang.GetDirtyFields<ISystemLanguage>().Should().Satisfy(
                property => nameof(ISystemLanguage.Label) == property
                );

        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Set_LabelNative_changing_LabelNative(bool acceptAllChanges)
        {
            var culture = CultureInfo.GetCultureInfo("de-DE", true);
            var lang = new SystemLanguage(culture);
            lang.FieldTracker.AcceptAllChanges = acceptAllChanges;

            lang.LabelNative.Should().Be(culture.NativeName);

            lang.LabelNative = culture.NativeName;

            lang.LabelNative.Should().Be(culture.NativeName);
            UpdateableTrackerTest.AssertFieldUpdates<ISystemLanguage>(lang, acceptAllChanges, nameof(ISystemLanguage.LabelNative));

            lang.LabelNative = "Deutsch";
            lang.Label.Should().NotBe(culture.NativeName);
            lang.GetDirtyFields<ISystemLanguage>().Should().Satisfy(
                property => nameof(ISystemLanguage.LabelNative) == property
                );
        }

        [TestMethod]
        public void Cast_from_CultureInfo_producing_valid_instance()
        {
            var culture = CultureInfo.GetCultureInfo("fr-FR", true);
            SystemLanguage lang = culture;

            lang.IsoCode.Should().Be(culture.Name);
            lang.Label.Should().Be(culture.EnglishName);
            lang.LabelNative.Should().Be(culture.NativeName);
        }

        [TestMethod]
        public void Cast_to_CultureInfo_producing_specific_CultureInfo()
        {
            var lang = new SystemLanguage("de", "German", "Deutsch");
            CultureInfo culture = lang;

            culture.Name.Should().Be(lang.IsoCode);
            culture.EnglishName.Should().Be(lang.Label);
            culture.NativeName.Should().Be(lang.LabelNative);
        }

        [TestMethod]
        public void Cast_to_CultureInfo_producing_invariant_CultureInfo()
        {
            var lang = new SystemLanguage(string.Empty, string.Empty);
            CultureInfo culture = lang;

            culture.Should().BeEquivalentTo(CultureInfo.InvariantCulture);
        }

        [TestMethod]
        public void Cast_to_string_producing_IsoCode()
        {
            var lang = new SystemLanguage("en-UK", "English (UK)");
            string code = lang;

            code.Should().Be(lang.IsoCode);
        }
    }
}
