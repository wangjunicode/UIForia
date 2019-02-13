using System.Collections.Generic;
using Klang.Seed.DataTypes;
using UIForia;
using UIForia.Util;
using UnityEngine;

namespace UI {

    [Template("Klang/Seed/UIForia/CharacterInfoPanel/CharacterInfoPanel.xml")]
    public class CharacterInfoPanel : UIElement {

        public CharacterData characterData;

        public override void OnCreate() {
            characterData = new CharacterData();
            characterData.skills = new RepeatableList<CharacterSkill>();
            characterData.skills.Add(new CharacterSkill("Agriculture", Random.Range(0f, 1f), Random.Range(0, 5)));
            characterData.skills.Add(new CharacterSkill("Construction", Random.Range(0f, 1f), Random.Range(0, 5)));
            characterData.skills.Add(new CharacterSkill("Consume", Random.Range(0f, 1f), Random.Range(0, 5)));
            characterData.skills.Add(new CharacterSkill("Cooking", Random.Range(0f, 1f), Random.Range(0, 5)));
            characterData.skills.Add(new CharacterSkill("Crafting", Random.Range(0f, 1f), Random.Range(0, 5)));
            characterData.skills.Add(new CharacterSkill("Geology", Random.Range(0f, 1f), Random.Range(0, 5)));
            characterData.skills.Add(new CharacterSkill("Medical", Random.Range(0f, 1f), Random.Range(0, 5)));
            characterData.skills.Add(new CharacterSkill("Physical Labor", Random.Range(0f, 1f), Random.Range(0, 5)));
        }

        public void ShowMenuPanel(string menuName) {
            switch (menuName) {
                case "skills":
                    Application.RoutingSystem.FindRouterInHierarchy(this).GoTo("/skills");
                    break;
                case "gear":
                    Application.RoutingSystem.FindRouterInHierarchy(this).GoTo("/gear");
                    break;
                case "mood":
                    Application.RoutingSystem.FindRouterInHierarchy(this).GoTo("/mood");
                    break;
                case "schedule":
                    Application.RoutingSystem.FindRouterInHierarchy(this).GoTo("/schedule");
                    break;
                case "health":
                    Application.RoutingSystem.FindRouterInHierarchy(this).GoTo("/health");
                    break;
                case "social":
                    Application.RoutingSystem.FindRouterInHierarchy(this).GoTo("/social");
                    break;
                
            }
        }
     

    }

}