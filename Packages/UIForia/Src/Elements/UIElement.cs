using System;
using System.Diagnostics;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Style;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia {

    [DebuggerDisplay("{ToString()}")]
    public abstract class UIElement {

        public ElementId elementId { get; internal set; }

        public UIApplication application { get; internal set; }

        protected internal UIElement() { }

        public StyleList styleList => new StyleList(elementId, application);

        public StylePropertyList style => new StylePropertyList(elementId, application);

        public int hierarchyDepth => application.depthTable[elementId.index];

        public UIView View => application.GetView(application.elementIdToViewId[elementId.index]);

        public bool isAlive {
            get => application.runtimeInfoTable[elementId.id & ElementId.k_IndexMask].generation == elementId.generation;
        }

        public bool isEnabled {
            get => (application.runtimeInfoTable[elementId.id & ElementId.k_IndexMask].flags & UIElementFlags.Enabled) != 0;
        }

        public bool isDestroyed {
            get => application.runtimeInfoTable[elementId.id & ElementId.k_IndexMask].generation != elementId.generation;
        }

        public bool isDisabled {
            get => (application.runtimeInfoTable[elementId.id & ElementId.k_IndexMask].flags & UIElementFlags.Enabled) == 0;
        }

        public virtual void OnCreate() { }

        public virtual void OnUpdate() { }

        public virtual void OnPostUpdate() { }

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }

        public virtual void OnDestroy() { }

        public override string ToString() {
            return "<" + GetDisplayName() + " " + elementId.index + ">";
        }

        public virtual string GetDisplayName() {
            return GetType().Name;
        }

        public void SetAttribute(string name, string value) {
            application.SetAttribute(elementId, name, value);
        }

        public bool TryGetAttribute(string key, out string value) {
            value = application.GetAttribute(elementId, key);
            return value != null;
        }

        public string GetAttribute(string key) {
            return application.GetAttribute(elementId, key);
        }

        public bool HasAttribute(string name) {
            return GetAttribute(name) != null;
        }

        public void EnqueueAnimation(AnimationId animationId, AnimationOptions? options = default) {
            //     application.EnqueueAnimation(elementId, animationId, options);
        }

        public virtual void AnimationTrigger() { }

        public void SendAnimationEvent(string eventName) {
            // forEach (runningAnim in runningAnimations) { runningAnim.TriggerEvent(eventName) }
        }

        public Size GetLayoutSize() => TryGetLayoutIndex(out int layoutIndex) ? application.layoutSizeByLayoutIndex[layoutIndex] : default;

        public OffsetRect GetLayoutBorders() => TryGetLayoutIndex(out int layoutIndex) ? application.layoutBordersByLayoutIndex[layoutIndex] : default;

        public OffsetRect GetLayoutPaddings() => TryGetLayoutIndex(out int layoutIndex) ? application.layoutPaddingsByLayoutIndex[layoutIndex] : default;

        public float2 GetLayoutLocalPosition() => TryGetLayoutIndex(out int layoutIndex) ? application.layoutLocalPositionByLayoutIndex[layoutIndex] : default;

        public OrientedBounds GetLayoutBounds() => TryGetLayoutIndex(out int layoutIndex) ? application.layoutBoundsByLayoutIndex[layoutIndex] : default;

        public float4x4 GetLayoutLocalMatrix() => TryGetLayoutIndex(out int layoutIndex) ? application.layoutLocalMatrixByLayoutIndex[layoutIndex] : default;

        public float GetFloatProperty(string propertyName, float defaultValue) {
            PropertyId propertyId = application.styleDatabase.GetCustomPropertyId(propertyName);
            return GetFloatProperty(propertyId, defaultValue, true);
        }

        public float GetFloatProperty(string propertyName) {
            PropertyId propertyId = application.styleDatabase.GetCustomPropertyId(propertyName);
            return GetFloatProperty(propertyId, 0, false);
        }

        public float GetFloatProperty(PropertyId propertyId) {
            return GetFloatProperty(propertyId, 0, false);
        }

        public float GetFloatProperty(PropertyId propertyId, float defaultValue) {
            return GetFloatProperty(propertyId, defaultValue, true);
        }

        private float GetFloatProperty(PropertyId propertyId, float defaultValue, bool useDefaultOverride) {
            unsafe {

                // todo -- this can only happen when NOT running styles, need to check the execution phase 

                if (propertyId.index == 0 || propertyId.index < PropertyParsers.PropertyCount || propertyId.index > application.styleDatabase.PropertyTypeCount) {
                    return defaultValue;
                }

                CustomPropertyInfo info = application.applicationLoop.appInfo->customPropertySolvers[propertyId - PropertyParsers.PropertyCount];

                if (info.propertyType != PropertyType.Single) {
                    return defaultValue;
                }

                CustomPropertySolver_Single* solver = (CustomPropertySolver_Single*) (info.solver);

                return solver->GetValue(elementId, defaultValue, useDefaultOverride);
            }

        }

        public T GetEnumProperty<T>(string propertyName, T defaultValue) where T : unmanaged, Enum {
            PropertyId propertyId = application.styleDatabase.GetCustomPropertyId(propertyName);
            return GetEnumProperty(propertyId, defaultValue, true);
        }

        public T GetEnumProperty<T>(string propertyName) where T : unmanaged, Enum {
            PropertyId propertyId = application.styleDatabase.GetCustomPropertyId(propertyName);
            return GetEnumProperty<T>(propertyId, default, false);
        }

        public T GetEnumProperty<T>(PropertyId propertyId) where T : unmanaged, Enum {
            return GetEnumProperty<T>(propertyId, default, false);
        }

        public T GetEnumProperty<T>(PropertyId propertyId, T defaultValue) where T : unmanaged, Enum {
            return GetEnumProperty<T>(propertyId, defaultValue, true);
        }
        
        private T GetEnumProperty<T>(PropertyId propertyId, T defaultValue, bool useDefaultOverride) where T : unmanaged, Enum {
            unsafe {

                // todo -- this can only happen when NOT running styles, need to check the execution phase 

                if (propertyId.index == 0 || propertyId.index < PropertyParsers.PropertyCount || propertyId.index > application.styleDatabase.PropertyTypeCount) {
                    return defaultValue;
                }

                CustomPropertyInfo info = application.applicationLoop.appInfo->customPropertySolvers[propertyId - PropertyParsers.PropertyCount];

                if (info.propertyType != PropertyType.Enum) {
                    return defaultValue;
                }

                CustomPropertySolver_EnumValue* solver = (CustomPropertySolver_EnumValue*) (info.solver);
                EnumValue enumValue = new EnumValue(*(int*) (&defaultValue));

                EnumValue retn = solver->GetValue(elementId, enumValue, useDefaultOverride);

                return *((T*) (&retn));

            }

        }

        public float2 GetFloat2Property(string propertyName, float2 defaultValue) {
            PropertyId propertyId = application.styleDatabase.GetCustomPropertyId(propertyName);
            return GetFloat2Property(propertyId, defaultValue, true);
        }

        public float2 GetFloat2Property(string propertyName) {
            PropertyId propertyId = application.styleDatabase.GetCustomPropertyId(propertyName);
            return GetFloat2Property(propertyId, default, false);
        }

        public float2 GetFloat2Property(PropertyId propertyId) {
            return GetFloat2Property(propertyId, default, false);
        }

        public float2 GetFloat2Property(PropertyId propertyId, float2 defaultValue) {
            return GetFloat2Property(propertyId, defaultValue, true);
        }

        private float2 GetFloat2Property(PropertyId propertyId, float2 defaultValue, bool useDefaultOverride) {
            unsafe {

                // todo -- this can only happen when NOT running styles, need to check the execution phase 

                if (propertyId.index == 0 || propertyId.index < PropertyParsers.PropertyCount || propertyId.index > application.styleDatabase.PropertyTypeCount) {
                    return defaultValue;
                }

                CustomPropertyInfo info = application.applicationLoop.appInfo->customPropertySolvers[propertyId - PropertyParsers.PropertyCount];

                if (info.propertyType != PropertyType.float2) {
                    return defaultValue;
                }

                CustomPropertySolver_float2* solver = (CustomPropertySolver_float2*) (info.solver);

                return solver->GetValue(elementId, defaultValue, useDefaultOverride);
            }

        }

        public UIColor GetColor(string propertyName) {
            PropertyId propertyId = application.styleDatabase.GetCustomPropertyId(propertyName);
            return GetColor(propertyId, default, false);
        }

        public UIColor GetColor(PropertyId propertyId, UIColor defaultValue, bool useDefaultOverride) {
            unsafe {
                if (propertyId.index == 0 || propertyId.index < PropertyParsers.PropertyCount || propertyId.index > application.styleDatabase.PropertyTypeCount) {
                    return defaultValue;
                }

                CustomPropertyInfo info = application.applicationLoop.appInfo->customPropertySolvers[propertyId - PropertyParsers.PropertyCount];

                if (info.propertyType != PropertyType.UIColor) {
                    return defaultValue;
                }

                CustomPropertySolver_UIColor* solver = (CustomPropertySolver_UIColor*) (info.solver);

                return solver->GetValue(elementId, defaultValue, useDefaultOverride);
            }
        }

        /// <summary>
        /// c3 = World Position
        /// </summary>
        /// <returns></returns>
        public float4x4 GetLayoutWorldMatrix() => TryGetLayoutIndex(out int layoutIndex) ? application.layoutWorldMatrixByLayoutIndex[layoutIndex] : default;

        private bool TryGetLayoutIndex(out int layoutIndex) {
            if (isDisabled || isDestroyed) {
                layoutIndex = -1;
                return false;
            }

            layoutIndex = application.layoutIndexByElementId[elementId.index];
            if (layoutIndex < 0) {
                return false;
            }

            return true;
        }

    }

}