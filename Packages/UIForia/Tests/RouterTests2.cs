using NUnit.Framework;
using UIForia.Routing2;

[TestFixture]
    public class RouterTests2 {

        [Test]
        public void TestRouteMatching() {
            Router router = new Router(0, "test");
            router.AddRoute("/users");
            router.AddRoute("/users2");
            
            router.GoTo("/users2");
            Assert.AreEqual("/users2", router.ActiveRoute.path);
            
            router.GoTo("/users");
            Assert.AreEqual("/users", router.ActiveRoute.path);
        }

    }
