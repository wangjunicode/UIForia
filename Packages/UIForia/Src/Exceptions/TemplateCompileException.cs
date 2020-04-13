using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Parsing.Expressions.AstNodes;
using UIForia.Parsing.Style.AstNodes;
using UIForia.UIInput;
using UIForia.Util;


namespace UIForia.Exceptions {

    public class TemplateCompileException : Exception {

        private string fileName = "";
        public string expression = "";

        public TemplateCompileException(string message = null) : base(message) { }


        public TemplateCompileException(StyleASTNode node, string message = null) :
            base($"Compile error for style token at line {node.line}, column {node.column}, node type '{node.type}'\n{message}") { }

        public TemplateCompileException(string fileName, StyleASTNode node, string message = null) :
            base($"Compile error for style token at line {node.line}, column {node.column}, node type '{node.type}'\n{message}") {
            this.fileName = fileName;
        }

        public void SetFileName(string name) {
            this.fileName = "Error in file " + name + ": ";
        }

        public override string Message {
            get {
                string retn = fileName + "\n" + base.Message;

                if (!string.IsNullOrEmpty(expression)) {
                    retn += "\nExpression was: " + expression;
                }

                return retn;
            }
        }

        public void SetExpression(string input) {
            expression = input;
        }

        // todo -- add more debug info to these and make them actually useful. These are basically placeholder but need help to be really useful to people

        public static TemplateCompileException MissingAliasResolver(string aliasName) {
            return new TemplateCompileException($"No alias resolvers were registered that could handle alias {aliasName}");
        }

        public static TemplateCompileException NoStatementsRootBlock() {
            return new TemplateCompileException($"Cannot compile the lambda because there are not statements emitted in the main block of the function");
        }

        public static TemplateCompileException InvalidActionArgumentCount(IList<ParameterExpression> parameters, Type[] genericArguments) {
            return new TemplateCompileException($"Cannot compile the action because the declared parameter count {parameters.Count} is not the same as the required signatures parameter count {genericArguments.Length}");
        }

        public static TemplateCompileException MissingBinaryOperator(OperatorType opType, Type a, Type b) {
            return new TemplateCompileException($"Missing operator: the binary operator {opType} is not defined for the types {a} and {b}");
        }

        public static TemplateCompileException UnresolvedIdentifier(string identifierNodeName) {
            return new TemplateCompileException($"Unable to resolve the variable or parameter {identifierNodeName}");
        }

        public static TemplateCompileException InvalidTargetType(Type expected, Type actual) {
            return new TemplateCompileException($"Expected expression to be compatible with type {expected} but got {actual} which was not convertible");
        }

        public static TemplateCompileException InvalidAccessExpression() {
            return new TemplateCompileException("Expected access expression to have more symbols, the last expression is not a valid terminal");
        }

        public static TemplateCompileException InvalidIndexOrInvokeOperator() {
            return new TemplateCompileException("Index or Invoke operations are not valid on static types or namespaces");
        }

        public static TemplateCompileException UnknownStaticOrConstMember(Type type, string memberName) {
            return new TemplateCompileException($"Unable to find a field or property with the name {memberName} on type {type}");
        }

        public static TemplateCompileException AccessNonReadableStaticOrConstField(Type type, string memberName) {
            return new TemplateCompileException($"Unable to read static or const field {memberName} on type {type} because it is not marked as public");
        }

        public static TemplateCompileException AccessNonReadableStaticProperty(Type type, string memberName) {
            return new TemplateCompileException($"Unable to read static property {memberName} on type {type} because has no read accessor");
        }

        public static TemplateCompileException AccessNonReadableProperty(Type type, PropertyInfo propertyInfo) {
            return new TemplateCompileException($"Unable to read {(propertyInfo.GetMethod.IsStatic ? "static" : "instance")} property {propertyInfo.Name} on type {type} because has no public read accessor");
        }

        public static TemplateCompileException AccessNonPublicStaticProperty(Type type, string memberName) {
            return new TemplateCompileException($"Unable to read static property {memberName} on type {type} because it's read accessor is not public");
        }

        public static TemplateCompileException AccessNonReadableField(Type type, FieldInfo fieldInfo) {
            if (fieldInfo.IsStatic) {
                return new TemplateCompileException($"Unable to read static field {fieldInfo.Name} on type {type} because it is not marked as public");
            }
            else {
                return new TemplateCompileException($"Unable to read instance field {fieldInfo.Name} on type {type} because it is not marked as public");
            }
        }

        public static TemplateCompileException InvalidLambdaArgument() {
            return new TemplateCompileException($"Type mismatch with Lambda Argument");
        }

        public static TemplateCompileException NoImplicitConversion(Type a, Type b) {
            return new TemplateCompileException($"Implicit conversion exists between types {a} and {b}. Please use casing to fix this");
        }

        public static TemplateCompileException NoSuchProperty(Type type, string fieldOrPropertyName) {
            return new TemplateCompileException($"Type {type} has no field or property {fieldOrPropertyName}");
        }

        public static TemplateCompileException UnresolvedType(TypeLookup typeLookup, IReadOnlyList<string> searchedNamespaces = null) {
            string retn = string.Empty;
            if (searchedNamespaces != null) {
                retn += " searched in the following namespaces: ";
                for (int i = 0; i < searchedNamespaces.Count - 1; i++) {
                    retn += searchedNamespaces[i] + ",";
                }

                retn += searchedNamespaces[searchedNamespaces.Count - 1];
            }

            return new TemplateCompileException($"Unable to resolve type {typeLookup}, are you missing a namespace?{retn}");
        }

        public static TemplateCompileException InvalidNamespaceOperation(string namespaceName, Type type) {
            return new TemplateCompileException($"Resolved namespace {namespaceName} but {type} is not a valid next token");
        }

        public static TemplateCompileException UnknownEnumValue(Type type, string value) {
            return new TemplateCompileException($"Unable to enum value {value} on type {type}");
        }

        public static TemplateCompileException UnresolvedStaticMethod(Type type, string value) {
            return new TemplateCompileException($"Unable to find a public method on {type} with the name {value}");
        }

        public static TemplateCompileException NonPublicType(Type type) {
            return new TemplateCompileException($"The type {type} is not public and cannot be used in expressions.");
        }

        public static TemplateCompileException SignatureNotDefined() {
            return new TemplateCompileException($"The signature must be set before calling builder methods on {nameof(LinqCompiler)}");
        }

        public static TemplateCompileException UnresolvedMethodOverload(Type type, string methodName, Type[] inputTypeArguments) {
            string argumentTypeString = "";
            for (int i = 0; i < inputTypeArguments.Length; i++) {
                argumentTypeString += inputTypeArguments[i].FullName;
                if (i != inputTypeArguments.Length - 1) {
                    argumentTypeString += ", ";
                }
            }

            return new TemplateCompileException($"Unable to find a public method '{methodName}' on type {type} with a signature matching ({argumentTypeString})");
        }

        public static TemplateCompileException UnresolvedInstanceMethodOverload(Type type, string methodName, Type[] inputTypeArguments) {
            string argumentTypeString = "";
            for (int i = 0; i < inputTypeArguments.Length; i++) {
                argumentTypeString += inputTypeArguments[i].FullName;
                if (i != inputTypeArguments.Length - 1) {
                    argumentTypeString += ", ";
                }
            }

            return new TemplateCompileException($"Unable to find a public instance method '{methodName}' on type {type} with a signature matching ({argumentTypeString})");
        }

        public static TemplateCompileException UnresolvedConstructor(Type type, Type[] arguments) {
            string BuildArgumentList() {
                if (arguments == null || arguments.Length == 0) {
                    return "no arguments";
                }

                string retn = "arguments (";
                for (int i = 0; i < arguments.Length; i++) {
                    retn += arguments[i];
                    if (i != arguments.Length - 1) {
                        retn += ", ";
                    }
                }

                retn += ")";
                return retn;
            }

            return new TemplateCompileException($"Unable to find a suitable constructor on type {type} that accepts {BuildArgumentList()}");
        }

        public static TemplateCompileException UnresolvedMethod(Type type, string methodName) {
            return new TemplateCompileException($"Unable to find a method called `{methodName}` on type {type}");
        }

        public static TemplateCompileException UnresolvedFieldOrProperty(Type type, string fieldOrPropertyName) {
            return new TemplateCompileException($"Unable to find a field or property called `{fieldOrPropertyName}` on type {type}");
        }

        public static TemplateCompileException UnresolvedGenericElement(ProcessedType processedType, TemplateNodeDebugData data) {
            return new TemplateCompileException($"Unable to resolve the concrete type for " + processedType.rawType + $"\n\nFailed parsing {data.tagName} at {data.fileName}:{data.lineInfo}\n" +
                                        $"You can try to fix this by providing the type explicitly. (add an attribute generic:type=\"your,types,here\"");
        }

        public static TemplateCompileException GenericElementMissingResolver(ProcessedType processedType) {
            return new TemplateCompileException($"{processedType.rawType} requires a class attribute of type {nameof(GenericElementTypeResolvedByAttribute)} in order to be used in a template");
        }

        public static TemplateCompileException UnresolvableGenericElement(ProcessedType processedType, string value) {
            return new TemplateCompileException($"{processedType.rawType} requires a class attribute of type {nameof(GenericElementTypeResolvedByAttribute)} in order to be used in a template and a value that is not null or default also declared in the template");
        }

        public static TemplateCompileException UnmatchedSlot(string slotName, string path) {
            return new TemplateCompileException($"Unable to find a matching slot with the name {slotName} in template {path}");
        }

        public static TemplateCompileException InvalidInputHandlerLambda(string value, int signatureSize) {
            return new TemplateCompileException($"Input handler lambda is invalid. Expected 0 or 1 arguments for handler {value} but found {signatureSize}");
        }

        public static TemplateCompileException UnknownStyleState(in AttributeNodeDebugData data, string s) {
            return new TemplateCompileException($"file: {data.fileName}{data.lineInfo}\nUnable to handle style state declaration '{s}' Expected 'active', 'focus', or 'hover'");
        }

        public static TemplateCompileException UnresolvedRepeatType(string provided, params string[] others) {
            return new TemplateCompileException("Unable to determine repeat type: " + provided + " was provided but is not legal in combination with " + StringUtil.ListToString(others));
        }

        public static TemplateCompileException UnresolvedPropertyChangeHandler(string methodInfoName, Type propertyType) {
            return new TemplateCompileException($"Unable to use {methodInfoName} as a property change handler. Please be sure the signature either accepts no arguments or only 1 argument with a type matching the type of the property it is bound to: {propertyType}");
        }

        public static TemplateCompileException NonPublicPropertyChangeHandler(string methodInfoName, Type propertyType) {
            return new TemplateCompileException($"Unable to use {methodInfoName} as a property change handler because it is not public");
        }

        public static TemplateCompileException UnknownAlias(string aliasName) {
            return new TemplateCompileException($"Unknown alias `{aliasName}`");
        }

        public static TemplateCompileException DuplicateResolvedGenericArgument(string tagName, string argName, Type original, Type duplicate) {
            return new TemplateCompileException($"When attempting to resolve generic element tag {tagName}, {argName} was resolved first to {original} and later to {duplicate}. Ensure multiple usages of {argName} resolve to the same type.");
        }

        public static TemplateCompileException MultipleConditionalBindings(TemplateNodeDebugData data) {
            return new TemplateCompileException($"Encountered multiple conditional bindings (if) on element {data.tagName} in file: {data.fileName} {data.lineInfo}. Only one conditional binding is permitted per element");
        }

        public static TemplateCompileException UnknownStyleMapping() {
            return new TemplateCompileException($"Unknown style mapping");
        }

        public static TemplateCompileException InvalidInputAnnotation(string methodName, Type type, Type annotationType, Type expectedParameterType, Type actualParameterType) {
            return new TemplateCompileException($"Method {methodName} in type {type.Name} is annotated with {annotationType.Name} which expects 0 or 1 arguments of type {expectedParameterType} but was declared with {actualParameterType} which is invalid");
        }

        public static TemplateCompileException TooManyInputAnnotationArguments(string methodName, Type type, Type annotationType, Type expectedParameterType, int parameterCount) {
            return new TemplateCompileException($"Method {methodName} in type {type.Name} is annotated with {annotationType.Name} which expects 0 or 1 arguments of type {expectedParameterType} but was declared with {parameterCount} arguments which is invalid");
        }

        public static TemplateCompileException InvalidDragCreatorAnnotationReturnType(string methodName, Type type, Type returnType) {
            return new TemplateCompileException($"Method {methodName} in type {type.Name} is annotated with {nameof(OnDragCreateAttribute)} which expects a return type assignable to {nameof(DragEvent)}. The method returns {returnType} which is invalid");
        }

     

    }

}