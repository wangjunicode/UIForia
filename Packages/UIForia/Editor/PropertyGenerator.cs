using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Systems;
using UnityEngine;

namespace UIForia.Editor {

    public class PropertyGenerator<T> : PropertyGenerator {

        public PropertyGenerator(StylePropertyId propertyId, T defaultValue, InheritanceType inheritanceType = InheritanceType.NotInherited, string defaultValueOverride = null)
            : base(propertyId, typeof(T), defaultValue, inheritanceType, defaultValueOverride) { }

    }

    public class PropertyGenerator {

        public StylePropertyId propertyId;
        public readonly Type type;
        public readonly InheritanceType inheritanceType;
        public readonly string propertyIdName;
        private readonly object defaultValue;
        private readonly string defaultValueOverride;

        protected PropertyGenerator(StylePropertyId propertyId, Type type, object defaultValue, InheritanceType inheritanceType, string defaultValueOverride) {
            this.propertyId = propertyId;
            this.propertyIdName = StyleUtil.GetPropertyName(propertyId);
            this.type = type;
            this.inheritanceType = inheritanceType;
            this.defaultValue = defaultValue;
            this.defaultValueOverride = defaultValueOverride;
        }

        public string AsStyleProperty {
            get {
                string preamble = $"new StyleProperty({nameof(StylePropertyId)}.{propertyIdName}, ";
                if (typeof(int) == type
                    || typeof(float) == type
                    || typeof(UIMeasurement) == type
                    || typeof(UIFixedLength) == type
                    || typeof(TransformOffset) == type
                    || typeof(GridTrackSize) == type
                    || typeof(Color) == type
                ) {
                    return preamble + $"{GetDefaultValue()})";
                }

                if (type.IsEnum) {
                    return preamble + $"(int){GetDefaultValue()})";
                }

                return preamble + $"{GetDefaultValue()})";
            }
        }

        public string StyleSetGetComputed {
            get {
                if (type.IsEnum) {
                    return $"new StyleProperty(StylePropertyId.{propertyIdName}, (int){propertyIdName})";
                }

                if (type == typeof(int)) {
                    return $"new StyleProperty(StylePropertyId.{propertyIdName}, {propertyIdName})";
                }

                if (type == typeof(float)) {
                    return $"new StyleProperty(StylePropertyId.{propertyIdName}, {propertyIdName})";
                }

                if (typeof(UIMeasurement) == type
                    || typeof(UIFixedLength) == type
                    || typeof(TransformOffset) == type
                    || typeof(GridTrackSize) == type
                    || typeof(Color) == type
                ) {
                    return $"new StyleProperty(StylePropertyId.{propertyIdName}, {propertyIdName})";
                }

                return $"new StyleProperty(StylePropertyId.{propertyIdName}, {propertyIdName})";
            }
        }
        
        public string StylePropertyConstructor {
            get {
                if (type.IsEnum) {
                    return $"new StyleProperty(StylePropertyId.{propertyIdName}, (int)value)";
                }

                if (type == typeof(int)) {
                    return $"new StyleProperty(StylePropertyId.{propertyIdName}, value)";
                }

                if (type == typeof(float)) {
                    return $"new StyleProperty(StylePropertyId.{propertyIdName}, value)";
                }

                if (type == typeof(Alignment)) {
                    return $"new StyleProperty(StylePropertyId.{propertyIdName}, value)";
                }
                
                if (typeof(UIMeasurement) == type
                    || typeof(UIFixedLength) == type
                    || typeof(GridTrackSize) == type
                    || typeof(TransformOffset) == type
                    || typeof(Color) == type
                ) {
                    return $"new StyleProperty(StylePropertyId.{propertyIdName}, value)";
                }

                return $"new StyleProperty(StylePropertyId.{propertyIdName}, value)";
            }
        }

        public bool IsInherited => inheritanceType == InheritanceType.Inherited;

        public string GetIsUnset() {
            if (type.IsEnum) {
                return $"{nameof(StyleProperty.int0)} == 0 || IntUtil.UnsetValue == {nameof(StyleProperty.int0)}";
            }

            if (type == typeof(float)) {
                return $"!FloatUtil.IsDefined({nameof(StyleProperty.float1)})";
            }

            if (type == typeof(int)) {
                return $"!IntUtil.IsDefined({nameof(StyleProperty.int0)})";
            }

            if (type == typeof(UIMeasurement)) {
                return $"!FloatUtil.IsDefined({nameof(StyleProperty.float1)}) || {nameof(StyleProperty.int1)} == 0";
            }

            if (type == typeof(TransformOffset)) {
                return $"!FloatUtil.IsDefined({nameof(StyleProperty.float1)}) || {nameof(StyleProperty.int1)} == 0";
            }
            
            if (type == typeof(UIFixedLength)) {
                return $"!FloatUtil.IsDefined({nameof(StyleProperty.float1)}) || {nameof(StyleProperty.int1)} == 0";
            }

            if (type == typeof(GridTrackSize)) {
                return $"!FloatUtil.IsDefined({nameof(StyleProperty.float1)}) || {nameof(StyleProperty.int1)} == 0";
            }

            if (type == typeof(Color)) {
                return $"{nameof(StyleProperty.int1)} == 0";
            }

            return $"{nameof(StyleProperty.objectField)} == null";
        }

        public string GetTypeName() {
            if (type == typeof(IReadOnlyList<GridTrackSize>)) {
                return "IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize>";
            }

            if (type == typeof(float)) return "float";
            if (type == typeof(int)) return "int";
            if (type == typeof(string)) return "string";

            return type.Name;
        }

        public string GetFullTypeName() {
            if (type == typeof(IReadOnlyList<GridTrackSize>)) {
                return "System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize>";
            }

            if (type == typeof(float)) return "float";
            if (type == typeof(int)) return "int";
            if (type == typeof(string)) return "string";

            return type.FullName;
        }

        public string GetDefaultValue() {
            if (defaultValueOverride != null) {
                return defaultValueOverride;
            }

            if (type.IsEnum) {
                return $"{type.FullName}.{Enum.GetName(type, defaultValue)}";
            }

            if (defaultValue is UIMeasurement) {
                UIMeasurement measurement = (UIMeasurement) defaultValue;
                return $"new {nameof(UIMeasurement)}({measurement.value.ToString(CultureInfo.InvariantCulture)}f, {nameof(UIMeasurementUnit)}.{Enum.GetName(typeof(UIMeasurementUnit), measurement.unit)})";
            }
            
            if (defaultValue is TransformOffset) {
                TransformOffset measurement = (TransformOffset) defaultValue;
                return $"new {nameof(TransformOffset)}({measurement.value.ToString(CultureInfo.InvariantCulture)}f, {nameof(TransformUnit)}.{Enum.GetName(typeof(TransformUnit), measurement.unit)})";
            }

            if (defaultValue is UIFixedLength) {
                UIFixedLength length = (UIFixedLength) defaultValue;
                string v = Enum.GetName(typeof(UIFixedUnit), length.unit);
                return $"new {nameof(UIFixedLength)}({length.value.ToString(CultureInfo.InvariantCulture)}f, {nameof(UIFixedUnit)}.{v})";
            }

            if (defaultValue is GridTrackSize) {
                GridTrackSize size = (GridTrackSize) defaultValue;
                return $"new {nameof(GridTrackSize)}({size.minValue.ToString(CultureInfo.InvariantCulture)}f, {nameof(GridTemplateUnit)}.{Enum.GetName(typeof(GridTemplateUnit), size.minUnit)})";
            }

            if (defaultValue is Color) {
                Color c = (Color) defaultValue;
                return $"new Color({c.r.ToString(CultureInfo.InvariantCulture)}f, {c.g.ToString(CultureInfo.InvariantCulture)}f, {c.b.ToString(CultureInfo.InvariantCulture)}f, {c.a.ToString(CultureInfo.InvariantCulture)}f)";
            }

            if (defaultValue is Alignment alignment) {
                return $"new Alignment({alignment.value.ToString(CultureInfo.InvariantCulture)}f, {alignment.pivot.ToString(CultureInfo.InvariantCulture)}f, {Enum.GetName(typeof(UIMeasurementUnit), alignment.target)})";
            }

            if (defaultValue is float) {
                return defaultValue.ToString() + "f";
            }

            if (defaultValue is string) {
                return '"' + defaultValue.ToString() + '"';
            }
            
            if (defaultValue == null) return "null";

            return defaultValue.ToString();
        }

        public string GetCastAccessor() {
            if (typeof(IReadOnlyList<GridTrackSize>) == type) {
                return "GridTemplate";
            }

            if (typeof(TMP_FontAsset) == type) {
                return "Font";
            }

            string n = GetTypeName();
            return n.First().ToString().ToUpper() + n.Substring(1);
        }

        public string GetStyleSetSetter() {
            return $"SetProperty({StylePropertyConstructor}, state)";
        }

        public string GetStyleSetGetter() {
            return $"GetPropertyValueInState(StylePropertyId.{propertyIdName}, state).As{GetCastAccessor()}";
        }

        public string GetPrintableTypeName() {
            if (type == typeof(IReadOnlyList<GridTrackSize>)) {
                return "GridTrackTemplate";
            }

            if (type == typeof(float)) return "float";
            if (type == typeof(int)) return "int";
            if (type == typeof(string)) return "string";
            return type.Name;
        }

        public string GetAliasSources() {
            if (type.IsEnum) {
                return $"s_EnumSource_{GetPrintableTypeName()}";
            }

            if (type == typeof(int)) {
                return "null";
            }

            if (type == typeof(float)) {
                return "null";
            }

            if (type == typeof(TMP_FontAsset)) {
                return "fontUrlSource";
            }

            if (type == typeof(Texture2D)) {
                return "textureUrlSource";
            }

            if (type == typeof(UIMeasurement)) {
                return "measurementSources";
            }

            if (type == typeof(UIFixedLength)) {
                return "fixedSources";
            }

            if (type == typeof(GridTrackSize)) {
                return "null";
            }

            if (type == typeof(Color)) {
                return "colorSources";
            }

            return "null";
        }

        public string StylePropertyConstructorParameterized(string paramName, string valueName = "value") {
            if (type.IsEnum) {
                return $"new StyleProperty({paramName}, (int){valueName})";
            }

            if (type == typeof(int)) {
                return $"new StyleProperty({paramName}, {valueName})";
            }

            if (type == typeof(float)) {
                return $"new StyleProperty({paramName}, {valueName})";
            }

            if (typeof(UIMeasurement) == type
                || typeof(UIFixedLength) == type
                || typeof(GridTrackSize) == type
                || typeof(TransformOffset) == type
                || typeof(Color) == type
            ) {
                return $"new StyleProperty({paramName}, {valueName})";
            }

            return $"new StyleProperty({paramName}, {valueName})";
        }

    }

}