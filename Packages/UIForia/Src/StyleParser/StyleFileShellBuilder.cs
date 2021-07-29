// using System;
// using UIForia.Compilers;
// using UIForia.Util;
//
// namespace UIForia.Style {
//
//     public class StyleFileShellBuilder {
//
//         public StructList<StyleNode> styleNodes;
//         public StructList<char> charBuffer;
//
//         public StyleFileShellBuilder() {
//             charBuffer = new StructList<char>(4096);
//             styleNodes = new StructList<StyleNode>();
//         }
//
//         public StyleASTBuilder AddStyleNode(CharSpan styleName) {
//             for (int i = 0; i < styleNodes.size; i++) {
//                 if (styleNodes.array[i].name == styleName) {
//                     // todo -- log error but allow
//                 }
//             }
//
//             styleNodes.Add(new StyleNode() {
//                 name = styleName
//             });
//
//             return new StyleASTBuilder(styleNodes.size - 1, this);
//         }
//
//         public bool Build() {
//             // todo -- validate general structure here
//             // ie no condition contains a style
//             // no duplicate style names
//             // no unknown references (only checking @theme, not @theme.whatever) -- let compiler do this since i dont know what the implicit references are at parse time
//
//             // need a tree of nodes to transform into bucket hierarchies
//
//             return true;
//         }
//
//         public struct StyleASTBuilder {
//
//             private StyleFileShellBuilder builder;
//             internal readonly int index;
//
//             internal StyleASTBuilder(int index, StyleFileShellBuilder builder) {
//                 this.index = index;
//                 this.builder = builder;
//             }
//
//             public void AddConditionNode(CharSpan condition) { }
//
//             public void AddStyleProperty(PropertyKeyInfoFlag flags, CharSpan key, CharSpan value) {
//                 builder.AddStyleProperty(index, flags, key, value);
//             }
//
//             public StyleASTBuilder AddAttributeBlock(CharSpan key, CharSpan value) {
//                 // builder.AddAttributeBlock();
//                 return default;
//             }
//
//             public StyleASTBuilder AddFirstChildBlock() {
//                 throw new NotImplementedException();
//             }
//
//             public StyleASTBuilder AddLastChildBlock() {
//                 throw new NotImplementedException();
//             }
//
//             public StyleASTBuilder AddFirstWithTagBlock(CharSpan tag) {
//                 throw new NotImplementedException();
//             }
//
//             public StyleASTBuilder AddLastWithTagBlock(CharSpan tag) {
//                 throw new NotImplementedException();
//             }
//
//             public StyleASTBuilder AddNthWithTagBlock(CharSpan tag, CharSpan expression) {
//                 throw new NotImplementedException();
//             }
//
//             public StyleASTBuilder AddNthChildBlock(CharSpan expression) {
//                 throw new NotImplementedException();
//             }
//
//         }
//
//         private void AddStyleProperty(int index, PropertyKeyInfoFlag flags, CharSpan key, CharSpan value) { }
//
//         public void Clear() { }
//
//     }
//
// }