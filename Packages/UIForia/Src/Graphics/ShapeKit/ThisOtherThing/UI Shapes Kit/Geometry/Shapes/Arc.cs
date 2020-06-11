// using ThisOtherThing.UI.ShapeUtils;
// using UIForia.Graphics.ShapeKit;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace ThisOtherThing.UI.Shapes
// {
// 	[AddComponentMenu("UI/Shapes/Arc", 50)]
// 	public class Arc : MaskableGraphic, IShape {
//
// 		public Color fillColor;
// 		public EllipseProperties EllipseProperties =
// 			new EllipseProperties();
//
// 		public ArcProperties ArcProperties = 
// 			new ArcProperties();
//
// 		public LineProperties LineProperties =
// 			new LineProperties();
// 		public PointListProperties PointListProperties =
// 				new PointListProperties();
// 		PointsData PointsData = new PointsData();
//
// 		public OutlineProperties OutlineProperties = 
// 			new OutlineProperties();
//
// 		public ShadowsProperties ShadowProperties = new ShadowsProperties();
//
// 		public AntiAliasingProperties AntiAliasingProperties = 
// 			new AntiAliasingProperties();
//
//
// 		UnitPositionData unitPositionData;
// 		EdgeGradientData edgeGradientData;
// 		Vector2 radius = Vector2.one;
//
// 		protected override void OnEnable() {
// 			PointListProperties.GeneratorData.pointListGeneratorType =
// 				PointListGeneratorType.Round;
//
// 			PointListProperties.GeneratorData.Center.x = 0.0f;
// 			PointListProperties.GeneratorData.Center.y = 0.0f;
//
// 			base.OnEnable();
// 		}
//
// 		public void ForceMeshUpdate()
// 		{
// 			PointListProperties.GeneratorData.NeedsUpdate = true;
// 			PointsData.NeedsUpdate = true;
//
// 			SetVerticesDirty();
// 			SetMaterialDirty();
// 		}
//
// 		#if UNITY_EDITOR
// 		protected override void OnValidate()
// 		{
// 			// EllipseProperties.OnCheck();
// 			// OutlineProperties.OnCheck();
// 			//AntiAliasingProperties.OnCheck();
//
// 			ForceMeshUpdate();
// 		}
// 		#endif
//
// 		protected void OnPopulateMesh(UIVertexHelper vh)	{
// 			vh.Clear();
//
// 			Rect pixelRect = RectTransformUtility.PixelAdjustRect(rectTransform, canvas);
//
// 			ShadowProperties.UpdateAdjusted();
//
// 			// Ellipses.SetRadius(
// 			// 	ref radius,
// 			// 	pixelRect.width,
// 			// 	pixelRect.height,
// 			// 	EllipseProperties
// 			// );
//
// 			PointListProperties.GeneratorData.Width = radius.x * 2.0f;
// 			PointListProperties.GeneratorData.Height = radius.y * 2.0f;
//
// 			//EllipseProperties.UpdateAdjusted(radius, OutlineProperties.GetOuterDistance());
// 			//ArcProperties.UpdateAdjusted(EllipseProperties.AdjustedResolution, EllipseProperties.baseAngle);
// 			AntiAliasingProperties.UpdateAdjusted(canvas.scaleFactor);
//
// 			// PointListProperties.GeneratorData.Resolution = EllipseProperties.AdjustedResolution * 2;
// 			PointListProperties.GeneratorData.Length = ArcProperties.Length;
//
// 			switch (ArcProperties.Direction) {
// 				case ArcDirection.Forward:
// 					PointListProperties.GeneratorData.Direction = 1.0f;
// 					PointListProperties.GeneratorData.FloatStartOffset = EllipseProperties.baseAngle * 0.5f;
// 					break;
//
// 				case ArcDirection.Centered:
// 					PointListProperties.GeneratorData.Direction = -1.0f;
// 					PointListProperties.GeneratorData.FloatStartOffset = EllipseProperties.baseAngle * 0.5f + (ArcProperties.Length * 0.5f);
// 					break;
//
// 				case ArcDirection.Backward:
// 					PointListProperties.GeneratorData.Direction = -1.0f;
// 					PointListProperties.GeneratorData.FloatStartOffset = EllipseProperties.baseAngle * 0.5f;
// 					break;
// 			}
//
// 			// shadows
// 			if (ShadowProperties.ShadowsEnabled)
// 			{
// 				if (AntiAliasingProperties.Adjusted > 0.0f)
// 				{
// 					edgeGradientData.SetActiveData(
// 						1.0f,
// 						0.0f,
// 						AntiAliasingProperties.Adjusted
// 					);
// 				}
// 				else
// 				{
// 					edgeGradientData.Reset();
// 				}
//
// 				// use segment if LineWeight is overshooting the center
// 				if (
// 					(
// 						OutlineProperties.type == LineType.Center ||
// 						OutlineProperties.type == LineType.Inner
// 					) &&
// 					(
// 						radius.x + OutlineProperties.GetInnerDistance() < 0.0f ||
// 						radius.y + OutlineProperties.GetInnerDistance() < 0.0f
// 					)
// 				) {
// 					if (OutlineProperties.type == LineType.Center)
// 					{
// 						radius *= 2.0f;
// 					}
//
// 					for (int i = 0; i < ShadowProperties.Shadows.Length; i++)
// 					{
// 						edgeGradientData.SetActiveData(
// 							1.0f - ShadowProperties.Shadows[i].Softness,
// 							ShadowProperties.Shadows[i].Size,
// 							AntiAliasingProperties.Adjusted
// 						);
//
// 						Arcs.AddSegment(
// 							ref vh,
// 							ShadowProperties.GetCenterOffset(pixelRect.center, i),
// 							radius,
// 							EllipseProperties,
// 							ArcProperties,
// 							ShadowProperties.Shadows[i].Color,
// 							GeoUtils.ZeroV2,
// 							ref unitPositionData,
// 							edgeGradientData
// 						);
// 					}
//
// 				}
// 				else
// 				{
// 					for (int i = 0; i < ShadowProperties.Shadows.Length; i++)
// 					{
// 						edgeGradientData.SetActiveData(
// 							1.0f - ShadowProperties.Shadows[i].Softness,
// 							ShadowProperties.Shadows[i].Size,
// 							AntiAliasingProperties.Adjusted
// 						);
//
// 						if (LineProperties.LineCap == LineCapTypes.Close)
// 						{
// 							Arcs.AddArcRing(
// 								ref vh,
// 								ShadowProperties.GetCenterOffset(pixelRect.center, i),
// 								radius,
// 								EllipseProperties,
// 								ArcProperties,
// 								OutlineProperties,
// 								ShadowProperties.Shadows[i].Color,
// 								GeoUtils.ZeroV2,
// 								ref unitPositionData,
// 								edgeGradientData
// 							);
// 						}
// 						else
// 						{
// 							Lines.AddLine(
// 								ref vh,
// 								LineProperties,
// 								PointListProperties,
// 								ShadowProperties.GetCenterOffset(pixelRect.center, i),
// 								OutlineProperties,
// 								ShadowProperties.Shadows[i].Color,
// 								GeoUtils.ZeroV2,
// 								ref PointsData,
// 								edgeGradientData
// 							);
// 						}
// 					}
//
// 				}
// 			}
//
// 			// fill
// 			if (ShadowProperties.ShowShape)
// 			{
// 				if (AntiAliasingProperties.Adjusted > 0.0f)
// 				{
// 					edgeGradientData.SetActiveData(
// 						1.0f,
// 						0.0f,
// 						AntiAliasingProperties.Adjusted
// 					);
// 				}
// 				else
// 				{
// 					edgeGradientData.Reset();
// 				}
//
// 				// use segment if LineWeight is overshooting the center
// 				if (
// 					(
// 						OutlineProperties.type == LineType.Center ||
// 						OutlineProperties.type == LineType.Inner
// 					) &&
// 					(
// 						radius.x + OutlineProperties.GetInnerDistance() < 0.0f ||
// 						radius.y + OutlineProperties.GetInnerDistance() < 0.0f
// 					)
//
// 				) {
// 					if (OutlineProperties.type == LineType.Center)
// 					{
// 						radius.x *= 2.0f;
// 						radius.y *= 2.0f;
// 					}
//
// 					Arcs.AddSegment(
// 						ref vh,
// 						pixelRect.center,
// 						radius,
// 						EllipseProperties,
// 						ArcProperties,
// 						fillColor,
// 						GeoUtils.ZeroV2,
// 						ref unitPositionData,
// 						edgeGradientData
// 					);
// 				}
// 				else
// 				{
// 					if (LineProperties.LineCap == LineCapTypes.Close)
// 					{
// 						Arcs.AddArcRing(
// 							ref vh,
// 							pixelRect.center,
// 							radius,
// 							EllipseProperties,
// 							ArcProperties,
// 							OutlineProperties,
// 							fillColor,
// 							GeoUtils.ZeroV2,
// 							ref unitPositionData,
// 							edgeGradientData
// 						);
// 					}
// 					else
// 					{
// 						Lines.AddLine(
// 							ref vh,
// 							LineProperties,
// 							PointListProperties,
// 							pixelRect.center,
// 							OutlineProperties,
// 							fillColor,
// 							GeoUtils.ZeroV2,
// 							ref PointsData,
// 							edgeGradientData
// 						);
// 					}
// 				}
// 			}
//
//
// 		}
// 	}
//
// }