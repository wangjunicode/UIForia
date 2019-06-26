using System;
using NUnit.Framework;
using UIForia.Elements;
using UIForia.Layout;

[TestFixture]
public class ResetTests {

    private class ResetThing : UIElement {

        public int autoInt { get; set; }
        private Action callback;
        public event Action evt;
        public readonly int readonlyValue = 12;

        private float x;

        public void SetCallback(Action action) {
            this.callback = action;
        }

        public Action GetCallback() {
            return callback;
        }

        public bool EvtIsNull() {
            return evt == null;
        }

        public event Action propertyEvent {
            add { }
            remove { }
        }

    }

    [Test]
    public void Clears() {
        ResetThing element = new ResetThing();
        element.flags |= UIElementFlags.TextElement;

        element.autoInt = 1414;
        element.SetCallback(() => { });
        element.evt += () => { };

        Assert.NotNull(element.GetCallback());
        Assert.NotNull(element.layoutResult);
        Action<object> clear = UIForia.LinqExpressions.LinqExpressions.CompileClear(typeof(ResetThing));
        clear(element);
        Assert.AreEqual(default(int), element.autoInt);
        Assert.AreEqual(default(Action), element.GetCallback());
        Assert.AreEqual(default(UIElementFlags), element.flags);
        Assert.AreEqual(default(LayoutResult), element.layoutResult);
        Assert.AreEqual(default(int), element.readonlyValue);
        Assert.IsTrue(element.EvtIsNull());
    }

}