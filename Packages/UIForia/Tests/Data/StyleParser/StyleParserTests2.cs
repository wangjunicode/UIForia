using NUnit.Framework;
using UIForia;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Style;
using UIForia.Style2;
using UnityEngine;
using Style = UIForia.Style2.Style;

namespace Tests.StyleParser {

    public class DummyElement : UIElement { }

    public class StyleParserTests2 {

        [Test]
        public void ParseStyleBlockNormalNoConditions() {
            Module module = new Module<DummyElement>();

            StyleSheet2 sheet = StyleSheetParser.ParseString(module, @"
                style testStyle {

                    BackgroundColor = red;
                    BackgroundColor = orange;
                    PreferredWidth = 32%;
                   
                }
            ");
            
            sheet.Build();

            sheet.TryGetStyle("testStyle", out Style style);

            style.TryGetProperty(PropertyId.PreferredWidth, out StyleProperty2 width);
            style.TryGetProperty(PropertyId.BackgroundColor, out StyleProperty2 color);
            
            Assert.AreEqual(width.AsUIMeasurement, new UIMeasurement(0.32f, UIMeasurementUnit.Percentage));
            Assert.AreEqual(color.AsColor, new Color32(255, 165, 0, 255));
            Assert.AreEqual(style.GetPropertyCount(), 2);

        }
        
        [Test]
        public void ParseStyleBlockNormal_Constants() {
            Module module = new Module<DummyElement>();

            StyleSheet2 sheet = StyleSheetParser.ParseString(module, @"

                const x = red;

                style testStyle {

                    BackgroundColor = @x;
                    PreferredWidth = 32%;
                   
                }
            ");
            
            sheet.Build();

            sheet.TryGetStyle("testStyle", out Style style);

            style.TryGetProperty(PropertyId.PreferredWidth, out StyleProperty2 width);
            style.TryGetProperty(PropertyId.BackgroundColor, out StyleProperty2 color);
            
            Assert.AreEqual(width.AsUIMeasurement, new UIMeasurement(0.32f, UIMeasurementUnit.Percentage));
            Assert.AreEqual(color.AsColor, new Color32(255, 0, 0, 255));
            Assert.AreEqual(style.GetPropertyCount(), 2);

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

            Assert.AreEqual("red", sheet.GetConstant("x"));

            module.UpdateConditions(new DisplayConfiguration(500f, 300f, 1f));

            Assert.AreEqual("green", sheet.GetConstant("x"));

            module.UpdateConditions(new DisplayConfiguration(0, 0, 1f));

            Assert.AreEqual("orange", sheet.GetConstant("x"));
        }

    }

}