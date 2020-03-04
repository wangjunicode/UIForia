using NUnit.Framework;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;

namespace Util {

    public class CharStreamTests {

        [Test]
        public void ParseInt() {
            CharStream stream = new CharStream(new[] {'1', '2', '3'});

            stream.TryParseInt(out int val);

            Assert.AreEqual(123, val);

            stream = new CharStream(new[] {'-', '1', '2', '3'});

            stream.TryParseInt(out int val2);

            Assert.AreEqual(-123, val2);

            stream = new CharStream(new[] {'-', '1', '2', '3', 'f'});

            stream.TryParseInt(out int val3);

            Assert.AreEqual(-123, val3);

            stream = new CharStream(new[] {'1', '-', '3', 'f'});

            stream.TryParseInt(out int val4);

            Assert.AreEqual(1, val4);

            stream = new CharStream(new[] {'a', '1', '3', 'f'});

            Assert.IsFalse(stream.TryParseInt(out int val5));

        }

        [Test]
        public void ParseFloat() {
            CharStream stream = new CharStream(new[] {'1', '2', '3'});

            stream.TryParseFloat(out float val);

            Assert.AreEqual(123f, val);

            stream = new CharStream(new[] {'1', '2', '3', 'f'});

            stream.TryParseFloat(out val);

            Assert.AreEqual(123f, val);

            stream = new CharStream(new[] {'-', '1', '2', '3', 'f'});

            stream.TryParseFloat(out val);

            Assert.AreEqual(-123f, val);

            stream = new CharStream("-1235.456f".ToCharArray());

            stream.TryParseFloat(out val);

            Assert.AreEqual(-1235.456f, val);

        }

        [Test]
        public void ParseEnum() {
            CharStream stream = new CharStream("Horizontal".ToCharArray());

            stream.TryParseEnum<LayoutDirection>(out int d);
            
            Assert.AreEqual(LayoutDirection.Horizontal, (LayoutDirection)d);

        }

    }

}