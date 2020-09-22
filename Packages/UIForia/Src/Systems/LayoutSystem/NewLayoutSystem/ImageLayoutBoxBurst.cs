using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia.Layout {

    internal unsafe struct ImageLayoutBoxBurst : ILayoutBox {

        public ElementId elementId;
        public float textureWidth;
        public float textureHeight;
        public UIMeasurement preferredHeight;
        public UIMeasurement minHeight;
        public UIMeasurement maxHeight;
        public LayoutBoxUnion* layoutBox;


        public void OnInitialize(LayoutSystem layoutSystem, UIElement element, UIElement proxy) {
            this.elementId = element.id;

            Size size = LayoutSystem.UpdateTextureSize(element);
            textureWidth = size.width;
            textureHeight = size.height;

            preferredHeight = proxy.style.PreferredHeight;
            minHeight = proxy.style.MinHeight;
            maxHeight = proxy.style.MaxHeight;

            LayoutBoxType boxType = LayoutBoxUnion.GetLayoutBoxTypeForProxy(element);
            layoutBox = TypedUnsafe.Malloc<LayoutBoxUnion>(Allocator.Persistent);
            layoutBox->Initialize(boxType, layoutSystem, element, proxy);
        }

        public void Dispose() {
            layoutBox->Dispose();
            TypedUnsafe.Dispose(layoutBox, Allocator.Persistent);
            layoutBox = null;
        }

        public float ResolveSelfAutoWidth<T>(ref BurstLayoutRunner runner, in T parent, in BlockSize blockSize, BurstLayoutRunner.MeasurementAxis measurementType) where T : ILayoutBox {
            // can only compute auto width if height is auto, px, vw, vh, em
            // otherwise auto width = texture box width

            // auto always equals texture size if unresolvable
            // width auto resolvable from px resolvable height
            // otherwise texture size

            if (textureHeight == 0) {
                return 0;
            }

            float aspectScale = textureWidth / textureHeight;

            UIMeasurement pairMeasurement = preferredHeight;
            if (measurementType == BurstLayoutRunner.MeasurementAxis.Min) pairMeasurement = minHeight;
            if (measurementType == BurstLayoutRunner.MeasurementAxis.Max) pairMeasurement = maxHeight;

            switch (pairMeasurement.unit) {
                default:
                case UIMeasurementUnit.Unset:
                case UIMeasurementUnit.Content:
                case UIMeasurementUnit.BlockSize:
                case UIMeasurementUnit.Percentage:
                case UIMeasurementUnit.ParentContentArea:
                    return textureWidth;

                case UIMeasurementUnit.Pixel:
                case UIMeasurementUnit.ViewportWidth:
                case UIMeasurementUnit.ViewportHeight:
                case UIMeasurementUnit.Em:
                case UIMeasurementUnit.BackgroundImageWidth:
                case UIMeasurementUnit.BackgroundImageHeight:
                    float retn = runner.ResolveHeight(parent, elementId, blockSize, pairMeasurement, ref runner.GetVerticalLayoutInfo(elementId));
                    return retn * aspectScale;
            }
        }

        public float ResolveSelfAutoHeight<T>(ref BurstLayoutRunner runner) where T : ILayoutBox {
            if (textureWidth == 0) {
                return 0;
            }

            float aspectScale = textureHeight / textureWidth;

            float finalWidth = runner.GetHorizontalLayoutInfo(elementId).finalSize;
            return finalWidth * aspectScale;
        }

        public float ResolveAutoWidth(ref BurstLayoutRunner runner, ElementId elementId, in BlockSize blockSize) {
            return layoutBox->ResolveAutoWidth(ref runner, elementId, blockSize);
        }

        public float ResolveAutoHeight(ref BurstLayoutRunner runner, ElementId elementId, in BlockSize blockSize) {
            return layoutBox->ResolveAutoHeight(ref runner, elementId, blockSize);
        }

        public void RunHorizontal(BurstLayoutRunner* runner) {
            layoutBox->RunLayoutHorizontal(runner);
        }

        public void RunVertical(BurstLayoutRunner* runner) {
            layoutBox->RunLayoutVertical(runner);
        }

        public float ComputeContentWidth(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            return layoutBox->ComputeContentWidth(ref layoutRunner, blockSize);
        }

        public float ComputeContentHeight(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            return layoutBox->ComputeContentHeight(ref layoutRunner, blockSize);
        }

        public void OnStylePropertiesChanged(LayoutSystem layoutSystem, UIElement element, StyleProperty[] propertyList, int propertyCount) {
            bool updateTextureParams = false;
            for (int i = 0; i < propertyCount; i++) {
                switch (propertyList[i].propertyId) {
                    case StylePropertyId.BackgroundImage:
                    case StylePropertyId.BackgroundRectMinX:
                    case StylePropertyId.BackgroundRectMinY:
                    case StylePropertyId.BackgroundRectMaxX:
                    case StylePropertyId.BackgroundRectMaxY:
                        updateTextureParams = true;
                        break;

                    case StylePropertyId.PreferredHeight:
                        preferredHeight = propertyList[i].AsUIMeasurement;
                        break;


                    case StylePropertyId.MinHeight:
                        minHeight = propertyList[i].AsUIMeasurement;
                        break;


                    case StylePropertyId.MaxHeight:
                        maxHeight = propertyList[i].AsUIMeasurement;
                        break;
                }
            }

            if (updateTextureParams) {
                Size size = LayoutSystem.UpdateTextureSize(element);
                textureWidth = size.width;
                textureHeight = size.height;
            }
        }

        public void OnChildStyleChanged(LayoutSystem layoutSystem, ElementId childId, StyleProperty[] properties, int propertyCount) {
            layoutBox->OnChildStyleChanged(layoutSystem, childId, properties, propertyCount);
        }

        public float GetActualContentWidth(ref BurstLayoutRunner runner) {
            return 0;
        }

        public float GetActualContentHeight(ref BurstLayoutRunner runner) {
            return 0;
        }

        public void OnChildrenChanged(LayoutSystem layoutSystem) {
            layoutBox->OnChildrenChanged(layoutSystem);
        }

    }

}