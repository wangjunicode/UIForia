using UIForia.Util;
using System;
using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;
using UIForia.Util.Unsafe;
using UIForia;
using UIForia.Rendering;
using UIForia.Layout;
using UIForia.Text;
// ReSharper disable RedundantNameQualifier
// ReSharper disable UnusedMember.Global


namespace UIForia.Style {

    internal unsafe partial class StyleDatabase {

        partial void SerializeDatabase(ref ManagedByteBuffer writer) {
            writer.Write(styles);
            writer.Write(styleBlocks);
            writer.Write(stateHooks);
            writer.Write(blockQueryRequirements);
            writer.Write(queries);
            writer.Write(queryTables);
            writer.Write(defaultValueIndices);
            writer.Write(propertyLocatorBuffer);
            writer.Write(transitionLocatorBuffer);
            writer.Write(transitionTable);
            writer.Write(gridTemplateTable);
            writer.Write(gridCellTable);
            writer.Write(nameMappings);
            writer.Write(animationOptionsTable);
            SerializeAnimationMap(ref writer);
            writer.Write(animationKeyFrameRanges);
            writer.Write(animationProperties);
            writer.Write(animationKeyFrames);
            SerializeCustomPropertyIdMap(ref writer);
            writer.Write(customPropertyTypes);
            writer.Write(propertyTypeById);
            SerializeStyleSheetMap(ref writer);
            attributeTagger.Serialize(ref writer);
            variableTagger.Serialize(ref writer);
            conditionTagger.Serialize(ref writer);
            tagger.Serialize(ref writer);
            writer.Write(numberRangeStart);
            writer.Write(numberRangeEnd);
            writer.Write(propertyTable_float2);
            writer.Write(propertyTable_Single);
            writer.Write(propertyTable_half);
            writer.Write(propertyTable_Byte);
            writer.Write(propertyTable_UInt16);
            writer.Write(propertyTable_AspectRatio);
            writer.Write(propertyTable_FontAssetId);
            writer.Write(propertyTable_PainterId);
            writer.Write(propertyTable_Int32);
            writer.Write(propertyTable_TextureInfo);
            writer.Write(propertyTable_UIColor);
            writer.Write(propertyTable_GridItemPlacement);
            writer.Write(propertyTable_GridLayoutTemplate);
            writer.Write(propertyTable_UIMeasurement);
            writer.Write(propertyTable_UIFixedLength);
            writer.Write(propertyTable_UIOffset);
            writer.Write(propertyTable_UIAngle);
            writer.Write(propertyTable_UIFontSize);
            writer.Write(propertyTable_UISpaceSize);
            writer.Write(propertyTable_UISizeConstraint);
            writer.Write(propertyTable_EnumValue);
        }

        partial void DeserializeDatabase(ref ManagedByteBuffer reader) {
            reader.Read(ref styles, Unity.Collections.Allocator.Persistent);
            reader.Read(ref styleBlocks, Unity.Collections.Allocator.Persistent);
            reader.Read(ref stateHooks, Unity.Collections.Allocator.Persistent);
            reader.Read(ref blockQueryRequirements, Unity.Collections.Allocator.Persistent);
            reader.Read(ref queries, Unity.Collections.Allocator.Persistent);
            reader.Read(ref queryTables, Unity.Collections.Allocator.Persistent);
            reader.Read(ref defaultValueIndices, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyLocatorBuffer, Unity.Collections.Allocator.Persistent);
            reader.Read(ref transitionLocatorBuffer, Unity.Collections.Allocator.Persistent);
            reader.Read(ref transitionTable, Unity.Collections.Allocator.Persistent);
            reader.Read(ref gridTemplateTable, Unity.Collections.Allocator.Persistent);
            reader.Read(ref gridCellTable, Unity.Collections.Allocator.Persistent);
            reader.Read(ref nameMappings, Unity.Collections.Allocator.Persistent);
            reader.Read(ref animationOptionsTable, Unity.Collections.Allocator.Persistent);
            DeserializeAnimationMap(ref reader);
            reader.Read(ref animationKeyFrameRanges, Unity.Collections.Allocator.Persistent);
            reader.Read(ref animationProperties, Unity.Collections.Allocator.Persistent);
            reader.Read(ref animationKeyFrames, Unity.Collections.Allocator.Persistent);
            DeserializeCustomPropertyIdMap(ref reader);
            reader.Read(ref customPropertyTypes, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTypeById, Unity.Collections.Allocator.Persistent);
            DeserializeStyleSheetMap(ref reader);
            attributeTagger = StringTagger.Deserialize(ref reader);
            variableTagger = StringTagger.Deserialize(ref reader);
            conditionTagger = StringTagger.Deserialize(ref reader);
            tagger = StringTagger.Deserialize(ref reader);
            reader.Read(out numberRangeStart);
            reader.Read(out numberRangeEnd);
            reader.Read(ref propertyTable_float2, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_Single, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_half, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_Byte, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_UInt16, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_AspectRatio, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_FontAssetId, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_PainterId, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_Int32, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_TextureInfo, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_UIColor, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_GridItemPlacement, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_GridLayoutTemplate, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_UIMeasurement, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_UIFixedLength, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_UIOffset, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_UIAngle, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_UIFontSize, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_UISpaceSize, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_UISizeConstraint, Unity.Collections.Allocator.Persistent);
            reader.Read(ref propertyTable_EnumValue, Unity.Collections.Allocator.Persistent);
        }

        partial void VerifySerializedDatabase(StyleDatabase b) {
            Exception error = new Exception("Style databases not equal");
            if(!styles.Equals(b.styles)) throw error;
            if(!styleBlocks.Equals(b.styleBlocks)) throw error;
            if(!stateHooks.Equals(b.stateHooks)) throw error;
            if(!blockQueryRequirements.Equals(b.blockQueryRequirements)) throw error;
            if(!queries.Equals(b.queries)) throw error;
            if(!queryTables.Equals(b.queryTables)) throw error;
            if(!defaultValueIndices.Equals(b.defaultValueIndices)) throw error;
            if(!propertyLocatorBuffer.Equals(b.propertyLocatorBuffer)) throw error;
            if(!transitionLocatorBuffer.Equals(b.transitionLocatorBuffer)) throw error;
            if(!transitionTable.Equals(b.transitionTable)) throw error;
            if(!gridTemplateTable.Equals(b.gridTemplateTable)) throw error;
            if(!gridCellTable.Equals(b.gridCellTable)) throw error;
            if(!nameMappings.Equals(b.nameMappings)) throw error;
            if(!animationOptionsTable.Equals(b.animationOptionsTable)) throw error;
            if(!animationKeyFrameRanges.Equals(b.animationKeyFrameRanges)) throw error;
            if(!animationProperties.Equals(b.animationProperties)) throw error;
            if(!animationKeyFrames.Equals(b.animationKeyFrames)) throw error;
            if(!customPropertyTypes.Equals(b.customPropertyTypes)) throw error;
            if(!propertyTypeById.Equals(b.propertyTypeById)) throw error;
            if(!StyleSheetMapsEqual(b)) throw error;
            if(!attributeTagger.Equals(b.attributeTagger)) throw error;
            if(!variableTagger.Equals(b.variableTagger)) throw error;
            if(!conditionTagger.Equals(b.conditionTagger)) throw error;
            if(!tagger.Equals(b.tagger)) throw error;
            if(numberRangeStart != b.numberRangeStart) throw error;
            if(numberRangeEnd != b.numberRangeEnd) throw error;
            if(!propertyTable_float2.Equals(b.propertyTable_float2)) throw error;
            if(!propertyTable_Single.Equals(b.propertyTable_Single)) throw error;
            if(!propertyTable_half.Equals(b.propertyTable_half)) throw error;
            if(!propertyTable_Byte.Equals(b.propertyTable_Byte)) throw error;
            if(!propertyTable_UInt16.Equals(b.propertyTable_UInt16)) throw error;
            if(!propertyTable_AspectRatio.Equals(b.propertyTable_AspectRatio)) throw error;
            if(!propertyTable_FontAssetId.Equals(b.propertyTable_FontAssetId)) throw error;
            if(!propertyTable_PainterId.Equals(b.propertyTable_PainterId)) throw error;
            if(!propertyTable_Int32.Equals(b.propertyTable_Int32)) throw error;
            if(!propertyTable_TextureInfo.Equals(b.propertyTable_TextureInfo)) throw error;
            if(!propertyTable_UIColor.Equals(b.propertyTable_UIColor)) throw error;
            if(!propertyTable_GridItemPlacement.Equals(b.propertyTable_GridItemPlacement)) throw error;
            if(!propertyTable_GridLayoutTemplate.Equals(b.propertyTable_GridLayoutTemplate)) throw error;
            if(!propertyTable_UIMeasurement.Equals(b.propertyTable_UIMeasurement)) throw error;
            if(!propertyTable_UIFixedLength.Equals(b.propertyTable_UIFixedLength)) throw error;
            if(!propertyTable_UIOffset.Equals(b.propertyTable_UIOffset)) throw error;
            if(!propertyTable_UIAngle.Equals(b.propertyTable_UIAngle)) throw error;
            if(!propertyTable_UIFontSize.Equals(b.propertyTable_UIFontSize)) throw error;
            if(!propertyTable_UISpaceSize.Equals(b.propertyTable_UISpaceSize)) throw error;
            if(!propertyTable_UISizeConstraint.Equals(b.propertyTable_UISizeConstraint)) throw error;
            if(!propertyTable_EnumValue.Equals(b.propertyTable_EnumValue)) throw error;
        }
        
    }

}
