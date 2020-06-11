using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ThisOtherThing.UI.ShapeUtils;
using UIForia.Graphics.ShapeKit;

namespace ThisOtherThing.UI.Shapes
{
	[AddComponentMenu("UI/Shapes/Sector", 50)]
	public class Sector : MaskableGraphic, IShape
	{

		public EllipseProperties EllipseProperties =
			new EllipseProperties();

		public ArcProperties ArcProperties = 
			new ArcProperties();

		public ShadowsProperties ShadowProperties = new ShadowsProperties();

		public AntiAliasingProperties AntiAliasingProperties = 
			new AntiAliasingProperties();

		UnitPositionData unitPositionData;
		EdgeGradientData edgeGradientData;
		Vector2 radius = Vector2.one;

		Rect pixelRect;
		public Color fillColor;
		
		public void ForceMeshUpdate()
		{
			SetVerticesDirty();
			SetMaterialDirty();
		}

		#if UNITY_EDITOR
		protected override void OnValidate()
		{
			// EllipseProperties.OnCheck();
			//AntiAliasingProperties.OnCheck();

			ForceMeshUpdate();
		}
		#endif

		protected void OnPopulateMesh(UIVertexHelper vh)	{
			vh.Clear();

			pixelRect = RectTransformUtility.PixelAdjustRect(rectTransform, canvas);
			//
			// ShapeUtils.Ellipses.SetRadius(
			// 	ref radius,
			// 	pixelRect.width,
			// 	pixelRect.height,
			// 	EllipseProperties
			// );

			//EllipseProperties.UpdateAdjusted(radius, 0.0f);
			//ArcProperties.UpdateAdjusted(EllipseProperties.AdjustedResolution, EllipseProperties.baseAngle);
			AntiAliasingProperties.UpdateAdjusted(canvas.scaleFactor);
			ShadowProperties.UpdateAdjusted();

			// shadows
			if (ShadowProperties.ShadowsEnabled)
			{
				for (int i = 0; i < ShadowProperties.Shadows.Length; i++)
				{
					edgeGradientData.SetActiveData(
						1.0f - ShadowProperties.Shadows[i].Softness,
						ShadowProperties.Shadows[i].Size,
						AntiAliasingProperties.Adjusted
					);
					//
					// ShapeUtils.Arcs.AddSegment(
					// 	ref vh,
					// 	ShadowProperties.GetCenterOffset(pixelRect.center, i),
					// 	radius,
					// 	EllipseProperties,
					// 	ArcProperties,
					// 	ShadowProperties.Shadows[i].Color,
					// 	GeoUtils.ZeroV2,
					// 	ref unitPositionData,
					// 	edgeGradientData
					// );
				}
			}

			// fill
			if (ShadowProperties.ShowShape)
			{
				if (AntiAliasingProperties.Adjusted > 0.0f)
				{
					edgeGradientData.SetActiveData(
						1.0f,
						0.0f,
						AntiAliasingProperties.Adjusted
					);
				}
				else
				{
					edgeGradientData.Reset();
				}
				//
				// ShapeUtils.Arcs.AddSegment(
				// 	ref vh,
				// 	(Vector3)pixelRect.center,
				// 	radius,
				// 	EllipseProperties,
				// 	ArcProperties,
				// 	fillColor,
				// 	GeoUtils.ZeroV2,
				// 	ref unitPositionData,
				// 	edgeGradientData
				// );
			}
		}
	}
}
