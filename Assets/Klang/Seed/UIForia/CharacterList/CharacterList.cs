using System.Collections.Generic;
using Klang.Seed.DataTypes;
using UIForia;
using UIForia.Input;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UI {

    [Template("Klang/Seed/UIForia/CharacterList/CharacterList.xml")]
    public class CharacterList : UIElement {

        public RepeatableList<CharacterData> characters;

        public const string DirectControlIcon_On = "SpriteSettingOverwrite/ui_direct_control_on";
        public const string DirectControlIcon_Off = "SpriteSettingOverwrite/ui_direct_control_off";
        
        private UIElement selectedCharacter;
        
        public override void OnCreate() {
            characters = new RepeatableList<CharacterData>();
            characters.Add(new CharacterData() {
                name = "Matt",
                status = "Sleeping on the ground",
                iconUrl = "Sprites/Expressions/other/adorable",
                statusIconUrl = "SpriteSettingOverwrite/ui_icon_health_infection",
                health =  0.785f,
                mood = 1f
            });
            
            characters.Add(new CharacterData() {
                name = "Cracker",
                status = "Sleeping on the ground",
                iconUrl = "Sprites/Expressions/other/adorable",
                statusIconUrl = "SpriteSettingOverwrite/ui_icon_eye",
                health =  1f,
                mood = 1f
            });
            
        }

        public void ToggleDirectControl(CharacterData characterData, MouseInputEvent evt) {
            characterData.isInDirectControl = !characterData.isInDirectControl;
            evt.StopPropagation();
        }

        public void SelectCharacter(UIElement element) {
           selectedCharacter?.style.SetBackgroundColor(ColorUtil.UnsetValue, StyleState.Normal);
           selectedCharacter = element;
           selectedCharacter?.style.SetBackgroundColor(Color.blue, StyleState.Normal);
        }

    }

}