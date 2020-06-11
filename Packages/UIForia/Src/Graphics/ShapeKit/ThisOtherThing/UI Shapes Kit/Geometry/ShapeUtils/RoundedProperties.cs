using UnityEngine;

namespace ThisOtherThing.UI.ShapeUtils {

    public struct RoundedProperties {

        public enum RoundedType {

            None,
            Uniform,
            Individual

        }

        public enum ResolutionType {

            Uniform,
            Individual

        }

        public RoundedType Type; // = RoundedType.None;
        public ResolutionType ResolutionMode; // = ResolutionType.Uniform;

        public float UniformRadius; // = 15.0f;
        public bool UseMaxRadius; // = false;

        public float TLRadius; //= 15.0f;
        public RoundingProperties TLResolution; //= new RoundingProperties();

        public float TRRadius; //= 15.0f;
        public RoundingProperties TRResolution; //= new RoundingProperties();

        public float BRRadius; //= 15.0f;
        public RoundingProperties BRResolution; //= new RoundingProperties();

        public float BLRadius; //= 15.0f;
        public RoundingProperties BLResolution; //= new RoundingProperties();

        public RoundingProperties UniformResolution; //= new RoundingProperties();
        
        public float AdjustedTLRadius { get; private set; }
        public float AdjustedTRRadius { get; private set; }
        public float AdjustedBRRadius { get; private set; }
        public float AdjustedBLRadius { get; private set; }

        public void UpdateAdjusted(Rect rect, float offset) {
            switch (Type) {
                case RoundedType.Uniform:
                    if (UseMaxRadius) {
                        AdjustedTLRadius = Mathf.Min(rect.width, rect.height) * 0.5f;
                        AdjustedTRRadius = AdjustedTLRadius;
                        AdjustedBRRadius = AdjustedTLRadius;
                        AdjustedBLRadius = AdjustedTLRadius;
                    }
                    else {
                        AdjustedTLRadius = UniformRadius;
                        AdjustedTRRadius = AdjustedTLRadius;
                        AdjustedBRRadius = AdjustedTLRadius;
                        AdjustedBLRadius = AdjustedTLRadius;
                    }

                    break;

                case RoundedType.Individual:
                    AdjustedTLRadius = TLRadius;
                    AdjustedTRRadius = TRRadius;
                    AdjustedBRRadius = BRRadius;
                    AdjustedBLRadius = BLRadius;
                    break;

                case RoundedType.None:
                    AdjustedTLRadius = 0.0f;
                    AdjustedTRRadius = AdjustedTLRadius;
                    AdjustedBRRadius = AdjustedTLRadius;
                    AdjustedBLRadius = AdjustedTLRadius;
                    break;

                default:
                    throw new System.ArgumentOutOfRangeException();
            }

            if (ResolutionMode == ResolutionType.Uniform) {
                TLResolution.UpdateAdjusted(AdjustedTLRadius, offset, UniformResolution, 4.0f);
                TRResolution.UpdateAdjusted(AdjustedTRRadius, offset, UniformResolution, 4.0f);
                BRResolution.UpdateAdjusted(AdjustedBRRadius, offset, UniformResolution, 4.0f);
                BLResolution.UpdateAdjusted(AdjustedBLRadius, offset, UniformResolution, 4.0f);
            }
            else {
                TLResolution.UpdateAdjusted(AdjustedTLRadius, offset, 4.0f);
                TRResolution.UpdateAdjusted(AdjustedTRRadius, offset, 4.0f);
                BRResolution.UpdateAdjusted(AdjustedBRRadius, offset, 4.0f);
                BLResolution.UpdateAdjusted(AdjustedBLRadius, offset, 4.0f);
            }
        }

        public void OnCheck(Rect rect) {
            float shorterSide = Mathf.Min(rect.width, rect.height);
            float halfShorterSide = shorterSide * 0.5f;

            // check radii don't overlap
            switch (Type) {
                case RoundedType.Uniform:
                    UniformRadius = Mathf.Clamp(UniformRadius, 0.0f, halfShorterSide);
                    break;

                case RoundedType.Individual:
                    TLRadius = Mathf.Max(TLRadius, 0.0f);
                    TRRadius = Mathf.Max(TRRadius, 0.0f);
                    BRRadius = Mathf.Max(BRRadius, 0.0f);
                    BLRadius = Mathf.Max(BLRadius, 0.0f);
                    break;
            }

            // TLResolution.OnCheck();
            // TRResolution.OnCheck();
            // BRResolution.OnCheck();
            // BLResolution.OnCheck();
            //
            // UniformResolution.OnCheck();
        }

    }

}