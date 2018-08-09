using System;

namespace Src {

    public class UnaryPlusBinding : ExpressionBinding {

        public readonly ExpressionBinding binding;

        public UnaryPlusBinding(ExpressionBinding binding) {
            this.binding = binding;
        }

        public override object Evaluate(TemplateContext context) {
            object value = binding.Evaluate(context);
            if (value is int) return +(int) value;
            if (value is float) return +(float) value;
            if (value is double) return +(double) value;
            if (value is short) return +(short) value;
            if (value is ushort) return +(ushort) value;
            if (value is byte) return +(byte) value;
            if (value is sbyte) return +(sbyte) value;
            if (value is long) return +(long) value;
            if (value is ulong) return +(ulong) value;
            throw new Exception("Failed to match numeric type");
        }

    }

}