// using UIForia.Graphics.ShapeKit;
// using UnityEngine;
//
// namespace ThisOtherThing.UI.ShapeUtils {
//
//     public class Polygons {
//
//         static Vector3 tmpPos = Vector3.zero;
//
//         public static void AddPolygon(
//             ref UIVertexHelper vh,
//             PolygonProperties polygonProperties,
//             PointListProperties pointListProperties,
//             Vector2 positionOffset,
//             Color32 color,
//             Vector2 uv,
//             ref PointsData pointsData,
//             EdgeGradientData edgeGradientData
//         ) {
//             pointListProperties.SetPoints();
//             PointsList.SetLineData(pointListProperties, ref pointsData);
//
//             int numVertices = vh.currentVertCount;
//             int firstOuterVertex = vh.currentVertCount + polygonProperties.CutoutProperties.Resolution - 1;
//
//             bool usesCutout = polygonProperties.polygonCenterType == PolygonCenterTypes.Cutout;
//
//             if (usesCutout) {
//                 float cutoutOffsetDistance = polygonProperties.CutoutProperties.Radius - edgeGradientData.shadowOffset;
//                 cutoutOffsetDistance += Mathf.LerpUnclamped(
//                     pointsData.PositionNormals[0].magnitude * edgeGradientData.shadowOffset * 3.0f,
//                     0.0f,
//                     edgeGradientData.innerScale
//                 );
//
//                 for (int i = 0; i < polygonProperties.CutoutProperties.Resolution; i++) {
//                     tmpPos.x =
//                         polygonProperties.AdjustedCenter.x +
//                         positionOffset.x +
//                         polygonProperties.CutoutProperties.UnitPositionData.UnitPositions[i].x * cutoutOffsetDistance;
//                     tmpPos.y =
//                         polygonProperties.AdjustedCenter.y +
//                         positionOffset.y +
//                         polygonProperties.CutoutProperties.UnitPositionData.UnitPositions[i].y * cutoutOffsetDistance;
//                     tmpPos.z = 0.0f;
//
//                     vh.AddVert(tmpPos, color, uv);
//                 }
//             }
//             else {
//                 // add center
//                 tmpPos.x = polygonProperties.AdjustedCenter.x + positionOffset.x;
//                 tmpPos.y = polygonProperties.AdjustedCenter.y + positionOffset.y;
//                 tmpPos.z = 0.0f;
//
//                 vh.AddVert(tmpPos, color, uv);
//             }
//
//             // add first position
//             tmpPos.x = positionOffset.x + Mathf.LerpUnclamped(
//                 polygonProperties.AdjustedCenter.x,
//                 pointsData.Positions[0].x + pointsData.PositionNormals[0].x * edgeGradientData.shadowOffset,
//                 edgeGradientData.innerScale
//             );
//             tmpPos.y = positionOffset.y + Mathf.LerpUnclamped(
//                 polygonProperties.AdjustedCenter.y,
//                 pointsData.Positions[0].y + pointsData.PositionNormals[0].y * edgeGradientData.shadowOffset,
//                 edgeGradientData.innerScale
//             );
//             tmpPos.z = 0.0f;
//
//             vh.AddVert(tmpPos, color, uv);
//
//             for (int i = 1; i < pointsData.NumPositions; i++) {
//                 tmpPos.x = positionOffset.x + Mathf.LerpUnclamped(
//                     polygonProperties.AdjustedCenter.x,
//                     pointsData.Positions[i].x + pointsData.PositionNormals[i].x * edgeGradientData.shadowOffset,
//                     edgeGradientData.innerScale
//                 );
//                 tmpPos.y = positionOffset.y + Mathf.LerpUnclamped(
//                     polygonProperties.AdjustedCenter.y,
//                     pointsData.Positions[i].y + pointsData.PositionNormals[i].y * edgeGradientData.shadowOffset,
//                     edgeGradientData.innerScale
//                 );
//
//                 vh.AddVert(tmpPos, color, uv);
//
//                 if (!usesCutout) {
//                     vh.AddTriangle(numVertices, numVertices + i, numVertices + i + 1);
//                 }
//             }
//
//             // add cutout indices
//             if (usesCutout) {
//                 for (int i = 1; i < pointsData.NumPositions; i++) {
//                     vh.AddTriangle(
//                         numVertices + GeoUtils.SimpleMap(i, pointsData.NumPositions, polygonProperties.CutoutProperties.Resolution),
//                         firstOuterVertex + i,
//                         firstOuterVertex + i + 1
//                     );
//                 }
//
//                 for (int i = 1; i < polygonProperties.CutoutProperties.Resolution; i++) {
//                     vh.AddTriangle(
//                         numVertices + i,
//                         numVertices + i - 1,
//                         firstOuterVertex + Mathf.CeilToInt(GeoUtils.SimpleMap((float) i, (float) polygonProperties.CutoutProperties.Resolution, (float) pointsData.NumPositions))
//                     );
//                 }
//             }
//
//             // add last triangle
//             if (usesCutout) {
//                 vh.AddTriangle(
//                     numVertices,
//                     firstOuterVertex + pointsData.NumPositions,
//                     firstOuterVertex + 1
//                 );
//
//                 vh.AddTriangle(
//                     numVertices,
//                     firstOuterVertex,
//                     firstOuterVertex + pointsData.NumPositions
//                 );
//             }
//             else {
//                 vh.AddTriangle(numVertices, numVertices + pointsData.NumPositions, numVertices + 1);
//             }
//
//             if (edgeGradientData.isActive) {
//                 color.a = 0;
//
//                 int outerFirstIndex = numVertices + pointsData.NumPositions;
//
//                 if (usesCutout) {
//                     outerFirstIndex = firstOuterVertex + pointsData.NumPositions;
//                 }
//                 else {
//                     firstOuterVertex = numVertices;
//                 }
//
//                 float offset = edgeGradientData.sizeAdd + edgeGradientData.shadowOffset;
//
//                 vh.AddVert(tmpPos, color, uv);
//
//                 for (int i = 1; i < pointsData.NumPositions; i++) {
//                     
//                     vh.AddVert(positionOffset + pointsData.Positions[i] + pointsData.PositionNormals[i] * offset, color, uv);
//
//                     vh.AddTriangle(firstOuterVertex + i + 1, outerFirstIndex + i, outerFirstIndex + i + 1);
//                     vh.AddTriangle(firstOuterVertex + i + 1, outerFirstIndex + i + 1, firstOuterVertex + i + 2);
//                 }
//
//                 // fill last outer quad
//                 vh.AddTriangle(firstOuterVertex + 1, outerFirstIndex, outerFirstIndex + 1);
//                 vh.AddTriangle(firstOuterVertex + 2, firstOuterVertex + 1, outerFirstIndex + 1);
//
//                 if (usesCutout) {
//                     float radius = (polygonProperties.CutoutProperties.Radius - offset);
//                     for (int i = 0; i < polygonProperties.CutoutProperties.Resolution; i++) {
//                         tmpPos.x =
//                             polygonProperties.AdjustedCenter.x +
//                             positionOffset.x +
//                             polygonProperties.CutoutProperties.UnitPositionData.UnitPositions[i].x * radius;
//                         tmpPos.y =
//                             polygonProperties.AdjustedCenter.y +
//                             positionOffset.y +
//                             polygonProperties.CutoutProperties.UnitPositionData.UnitPositions[i].y * radius;
//                         tmpPos.z = 0.0f;
//
//                         vh.AddVert(tmpPos, color, uv);
//                     }
//
//                     for (int i = 1; i < polygonProperties.CutoutProperties.Resolution; i++) {
//                         vh.AddTriangle(
//                             numVertices + i - 1,
//                             numVertices + i,
//                             outerFirstIndex + pointsData.NumPositions + i
//                         );
//
//                         vh.AddTriangle(
//                             numVertices + i,
//                             outerFirstIndex + pointsData.NumPositions + i + 1,
//                             outerFirstIndex + pointsData.NumPositions + i
//                         );
//                     }
//
//                     vh.AddTriangle(
//                         firstOuterVertex,
//                         numVertices,
//                         outerFirstIndex + pointsData.NumPositions + polygonProperties.CutoutProperties.Resolution
//                     );
//
//                     vh.AddTriangle(
//                         numVertices,
//                         outerFirstIndex + pointsData.NumPositions + 1,
//                         outerFirstIndex + pointsData.NumPositions + polygonProperties.CutoutProperties.Resolution
//                     );
//                 }
//             }
//         }
//
//     }
//
// }