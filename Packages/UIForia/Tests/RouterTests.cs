using NUnit.Framework;
using Tests.Mocks;
using UIForia;

[TestFixture]
public class RouterTests {

    [Template(TemplateType.String, @"
    <UITemplate>
        <Contents>
            <Router path='/'/>
        </Contents>
    </UITemplate>
    ")]
    private class ParsersRouterRootThing : UIElement { }
    

    [Test]
    public void ParsersRouterRoot() {
        MockApplication app = new MockApplication(typeof(ParsersRouterRootThing));
        Assert.IsInstanceOf<UIRouterElement>(app.RootElement.GetChild(0));
        UIRouterElement router = (UIRouterElement)app.RootElement.GetChild(0);
        Assert.AreEqual("/", router.path);
    }  

    [Template(TemplateType.String, @"
    <UITemplate>
        <Contents>
            <Router path='/users'>
                <Route path='/options/:id'/>
                <Route path='/options2/:id'/>
                <UnmatchedRoute/>
            </Router>
        </Contents>
    </UITemplate>
    ")]
    private class ParsersRouterNestedThing : UIElement { }

    [Test]
    public void ParsesRouterWithSubRoutes() {
        MockApplication app = new MockApplication(typeof(ParsersRouterNestedThing));
        Assert.IsInstanceOf<UIRouterElement>(app.RootElement.GetChild(0));
        UIRouterElement router = (UIRouterElement)app.RootElement.GetChild(0);
        Assert.AreEqual("/users", router.path);
        Assert.AreEqual("/options/:id", router.FindByType<UIRouteElement>()[0].path);
        Assert.AreEqual("/options2/:id", router.FindByType<UIRouteElement>()[1].path);
        Assert.IsTrue(router.FindByType<UIRouteElement>()[0].isDisabled);
        Assert.IsTrue(router.FindByType<UIRouteElement>()[1].isDisabled);
        Assert.IsTrue(router.FindByType<UIRouteElement>()[2].isDisabled);
    }

    [Test]
    public void CallsEnterHandlerForMatchedRoute() {
        MockApplication app = new MockApplication(typeof(ParsersRouterNestedThing));
        UIRouterElement router = (UIRouterElement)app.RootElement.GetChild(0);
        bool didEnter = false;
        router.onRouteEnter += () => { didEnter = true; };
        app.Router.GoTo("/users/options/1");
        Assert.IsTrue(didEnter);
        Assert.IsTrue(router.FindByType<UIRouteElement>()[0].isEnabled);
        Assert.IsTrue(router.FindByType<UIRouteElement>()[1].isDisabled);
        Assert.IsTrue(router.FindByType<UIRouteElement>()[2].isDisabled);
    }
    
    [Test]
    public void CallsExitHandlerForMatchedRoute() {
        MockApplication app = new MockApplication(typeof(ParsersRouterNestedThing));
        UIRouterElement router = (UIRouterElement)app.RootElement.GetChild(0);
        bool didExit = false;
        router.onRouteExit += () => { didExit = true; };
        app.Router.GoTo("/users/options/1");
        app.Router.GoTo("/users/options2/4");
        Assert.IsTrue(didExit);
        Assert.IsTrue(router.FindByType<UIRouteElement>()[0].isDisabled);
        Assert.IsTrue(router.FindByType<UIRouteElement>()[1].isEnabled);
        Assert.IsTrue(router.FindByType<UIRouteElement>()[2].isDisabled);
    }

    [Test]
    public void MatchesUnmatchedRoute() {
        MockApplication app = new MockApplication(typeof(ParsersRouterNestedThing));
        UIRouterElement router = (UIRouterElement)app.RootElement.GetChild(0);
        app.Router.GoTo("/users/opt");
        Assert.IsTrue(router.FindByType<UIRouteElement>()[0].isDisabled);
        Assert.IsTrue(router.FindByType<UIRouteElement>()[1].isDisabled);
        Assert.IsTrue(router.FindByType<UIRouteElement>()[2].isEnabled);
    }
}