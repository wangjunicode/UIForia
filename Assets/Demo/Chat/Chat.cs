using System;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;

namespace Demo {

    [Template("Demo/Chat/Chat.xml")]
    public class Chat : UIElement {

        public RepeatableList<ChatMessage> messages;

        public string message;

        public Action onClose;

        public override void OnCreate() {
            SetEnabled(false);
            onClose = () => SetEnabled(false);
            CreateDummyData();
        }

        // public override void HandleUIEvent(UIEvent evt) {
        //     if (evt is SubmitEvent) {
        //         messages.Add(new ChatMessage() {
        //             icon = "icon_2",
        //             date = "09/26/2019",
        //             text = message,
        //             sender = "You"
        //         });
        //
        //         message = "";
        //         application.RegisterAfterUpdateTask(new ScrollTask(FindById<ScrollView>("scrollview"), 1));
        //     }
        // }

        private void CreateDummyData() {
            messages = new RepeatableList<ChatMessage>() {
                new ChatMessage() {
                    icon = "icon_1",
                    date = "09/26/2019",
                    text = "hey maður!",
                    sender = "Svenson"
                },
                new ChatMessage() {    
                    icon = "icon_3",
                    date = "09/26/2019",
                    text = "hvað er að frétta?",
                    sender = "Vondi"
                }
            };
        }
    }
}
