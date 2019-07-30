using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;
using UnityEngine;

namespace Demo {

    public struct EntityId {

        public long id;

    }

    [Template("CharacterList/CharacterList.xml")]
    public class CharacterList : UIElement {

        public const string DirectControlIcon_On = "Client/UI/SpriteSettingOverwrite/ui_direct_control_on";
        public const string DirectControlIcon_Off = "Client/UI/SpriteSettingOverwrite/ui_direct_control_off";

        private EntityId selectedCharacterEntityId;

        public Color HealthColor;
        public Color MoodColor;

        public override void OnCreate() {
            ColorUtility.TryParseHtmlString("#00D175", out HealthColor);
            ColorUtility.TryParseHtmlString("#9873E6", out MoodColor);
        }

        private void OnSelected(int selectedCharacter) { }

        public void ToggleDirectControl(int characterId, MouseInputEvent evt) {
            evt.StopPropagation();
        }

        public void OnClickCharacter(MouseInputEvent evt, EntityId id) {
            evt.StopPropagation();
            SelectCharacter(id);
        }

        private void SelectCharacter(EntityId id) {
            selectedCharacterEntityId = id;
        }

        public string SelectedStyle(EntityId id) {
            if (id.id == selectedCharacterEntityId.id) {
                return "selected";
            }

            return string.Empty;
        }

    }

}