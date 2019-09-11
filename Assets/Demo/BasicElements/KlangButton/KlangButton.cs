using SVGX;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Routing;
using UIForia.Systems;
using UIForia.UIInput;
using UnityEngine;
using Vertigo;

namespace Klang.Seed.Client.UI.UIForia {

    [Template("Demo/BasicElements/KlangButton/KlangButton.xml")]
    public class KlangButton : UIElement {

        public string path;

        public string label;
    }

    [Template("Demo/BasicElements/KlangButton/KlangLinkButton.xml")]
    public class KlangLinkButton : KlangButton {

        [OnMouseClick]
        public void OnClick(MouseInputEvent evt) {
            if (path == null) {
                return;
            }
            Router gameRouter = Application.RoutingSystem.FindRouterInHierarchy(this);
            if (gameRouter == null) {
                return;
            }

            evt.StopPropagation();
            gameRouter.GoTo(path);
        }
    }

    [Template("Demo/BasicElements/KlangButton/KlangLink.xml")]
    public class KlangLink : KlangButton, ISVGXPaintable {

        [OnMouseClick]
        public void OnClick(MouseInputEvent evt) {
            if (path == null) {
                return;
            }

            Router gameRouter = Application.RoutingSystem.FindRouterInHierarchy(this);
            if (gameRouter == null) {
                return;
            }

            evt.StopPropagation();
            gameRouter.GoTo(path);
        }

        public void Paint(VertigoContext ctx, in Matrix4x4 matrix) {
            throw new System.NotImplementedException();
        }

        public void Paint(ImmediateRenderContext ctx, in SVGXMatrix matrix) {
            ctx.SetTransform(matrix);
            SVGXRenderSystem.PaintElement(ctx, this);

            ctx.BeginPath();
            ctx.MoveTo(0, layoutResult.ActualHeight);
            ctx.LineTo(layoutResult.ActualWidth, layoutResult.ActualHeight);
            ctx.SetStrokeWidth(1f);
            ctx.SetStroke(style.TextColor);
            ctx.Stroke();
        }
    }    
    
    [Template("Demo/BasicElements/KlangButton/KlangLinkButton.xml")]
    public class KlangBackButton : KlangButton {

        [OnMouseClick]
        public void OnClick(MouseInputEvent evt) {

            Router gameRouter = Application.RoutingSystem.FindRouterInHierarchy(this);
            if (gameRouter == null) {
                return;
            }

            evt.StopPropagation();
            gameRouter.GoBack();
        }
    }

}