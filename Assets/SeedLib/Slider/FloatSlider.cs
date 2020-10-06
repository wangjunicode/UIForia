using UIForia;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace SeedLib {

    [Template("SeedLib/Slider/Slider.xml")]
    public class Slider : UIElement {

        public bool useBothHandles;
        public bool showStepTicks;

        private float _rangeStart;
        private float _rangeEnd;
        private float _stepSize;

        private float _value;
        private float _valueFar;
        private bool _showTrail;

        private UIElement handle0;
        private UIElement handle1;
        private UIElement trail;

        private float handle0Alignment;
        private float handle1Alignment;
        private bool needsRecompute;

        public float stepSize {
            get => _stepSize;
            set {
                needsRecompute ^= _stepSize != value;
                _stepSize = value;
            }
        }

        public float rangeStart {
            get => _rangeStart;
            set {
                needsRecompute ^= _rangeStart != value;
                _rangeStart = value;
            }
        }

        public float rangeEnd {
            get => _rangeEnd;
            set {
                needsRecompute ^= _rangeEnd != value;
                _rangeEnd = value;
            }
        }

        public bool showTrail {
            get => _showTrail;
            set {
                if (value == _showTrail) {
                    return;
                }

                _showTrail = value;
                trail?.SetEnabled(_showTrail);
                UpdateElements();
            }
        }

        public float valueFar {
            get {
                if (needsRecompute) {
                    Recompute();
                }

                return _valueFar;
            }
            set {
                _valueFar = value;
                Recompute();
            }
        }

        public float value {
            get {
                if (needsRecompute) {
                    Recompute();
                }

                return _value;
            }
            set {
                _value = value;
                Recompute();
            }
        }

        private void Recompute(bool updateHandles = true) {
            needsRecompute = false;
            if (_rangeEnd < _rangeStart) {
                float tmp = _rangeEnd;
                _rangeEnd = _rangeStart;
                _rangeStart = tmp;
            }

            if (_stepSize > 0) {
                _value = MathUtil.Round(_value, _stepSize);
                if (useBothHandles) {
                    _valueFar = MathUtil.Round(_valueFar, _stepSize);
                }
            }

            if (_value > _valueFar) {
                float tmp = _valueFar;
                _valueFar = _value;
                _value = tmp;
            }

            if (updateHandles) {
                handle0Alignment = MathUtil.PercentOfRange(_value, _rangeStart, _rangeEnd);
                handle1Alignment = MathUtil.PercentOfRange(_valueFar, _rangeStart, _rangeEnd);
            }

            UpdateElements();
        }

        public override void OnEnable() {
            handle0 = FindById("handle");
            handle1 = FindById("handle-far");
            trail = FindById("trail");
            trail.SetEnabled(_showTrail);
            handle1.SetEnabled(useBothHandles);
            Recompute();
        }

        private void UpdateElements() {
            if (handle0 == null || trail == null) {
                return;
            }

            handle0.style.SetAlignmentPercentageX(handle0Alignment);

            if (handle1.isEnabled) {
                handle1.style.SetAlignmentPercentageX(handle1Alignment);
                if (_showTrail) {

                    trail.style.SetAlignmentOriginX(new OffsetMeasurement(Mathf.Min(handle0Alignment, handle1Alignment), OffsetMeasurementUnit.Percent), StyleState.Normal);
                    trail.style.SetPreferredWidth(new UIMeasurement(Mathf.Abs(handle0Alignment - handle1Alignment), UIMeasurementUnit.Percentage), StyleState.Normal);
                }
            }
            else {
                if (_showTrail) {
                    trail.style.SetPreferredWidth(new UIMeasurement(handle0Alignment, UIMeasurementUnit.Percentage), StyleState.Normal);
                }
            }
        }

        private void ClampDragResult() {
            Recompute();
        }

        public DragEvent DragHandle(MouseInputEvent evt) {
            return new SliderDragEvent(this, evt.Origin == handle0 || evt.Origin.Parent == handle0);
        }

        protected override void OnSetAttribute(string attrName, string newValue, string oldValue) {
            UIElementExtensions.VisitDescendents(this, (child) => { child.SetAttribute(attrName, newValue); });
        }

        private void SetValuesFromHandleAlignments() {
            //Debug.Log("0: " + handle0Alignment + " 1: " + handle1Alignment);
            float v0 = Mathf.Lerp(rangeStart, rangeEnd, handle0Alignment);
            float v1 = Mathf.Lerp(rangeStart, rangeEnd, handle1Alignment);
            if (useBothHandles) {
                if (v0 >= v1) {
                    _valueFar = v0;
                    _value = v1;
                }
                else {
                    _valueFar = v1;
                    _value = v0;
                }
            }
            else {
                _value = v0;
            }

            Recompute(false);
            UpdateElements();
        }

        public class SliderDragEvent : DragEvent {

            private readonly Slider source;
            private bool isHandle0;

            public SliderDragEvent(Slider source, bool isHandle0) {
                this.source = source;
                this.isHandle0 = isHandle0;
            }

            public override void Update() {
                Rect screenRect = source.layoutResult.ScreenRect;
                if (isHandle0) {
                    source.handle0Alignment = Mathf.Clamp01(MathUtil.PercentOfRange(MousePosition.x, screenRect.x, screenRect.x + screenRect.width));
                }
                else {
                    source.handle1Alignment = Mathf.Clamp01(MathUtil.PercentOfRange(MousePosition.x, screenRect.x, screenRect.x + screenRect.width));
                }

                source.SetValuesFromHandleAlignments();
            }

            public override void OnComplete() {
                source.ClampDragResult();
            }

        }

    }

}