using UIForia;
using UnityEngine;

namespace UI.LoginFlow {

    [Template("Klang/Seed/UIForia/LoginFlow/LoginScreen/LoginScreen.xml")]
    public class LoginScreen : UIElement {

        public string username;
        public string password;

        public void Login() {
            Debug.Log("Login");
        }

        public void CreateAccount() {
            Debug.Log("Go To Route");
            view.Application.Router.GoTo("/create_account"); //?username=matt?password=password
        }

    }

}