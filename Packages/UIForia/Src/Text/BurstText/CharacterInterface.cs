using System;
using UIForia.Graphics;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Text {

    public unsafe struct CharacterInterface {

        internal int charIndex;
        internal TextEffectInfo* vertexPtr;
        internal FontAssetInfo* fontAssetMap;
        internal BurstCharInfo* charptr;
        internal TextSystem textSystem;
        internal TextInfo* textInfo;

        private const float inverseByteFloat = 1f / 255f;

        public char character {
            get => (char) charptr->character;
        }

        public void Translate(float3 topLeft, float3 topRight, float3 bottomRight, float3 bottomLeft) {
            InitDataPtr();
            vertexPtr->topLeft += topLeft;
            vertexPtr->topRight += topRight;
            vertexPtr->bottomRight += bottomRight;
            vertexPtr->bottomLeft += bottomLeft;
        }

        private void InitMaterialOverride() {
            if (charptr->materialIndex == charptr->baseMaterialIndex) {
                charptr->materialIndex = (ushort) textInfo->materialBuffer.size;
                textInfo->materialBuffer.Add(textInfo->materialBuffer.array[charptr->baseMaterialIndex]);
            }
        }

        public void SetFaceColor(Color32 color) {
            InitMaterialOverride();
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            materialInfo.faceColor = color;
        }

        public Color32 GetFaceColor() {
            return textInfo->materialBuffer.array[charptr->materialIndex].faceColor;
        }

        public Color32 GetDefaultFaceColor() {
            return textInfo->materialBuffer.array[charptr->baseMaterialIndex].faceColor;
        }

        public void SetOutlineColor(Color32 outlineColor) {
            InitMaterialOverride();
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            materialInfo.outlineColor = outlineColor;
        }

        public Color32 GetDefaultOutlineColor() {
            return textInfo->materialBuffer.array[charptr->baseMaterialIndex].outlineColor;
        }

        public Color32 GetOutlineColor() {
            return textInfo->materialBuffer.array[charptr->materialIndex].outlineColor;
        }

        public void SetGlowColor(Color32 glowColor) {
            InitMaterialOverride();
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            materialInfo.glowColor = glowColor;
        }

        public Color32 GetDefaultGlowColor() {
            return textInfo->materialBuffer.array[charptr->baseMaterialIndex].glowColor;
        }

        public Color32 GetGlowColor() {
            return textInfo->materialBuffer.array[charptr->materialIndex].glowColor;
        }

        public void SetUnderlayColor(Color32 underlayColor) {
            InitMaterialOverride();
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            materialInfo.underlayColor = underlayColor;
        }

        public Color32 GetDefaultUnderlayColor() {
            return textInfo->materialBuffer.array[charptr->baseMaterialIndex].underlayColor;
        }

        public Color32 GetUnderlayColor() {
            return textInfo->materialBuffer.array[charptr->materialIndex].underlayColor;
        }

        public void SetOutlineWidth(float width) {
            InitMaterialOverride();
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            materialInfo.outlineWidth = MathUtil.Float01ToByte(width);
        }

        public float GetOutlineWidth() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            return materialInfo.outlineWidth * inverseByteFloat;
        }

        public float GetDefaultOutlineWidth() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->baseMaterialIndex];
            return materialInfo.outlineWidth * inverseByteFloat;
        }

        public void SetOutlineSoftness(float softness) {
            InitMaterialOverride();
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            materialInfo.outlineSoftness = MathUtil.Float01ToByte(softness);
        }

        public float GetOutlineSoftness() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            return materialInfo.outlineSoftness * inverseByteFloat;
        }

        public float GetDefaultOutlineSoftness() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->baseMaterialIndex];
            return materialInfo.outlineSoftness * inverseByteFloat;
        }

        public void SetFaceDilate(float dilate) {
            InitMaterialOverride();
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            materialInfo.faceDilate = MathUtil.FloatMinus1To1ToUshort(dilate);
        }

        public float GetFaceDilate() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            return MathUtil.UShortToFloatOneMinusOne(materialInfo.faceDilate);
        }

        public float GetDefaultFaceDilate() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->baseMaterialIndex];
            return MathUtil.UShortToFloatOneMinusOne(materialInfo.faceDilate);
        }

        public void SetUnderlayDilate(float dilate) {
            InitMaterialOverride();
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            materialInfo.underlayDilate = MathUtil.FloatMinus1To1ToUshort(dilate);
        }

        public float GetUnderlayDilate() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            return MathUtil.UShortToFloatOneMinusOne(materialInfo.underlayDilate);
        }

        public float GetDefaultUnderlayDilate() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->baseMaterialIndex];
            return MathUtil.UShortToFloatOneMinusOne(materialInfo.underlayDilate);
        }

        public void SetGlowOffset(float offset) {
            InitMaterialOverride();
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            materialInfo.glowOffset = MathUtil.FloatMinus1To1ToUshort(offset);
        }

        public float GetGlowOffset() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            return MathUtil.UShortToFloatOneMinusOne(materialInfo.glowOffset);
        }

        public float GetDefaultGlowOffset() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->baseMaterialIndex];
            return MathUtil.UShortToFloatOneMinusOne(materialInfo.glowOffset);
        }

        public void SetUnderlayX(float underlayX) {
            InitMaterialOverride();
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            materialInfo.underlayX = underlayX;
        }

        public float GetUnderlayX() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            return materialInfo.underlayX;
        }

        public float GetDefaultUnderlayX() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->baseMaterialIndex];
            return materialInfo.underlayX;
        }

        public void SetUnderlayY(float underlayY) {
            InitMaterialOverride();
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            materialInfo.underlayY = underlayY;
        }

        public float GetUnderlayY() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            return materialInfo.underlayY;
        }

        public float GetDefaultUnderlayY() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->baseMaterialIndex];
            return materialInfo.underlayY;
        }

        public void SetUnderlaySoftness(float softness) {
            InitMaterialOverride();
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            materialInfo.underlaySoftness = MathUtil.Float01ToByte(softness);
        }

        public float GetUnderlaySoftness() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            return materialInfo.underlaySoftness * inverseByteFloat;
        }

        public float GetDefaultUnderlaySoftness() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->baseMaterialIndex];
            return materialInfo.underlaySoftness * inverseByteFloat;
        }

        public void SetGlowPower(float power) {
            InitMaterialOverride();
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            materialInfo.glowPower = MathUtil.Float01ToByte(power);
        }

        public float GetGlowPower() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            return materialInfo.glowPower * inverseByteFloat;
        }

        public float GetDefaultGlowPower() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->baseMaterialIndex];
            return materialInfo.glowPower * inverseByteFloat;
        }

        public void SetGlowInner(float inner) {
            InitMaterialOverride();
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            materialInfo.glowInner = MathUtil.Float01ToByte(inner);
        }

        public float GetGlowInner() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            return materialInfo.glowInner * inverseByteFloat;
        }

        public float GetDefaultGlowInner() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->baseMaterialIndex];
            return materialInfo.glowInner * inverseByteFloat;
        }

        public void SetGlowOuter(float outer) {
            InitMaterialOverride();
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            materialInfo.glowOuter = MathUtil.Float01ToByte(outer);
        }

        public float GetGlowOuter() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->materialIndex];
            return materialInfo.glowOuter * inverseByteFloat;
        }

        public float GetDefaultGlowOuter() {
            ref TextMaterialInfo materialInfo = ref textInfo->materialBuffer.array[charptr->baseMaterialIndex];
            return materialInfo.glowOuter * inverseByteFloat;
        }

        public void GetDefaultVertices(out float3 topLeft, out float3 topRight, out float3 bottomRight, out float3 bottomLeft) {
            ref UIForiaGlyph glyph = ref fontAssetMap[charptr->fontAssetId].glyphList[charptr->glyphIndex];

            float width = glyph.width * charptr->scale;
            float height = glyph.height * charptr->scale;

            float2 p = charptr->renderPosition;
            topLeft = new float3(p.x, p.y, 0);
            topRight = new float3(p.x + width, p.y, 0);
            bottomLeft = new float3(p.x + width, p.y + height, 0);
            bottomRight = new float3(p.x, p.y + height, 0);
        }

        public void InitDefaultVertices() {
            InitDataPtr();

            ref UIForiaGlyph glyph = ref fontAssetMap[charptr->fontAssetId].glyphList[charptr->glyphIndex];

            float width = glyph.width * charptr->scale;
            float height = glyph.height * charptr->scale;

            float2 p = charptr->renderPosition;
            vertexPtr->topLeft = new float3(p.x, -p.y, 0);
            vertexPtr->topRight = new float3(p.x + width, -p.y, 0);
            vertexPtr->bottomLeft = new float3(p.x + width, -(p.y + height), 0);
            vertexPtr->bottomRight = new float3(p.x, -(p.y + height), 0);
        }

        public void ResetVertices() {
            if (charptr->effectIdx == 0) return;
            textSystem.DeallocateEffectIndex(charptr->effectIdx);
            charptr->effectIdx = 0;
            vertexPtr = null;
        }

        private void InitDataPtr() {
            if (vertexPtr == null) {

                if (charptr->effectIdx == 0) {
                    charptr->effectIdx = (ushort) textSystem.AllocateEffectIndex();
                    InitDefaultVertices();
                }

                vertexPtr = textSystem.textEffectVertexInfoTable.GetArrayPointer() + charptr->effectIdx;
            }
        }

        public void Translate(float3 offset) {

            InitDataPtr();

            ref TextEffectInfo vertexData = ref vertexPtr[0];
            vertexData.topLeft += offset;
            vertexData.topRight += offset;
            vertexData.bottomRight += offset;
            vertexData.bottomLeft += offset;

        }

        public static float3 RotateAround(float3 vec, Vector2 center, float rot) {
            // rot *= Mathf.Deg2Rad;

            float tempX = vec.x - center.x;
            float tempY = vec.y - center.y;

            float rotatedX = tempX * Mathf.Cos(rot) - tempY * Mathf.Sin(rot);
            float rotatedY = tempX * Mathf.Sin(rot) + tempY * Mathf.Cos(rot);

            vec.x = rotatedX + center.x;
            vec.y = rotatedY + center.y;

            return vec;
        }

        public float2 GetCenterPoint() {

            if (vertexPtr == null) {
                if (charptr->effectIdx == 0) {
                    InitDataPtr();
                    InitDefaultVertices();
                }
            }

            float centerX = vertexPtr->topLeft.x + ((vertexPtr->bottomRight.x - vertexPtr->topLeft.x) * 0.5f);
            float centerY = vertexPtr->topLeft.y - ((vertexPtr->topLeft.y - vertexPtr->bottomRight.y) * 0.5f);
            return new float2(centerX, centerY);
        }

        public float2 GetTopPoint() {
            if (vertexPtr == null && charptr->effectIdx == 0) {
                ref UIForiaGlyph glyph = ref fontAssetMap[charptr->fontAssetId].glyphList[charptr->glyphIndex];

                float width = glyph.width * charptr->scale;

                return new float2(charptr->renderPosition.x + (width * 0.5f), -charptr->renderPosition.y);
            }

            float centerX = vertexPtr->topLeft.x + ((vertexPtr->topRight.x - vertexPtr->topLeft.x) * 0.5f);

            return new float2(centerX, vertexPtr->topLeft.y);
        }

        public float2 GetLayoutBottomLeft() {
            ref UIForiaGlyph glyph = ref fontAssetMap[charptr->fontAssetId].glyphList[charptr->glyphIndex];

            float height = glyph.height * charptr->scale;
            return new float2(charptr->renderPosition.x, -(charptr->renderPosition.y + height));
        }

        public float2 GetLayoutBottomRight() {
            ref UIForiaGlyph glyph = ref fontAssetMap[charptr->fontAssetId].glyphList[charptr->glyphIndex];

            float width = glyph.width * charptr->scale;
            float height = glyph.height * charptr->scale;
            return new float2(charptr->renderPosition.x + width, -(charptr->renderPosition.y + height));
        }

        public float2 GetLayoutBottomCenter() {
            ref UIForiaGlyph glyph = ref fontAssetMap[charptr->fontAssetId].glyphList[charptr->glyphIndex];

            float width = glyph.width * charptr->scale;
            float height = glyph.height * charptr->scale;
            return new float2(charptr->renderPosition.x + (width * 0.5f), -(charptr->renderPosition.y + height));
        }

        public float2 GetLayoutTopLeft() {
            return new float2(charptr->renderPosition.x, charptr->renderPosition.y);
        }

        public float2 GetLayoutTopRight() {
            ref UIForiaGlyph glyph = ref fontAssetMap[charptr->fontAssetId].glyphList[charptr->glyphIndex];

            float width = glyph.width * charptr->scale;
            return new float2(charptr->renderPosition.x + width, charptr->renderPosition.y);
        }

        public float2 GetLayoutCenterLeft() {
            ref UIForiaGlyph glyph = ref fontAssetMap[charptr->fontAssetId].glyphList[charptr->glyphIndex];

            float height = glyph.height * charptr->scale;
            return new float2(charptr->renderPosition.x, -(charptr->renderPosition.y + (height * 0.5f)));
        }

        public float2 GetLayoutCenterRight() {
            ref UIForiaGlyph glyph = ref fontAssetMap[charptr->fontAssetId].glyphList[charptr->glyphIndex];

            float width = glyph.width * charptr->scale;
            float height = glyph.height * charptr->scale;
            return new float2(charptr->renderPosition.x + width, -(charptr->renderPosition.y + (height * 0.5f)));
        }

        public float3 GetDefaultTopLeft() {
            float2 p = charptr->renderPosition;
            return new float3(p.x, -p.y, 0);
        }

        public float3 GetDefaultTopRight() {
            ref UIForiaGlyph glyph = ref fontAssetMap[charptr->fontAssetId].glyphList[charptr->glyphIndex];

            float2 p = charptr->renderPosition;
            return new float3(p.x + (glyph.width * charptr->scale), -p.y, 0);
        }

        public float3 GetDefaultBottomRight() {
            ref UIForiaGlyph glyph = ref fontAssetMap[charptr->fontAssetId].glyphList[charptr->glyphIndex];

            float2 p = charptr->renderPosition;
            return new float3(p.x + (glyph.width * charptr->scale), -(p.y + (glyph.height * charptr->scale)), 0);
        }

        public float3 GetDefaultBottomLeft() {
            ref UIForiaGlyph glyph = ref fontAssetMap[charptr->fontAssetId].glyphList[charptr->glyphIndex];

            float2 p = charptr->renderPosition;
            return new float3(p.x, -(p.y + (glyph.height * charptr->scale)), 0);
        }

        public float3 GetDefaultBottomCenter() {
            ref UIForiaGlyph glyph = ref fontAssetMap[charptr->fontAssetId].glyphList[charptr->glyphIndex];

            float2 p = charptr->renderPosition;
            return new float3(p.x + ((glyph.width * charptr->scale) * 0.5f), -(p.y + (glyph.height * charptr->scale)), 0);
        }

        public float3 GetTopLeft() {
            if (vertexPtr == null && charptr->effectIdx == 0) {
                return GetDefaultTopLeft();
            }

            InitDataPtr();
            return vertexPtr->topLeft;
        }

        public void SetTopLeft(float3 topTopLeft) {
            InitDataPtr();
            vertexPtr->topLeft = topTopLeft;
        }

        public float3 GetTopRight() {
            if (vertexPtr == null && charptr->effectIdx == 0) {
                return GetDefaultTopRight();
            }

            InitDataPtr();
            return vertexPtr->topRight;
        }

        public void SetTopRight(float3 topRight) {
            InitDataPtr();
            vertexPtr->topRight = topRight;
        }

        public float3 GetBottomLeft() {
            if (vertexPtr == null && charptr->effectIdx == 0) {
                return GetDefaultBottomLeft();
            }

            InitDataPtr();
            return vertexPtr->bottomLeft;
        }

        public void SetBottomLeft(float3 bottomLeft) {
            InitDataPtr();
            vertexPtr->bottomLeft = bottomLeft;
        }

        public float3 GetBottomRight() {
            if (vertexPtr == null && charptr->effectIdx == 0) {
                return GetDefaultBottomRight();
            }

            InitDataPtr();
            return vertexPtr->bottomRight;
        }

        public void SetBottomRight(float3 bottomRight) {
            InitDataPtr();
            vertexPtr->bottomRight = bottomRight;
        }

        public void Rotate2D(float radians) {

            InitDataPtr();

            float centerX = vertexPtr->topLeft.x + ((vertexPtr->topRight.x - vertexPtr->topLeft.x) * 0.5f);
            float centerY = vertexPtr->topLeft.y - ((vertexPtr->topLeft.y - vertexPtr->bottomRight.y) * 0.5f);

            math.sincos(radians, out float s, out float c);

            float3 tmp = vertexPtr->topLeft;
            vertexPtr->topLeft.x = c * (tmp.x - centerX) + s * (tmp.y - centerY) + centerX;
            vertexPtr->topLeft.y = c * (tmp.y - centerY) - s * (tmp.x - centerX) + centerY;

            tmp = vertexPtr->topRight;
            vertexPtr->topRight.x = c * (tmp.x - centerX) + s * (tmp.y - centerY) + centerX;
            vertexPtr->topRight.y = c * (tmp.y - centerY) - s * (tmp.x - centerX) + centerY;

            tmp = vertexPtr->bottomRight;
            vertexPtr->bottomRight.x = c * (tmp.x - centerX) + s * (tmp.y - centerY) + centerX;
            vertexPtr->bottomRight.y = c * (tmp.y - centerY) - s * (tmp.x - centerX) + centerY;

            tmp = vertexPtr->bottomLeft;
            vertexPtr->bottomLeft.x = c * (tmp.x - centerX) + s * (tmp.y - centerY) + centerX;
            vertexPtr->bottomLeft.y = c * (tmp.y - centerY) - s * (tmp.x - centerX) + centerY;
        }

        public void RotateAround2D(float2 pivot, float radians) {

            InitDataPtr();

            float centerX = pivot.x;
            float centerY = pivot.y;

            math.sincos(radians, out float s, out float c);

            float3 tmp = vertexPtr->topLeft;
            vertexPtr->topLeft.x = c * (tmp.x - centerX) + s * (tmp.y - centerY) + centerX;
            vertexPtr->topLeft.y = c * (tmp.y - centerY) - s * (tmp.x - centerX) + centerY;

            tmp = vertexPtr->topRight;
            vertexPtr->topRight.x = c * (tmp.x - centerX) + s * (tmp.y - centerY) + centerX;
            vertexPtr->topRight.y = c * (tmp.y - centerY) - s * (tmp.x - centerX) + centerY;

            tmp = vertexPtr->bottomRight;
            vertexPtr->bottomRight.x = c * (tmp.x - centerX) + s * (tmp.y - centerY) + centerX;
            vertexPtr->bottomRight.y = c * (tmp.y - centerY) - s * (tmp.x - centerX) + centerY;

            tmp = vertexPtr->bottomLeft;
            vertexPtr->bottomLeft.x = c * (tmp.x - centerX) + s * (tmp.y - centerY) + centerX;
            vertexPtr->bottomLeft.y = c * (tmp.y - centerY) - s * (tmp.x - centerX) + centerY;
        }

        public void InvertHorizontal(bool invert) {
            if (invert) {
                charptr->displayFlags |= CharacterDisplayFlags.InvertHorizontalUV;
            }
            else {
                charptr->displayFlags &= ~CharacterDisplayFlags.InvertHorizontalUV;
            }
        }

        public void InvertVertical(bool invert) {
            if (invert) {
                charptr->displayFlags |= CharacterDisplayFlags.InvertVerticalUV;
            }
            else {
                charptr->displayFlags &= ~CharacterDisplayFlags.InvertVerticalUV;
            }
        }

        public void SetOpacity(float opacity) {
            charptr->opacityMultiplier = MathUtil.Float01ToByte(opacity);
        }

        public void RotateAngleAxis(float rot, Vector3 axis) {
            InitDataPtr();
            Quaternion quaternion = Quaternion.AngleAxis(rot, axis);
            vertexPtr->topLeft = quaternion * vertexPtr->topLeft;
            vertexPtr->topRight = quaternion * vertexPtr->topRight;
            vertexPtr->bottomRight = quaternion * vertexPtr->bottomRight;
            vertexPtr->bottomLeft = quaternion * vertexPtr->bottomLeft;
        }

        public void RotateLocalAngleAxis(float degrees, Vector3 axis) {

            InitDataPtr();

            float2 center = GetCenterPoint();
            float3 offset = new float3(center.x, center.y, 0);
            vertexPtr->topLeft -= offset;
            vertexPtr->topRight -= offset;
            vertexPtr->bottomRight -= offset;
            vertexPtr->bottomLeft -= offset;

            Quaternion quaternion = Quaternion.AngleAxis(degrees, axis);
            vertexPtr->topLeft = quaternion * vertexPtr->topLeft;
            vertexPtr->topRight = quaternion * vertexPtr->topRight;
            vertexPtr->bottomRight = quaternion * vertexPtr->bottomRight;
            vertexPtr->bottomLeft = quaternion * vertexPtr->bottomLeft;

            vertexPtr->topLeft += offset;
            vertexPtr->topRight += offset;
            vertexPtr->bottomRight += offset;
            vertexPtr->bottomLeft += offset;
        }

        public float GetScale() {
            InitDataPtr();
            if (vertexPtr->scaleTopLeft == 0) {
                return 1;
            }

            return vertexPtr->scaleTopLeft;
        }

        public void SetScale(float scale, float3 localPivot) {
            InitDataPtr();
            vertexPtr->topLeft -= localPivot;
            vertexPtr->topRight -= localPivot;
            vertexPtr->bottomRight -= localPivot;
            vertexPtr->bottomLeft -= localPivot;
            vertexPtr->topLeft *= scale;
            vertexPtr->topRight *= scale;
            vertexPtr->bottomRight *= scale;
            vertexPtr->bottomLeft *= scale;
            vertexPtr->scaleTopLeft = scale;
            vertexPtr->scaleTopRight = scale;
            vertexPtr->scaleBottomLeft = scale;
            vertexPtr->scaleBottomRight = scale;
            vertexPtr->topLeft += localPivot;
            vertexPtr->topRight += localPivot;
            vertexPtr->bottomRight += localPivot;
            vertexPtr->bottomLeft += localPivot;
        }

        public void SetScale(float scale) {
            InitDataPtr();

            // I dont even want to keep this old scale around, I just need to know the total scale for the uvs
            // so maybe store the diff if not present?
            // todo -- probably wrong since i dont really know the last scale origin if it wasn't the center
            if (vertexPtr->scaleTopLeft != 0) {
                float invOldScale = 1f / vertexPtr->scaleTopLeft;
                vertexPtr->topLeft *= invOldScale;
                vertexPtr->topRight *= invOldScale;
                vertexPtr->bottomRight *= invOldScale;
                vertexPtr->bottomLeft *= invOldScale;
            }

            float2 center = GetCenterPoint();
            float3 offset = new float3(center.x, center.y, 0);

            vertexPtr->topLeft -= offset;
            vertexPtr->topRight -= offset;
            vertexPtr->bottomRight -= offset;
            vertexPtr->bottomLeft -= offset;

            vertexPtr->topLeft *= scale;
            vertexPtr->topRight *= scale;
            vertexPtr->bottomRight *= scale;
            vertexPtr->bottomLeft *= scale;
            vertexPtr->scaleTopLeft = scale;
            vertexPtr->scaleTopRight = scale;
            vertexPtr->scaleBottomLeft = scale;
            vertexPtr->scaleBottomRight = scale;

            vertexPtr->topLeft += offset;
            vertexPtr->topRight += offset;
            vertexPtr->bottomRight += offset;
            vertexPtr->bottomLeft += offset;

        }

    }

}