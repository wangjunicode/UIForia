using NUnit.Framework;
using UIForia;
using UIForia.Elements;
using UIForia.Style;
using UIForia.Style2;

namespace Tests.StyleParser {

    public class DummyElement : UIElement { }

    public class StyleParserTests2 {

        [Test]
        public void ParseStyleBlockNormalNoConditions() {
            Module module = new Module<DummyElement>();

            StyleSheet2 sheet = StyleSheetParser.ParseString(module, @"
                style testStyle {

                    BackgroundColor = orange;
                    PreferredWidth = 32%;
                    FlexLayoutDirection = Horizontal;
                    TextFontSize = 12px;

                }
            ");

            Style style = sheet.GetStyle("testStyle");

        }

        [Test]
        public void ParseLocalConstantWithoutCondition() {
            Module module = new Module<DummyElement>();

            StyleSheet2 sheet = StyleSheetParser.ParseString(module, @"const x = red;");

            Assert.AreEqual("red", sheet.GetConstant("x"));
        }

        [Test]
        public void ParseLocalConstantWithCondition() {
            Module module = new Module<DummyElement>();

            module.RegisterDisplayCondition("one", (d) => d.screenWidth > 200 && d.screenWidth < 400);
            module.RegisterDisplayCondition("two", (d) => d.screenWidth >= 400);

            StyleSheet2 sheet = StyleSheetParser.ParseString(module, @"
                const x { 
                    #one = red;
                    #two = green;
                    default = orange;
                }"
            );

            module.UpdateConditions(new DisplayConfiguration(300f, 300f, 1f));

            Assert.AreEqual("red", sheet.GetConstant("x", module.GetDisplayConditions()));

            module.UpdateConditions(new DisplayConfiguration(500f, 300f, 1f));

            Assert.AreEqual("green", sheet.GetConstant("x", module.GetDisplayConditions()));

            module.UpdateConditions(new DisplayConfiguration(0, 0, 1f));

            Assert.AreEqual("orange", sheet.GetConstant("x", module.GetDisplayConditions()));
        }

    }

}