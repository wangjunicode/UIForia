using NUnit.Framework;
using Tests.Mocks;
using UIForia;
using UIForia.Routing;

[TestFixture]
public class RouterTests {

    [Template(TemplateType.String, @"
    <UITemplate>
        <Contents>
            <Router>
                <Route path=""'/user'""/>
            </Router>
        </Contents>
    </UITemplate>
    ")]
    private class ParsersRouterRootThing : UIElement { }

    [Test]
    public void ParsersRouterRoot() {
        MockApplication app = new MockApplication(typeof(ParsersRouterRootThing));
        Assert.IsInstanceOf<RouterElement>(app.RootElement.GetChild(0));
    }

    [Template(TemplateType.String, @"
    <UITemplate>
        <Contents>
            <Router>
                <Route path=""'/options/:id'""/>
                <Route path=""'/options2/:id'""/>
                <UnmatchedRoute/>
            </Router>
        </Contents>
    </UITemplate>
    ")]
    private class ParsersRouterNestedThing : UIElement { }

    [Test]
    public void ParsesRouterWithSubRoutes() {
        MockApplication app = new MockApplication(typeof(ParsersRouterNestedThing));
        Assert.IsInstanceOf<RouterElement>(app.RootElement.GetChild(0));
        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
        Assert.AreEqual("/options/:id", router.FindByType<RouteElement>()[0].path);
        Assert.AreEqual("/options2/:id", router.FindByType<RouteElement>()[1].path);
        Assert.IsTrue(router.FindByType<RouteElement>()[0].isDisabled);
        Assert.IsTrue(router.FindByType<RouteElement>()[1].isDisabled);
        Assert.IsTrue(router.FindByType<RouteElement>()[2].isEnabled); 
    }

    [Test]
    public void CallsEnterHandlerForMatchedRoute() {
        MockApplication app = new MockApplication(typeof(ParsersRouterNestedThing));
        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
        bool didEnter = false;
        router.onRouteEnter += () => { didEnter = true; };
        app.Router.GoTo("/options/1");
        Assert.IsTrue(didEnter);
        Assert.IsTrue(router.FindByType<RouteElement>()[0].isEnabled);
        Assert.IsTrue(router.FindByType<RouteElement>()[1].isDisabled);
        Assert.IsTrue(router.FindByType<RouteElement>()[2].isDisabled);
    }

    [Test]
    public void CallsExitHandlerForMatchedRoute() {
        MockApplication app = new MockApplication(typeof(ParsersRouterNestedThing));
        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
        bool didExit = false;
        router.onRouteExit += () => { didExit = true; };
        app.Router.GoTo("/options/1");
        app.Router.GoTo("/options2/4");
        Assert.IsTrue(didExit);
        Assert.IsTrue(router.FindByType<RouteElement>()[0].isDisabled);
        Assert.IsTrue(router.FindByType<RouteElement>()[1].isEnabled);
        Assert.IsTrue(router.FindByType<RouteElement>()[2].isDisabled);
    }

    [Test]
    public void MatchesUnmatchedRoute() {
        MockApplication app = new MockApplication(typeof(ParsersRouterNestedThing));
        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
        app.Router.GoTo("/opt");
        Assert.IsTrue(router.FindByType<RouteElement>()[0].isDisabled);
        Assert.IsTrue(router.FindByType<RouteElement>()[1].isDisabled);
        Assert.IsTrue(router.FindByType<RouteElement>()[2].isEnabled);
    }

    [Template(TemplateType.String, @"
    <UITemplate>
        <Contents>
            <Router>
                <Route path=""'/users'"">
                    <ChildRouter>
                        <Route path=""'/settings'""/>        
                        <Route path=""'/friends'""/>        
                        <Route path=""'/:id'""/>        
                    </ChildRouter>
                 </Route>
                <Route path=""'/options/:id'""/>
                <Route path=""'/options2/:id'""/>
                <UnmatchedRoute/>
            </Router>
        </Contents>
    </UITemplate>
    ")]
    private class ParsersRouterChildNestedThing : UIElement { }

    [Test]
    public void MatchChildRouter() {
        MockApplication app = new MockApplication(typeof(ParsersRouterChildNestedThing));
        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
        app.Router.GoTo("/users/settings");
        ChildRouterElement childRouter = router.FindFirstByType<ChildRouterElement>();

        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isEnabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isDisabled);
    }

    [Test]
    public void UpdateChildRouter() {
        MockApplication app = new MockApplication(typeof(ParsersRouterChildNestedThing));
        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
        app.Router.GoTo("/users/settings");

        ChildRouterElement childRouter = router.FindFirstByType<ChildRouterElement>();
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isEnabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isDisabled);

        app.Router.GoTo("/users/friends");

        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isEnabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isDisabled);

        app.Router.GoTo("/users/other");
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isEnabled);
        Assert.AreEqual(childRouter.FindByType<RouteElement>()[2].GetRouteParameter("id").value, "other");
    }

    [Test]
    public void ExitReEnterChildRouter() {
        MockApplication app = new MockApplication(typeof(ParsersRouterChildNestedThing));
        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
        app.Router.GoTo("/users/settings");

        ChildRouterElement childRouter = router.FindFirstByType<ChildRouterElement>();
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isEnabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isDisabled);

        app.Router.GoTo("/options");

        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isDisabled);

        app.Router.GoTo("/users/other");
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isEnabled);
        Assert.AreEqual(childRouter.FindByType<RouteElement>()[2].GetRouteParameter("id").value, "other");
    }

    [Test]
    public void ChangeParameterChildRouter() {
        MockApplication app = new MockApplication(typeof(ParsersRouterChildNestedThing));
        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
        app.Router.GoTo("/users/settings");

        ChildRouterElement childRouter = router.FindFirstByType<ChildRouterElement>();

        app.Router.GoTo("/users/other");
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isEnabled);
        Assert.AreEqual("other", childRouter.FindByType<RouteElement>()[2].GetRouteParameter("id").value);

        app.Router.GoTo("/users/other_thing");

        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isEnabled);
        Assert.AreEqual("other_thing", childRouter.FindByType<RouteElement>()[2].GetRouteParameter("id").value);
    }

    [Test]
    public void ChildRouterOnRouteChanged() {
        MockApplication app = new MockApplication(typeof(ParsersRouterChildNestedThing));
        RouterElement router = (RouterElement) app.RootElement.GetChild(0);

        RouteElement current = null;
        int callCount = 0;
        ChildRouterElement childRouter = router.FindFirstByType<ChildRouterElement>();
        childRouter.onRouteChanged += () => {
            callCount++;
            current = childRouter.ActiveChild;
        };

        app.Router.GoTo("/users/settings");
        Assert.AreEqual(0, callCount);

        app.Router.GoTo("/users/friends");
        Assert.AreEqual(current, childRouter.FindByType<RouteElement>()[1]);
        Assert.AreEqual(1, callCount);

        app.Router.GoTo("/users/other_thing");
        Assert.AreEqual(current, childRouter.FindByType<RouteElement>()[2]);
        Assert.AreEqual(2, callCount);

        app.Router.GoTo("/users/blah");
        Assert.AreEqual(current, childRouter.FindByType<RouteElement>()[2]);
        Assert.AreEqual(2, callCount);
    }

    [Template(TemplateType.String, @"
    <UITemplate>
        <Contents>
            <Router>
                <Route path=""'/users'"">
                    <ChildRouter path=""'/extra'"">
                        <Route path=""'/settings'""/>        
                        <Route path=""'/friends'""/>        
                        <Route path=""'/:id'""/>        
                    </ChildRouter>
                 </Route>
                <Route path=""'/options/:id'""/>
                <Route path=""'/options2/:id'""/>
                <UnmatchedRoute/>
            </Router>
        </Contents>
    </UITemplate>
    ")]
    private class ParsersRouterChildWithPathNestedThing : UIElement { }
    
    [Test]
    public void ChildRouterWithPath() {
        MockApplication app = new MockApplication(typeof(ParsersRouterChildWithPathNestedThing));
        RouterElement router = (RouterElement) app.RootElement.GetChild(0);
//        app.Router.GoTo("/users");
        app.Router.GoTo("/users/extra/settings");

        ChildRouterElement childRouter = router.FindFirstByType<ChildRouterElement>();
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isEnabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isDisabled);

        app.Router.GoTo("/users/extra/friends");

        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isEnabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isDisabled);

        app.Router.GoTo("/users/extra/other");
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[0].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[1].isDisabled);
        Assert.IsTrue(childRouter.FindByType<RouteElement>()[2].isEnabled);
        Assert.AreEqual(childRouter.FindByType<RouteElement>()[2].GetRouteParameter("id").value, "other");
    }

}