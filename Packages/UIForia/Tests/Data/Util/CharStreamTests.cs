using NUnit.Framework;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;

namespace Util {

    public class CharStreamTests {

        [Test]
        public unsafe void ParseInt() {
            fixed (char* ptr = "123") {
                CharStream stream = new CharStream(ptr, 0, 3);
                stream.TryParseInt(out int val);
                Assert.AreEqual(123, val);
            }

            fixed (char* ptr = "-123") {
                CharStream stream = new CharStream(ptr, 0, 4);
                stream.TryParseInt(out int val2);
                Assert.AreEqual(-123, val2);
            }

            fixed (char* ptr = "-123f") {
                CharStream stream = new CharStream(ptr, 0, 5);
                stream.TryParseInt(out int val3);
                Assert.AreEqual(-123, val3);
            }

            fixed (char* ptr = "1-3f") {
                CharStream stream = new CharStream(ptr, 0, 4);
                stream.TryParseInt(out int val4);
                Assert.AreEqual(1, val4);
            }

            fixed (char* ptr = "a123f") {
                CharStream stream = new CharStream(ptr, 0, 4);
                Assert.IsFalse(stream.TryParseInt(out int val5));

            }

        }

        [Test]
        public unsafe void ParseFloat() {
            fixed (char* charptr = "123") {
                CharStream stream = new CharStream(charptr, 0, 3);
                stream.TryParseFloat(out float val);
                Assert.AreEqual(123f, val);
            }

            fixed (char* charptr = "123f") {
                CharStream stream = new CharStream(charptr, 0, 4);
                stream.TryParseFloat(out float val);
                Assert.AreEqual(123f, val);
            }

            fixed (char* charptr = "-123f") {
                CharStream stream = new CharStream(charptr, 0, 5);
                stream.TryParseFloat(out float val);
                Assert.AreEqual(-123f, val);
            }

            fixed (char* charptr = "-1235.456f") {
                CharStream stream = new CharStream(charptr, 0, (uint) "-1235.456f".Length);
                stream.TryParseFloat(out float val);
                Assert.AreEqual(-1235.456f, val);
            }

        }

        [Test]
        public unsafe void ParseEnum() {

            fixed (char* charptr = "Horizontal") {
                CharStream stream = new CharStream(charptr, 0, (uint) "Horizontal".Length);
                stream.TryParseEnum<LayoutDirection>(out int d);
                Assert.AreEqual(LayoutDirection.Horizontal, (LayoutDirection) d);
            }

        }

    }

}