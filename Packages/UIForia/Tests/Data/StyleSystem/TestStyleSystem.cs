using UIForia.Selectors;
using NUnit.Framework;
using Tests.Mocks;
using UIForia;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UnityEngine;

namespace StyleSystemTest {

    public class TestStyleSystem {

        public class MockRootQuery : SelectorQuery {

            public string attr;

            public MockRootQuery(string attr, AttributeIndex attributeIndex) {
                this.attr = attr;
            }

        }

        // public class MockSelector : Selector {
        //
        //     public string attr;
        //
        //     public MockSelector(string attr) {
        //         this.attr = attr;
        //        // this.rootQuery = new MockRootQuery(attr, null);
        //     }
        //
        // }

        [Template("Data/StyleSystem/StyleTestElementSimple.xml")]
        public class StyleTestElementSimple : UIElement { }

        private static int groupId;

        public static int NextGroupId => ++groupId;

        [Test]
        public void Works() {
            MockApplication app = MockApplication.Setup<StyleTestElementSimple>();

            // StyleGroup group = new StyleGroup(NextGroupId) {
            //     normal = new StylePropertyBlock(new[] {
            //         StyleProperty.BackgroundColor(Color.cyan),
            //         StyleProperty.BorderLeft(4f),
            //     }),
            //     hover = new StylePropertyBlock(new[] {
            //         StyleProperty.BackgroundColor(Color.red),
            //         StyleProperty.BorderLeft(12f)
            //     }),
            //     selectors =  new Selector[] { new MockSelector("one") }
            // };

            /*
             *    style = new Style
             *     [hover] = new StyleStateGroup
             *         selector[]
             *             selector x
             *             selector y
             *                 run commands
             *                 properties
             *         properties
             *         run commands
             * 
             */


            // UIElement root = app.RootElement;
            //
            // root.styleSet2.Initialize(group);
            //
            // root.styleSet2.SetStyleProperty(StyleProperty.Opacity(0.5f));
            //
            // app.styleSystem2.Update();
            //
            // StyleProperty property = root.styleSet2.GetProperty(StylePropertyId.Opacity);
            //
            // Assert.AreEqual(StyleProperty.Opacity(0.5f), property);
        }

    }

}