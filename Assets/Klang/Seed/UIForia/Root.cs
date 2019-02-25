using UIForia;
using UIForia.Animation;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Routing2;

namespace UI {

    [Template(BasePath + "Root.xml")]
    public class Root : UIElement {

        public const string BasePath = "Klang/Seed/UIForia/";

        public override void OnCreate() {
            
            Router gameRouter = Application.RoutingSystem.FindRouter("game");
            
            gameRouter.AddTransition("/splash", "/login_flow", (elapsed) => {

                if (elapsed == 0) {
                    UIElement currentRouteRoot = FindFirstByType<SeedSplashScreen>();
                    UIElement targetRouteRoot = FindFirstByType<LoginFlow>();
                    
                    targetRouteRoot.SetEnabled(true);
                    currentRouteRoot.style.SetLayoutBehavior(LayoutBehavior.Ignored, StyleState.Normal);
                    currentRouteRoot.style.PlayAnimation(FadeOut());
//                    targetRouteRoot.style.PlayAnimation(FadeIn());
                }
                
                if (elapsed < 1f) {
                    return RouteTransitionState.Pending;
                }

                return RouteTransitionState.Completed;
            });
            
        }

        [OnMouseDown]
        public void OnClick() {
            UIElement element = FindFirstByType<SeedSplashScreen>();
            string variant = element.GetAttribute("variant");
            if (variant == null || variant == "0") {
                element.SetAttribute("variant", "1");
            }
            else if (variant == "1") {
                element.SetAttribute("variant", "2");
            }
            else {
                element.SetAttribute("variant", "0");
            }
        }
        
        private static StyleAnimation FadeOut() {
            AnimationOptions options = new AnimationOptions();
            options.duration = 1f;
            options.timingFunction = EasingFunction.CubicEaseOut;
            return new PropertyAnimation(StyleProperty.TransformPositionX(new TransformOffset(-1f, TransformUnit.ActualWidth)), options);
        }
        
        private static StyleAnimation FadeIn() {
            AnimationOptions options = new AnimationOptions();
            return new PropertyAnimation(StyleProperty.Opacity(1f), options);
        }
    }

}