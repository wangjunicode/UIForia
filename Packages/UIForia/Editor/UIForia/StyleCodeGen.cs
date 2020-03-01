using System;
using System.Text;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Editor {

    public struct StylePropertyType {

        public string name;
        public Type type;
        public StyleFlags flags;
        public object parser;

        public StylePropertyType(string name, Type type, StyleFlags flags = 0, object parser = null) {
            this.name = name;
            this.type = type;
            this.flags = flags;
            this.parser = parser;
        }

    }

    [Flags]
    public enum StyleFlags {

        Inherited = 1 << 0,
        Animated = 1 << 1

    }

    public class PropertyCoercer<T> {

        public Func<T, StyleProperty2> convertIn => default;

        public Func<StyleProperty2, T> convertOut => default;

        public Func<string, StyleProperty2> parse => default;

    }

  

   
    public class StyleCodeGen {

        public StylePropertyType[] stylePropertyTypes = new[] {

            new StylePropertyType("Opacity", typeof(float), StyleFlags.Inherited | StyleFlags.Animated),
            new StylePropertyType("OverflowX", typeof(Overflow)),
            new StylePropertyType("OverflowY", typeof(Overflow)),
            new StylePropertyType("ClipBounds", typeof(ClipBounds)),

        };

       

        public void Generate() {

            // todo take settings to search for user defined properties
            StringBuilder builder = new StringBuilder(4096);

            // generate an enum 

            // generate static StyleProperty function for setting values

            // generate list of string names somewhere

            // maybe generate map name to type

            builder.Append("public enum StylePropertyId2 {\n)");

            // struct StylePropertyId2 { 
            //     [FieldOffset(0)] public ushort id; // id could just be the index
            //     [FieldOffset(2)] public byte index;
            //     [FieldOffset(3)] public byte flags;
            for (int i = 0; i < stylePropertyTypes.Length; i++) { }

            builder.Append("}\n");

        }

    }

}