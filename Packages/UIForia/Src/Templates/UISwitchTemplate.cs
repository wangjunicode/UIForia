using System;
using System.Collections.Generic;
using System.Linq;

namespace UIForia {

    public class UISwitchTemplate : UITemplate {

        // todo -- bring all attribute constants into one file
        public const string CaseKeyAttribute = "when";
        public const string SwitchValueAttribute = "value";

        public UISwitchTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }

        protected override Type elementType => typeof(UISwitchTemplate);

        public override UIElement CreateScoped(TemplateScope inputScope) {
            throw new NotImplementedException();
        }

        public override bool Compile(ParsedTemplate template) {
            throw new NotImplementedException();
//            id = ParsedTemplate.MakeId();
//            AttributeDefinition valueAttr = GetAttribute(SwitchValueAttribute);
//
//            Expression valueExpression = template.compiler.Compile(valueAttr.value);
//
//            Type yieldedType = valueExpression.YieldedType;
//
////            AssertType(yieldedType, typeof(string), typeof(int), typeof(Enum));
//
//            if (yieldedType == typeof(string)) {
//                CompileStringVersion(template);
//            }
//            else if (yieldedType == typeof(int)) {
//                CompileIntVersion(template, (Expression<int>) valueExpression);
//            }
//            else if (yieldedType.IsEnum) {
//                throw new NotImplementedException();
//            }
//
//            return true;
        }

        private void CompileStringVersion(ParsedTemplate template) { }

        private void CompileIntVersion(ParsedTemplate template, Expression<int> valueExpression) {
            int valueCount = GetValueCount();
            int[] values = new int[valueCount];
            int count = 0;
            for (int i = 0; i < childTemplates.Count; i++) {

                if (childTemplates[i] is UISwitchCaseTemplate) {
                    UISwitchCaseTemplate child = (UISwitchCaseTemplate) childTemplates[i];
                    child.switchId = id;
                    child.index = count;
                    AttributeDefinition attr = child.GetAttribute(CaseKeyAttribute);
                    Expression<int> expression = template.compiler.Compile<int>(attr.value);
                    EnsureConstCompatibleExpression(typeof(int), expression);
                    values[count++] = expression.EvaluateTyped(null);
                }
                else {
//                    ((UISwitchDefaultTemplate) childTemplates[i]).switchId = id;
                }
            }

            bool isDistinct = values.Distinct().Count() == values.Length;
            if (!isDistinct) {
                throw new Exception("All switch case elements need to have compile-time constant unique values");
            }

            AddConditionalBinding(new SwitchBindingInt(id, values, valueExpression));
        }

        private int GetValueCount() {
            int count = 0;
            for (int i = 0; i < childTemplates.Count; i++) {
                if (childTemplates[i] is UISwitchCaseTemplate) {
                    count++;
                }
            }
            return count;
        }

        public static void EnsureConstCompatibleExpression(Type type, Expression expression) {
            if (!expression.IsConstant()) {
                throw new Exception($"Switch case {CaseKeyAttribute} expressions must be constant.");
            }
            if (expression.YieldedType != type) {
                throw new Exception("Unmatched Switch type");
            }
        }

        public static void AssertType(Type checkType, params Type[] types) {
            for (int i = 0; i < types.Length; i++) {
                if (types[i].IsEnum) {
                    if (checkType.IsEnum) {
                        // 
                    }

                }
            }
            if (types.Any((t) => t == checkType)) { }
            else { }

        }

    }

    // todo -- int / enum / string versions

}