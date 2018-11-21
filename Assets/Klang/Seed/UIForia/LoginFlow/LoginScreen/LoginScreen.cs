using UIForia;

namespace UI.LoginFlow {

    [Template("Klang/Seed/UIForia/LoginFlow/LoginScreen/LoginScreen.xml")]
    public class LoginScreen : UIElement {

        public string username;
        public string password;

        public void Login() {
        }

        public void CreateAccount() {
            view.Application.Router.GoTo("/create_account"); //?username=matt?password=password
        }

    }

}