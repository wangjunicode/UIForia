using JetBrains.Annotations;
using Src;
using UnityEngine;

namespace Rendering {

    public partial class UIStyleSet {

        public UIMeasurement positionX {
            get { return computedStyle.transform.position.x; }
            set { SetTransformPositionX(value, StyleState.Normal); }
        }

        public UIMeasurement positionY {
            get { return computedStyle.transform.position.y; }
            set { SetTransformPositionY(value, StyleState.Normal); }
        }

        public MeasurementVector2 position {
            get { return computedStyle.transform.position; }
            set { SetTransformPosition(value, StyleState.Normal); }
        }

        public float rotation {
            get { return computedStyle.transform.rotation; }
            set { SetTransformRotation(value, StyleState.Normal); }
        }

        public float scaleX {
            get { return computedStyle.transform.scale.x; }
            set { SetTransformScaleX(value, StyleState.Normal); }
        }

        public float scaleY {
            get { return computedStyle.transform.scale.y; }
            set { SetTransformScaleY(value, StyleState.Normal); }
        }

        public Vector2 scale {
            get { return computedStyle.transform.scale; }
            set { SetTransformScale(value, StyleState.Normal); }
        }

        public float pivotX {
            get { return computedStyle.transform.pivot.x; }
            set { SetTransformPivotX(value, StyleState.Normal); }
        }

        public float pivotY {
            get { return computedStyle.transform.pivot.y; }
            set { SetTransformPivotY(value, StyleState.Normal); }
        }

        public Vector2 pivot {
            get { return computedStyle.transform.pivot; }
            set { SetTransformPivot(value, StyleState.Normal); }
        }

        [PublicAPI]
        public UIMeasurement GetTransformPositionX(StyleState state) {
            return GetStyle(state).transform.position.x;
        }

        [PublicAPI]
        public void SetTransformPositionX(UIMeasurement x, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.transform.position = new MeasurementVector2(x, style.transform.position.y);
            if ((state & currentState) != 0 && style == FindActiveStyleWithoutDefault((s) => s.transform.position.x != UIMeasurement.Unset)) {
                computedStyle.transform.position = new MeasurementVector2(x, computedStyle.transform.position.y);
                changeHandler.SetTransform(element, computedStyle.transform);
            }
        }

        [PublicAPI]
        public UIMeasurement GetTransformPositionY(StyleState state) {
            return GetStyle(state).transform.position.y;
        }

        [PublicAPI]
        public void SetTransformPositionY(UIMeasurement y, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.transform.position = new MeasurementVector2(style.transform.position.x, y);
            if ((state & currentState) != 0 && style == FindActiveStyleWithoutDefault((s) => s.transform.position.y != UIMeasurement.Unset)) {
                computedStyle.transform.position = new MeasurementVector2(computedStyle.transform.position.x, y);
                changeHandler.SetTransform(element, computedStyle.transform);
            }
        }

        [PublicAPI]
        public MeasurementVector2 GetTransformPosition(StyleState state) {
            return GetStyle(state).transform.position;
        }

        [PublicAPI]
        public void SetTransformPosition(MeasurementVector2 newPosition, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.transform.position = newPosition;
            if ((state & currentState) != 0) {
                UIMeasurement x = FindActiveStyle((s) => s.transform.position.x.IsDefined()).transform.position.x;
                UIMeasurement y = FindActiveStyle((s) => s.transform.position.y.IsDefined()).transform.position.y;
                if (computedStyle.transform.position.x != x || computedStyle.transform.position.y != y) {
                    computedStyle.transform.position = new MeasurementVector2(x, y);
                    changeHandler.SetTransform(element, computedStyle.transform);
                }
            }
        }

        [PublicAPI]
        public UIMeasurement GetTransformRotation(StyleState state) {
            return GetStyle(state).transform.rotation;
        }

        [PublicAPI]
        public void SetTransformRotation(float rotationDeg, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.transform.rotation = rotationDeg;
            if ((state & currentState) != 0 && style == FindActiveStyleWithoutDefault((s) => FloatUtil.IsDefined(s.transform.rotation))) {
                computedStyle.transform.rotation = rotationDeg;
                changeHandler.SetTransform(element, computedStyle.transform);
            }
        }

        [PublicAPI]
        public float GetTransformScaleX(StyleState state) {
            return GetStyle(state).transform.scale.x;
        }

        [PublicAPI]
        public void SetTransformScaleX(float x, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.transform.scale = new Vector2(x, style.transform.scale.y);
            if ((state & currentState) != 0 && style == FindActiveStyleWithoutDefault((s) => FloatUtil.IsDefined(s.transform.scale.x))) {
                computedStyle.transform.scale = new Vector2(x, computedStyle.transform.scale.y);
                changeHandler.SetTransform(element, computedStyle.transform);
            }
        }

        [PublicAPI]
        public float GetTransformScaleY(StyleState state) {
            return GetStyle(state).transform.scale.y;
        }

        [PublicAPI]
        public void SetTransformScaleY(float y, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.transform.scale = new Vector2(style.transform.scale.x, y);
            if ((state & currentState) != 0 && style == FindActiveStyleWithoutDefault((s) => FloatUtil.IsDefined(s.transform.scale.y))) {
                computedStyle.transform.scale = new Vector2(computedStyle.transform.scale.x, y);
                changeHandler.SetTransform(element, computedStyle.transform);
            }
        }
        
        [PublicAPI]
        public Vector2 GetTransformScale(StyleState state) {
            return GetStyle(state).transform.scale;
        }

        [PublicAPI]
        public void SetTransformScale(Vector2 newScale, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.transform.scale = newScale;
            if ((state & currentState) != 0) {
                float x = FindActiveStyle((s) => FloatUtil.IsDefined(s.transform.scale.x)).transform.scale.x;
                float y = FindActiveStyle((s) => FloatUtil.IsDefined(s.transform.scale.y)).transform.scale.y;
                if (computedStyle.transform.scale.x != x || computedStyle.transform.scale.y != y) {
                    computedStyle.transform.scale = new Vector2(x, y);
                    changeHandler.SetTransform(element, computedStyle.transform);
                }
            }
        }
        
        [PublicAPI]
        public float GetTransformPivotX(StyleState state) {
            return GetStyle(state).transform.pivot.x;
        }

        [PublicAPI]
        public void SetTransformPivotX(float x, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.transform.pivot = new Vector2(x, style.transform.pivot.y);
            if ((state & currentState) != 0 && style == FindActiveStyleWithoutDefault((s) => FloatUtil.IsDefined(s.transform.pivot.x))) {
                computedStyle.transform.pivot = new Vector2(x, computedStyle.transform.pivot.y);
                changeHandler.SetTransform(element, computedStyle.transform);
            }
        }

        [PublicAPI]
        public float GetTransformPivotY(StyleState state) {
            return GetStyle(state).transform.pivot.y;
        }

        [PublicAPI]
        public void SetTransformPivotY(float y, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.transform.pivot = new Vector2(style.transform.pivot.x, y);
            if ((state & currentState) != 0 && style == FindActiveStyleWithoutDefault((s) => FloatUtil.IsDefined(s.transform.pivot.y))) {
                computedStyle.transform.pivot = new Vector2(computedStyle.transform.pivot.x, y);
                changeHandler.SetTransform(element, computedStyle.transform);
            }
        }
        
        [PublicAPI]
        public Vector2 GetTransformPivot(StyleState state) {
            return GetStyle(state).transform.pivot;
        }

        [PublicAPI]
        public void SetTransformPivot(Vector2 newPivot, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.transform.pivot = newPivot;
            if ((state & currentState) != 0) {
                float x = FindActiveStyle((s) => FloatUtil.IsDefined(s.transform.pivot.x)).transform.pivot.x;
                float y = FindActiveStyle((s) => FloatUtil.IsDefined(s.transform.pivot.y)).transform.pivot.y;
                if (computedStyle.transform.pivot.x != x || computedStyle.transform.pivot.y != y) {
                    computedStyle.transform.pivot = new Vector2(x, y);
                    changeHandler.SetTransform(element, computedStyle.transform);
                }
            }
        }

    }

}