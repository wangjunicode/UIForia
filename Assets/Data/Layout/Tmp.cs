using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;
using UnityEngine;

[Template("Data/Layout/Tmp.xml")]
public class Tmp : UIElement {

  
            public string output_NoParams;
            public string output_EvtParam;
            public string output_MixedParams;
            public string output_NoEvtParam;

            public void HandleMouseClick_NoParams() {
                output_NoParams = "No Params Was Called";
            }

            public void HandleMouseClick_EvtParam(MouseInputEvent evt) {
                output_EvtParam = $"EvtParam was called {evt.MousePosition.x}, {evt.MousePosition.y}";
            }

            public void HandleMouseClick_MixedParams(MouseInputEvent evt, int param) {
                output_MixedParams = $"MixedParams was called {evt.MousePosition.x}, {evt.MousePosition.y} param = {param}";
                Debug.Log(output_MixedParams);
            }

            public void HandleMouseClick_NoEvtParam(string str, int param) {
                output_NoEvtParam = $"NoEvtParam was called str = {str} param = {param}";
                Debug.Log(output_NoEvtParam);
            }

            public float output_value;

            public void SetValue(float value) {
                output_value = value;
            }

            
}