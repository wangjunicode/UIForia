using System.Diagnostics;
using ThisOtherThing.UI;
using ThisOtherThing.UI.ShapeUtils;
using UIForia;
using UIForia.Graphics.ShapeKit;
using UIForia.Rendering;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace DefaultNamespace {

    [CustomPainter("ShapeKitTest")]
    public class ShapeKitPainter : StandardRenderBox, IUnityInspector {

        public Mesh mesh;
        public Material material;

        private UIVertexHelper vh;
        public float spikeAmount;
        public float lineWeight = 2f;
        public float roundingDistance = 0f;
        public float length = 1f;
        public float maxAngle = 0f;
        
        public override void OnInitialize() {
            material = Resources.Load<Material>("ShapeKitMaterial");
            // material = Resources.Load<Material>("Wireframe/Wireframe");
            mesh = new Mesh();
            vh = UIVertexHelper.Create(Allocator.Persistent);
        }

        public override void OnDestroy() {
            vh.Dispose(); // todo -- this probably leaks right now
        }

        public void PaintBackground(RenderContext ctx) {
            float width = element.layoutResult.actualSize.width;
            float height = element.layoutResult.actualSize.height;
            Rect pixelRect = new Rect(0, 0, width, height);

            float4x4 mat = element.layoutResult.GetWorldMatrix();

            var start = Stopwatch.StartNew();
            vh.Clear();
            // DrawArc(ref vh, pixelRect, backgroundColor);
//             DrawPolygon(ref vh, pixelRect, Color.white);

            // DrawEllipse(ref vh, pixelRect, backgroundColor);
            Profiler.BeginSample("Draw");
            DrawEllipseStroke(ref vh, pixelRect, backgroundColor);
            Profiler.EndSample();
            start.Stop();
            vh.FillMesh(mesh);
         //   Debug.Log("time: " + start.Elapsed.TotalMilliseconds.ToString("F3") + " vertices: " + mesh.vertexCount);

            ctx.DrawMesh(mesh, material, mat);
        }

        public static float s_Scale = 1.2f;

        public float shadowSoftness;
        public float shadowSize;

        public void DrawEllipseStroke(ref UIVertexHelper vh, Rect pixelRect, Color fillColor) {

            using (ShapeKit shapeKit = new ShapeKit(Allocator.Temp)) {
                // shapeKit.AddCircle(ref vh, pixelRect, fillColor);
                // shapeKit.AddCircle(ref vh, pixelRect, fillColor);

                // pixelRect.width = 50;
                // pixelRect.height = 50;
                // shapeKit.AddRing(ref vh, pixelRect, 5f, fillColor);
                //
                // shapeKit.AddRect(ref vh, new Rect(100, 100, 100, 100), Color.yellow);
                //
                // shapeKit.AddRoundedRect(ref vh, new Rect(0, 0, 300, 300), Corner.Round(16), Color.red);
                shapeKit.AddSector(ref vh, new Vector2(150, -150), new Vector2(150, 150), new ArcProperties() {
                    length = length,
                    direction = ArcDirection.Forward,
                    baseAngle = 90
                }, Color.blue);
                
                // shapeKit.SetShadow(new ShadowData() {
                //     size = shadowSize,
                //     softness = shadowSoftness,
                // });
                //
                // shapeKit.SetDrawMode(DrawMode.Shadow);
                //
                // shapeKit.AddSectorOutline(ref vh, new Vector2(150, -150), new Vector2(150, 150), new ArcProperties() {
                //     direction = ArcDirection.Forward,
                //     length = 0.3f
                // }, new OutlineProperties(4f), color);

                // shapeKit.AddArc(ref vh, new Vector2(150, -150), new Vector2(150, 150), new ArcProperties() {
                //     direction = ArcDirection.Forward,
                //     length = 0.3f
                // });

                // List_float2 positions = new List_float2(128, Allocator.Temp);
                //
                // PointsGenerator.SetPointsRound(ref positions, new RoundPointListData() {
                //     width = pixelRect.width,
                //     height = pixelRect.height,
                //     length = 0.5f,
                //     direction = ArcDirection.Backward,
                //     startOffset = 0.25f,
                //     center = new float2(pixelRect.width * 0.5f, -pixelRect.height * 0.5f),
                //     skipLastPoint = true,
                // });
                //
                // PointsGenerator.SetPointsRect(ref positions, new RectPointListData() {
                //     width = pixelRect.width,
                //     height = pixelRect.height,
                //     startOffset = 3,
                //     center = new float2(pixelRect.width * 0.5f, -pixelRect.height * 0.5f),
                // });
                //
                // // generate point from center, length, angle
                // PointsGenerator.SetPointsAngleLine(ref positions, new PointListAngleLineData() {
                //     angle = 0.33f,
                //     length = 300f,
                //     center = new float2(pixelRect.width * 0.5f, -pixelRect.height * 0.5f),
                // });
                //
                // PointsGenerator.SetPointsStar(ref positions, new PointListStarData() {
                //     width = 50, //pixelRect.width,
                //     height = 50, // pixelRect.height,
                //     spikeAmount = spikeAmount,
                //     resolution = 5,
                //     length = length,
                //     center = new float2(0, 0) //pixelRect.width * 0.5f, -pixelRect.height * 0.5f),
                // });
                // //
                // shapeKit.SetShadow(new ShadowData() {
                //     size = shadowSize,
                //     softness = shadowSoftness
                // });
                // shapeKit.SetDrawMode(DrawMode.Shadow);
                // shapeKit.AddLine(ref vh,
                //     ref positions,
                //     new LineProperties() {
                //         closed = true,
                //         weight = lineWeight,
                //         capType = LineCapTypes.Round,
                //         lineType = LineType.Center,
                //         roundedCapResolution = new RoundingResolution() {
                //             resolutionType = ResolutionType.Calculated,
                //             maxDistance = 2
                //         }
                //     },
                //     color
                // );

                // shapeKit.SetDrawMode(DrawMode.Normal);
                // shapeKit.AddLine(ref vh,
                //     ref positions,
                //     new LineProperties() {
                //         closed = true,
                //         weight = lineWeight,
                //         capType = LineCapTypes.Round,
                //         lineType = LineType.Center,
                //         roundingData = new PointListProperties() {
                //             roundingDistance = roundingDistance,
                //             maxAngle = maxAngle,
                //             rounding = new RoundingResolution() {
                //                 resolutionType = ResolutionType.Calculated,
                //                 maxDistance = 4f
                //             }
                //         },
                //         roundedCapResolution = new RoundingResolution() {
                //             resolutionType = ResolutionType.Calculated,
                //             maxDistance = 2
                //         }
                //     },
                //     Color.red
                // );
                // PointsGenerator.SetPointsStar(ref positions, new PointListStarData() {
                //     width = 50, //pixelRect.width,
                //     height = 50, // pixelRect.height,
                //     spikeAmount = spikeAmount,
                //     resolution = 5,
                //     length = length,
                //     center = new float2(pixelRect.width * 0.5f, -pixelRect.height * 0.5f),
                // });
                // shapeKit.AddPolygon(
                //     ref vh,
                //     ref positions,
                //     new PolygonProperties() {
                //         polygonCenterType = PolygonCenterTypes.Calculated,
                //     },
                //     new PointListProperties() {
                //         roundingDistance = roundingDistance,
                //         rounding = new RoundingResolution() {
                //             maxDistance = 1,
                //             resolutionType = ResolutionType.Calculated
                //         }
                //     },
                //     default,
                //     Color.red
                // );

                // PointsGenerator.SetPoints(ref positions, new PointListGeneratorData() {
                //     pointListGeneratorType = PointListGeneratorType.Round,
                //     width = 180,
                //     height = 180,
                //     resolution = ShapeKit.ResolveEllipseResolution(pixelRect.size * 0.5f, default),
                //     length = 0.5f,
                //     direction = 1f,
                //     startOffset = 0f, // base angle *0.5f
                //     centerPoint = false,
                //     center = new float2(90, -90),
                //     skipLastPosition = true,
                //     endRadius = 0,
                // });
                // shapeKit.AddLine(ref vh,
                //     ref positions,
                //     new LineProperties() {
                //         closed = false,
                //         weight = 15f,
                //         capType = LineCapTypes.Round,
                //         lineType = LineType.Center,
                //         roundedCapResolution = new RoundingResolution() {
                //             resolutionType = ResolutionType.Calculated,
                //             maxDistance = 2
                //         }
                //     },
                //     default,
                //     Color.blue,
                //     ref pointsData
                // );
                //
                // positions.Dispose();

            }
        }

        public static void DrawEllipse(ref UIVertexHelper vh, Rect pixelRect, Color fillColor) {

            EllipseProperties ellipseProperties = new EllipseProperties();

            EdgeGradientData edgeGradientData = new EdgeGradientData();

            AntiAliasingProperties antiAliasingProperties = new AntiAliasingProperties() {
                AntiAliasing = 1.25f
            };

            if (antiAliasingProperties.Adjusted > 0.0f) {
                edgeGradientData.SetActiveData(1.0f, 0.0f, antiAliasingProperties.Adjusted);
            }
            else {
                edgeGradientData.Reset();
            }

            using (ShapeKit shapeKit = new ShapeKit(Allocator.Temp)) {
                shapeKit.AddCircle(ref vh, pixelRect, ellipseProperties, fillColor);
            }
        }

        public static void DrawRect(ref UIVertexHelper vh, Rect pixelRect, Color fillColor) {

            EdgeGradientData edgeGradientData = new EdgeGradientData();

            AntiAliasingProperties antiAliasingProperties = new AntiAliasingProperties() {
                AntiAliasing = 1.25f
            };

            if (antiAliasingProperties.Adjusted > 0.0f) {
                edgeGradientData.SetActiveData(1.0f, 0.0f, antiAliasingProperties.Adjusted);
            }
            else {
                edgeGradientData.Reset();
            }

            using (ShapeKit shapeKit = new ShapeKit(Allocator.Temp)) {

                edgeGradientData.SetActiveData(1.0f, 10.0f, antiAliasingProperties.Adjusted);

                shapeKit.AddRoundedRectOutline(
                    ref vh,
                    new Vector2(pixelRect.center.x, -pixelRect.center.y),
                    pixelRect.width * 2f,
                    pixelRect.height,
                    new OutlineProperties() {type = LineType.Outer, weight = 3f},
                    new CornerProperties() {
                        bottomLeft = Corner.Round(55f)
                    },
                    Color.yellow
                );
            }
        }

        public static void DrawPolygon(ref UIVertexHelper vh, Rect pixelRect, Color fillColor) {

            AntiAliasingProperties antiAliasingProperties = new AntiAliasingProperties() {
                AntiAliasing = 1.25f
            };

            antiAliasingProperties.UpdateAdjusted(s_Scale);

            ShadowsProperties shadowProperties = new ShadowsProperties() {
                ShowShadows = true,
                ShowShape = true,
                Angle = 0,
                Distance = 0,
                Shadows = new[] {
                    new ShadowDescription() {
                        Color = Color.black,
                        Offset = default,
                        Size = 10f,
                        Softness = 0.5f
                    }
                }
            };

            shadowProperties.UpdateAdjusted();

            PolygonProperties polygonProperties = new PolygonProperties() {
                polygonCenterType = PolygonCenterTypes.Calculated
            };

            EdgeGradientData edgeGradientData = new EdgeGradientData();

            if (antiAliasingProperties.Adjusted > 0.0f) {
                edgeGradientData.SetActiveData(1.0f, 0.0f, antiAliasingProperties.Adjusted);
            }
            else {
                edgeGradientData.Reset();
            }

            PointListGeneratorData generatorData = new PointListGeneratorData() {
                pointListGeneratorType = PointListGeneratorType.Round,
                center = default,
                width = 50f,
                height = 45f,
                startOffset = 0,
                length = 1,
                endRadius = 0,
                resolution = 3,
                skipLastPosition = true,
                centerPoint = false,
            };

            PointListProperties pointListProperties = new PointListProperties() {
                maxAngle = 0,
                roundingDistance = 8f,
                rounding = new RoundingResolution() {
                    fixedResolution = 2,
                    resolutionType = ResolutionType.Fixed
                },

            };
            // pointListProperties.SetPoints();
            // polygonProperties.UpdateAdjusted(pointListProperties);
            // Polygons.AddPolygon(
            //     ref vh,
            //     polygonProperties,
            //     pointListProperties,
            //     new Vector2(150, -150),
            //     Color.white,
            //     default,
            //     ref pointData,
            //     edgeGradientData
            // );

        }

        public static void DrawArc(ref UIVertexHelper vh, Rect pixelRect, Color fillColor) {
            //
            // EllipseProperties ellipseProperties = new EllipseProperties() {
            //     fitting = EllipseFitting.UniformInner,
            //     baseAngle = 1.76f,
            //     resolution = new CornerResolution() {
            //         maxDistance = 5f,
            //         resolutionType = ResolutionType.Calculated
            //     }
            // };
            //
            // ArcProperties arcProperties = new ArcProperties() {
            //     Direction = ArcDirection.Centered,
            //     Length = 0.48f
            // };
            //
            // LineProperties lineProperties = new LineProperties() {
            //     LineCap = LineCapTypes.Close,
            //     Closed = false,
            //     RoundedCapResolution = new RoundingProperties()
            // };
            //
            // PointListProperties pointListProperties = new PointListProperties();
            //
            // PointsData pointsData = new PointsData();
            //
            // OutlineProperties outlineProperties = new OutlineProperties() {
            //     type = LineType.Inner,
            //     weight = 5f
            // };
            //
            // ShadowsProperties shadowProperties = new ShadowsProperties() {
            //     ShowShadows = true,
            //     ShowShape = true,
            //     Angle = 0,
            //     Distance = 0,
            //     Shadows = new[] {
            //         new ShadowDescription() {
            //             Color = Color.black,
            //             Offset = default,
            //             Size = 10f,
            //             Softness = 0.5f
            //         }
            //     }
            // };
            //
            // AntiAliasingProperties antiAliasingProperties = new AntiAliasingProperties() {
            //     AntiAliasing = 1.25f
            // };
            //
            // UnitPositionData unitPositionData = new UnitPositionData();
            // EdgeGradientData edgeGradientData = default;
            // Vector2 radius = Vector2.one;
            //
            // shadowProperties.UpdateAdjusted();
            //
            // GeoUtils.SetRadius(ref radius, pixelRect.width, pixelRect.height, ellipseProperties);
            //
            // pointListProperties.GeneratorData.Width = radius.x * 2.0f;
            // pointListProperties.GeneratorData.Height = radius.y * 2.0f;
            //
            // // ellipseProperties.UpdateAdjusted(radius, outlineProperties.GetOuterDistance());
            // //
            // // arcProperties.UpdateAdjusted(ellipseProperties.AdjustedResolution, ellipseProperties.baseAngle);
            // //
            // // antiAliasingProperties.UpdateAdjusted(s_Scale);
            // //
            // // pointListProperties.GeneratorData.Resolution = ellipseProperties.AdjustedResolution * 2;
            // // pointListProperties.GeneratorData.Length = arcProperties.Length;
            //
            // switch (arcProperties.Direction) {
            //     case ArcDirection.Forward:
            //         pointListProperties.GeneratorData.Direction = 1.0f;
            //         pointListProperties.GeneratorData.FloatStartOffset = ellipseProperties.baseAngle * 0.5f;
            //         break;
            //
            //     case ArcDirection.Centered:
            //         pointListProperties.GeneratorData.Direction = -1.0f;
            //         pointListProperties.GeneratorData.FloatStartOffset = ellipseProperties.baseAngle * 0.5f + (arcProperties.Length * 0.5f);
            //         break;
            //
            //     case ArcDirection.Backward:
            //         pointListProperties.GeneratorData.Direction = -1.0f;
            //         pointListProperties.GeneratorData.FloatStartOffset = ellipseProperties.baseAngle * 0.5f;
            //         break;
            // }
            //
            // pixelRect.center = new Vector2(pixelRect.center.x, -pixelRect.center.y);
            //
            // // shadows
            // if (shadowProperties.ShadowsEnabled) {
            //     if (antiAliasingProperties.Adjusted > 0.0f) {
            //         edgeGradientData.SetActiveData(1.0f, 0.0f, antiAliasingProperties.Adjusted);
            //     }
            //     else {
            //         edgeGradientData.Reset();
            //     }
            //
            //     // use segment if LineWeight is overshooting the center
            //     if (
            //         (
            //             outlineProperties.type == LineType.Center ||
            //             outlineProperties.type == LineType.Inner
            //         ) &&
            //         (
            //             radius.x + ShapeKit.GetInnerDistance(outlineProperties) < 0.0f ||
            //             radius.y + ShapeKit.GetInnerDistance(outlineProperties) < 0.0f
            //         )
            //         ) {
            //         if (outlineProperties.type == LineType.Center) {
            //             radius *= 2.0f;
            //         }
            //
            //         for (int i = 0; i < shadowProperties.Shadows.Length; i++) {
            //             edgeGradientData.SetActiveData(
            //                 1.0f - shadowProperties.Shadows[i].Softness,
            //                 shadowProperties.Shadows[i].Size,
            //                 antiAliasingProperties.Adjusted
            //             );
            //             //
            //             // Arcs.AddSegment(
            //             //     ref vh,
            //             //     shadowProperties.GetCenterOffset(pixelRect.center, i),
            //             //     radius,
            //             //     ellipseProperties,
            //             //     arcProperties,
            //             //     shadowProperties.Shadows[i].Color,
            //             //     GeoUtils.ZeroV2,
            //             //     ref unitPositionData,
            //             //     edgeGradientData
            //             // );
            //         }
            //
            //     }
            //     else {
            //         // for (int i = 0; i < shadowProperties.Shadows.Length; i++) {
            //         //     edgeGradientData.SetActiveData(
            //         //         1.0f - shadowProperties.Shadows[i].Softness,
            //         //         shadowProperties.Shadows[i].Size,
            //         //         antiAliasingProperties.Adjusted
            //         //     );
            //         //
            //         //     if (lineProperties.LineCap == LineCapTypes.Close) {
            //         //         Arcs.AddArcRing(
            //         //             ref vh,
            //         //             shadowProperties.GetCenterOffset(pixelRect.center, i),
            //         //             radius,
            //         //             ellipseProperties,
            //         //             arcProperties,
            //         //             outlineProperties,
            //         //             shadowProperties.Shadows[i].Color,
            //         //             default,
            //         //             ref unitPositionData,
            //         //             edgeGradientData
            //         //         );
            //         //     }
            //         //     else {
            //         //         Lines.AddLine(
            //         //             ref vh,
            //         //             lineProperties,
            //         //             pointListProperties,
            //         //             shadowProperties.GetCenterOffset(pixelRect.center, i),
            //         //             outlineProperties,
            //         //             shadowProperties.Shadows[i].Color,
            //         //             ref pointsData,
            //         //             edgeGradientData
            //         //         );
            //         //     }
            //         // }
            //
            //     }
            // }
            //
            // // fill
            // if (shadowProperties.ShowShape) {
            //     if (antiAliasingProperties.Adjusted > 0.0f) {
            //         edgeGradientData.SetActiveData(1.0f, 0.0f, antiAliasingProperties.Adjusted);
            //     }
            //     else {
            //         edgeGradientData.Reset();
            //     }
            //
            //     // use segment if LineWeight is overshooting the center
            //     if (
            //         (
            //             outlineProperties.type == LineType.Center ||
            //             outlineProperties.type == LineType.Inner
            //         ) &&
            //         (
            //             radius.x + outlineProperties.GetInnerDistance() < 0.0f ||
            //             radius.y + outlineProperties.GetInnerDistance() < 0.0f
            //         )
            //         ) {
            //         if (outlineProperties.type == LineType.Center) {
            //             radius.x *= 2.0f;
            //             radius.y *= 2.0f;
            //         }
            //
            //         // Arcs.AddSegment(
            //         //     ref vh,
            //         //     pixelRect.center,
            //         //     radius,
            //         //     ellipseProperties,
            //         //     arcProperties,
            //         //     fillColor,
            //         //     GeoUtils.ZeroV2,
            //         //     ref unitPositionData,
            //         //     edgeGradientData
            //         // );
            //     }
            //     else {
            //         if (lineProperties.LineCap == LineCapTypes.Close) {
            //             Arcs.AddArcRing(
            //                 ref vh,
            //                 pixelRect.center,
            //                 radius,
            //                 ellipseProperties,
            //                 arcProperties,
            //                 outlineProperties,
            //                 fillColor,
            //                 GeoUtils.ZeroV2,
            //                 ref unitPositionData,
            //                 edgeGradientData
            //             );
            //         }
            //         else {
            //             Lines.AddLine(
            //                 ref vh,
            //                 lineProperties,
            //                 pointListProperties,
            //                 pixelRect.center,
            //                 outlineProperties,
            //                 fillColor,
            //                 ref pointsData,
            //                 edgeGradientData
            //             );
            //         }
            // }
            // }
        }

        public Color color = Color.blue;

        public void OnGUI() {
#if UNITY_EDITOR

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Color");
            color = EditorGUILayout.ColorField(color);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Shadow Softness");
            shadowSoftness = EditorGUILayout.FloatField(shadowSoftness);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Shadow Size");
            shadowSize = EditorGUILayout.FloatField(shadowSize);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Line Weight");
            lineWeight = EditorGUILayout.Slider(lineWeight, 0.5f, 20f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Spike Amount");
            spikeAmount = EditorGUILayout.Slider(spikeAmount, 0, 10);
            EditorGUILayout.EndHorizontal();

            roundingDistance = SliderGUI("Rounding Distance", roundingDistance, 0, 50);
            length = SliderGUI("Length %", length, 0, 5f);
            maxAngle = SliderGUI("Max Angle", maxAngle, 0, 2 * Mathf.PI);
            EditorGUILayout.EndVertical();
#endif
        }

        public float SliderGUI(string label, float value, float min, float max) {
            #if UNITY_EDITOR
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            value = EditorGUILayout.Slider(value, min, max);
            EditorGUILayout.EndHorizontal();
            #endif
            return value;
        }

    }

}