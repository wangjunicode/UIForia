using System;
using System.Text;
using UIForia.Util;

namespace UIForia.Compilers {

    public static class TypeNameGenerator {

        public static string GetTypeName(Type type) {
            StringBuilder builder = new StringBuilder();
            GetTypeName(type, builder);
            return builder.ToString();
        }

        public static void GetTypeName(Type type, StringBuilder builder) {
            if (type.IsArray) {
                VisitArrayType(type, builder);
                return;
            }

            if (type.IsGenericParameter) {
                builder.Append(GetPrintableTypeName(type));
                return;
            }

            if (type.IsGenericType && type.IsGenericTypeDefinition) {
                VisitGenericTypeDefinition(type, builder);
                return;
            }

            if (type.IsGenericType && !type.IsGenericTypeDefinition) {
                VisitGenericTypeInstance(type, builder);
                return;
            }

            builder.Append(GetSimpleTypeName(type));
        }

         
        private static string CleanGenericName(Type type) {
            string name = GetPrintableTypeName(type);
            int position = name.LastIndexOf("`");
            if (position == -1) {
                return name;
            }

            return name.Substring(0, position);
        }

        private static string GetSimpleTypeName(Type type) {
            if (type == typeof(void))
                return "void";
            if (type == typeof(object))
                return "object";

            if (type.IsEnum) {
                return GetPrintableTypeName(type);
            }

            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
                    return "bool";
                case TypeCode.Byte:
                    return "byte";
                case TypeCode.Char:
                    return "char";
                case TypeCode.Decimal:
                    return "decimal";
                case TypeCode.Double:
                    return "double";
                case TypeCode.Int16:
                    return "short";
                case TypeCode.Int32:
                    return "int";
                case TypeCode.Int64:
                    return "long";
                case TypeCode.SByte:
                    return "sbyte";
                case TypeCode.Single:
                    return "float";
                case TypeCode.String:
                    return "string";
                case TypeCode.UInt16:
                    return "ushort";
                case TypeCode.UInt32:
                    return "uint";
                case TypeCode.UInt64:
                    return "ulong";
                default:
                    return GetPrintableTypeName(type);
            }
        }

        private static string GetPrintableTypeName(Type type) {
            string typeName = type.FullName;

            if (typeName.Contains("+")) {
                return typeName.Replace("+", ".");
            }

            return typeName;
        }

       
        private static void VisitGenericTypeInstance(Type type, StringBuilder builder) {
            Type[] genericArguments = type.GetGenericArguments();
            int argIndx = 0;

            // namespace.basetype
            // for each type in chain that has generic arguments
            // replace `{arg count} with < ,? > until no more args
            // UIForia.Test.NamespaceTest.SomeNamespace.NamespaceTestClass+SubType1`1+NestedSubType1`1[System.Int32,System.Int32]

            if (type.IsNullableType()) {
                GetTypeName(type.GetGenericArguments()[0], builder);
                builder.Append("?");
                return;
            }

            string typeName = type.ToString();

            for (int i = 0; i < typeName.Length; i++) {
                if (typeName[i] == '`') {
                    i++;
                    int count = int.Parse(typeName[i].ToString());
                    builder.Append("<");
                    for (int c = 0; c < count; c++) {
                        GetTypeName(genericArguments[argIndx++], builder);

                        if (c != count - 1) {
                            builder.Append(", ");
                        }
                    }

                    builder.Append(">");
                }
                else {
                    if (typeName[i] == '[') {
                        return;
                    }

                    if (typeName[i] == '+') {
                        builder.Append(".");
                    }
                    else {
                        builder.Append(typeName[i].ToString());
                    }
                }
            }
        }

        private static void VisitArrayType(Type type, StringBuilder builder) {
            GetTypeName(type.GetElementType(), builder);
            builder.Append("[");
            for (int i = 1; i < type.GetArrayRank(); i++) {
                builder.Append(",");
            }

            builder.Append("]");
        }

        private static void VisitGenericTypeDefinition(Type type, StringBuilder builder) {
            builder.Append(CleanGenericName(type));
            builder.Append("<");
            var arity = type.GetGenericArguments().Length;
            for (int i = 1; i < arity; i++) {
                builder.Append(",");
            }

            builder.Append(">");
        }

    }

}