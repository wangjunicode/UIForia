using UnityEngine;

public class ColorSelector : UIElement {

    [Prop] public Color[] colors;
    [Prop] public int selectedIndex;

    public ObservedProperty<float> hueValue;
    public ObservedProperty<string> title;

    public void OnInitialize() {
        title.Value = "new title";
    }

    private void OnHueChanged(float hueValue) { }

    public static string Template = @"
        <Container className='$styles.static.container'>
            <Text label='{$title}'/>
            <ColorSwatchGrid colors='$colors'/>
            <HueSlider value='$hueValue' onValueChanged='$OnHueChanged'/>
        </Container>
    ";
}