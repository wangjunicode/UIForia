using System;
using UIForia.Rendering;
using UIForia.Util.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Style {

    internal struct EnumValue : IEquatable<EnumValue> {

        public int value;

        public EnumValue(int value) {
            this.value = value;
        }

        public bool Equals(EnumValue other) {
            return value == other.value;
        }

        public override bool Equals(object obj) {
            return obj is EnumValue other && Equals(other);
        }

        public override int GetHashCode() {
            return value;
        }

    }

    internal struct EnumValue_Interpolator {

        public static EnumValue Interpolate(EnumValue prev, EnumValue next, float t) {
            return t <= 0.5f ? prev : next;
        }

    }

    internal struct TTEMPLATE_Interpolator {

        public static TTEMPLATE Interpolate(TTEMPLATE prev, TTEMPLATE next, float t) {
            throw new NotImplementedException();
        }

    }

    internal struct UIColor_Interpolator {

        public static UIColor Interpolate(UIColor prev, UIColor next, float t) {
            return Color.LerpUnclamped(prev, next, t);
        }

    }

    internal struct UIMeasurement_Interpolator {

        public static UIMeasurement Interpolate(UIMeasurement prev, UIMeasurement next, float t) {
            throw new NotImplementedException();
        }

    }

    internal struct UISpaceSize_Interpolator {

        public static UISpaceSize Interpolate(UISpaceSize prev, UISpaceSize next, float t) {
            return prev;
        }

    }

    internal struct AspectRatio_Interpolator {

        public static AspectRatio Interpolate(AspectRatio prev, AspectRatio next, float t) {
            throw new NotImplementedException();
        }

    }

    internal struct Byte_Interpolator {

        public static Byte Interpolate(Byte prev, Byte next, float t) {
            throw new NotImplementedException();
        }

    }

    internal struct FontAssetId_Interpolator {

        public static FontAssetId Interpolate(FontAssetId prev, FontAssetId next, float t) {
            throw new NotImplementedException();
        }

    }

    internal struct GridItemPlacement_Interpolator {

        public static GridItemPlacement Interpolate(GridItemPlacement prev, GridItemPlacement next, float t) {
            throw new NotImplementedException();
        }

    }

    internal struct GridLayoutTemplate_Interpolator {

        public static GridLayoutTemplate Interpolate(GridLayoutTemplate prev, GridLayoutTemplate next, float t) {
            throw new NotImplementedException();
        }

    }

    internal struct Single_Interpolator {

        public static float Interpolate(float prev, float next, float t) {
            return math.lerp(prev, next, t);
        }

    }

    internal struct float2_Interpolator {

        public static float2 Interpolate(float2 prev, float2 next, float t) {
            return math.lerp(prev, next, t);
        }

    }

    internal struct float3_Interpolator {

        public static float3 Interpolate(float3 prev, float3 next, float t) {
            return math.lerp(prev, next, t);
        }

    }

    internal struct float4_Interpolator {

        public static float4 Interpolate(float4 prev, float4 next, float t) {
            return math.lerp(prev, next, t);
        }

    }

    internal struct half_Interpolator {

        public static half Interpolate(half prev, half next, float t) {
            return (half) math.lerp(prev, next, t);
        }

    }

    internal struct Int32_Interpolator {

        public static Int32 Interpolate(Int32 prev, Int32 next, float t) {
            return (int) math.lerp(prev, next, t);
        }

    }

    internal struct PainterId_Interpolator {

        public static PainterId Interpolate(PainterId prev, PainterId next, float t) {
            throw new NotImplementedException();
        }

    }

    internal struct TextureInfo_Interpolator {

        public static TextureInfo Interpolate(TextureInfo prev, TextureInfo next, float t) {
            throw new NotImplementedException();
        }

    }

    internal struct UIAngle_Interpolator {

        public static UIAngle Interpolate(UIAngle prev, UIAngle next, float t) {
            throw new NotImplementedException();
        }

    }

    internal struct UIFixedLength_Interpolator {

        public static UIFixedLength Interpolate(UIFixedLength prev, UIFixedLength next, float t) {
            throw new NotImplementedException();
        }

    }

    internal struct UIFontSize_Interpolator {

        public static UIFontSize Interpolate(UIFontSize prev, UIFontSize next, float t) {
            throw new NotImplementedException();
        }

    }

    internal struct UInt16_Interpolator {

        public static UInt16 Interpolate(UInt16 prev, UInt16 next, float t) {
            return (ushort) math.lerp(prev, next, t);
        }

    }

    internal struct UIOffset_Interpolator {

        public static UIOffset Interpolate(UIOffset prev, UIOffset next, float t) {
            throw new NotImplementedException();
        }

    }

    internal struct UISizeConstraint_Interpolator {

        public static UISizeConstraint Interpolate(UISizeConstraint prev, UISizeConstraint next, float t) {
            throw new NotImplementedException();
        }

    }

    internal struct ActiveTransition_TTEMPLATE {

        public ElementId elementId;
        public int elapsed;

        public TTEMPLATE prev;
        public TTEMPLATE next;

        public int transitionId;
        public TransitionDefinition definition;

        public static TTEMPLATE Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_TTEMPLATE transition, float progress) {
            throw new NotImplementedException();
        }

        public static TTEMPLATE ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_TTEMPLATE transition, float progress) {
            throw new NotImplementedException();
        }

    }

    internal struct ActiveTransition_UISizeConstraint {

        public ElementId elementId;
        public int elapsed;

        public UISizeConstraint prev;
        public UISizeConstraint next;

        public int transitionId;
        public TransitionDefinition definition;

        public static UISizeConstraint Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UISizeConstraint transition, float progress) {
            throw new NotImplementedException();
        }

        public static UISizeConstraint ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UISizeConstraint transition, float progress) {
            throw new NotImplementedException();
        }

    }

    internal struct ActiveTransition_UISpaceSize {

        public ElementId elementId;
        public int elapsed;

        public UISpaceSize prev;
        public UISpaceSize next;

        public int transitionId;
        public TransitionDefinition definition;

        public static UISpaceSize Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UISpaceSize transition, float progress) {
            throw new NotImplementedException();
        }

        public static UISpaceSize ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UISpaceSize transition, float progress) {
            throw new NotImplementedException();
        }

    }

    internal struct ActiveTransition_AspectRatio {

        public ElementId elementId;
        public int elapsed;

        public AspectRatio prev;
        public AspectRatio next;

        public int transitionId;
        public TransitionDefinition definition;

        public static AspectRatio Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_AspectRatio transition, float progress) {
            throw new NotImplementedException();
        }

        public static AspectRatio ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_AspectRatio transition, float progress) {
            throw new NotImplementedException();
        }

    }

    internal struct ActiveTransition_UIFontSize {

        public ElementId elementId;
        public int elapsed;

        public UIFontSize prev;
        public UIFontSize next;

        public int transitionId;
        public TransitionDefinition definition;

        public static UIFontSize Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UIFontSize transition, float progress) {
            throw new NotImplementedException();
        }

        public static UIFontSize ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UIFontSize transition, float progress) {
            throw new NotImplementedException();
        }

    }

    internal struct ActiveTransition_Byte {

        public ElementId elementId;
        public int elapsed;

        public byte prev;
        public byte next;

        public int transitionId;
        public TransitionDefinition definition;

        public static byte Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_Byte transition, float progress) {
            if (enumTypeId == 0) {
                // do actual interpolation
                int prev = (int) transition.prev;
                int next = (int) transition.next;
                return (byte) math.lerp(prev, next, progress);
            }

            return progress > 0.5f ? transition.next : transition.prev;
        }

        public static byte ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_Byte transition, float progress) {
            if (enumTypeId == 0) {
                // do actual interpolation
                int prev = (int) transition.prev;
                int next = (int) transition.next;
                return (byte) math.lerp(prev, next, progress);
            }

            return progress > 0.5f ? transition.next : transition.prev;
        }

    }

    internal struct ActiveTransition_FontAssetId {

        public ElementId elementId;
        public int elapsed;

        public FontAssetId prev;
        public FontAssetId next;

        public int transitionId;
        public TransitionDefinition definition;

        public static FontAssetId Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_FontAssetId transition, float progress) {
            throw new NotImplementedException();
        }

        public static FontAssetId ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_FontAssetId transition, float progress) {
            throw new NotImplementedException();
        }

    }

    internal struct ActiveTransition_GridItemPlacement {

        public ElementId elementId;
        public int elapsed;

        public GridItemPlacement prev;
        public GridItemPlacement next;

        public int transitionId;
        public TransitionDefinition definition;

        public static GridItemPlacement Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_GridItemPlacement transition, float progress) {
            throw new NotImplementedException();
        }

        public static GridItemPlacement ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_GridItemPlacement transition, float progress) {
            throw new NotImplementedException();
        }

    }

    internal struct ActiveTransition_GridLayoutTemplate {

        public ElementId elementId;
        public int elapsed;

        public GridLayoutTemplate prev;
        public GridLayoutTemplate next;

        public int transitionId;
        public TransitionDefinition definition;

        public static GridLayoutTemplate Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_GridLayoutTemplate transition, float progress) {
            throw new NotImplementedException();
        }

        public static GridLayoutTemplate ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_GridLayoutTemplate transition, float progress) {
            throw new NotImplementedException();
        }

    }

    internal struct ActiveTransition_Int32 {

        public ElementId elementId;
        public int elapsed;

        public Int32 prev;
        public Int32 next;

        public int transitionId;
        public TransitionDefinition definition;

        public static Int32 Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_Int32 transition, float progress) {
            throw new NotImplementedException();
        }

        public static Int32 ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_Int32 transition, float progress) {
            throw new NotImplementedException();
        }

    }

    internal struct ActiveTransition_UInt16 {

        public ElementId elementId;
        public int elapsed;

        public UInt16 prev;
        public UInt16 next;

        public int transitionId;
        public TransitionDefinition definition;

        public static UInt16 Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UInt16 transition, float progress) {
            throw new NotImplementedException();
        }

        public static UInt16 ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UInt16 transition, float progress) {
            throw new NotImplementedException();
        }

    }

    internal struct ActiveTransition_PainterId {

        public ElementId elementId;
        public int elapsed;

        public PainterId prev;
        public PainterId next;

        public int transitionId;
        public TransitionDefinition definition;

        public static PainterId Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_PainterId transition, float progress) {
            throw new NotImplementedException();
        }

        public static PainterId ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_PainterId transition, float progress) {
            throw new NotImplementedException();
        }

    }

    internal struct ActiveTransition_TextureInfo {

        public ElementId elementId;
        public int elapsed;

        public TextureInfo prev;
        public TextureInfo next;

        public int transitionId;
        public TransitionDefinition definition;

        public static TextureInfo Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_TextureInfo transition, float progress) {
            throw new NotImplementedException();
        }

        public static TextureInfo ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_TextureInfo transition, float progress) {
            throw new NotImplementedException();
        }

    }

    internal struct ActiveTransition_UIAngle {

        public ElementId elementId;
        public int elapsed;

        public UIAngle prev;
        public UIAngle next;

        public int transitionId;
        public TransitionDefinition definition;

        public static UIAngle Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UIAngle transition, float progress) {
            throw new NotImplementedException();
        }

        public static UIAngle ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UIAngle transition, float progress) {
            throw new NotImplementedException();
        }

    }

    internal struct ActiveTransition_UIColor {

        public ElementId elementId;
        public int elapsed;

        public UIColor prev;
        public UIColor next;

        public int transitionId;
        public TransitionDefinition definition;

        public static UIColor Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UIColor transition, float progress) {

            Color prevColor = (Color) transition.prev;
            Color nextColor = (Color) transition.next;
            float t = Easing.Interpolate(progress, transition.definition.easing);
            return (UIColor) Color.LerpUnclamped(prevColor, nextColor, t);
        }

        public static UIColor ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UIColor transition, float progress) {
            Color prevColor = (Color) transition.prev;
            Color nextColor = (Color) transition.next;
            float t = Easing.Interpolate(progress, transition.definition.easing);
            return (UIColor) Color.LerpUnclamped(prevColor, nextColor, t);
        }

    }

    internal struct ActiveTransition_UIFixedLength {

        public ElementId elementId;
        public int elapsed;

        public UIFixedLength prev;
        public UIFixedLength next;

        public int transitionId;
        public TransitionDefinition definition;

        public static UIFixedLength Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UIFixedLength transition, float progress) {
            throw new NotImplementedException();
        }

        public static UIFixedLength ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UIFixedLength transition, float progress) {
            throw new NotImplementedException();
        }

    }

    internal struct ActiveTransition_UIMeasurement {

        public ElementId elementId;
        public int elapsed;

        public UIMeasurement prev;
        public UIMeasurement next;

        public int transitionId;
        public TransitionDefinition definition;

        public static UIMeasurement Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UIMeasurement transition, float progress) {
            throw new NotImplementedException();
        }

        public static UIMeasurement ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UIMeasurement transition, float progress) {
            // parameters.prefSizeTable[transition.elementId].resolveFloatValue;
            throw new NotImplementedException();
        }

    }

    internal struct ActiveTransition_UIOffset {

        public ElementId elementId;
        public int elapsed;

        public UIOffset prev;
        public UIOffset next;

        public int transitionId;
        public TransitionDefinition definition;

        public static UIOffset Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UIOffset transition, float progress) {
            throw new NotImplementedException();
        }

        public static UIOffset ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_UIOffset transition, float progress) {
            throw new NotImplementedException();
        }

    }

    internal struct ActiveTransition_half {

        public ElementId elementId;
        public int elapsed;

        public half prev;
        public half next;

        public int transitionId;
        public TransitionDefinition definition;

        public static half Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_half transition, float progress) {
            float t = Easing.Interpolate(progress, transition.definition.easing);
            return (half) math.lerp(transition.prev, transition.next, t);
        }

        public static half ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_half transition, float progress) {
            float t = Easing.Interpolate(progress, transition.definition.easing);
            return (half) math.lerp(transition.prev, transition.next, t);
        }

    }

    internal struct ActiveTransition_Single {

        public ElementId elementId;
        public int elapsed;

        public float prev;
        public float next;

        public int transitionId;
        public TransitionDefinition definition;

        public static float Interpolate(int enumTypeId, in SolverParameters parameters, in ActiveTransition_Single transition, float progress) {
            float t = Easing.Interpolate(progress, transition.definition.easing);
            return math.lerp(transition.prev, transition.next, t);
        }

        public static float ResolveTransitionResetValue(int enumTypeId, in SolverParameters parameters, in ActiveTransition_Single transition, float progress) {
            float t = Easing.Interpolate(progress, transition.definition.easing);
            return math.lerp(transition.prev, transition.next, t);
        }

    }

    internal static unsafe class PropertySolver {

        public static BumpList<ElementId> ComputeDefaults(ElementMap previousDefinitions, ElementMap definitions, in SolverParameters parameters, BumpAllocator* bumpAllocator) {

            ElementMap initMap = parameters.initMap;

            int defaultCount = 0;
            int mapSize = parameters.longsPerElementMap;

            for (int b = 0; b < mapSize; b++) {
                // default = prev & not current || init & not defined
                ulong initNotDefined = initMap.map[b] & ~definitions.map[b];
                ulong prevNotCurrent = previousDefinitions.map[b] & ~definitions.map[b];
                defaultCount += math.countbits(initNotDefined | prevNotCurrent);
            }

            if (defaultCount == 0) {
                return default;
            }

            BumpList<ElementId> retn = bumpAllocator->AllocateList<ElementId>(defaultCount);

            for (int b = 0; b < mapSize; b++) {

                ulong initNotDefined = initMap.map[b] & ~definitions.map[b];
                ulong prevNotCurrent = previousDefinitions.map[b] & ~definitions.map[b];

                long defaults = (long) (prevNotCurrent | initNotDefined);

                while (defaults != 0) {
                    int elementId = (b * 64) + math.tzcnt((ulong) defaults);
                    defaults ^= defaults & -defaults;

                    retn.array[retn.size++] = new ElementId(elementId);
                }
            }

            return retn;

        }

        public static PendingTransition FindAndRemovePending(ElementId elementId, PendingTransition* pendingTransitionList, ref int transitionCount) {

            int idx = 0;
            // entry will 100% be in this list so we can avoid for-loop overhead
            while (pendingTransitionList[idx].elementId != elementId) {
                idx++;
            }

            // swap remove the last pending transition
            PendingTransition retn = pendingTransitionList[idx];
            pendingTransitionList[idx] = pendingTransitionList[--transitionCount];
            return retn;

        }

        // public static BumpList<Inheritor> ComputeInheritors(in PropertySolverContext context, in SolverParameters parameters, BumpAllocator* tempAllocator) {
        //     int inheritorsCount = 0;
        //     int mapSize = parameters.longsPerElementMap;
        //     ulong* inheritMap = context.inheritMap;
        //
        //     for (int b = 0; b < mapSize; b++) {
        //         inheritorsCount += math.countbits(inheritMap[b]);
        //     }
        //
        //     if (inheritorsCount == 0) {
        //         return default;
        //     }
        //
        //     BumpList<Inheritor> inheritorList = tempAllocator->AllocateList<Inheritor>(inheritorsCount);
        //
        //     for (int b = 0; b < mapSize; b++) {
        //
        //         long inheritors = (long) inheritMap[b];
        //
        //         while (inheritors != 0) {
        //             int inheritorIndex = (b * 64) + math.tzcnt((ulong) inheritors);
        //             inheritors ^= inheritors & -inheritors;
        //
        //             inheritorList.array[inheritorList.size++] = new Inheritor() {
        //                 elementId = new ElementId(inheritorIndex),
        //             };
        //
        //         }
        //     }
        //
        //     for (int i = 0; i < inheritorsCount; i++) {
        //         inheritorList.array[i].elementIndex = parameters.elementIdToIndex[i];
        //     }
        //
        //     for (int i = 0; i < inheritorsCount; i++) {
        //         inheritorList.array[i].parentIndex = parameters.indexToParentIndex[inheritorList.array[i].elementIndex];
        //     }
        //
        //     for (int i = 0; i < inheritorsCount; i++) {
        //         inheritorList.array[i].parentId = parameters.indexToElementId[inheritorList.array[i].parentIndex];
        //     }
        //
        //     NativeSortExtension.Sort(inheritorList.array, inheritorList.size);
        //
        //     return inheritorList;
        // }

        // must inherit = isInherited & parent.changed
        // current setup isn't good for checking if parent changed
        // maybe we dont check? just do it? 
        // need to interleave transition & inherit 

        // inherit can trigger transition
        // transition can trigger inherit too 
        // need to do one after the other somehow 

        // hard to do inherit & parent change in bits

        // or we just aggressively write inheritors 
        // then transition
        // then re-write inherit
        // doesn't tht trigger transition again?
        // feels like i need to transition / inherit in order 

        // variable resolution 
        // foreach in write buffer
        // if is variable -> resolve it & replace value in write buffer
        // if resolved == previous, remove from write buffer, remove from change map 

        // fill write buffer with defaults here? i think so, except dont do it for inits this frame 

        // foreach in write buffer -- need to not write i think, or otherwise store the prev value to diff
        // if has pending transition & !animation defined (will never have pending & active)
        // remove from write buffer
        // unset changed
        // become active 

        // foreach inheritor -- sorted
        // if became inherited or parent changed
        // if has pending transition && !animated
        // get new value
        // if new value != old value
        // become active
        // unset changed (maybe not needed)
        // else
        // write new value
        // mark changed (if value different)

        // foreach variable subscription
        // find in write buffer (change would be set I think)
        // if variable changed -> add to write buffer if not in it already

        public static TTEMPLATE Interpolate(TransitionDefinition transitionDefinition, TTEMPLATE transitionPrev, TTEMPLATE transitionNext, float progress, int solverInfoEnumTypeId) {
            throw new NotImplementedException();
        }

        public static UIColor Interpolate(TransitionDefinition transition, UIColor prev, UIColor next, float progress, int solverInfoEnumTypeId) {

            Color prevColor = (Color) prev;
            Color nextColor = (Color) next;

            float t = Easing.Interpolate(progress, transition.easing);
            return (UIColor) Color.LerpUnclamped(prevColor, nextColor, t);

        }

        public static byte Interpolate(TransitionDefinition transition, byte prev, byte next, float progress, int solverInfoEnumTypeId) {

            float t = Easing.Interpolate(progress, transition.easing);

            if (solverInfoEnumTypeId != 0) {
                return t < 0.5f ? prev : next;
            }

            return (byte) (math.lerp((int) prev, (int) next, t));

        }

        public static ushort Interpolate(TransitionDefinition transition, ushort prev, ushort next, float progress, int solverInfoEnumTypeId) {

            float t = Easing.Interpolate(progress, transition.easing);

            if (solverInfoEnumTypeId != 0) {
                return t < 0.5f ? prev : next;
            }

            return (ushort) (math.lerp((int) prev, (int) next, t));

        }

        public static int Interpolate(TransitionDefinition transition, int prev, int next, float progress, int solverInfoEnumTypeId) {

            float t = Easing.Interpolate(progress, transition.easing);

            if (solverInfoEnumTypeId != 0) {
                return t < 0.5f ? prev : next;
            }

            return (int) math.lerp(prev, next, t);

        }

        public static uint Interpolate(TransitionDefinition transition, uint prev, uint next, float progress, int solverInfoEnumTypeId) {

            float t = Easing.Interpolate(progress, transition.easing);

            if (solverInfoEnumTypeId != 0) {
                return t < 0.5f ? prev : next;
            }

            return (uint) math.lerp(prev, next, t);

        }

        public static float Interpolate(TransitionDefinition transition, float prev, float next, float progress, int solverInfoEnumTypeId) {
            float t = Easing.Interpolate(progress, transition.easing);
            return math.lerp(prev, next, t);
        }

        public static half Interpolate(TransitionDefinition transition, half prev, half next, float progress, int solverInfoEnumTypeId) {
            float t = Easing.Interpolate(progress, transition.easing);
            return (half) math.lerp(prev, next, t);
        }

        public static UIAngle Interpolate(TransitionDefinition transition, UIAngle transitionPrev, UIAngle next, float progress, int solverInfoEnumTypeId) {
            throw new NotImplementedException();
        }

        public static UIOffset Interpolate(TransitionDefinition transition, UIOffset prev, UIOffset next, float progress, int solverInfoEnumTypeId) {
            throw new NotImplementedException();
        }

        public static UIFixedLength Interpolate(TransitionDefinition transition, UIFixedLength prev, UIFixedLength next, float progress, int solverInfoEnumTypeId) {
            throw new NotImplementedException();
        }

        public static UIMeasurement Interpolate(TransitionDefinition transition, UIMeasurement prev, UIMeasurement next, float progress, int solverInfoEnumTypeId) {
            throw new NotImplementedException();
        }

        public static TextureInfo Interpolate(TransitionDefinition transition, TextureInfo prev, TextureInfo next, float progress, int solverInfoEnumTypeId) {
            throw new NotImplementedException();
        }

        public static PainterId Interpolate(TransitionDefinition transition, PainterId prev, PainterId next, float progress, int solverInfoEnumTypeId) {
            throw new NotImplementedException();
        }

        public static FontAssetId Interpolate(TransitionDefinition transition, FontAssetId prev, FontAssetId next, float progress, int solverInfoEnumTypeId) {
            throw new NotImplementedException();
        }

        public static GridItemPlacement Interpolate(TransitionDefinition transition, GridItemPlacement prev, GridItemPlacement next, float progress, int solverInfoEnumTypeId) {
            throw new NotImplementedException();
        }

        public static GridLayoutTemplate Interpolate(TransitionDefinition transition, GridLayoutTemplate prev, GridLayoutTemplate next, float progress, int solverInfoEnumTypeId) {
            throw new NotImplementedException();
        }

        public static TTEMPLATE ResolveTransitionResetValue_TTEMPLATE(int solverInfoEnumTypeId, in ActiveTransition_TTEMPLATE prevTransition) {
            throw new NotImplementedException();
        }

        public static TTEMPLATE ResolveTransitionResetValue_TTEMPLATE(SolverParameters infoEnumTypeId, int solverInfoEnumTypeId, TransitionDefinition prevTransitionDefinition, int elapsed, TTEMPLATE prev, TTEMPLATE next) {
            throw new NotImplementedException();
        }

    }

}