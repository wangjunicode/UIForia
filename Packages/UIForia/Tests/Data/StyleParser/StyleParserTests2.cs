using NUnit.Framework;
using UIForia;
using UIForia.Elements;
using UIForia.Exceptions;
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

            Assert.AreEqual(new UIMeasurement(0.32f, UIMeasurementUnit.Percentage), width.AsUIMeasurement);
            Assert.AreEqual(new Color32(255, 165, 0, 255), color.AsColor);
            Assert.AreEqual(style.GetPropertyCount(), 2);
        }
        
        [Test]
        public void ParseStyleShorthand_NoVariables() {
            Module module = new Module<DummyElement>();

            StyleSheet2 sheet = StyleSheetParser.ParseString(module, @"
                style testStyle {

                    PreferredSize = 32px;
                   
                }
            ");

            sheet.Build();

            sheet.TryGetStyle("testStyle", out Style style);

            style.TryGetProperty(PropertyId.PreferredWidth, out StyleProperty2 width);
            style.TryGetProperty(PropertyId.PreferredHeight, out StyleProperty2 height);

            Assert.AreEqual(new UIMeasurement(32f), width.AsUIMeasurement);
            Assert.AreEqual(new UIMeasurement(32f), height.AsUIMeasurement);
            Assert.AreEqual(style.GetPropertyCount(), 2);
        }
        
        [Test]
        public void ParseStyleShorthand_TwoConstants() {
            Module module = new Module<DummyElement>();
    
            StyleSheet2 sheet = StyleSheetParser.ParseString(module, @"

                const width = 1242px;
                const height = 984785.4vh;

                style testStyle {
                    // with and without comma should both work
                    PreferredSize = @width, @height;
                    PreferredSize = @width @height;
                   
                }
            ");

            sheet.Build();

            sheet.TryGetStyle("testStyle", out Style style);

            style.TryGetProperty(PropertyId.PreferredWidth, out StyleProperty2 width);
            style.TryGetProperty(PropertyId.PreferredHeight, out StyleProperty2 height);
            Assert.AreEqual(new UIMeasurement(1242f), width.AsUIMeasurement);
            Assert.AreEqual(new UIMeasurement(984785.4f, UIMeasurementUnit.ViewportHeight), height.AsUIMeasurement);
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
        public void ParseStyleBlock_Hover() {
            Module module = new Module<DummyElement>();

            StyleSheet2 sheet = StyleSheetParser.ParseString(module, @"

                const x = red;

                style testStyle {

                    BackgroundColor = white;

                    [hover] {
                        BackgroundColor = @x;
                        PreferredWidth = 32%;
                    }
                   
                }
            ");

            sheet.Build();

            sheet.TryGetStyle("testStyle", out Style style);

            style.TryGetProperty(PropertyId.PreferredWidth, out StyleProperty2 width, StyleState.Hover);
            style.TryGetProperty(PropertyId.BackgroundColor, out StyleProperty2 color, StyleState.Hover);
            style.TryGetProperty(PropertyId.BackgroundColor, out StyleProperty2 normalColor);

            Assert.AreEqual(width.AsUIMeasurement, new UIMeasurement(0.32f, UIMeasurementUnit.Percentage));
            Assert.AreEqual(color.AsColor, new Color32(255, 0, 0, 255));
            Assert.AreEqual(normalColor.AsColor, new Color32(255, 255, 255, 255));
            Assert.AreEqual(style.GetPropertyCount(), 3);
            Assert.AreEqual(style.GetPropertyCount(StyleState.Normal), 1);
            Assert.AreEqual(style.GetPropertyCount(StyleState.Hover), 2);
        }

        [Test]
        public void ThowForUndeclaredConstant() {
            Module module = new Module<DummyElement>();
            ParseException ex = Assert.Throws<ParseException>(() => {
                StyleSheet2 sheet = StyleSheetParser.ParseString(module, @"
                style testStyle {

                    BackgroundColor = white;

                    [hover] {
                        BackgroundColor = @x;
                        PreferredWidth = 32%;
                    }
                   
                }
            ");
            });
            Assert.AreEqual("Error in file STRING: Cannot find a definition for constant 'x'. Be sure you declare it before using it on line 6.", ex.Message);
        }

        [Test]
        public void ParseStyleBlock_HoverConditional() {
            Module module = new Module<DummyElement>();
            module.RegisterDisplayCondition("one", (d) => d.screenWidth > 500);
            module.UpdateConditions(new DisplayConfiguration(1000, 1000, 1));

            StyleSheet2 sheet = StyleSheetParser.ParseString(module, @"

                style testStyle {

                    BackgroundColor = white;

                    #one {
                        [hover] {
                            BackgroundColor = red;
                            PreferredWidth = 32%;
                        }
                    }
                   
                }
            ");

            sheet.Build();

            sheet.TryGetStyle("testStyle", out Style style);

            style.TryGetProperty(PropertyId.PreferredWidth, out StyleProperty2 width, StyleState.Hover);
            style.TryGetProperty(PropertyId.BackgroundColor, out StyleProperty2 color, StyleState.Hover);
            style.TryGetProperty(PropertyId.BackgroundColor, out StyleProperty2 normalColor);

            Assert.AreEqual(width.AsUIMeasurement, new UIMeasurement(0.32f, UIMeasurementUnit.Percentage));
            Assert.AreEqual(new Color32(255, 0, 0, 255), color.AsColor);
            Assert.AreEqual(new Color32(255, 255, 255, 255), normalColor.AsColor);
            Assert.AreEqual(style.GetPropertyCount(), 3);
            Assert.AreEqual(style.GetPropertyCount(StyleState.Normal), 1);
            Assert.AreEqual(style.GetPropertyCount(StyleState.Hover), 2);

            module.UpdateConditions(new DisplayConfiguration(100, 100, 1));
            sheet.Build();

            Assert.AreEqual(style.GetPropertyCount(StyleState.Hover), 0);
            Assert.AreEqual(style.GetPropertyCount(StyleState.Normal), 1);
        }

        [Test]
        public void ParseStyleBlockConditional() {
            Module module = new Module<DummyElement>();

            module.RegisterDisplayCondition("one", (c) => c.screenWidth > 2000);

            module.UpdateConditions(new DisplayConfiguration(3000, 3000, 1));

            StyleSheet2 sheet = StyleSheetParser.ParseString(module, @"

                style testStyle {

                    BackgroundColor = white;
                    
                    #one {
                        BackgroundColor = red;
                        PreferredWidth = 32%;
                    }
                   
                }
            ");

            sheet.Build();

            sheet.TryGetStyle("testStyle", out Style style);

            style.TryGetProperty(PropertyId.PreferredWidth, out StyleProperty2 width);
            style.TryGetProperty(PropertyId.BackgroundColor, out StyleProperty2 color);

            Assert.AreEqual(new UIMeasurement(0.32f, UIMeasurementUnit.Percentage), width.AsUIMeasurement);
            Assert.AreEqual(new Color32(255, 0, 0, 255), color.AsColor);
            Assert.AreEqual(style.GetPropertyCount(), 2);
            Assert.AreEqual(style.GetPropertyCount(StyleState.Normal), 2);

            module.UpdateConditions(new DisplayConfiguration(1000, 1000, 1));

            sheet.Build();
            sheet.TryGetStyle("testStyle", out style);

            Assert.IsFalse(style.TryGetProperty(PropertyId.PreferredWidth, out width));
            style.TryGetProperty(PropertyId.BackgroundColor, out color);
            Assert.AreEqual(new Color32(255, 255, 255, 255), color.AsColor);
        }


        [Test]
        public void ParseLocalConstantWithoutCondition() {
            Module module = new Module<DummyElement>();

            StyleSheet2 sheet = StyleSheetParser.ParseString(module, @"const x = red;");

            sheet.Build();

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

            sheet.Build();

            Assert.AreEqual("red", sheet.GetConstant("x"));

            module.UpdateConditions(new DisplayConfiguration(500f, 300f, 1f));

            sheet.Build();

            Assert.AreEqual("green", sheet.GetConstant("x"));

            module.UpdateConditions(new DisplayConfiguration(0, 0, 1f));

            sheet.Build();

            Assert.AreEqual("orange", sheet.GetConstant("x"));
        }

        [Test]
        public void IgnoreComments() {
            Module module = new Module<DummyElement>();

            module.RegisterDisplayCondition("one", (d) => d.screenWidth > 200 && d.screenWidth < 400);
            module.RegisterDisplayCondition("two", (d) => d.screenWidth >= 400);

            StyleSheet2 sheet = StyleSheetParser.ParseString(module, @"
                const x { // comment
                    #one = red;// comment
                    // #two = green;
                    default = orange; // comment
                } // comment"
            );


            module.UpdateConditions(new DisplayConfiguration(300f, 300f, 1f));

            sheet.Build();

            Assert.AreEqual("red", sheet.GetConstant("x"));

            module.UpdateConditions(new DisplayConfiguration(500f, 300f, 1f));

            sheet.Build();

            Assert.AreEqual("orange", sheet.GetConstant("x"));

            module.UpdateConditions(new DisplayConfiguration(0, 0, 1f));

            sheet.Build();

            Assert.AreEqual("orange", sheet.GetConstant("x"));
        }

    }

}