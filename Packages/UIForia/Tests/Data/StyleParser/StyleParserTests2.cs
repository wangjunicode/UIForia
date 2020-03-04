using NUnit.Framework;
using UIForia;
using UIForia.Elements;
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
            
            // always hold instance style -> small list
            // always hold implicit style -> small list
            // always hold inherited style -> medium list
            // sometimes hold selector styles -> variable list
            
            // each property should have a source associated with it for debugging and removal

            // element.GetStyle("alias.name");
            // element.GetStyle("name");
            // element.CloneStyle(styleptr);
            // clone.hover.SetProperty(property);
            
            // element.AddStyle(style);
            // <Element style="regular {stylePointer}" hook:name=""/>
            // Style style = sheet.GetStyle("testStyle", StyleState.Hover);
            //
            // StylePtr ptr = sheet.GetStylePointer("testStyle", StyleState.Hover);
            //
            // sheet.TryGetStyleProperty(style, PropertyId.Cursor);
            
            // pass style around
            // debug?
            // get style in c#
            // apply style from c#?

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