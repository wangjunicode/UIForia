//using System;
//using Src;
//using NUnit.Framework;
//
//namespace Tests {
//
//    [TestFixture]
//    public class PropTests {
//
//        [Test]
//        public void ParseConstantPropValue_String() {
//            UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate<Spec.Props.Test1>();
//            UIElement root = parsedTemplate.CreateElement();
//            Assert.AreEqual(2, root.children.Length);
//            Assert.IsInstanceOf<Spec.Props.Test1Thing>(root.children[0]);
//            Assert.AreEqual("hello", ((Spec.Props.Test1Thing) root.children[0]).stringValue);
//        }
//
//        [Test]
//        public void ParseConstantPropValue_Int32() {
//            UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate<Spec.Props.Test1>();
//            UIElement root = parsedTemplate.CreateElement();
//            Assert.AreEqual(2, root.children.Length);
//            Assert.IsInstanceOf<Spec.Props.Test1Thing>(root.children[0]);
//            Assert.IsInstanceOf<Spec.Props.Test1Thing>(root.children[1]);
//            Assert.AreEqual(8494, ((Spec.Props.Test1Thing) root.children[0]).intValue);
//            Assert.AreEqual(-8494, ((Spec.Props.Test1Thing) root.children[1]).intValue);
//        }
//
//        [Test]
//        public void ParseConstantPropValue_Float() {
//            UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate<Spec.Props.Test1>();
//            UIElement root = parsedTemplate.CreateElement();
//            Assert.AreEqual(2, root.children.Length);
//            Assert.IsInstanceOf<Spec.Props.Test1Thing>(root.children[0]);
//            Assert.IsInstanceOf<Spec.Props.Test1Thing>(root.children[1]);
//            Assert.AreEqual(8494.5f, ((Spec.Props.Test1Thing) root.children[0]).floatValue);
//            Assert.AreEqual(-8494.5f, ((Spec.Props.Test1Thing) root.children[1]).floatValue);
//        }
//
//        [Test]
//        public void ParseConstantPropValue_Bool() {
//            UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate<Spec.Props.Test1>();
//            UIElement root = parsedTemplate.CreateElement();
//            Assert.AreEqual(2, root.children.Length);
//            Assert.IsInstanceOf<Spec.Props.Test1Thing>(root.children[0]);
//            Assert.IsInstanceOf<Spec.Props.Test1Thing>(root.children[1]);
//            Assert.AreEqual(true, ((Spec.Props.Test1Thing) root.children[0]).boolValue);
//            Assert.AreEqual(false, ((Spec.Props.Test1Thing) root.children[1]).boolValue);
//        }
//
//        [Test]
//        public void ParseConstantValueTypeMismatch() {
//            Assert.Throws<FormatException>(() => {
//                TemplateParser.GetParsedTemplate<Spec.Props.Test2>();
//            });
//        }
//
//        [Test]
//        public void ReceiveValueFromObservedPropertyOnInitalize() {
//            UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate<Spec.Props.Test3>();
//            UIElement root = parsedTemplate.CreateElement();
//            Assert.AreEqual(((Spec.Props.Test1Thing) root.children[0]).intValue, ((Spec.Props.Test3) root).value.Value);
//        }
//
//        [Test]
//        public void ChangeBoundValueShouldUpdateProp() {
//            UIElementTemplate parsedTemplate = TemplateParser.GetParsedTemplate<Spec.Props.Test3>();
//            UIElement root = parsedTemplate.CreateElement();
//            Spec.Props.Test3 rootThing = (Spec.Props.Test3) root;
//            Assert.AreEqual(((Spec.Props.Test1Thing) root.children[0]).intValue, rootThing.value.Value);
//            rootThing.value.Value = 124;
//            rootThing.providedContext.FlushChanges();
//            Assert.AreEqual(((Spec.Props.Test1Thing) root.children[0]).intValue, rootThing.value.Value);
//            
//        }
//        // constant value
//        // observed
//        // type mismatch
//        // unknown binding
//        // unknown prop
//
//    }
//
//}