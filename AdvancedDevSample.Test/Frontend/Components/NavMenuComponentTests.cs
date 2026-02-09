using AdvancedDevSample.Frontend.Layout;
using Bunit;
using Bunit.TestDoubles;

namespace AdvancedDevSample.Test.Frontend.Components;

public class NavMenuComponentTests : TestContext
{
    [Fact]
    public void NavMenu_WhenTogglerClicked_TogglesCollapseClass()
    {
        TestAuthorizationContext authContext = this.AddTestAuthorization();
        authContext.SetNotAuthorized();

        IRenderedComponent<NavMenu> component = RenderComponent<NavMenu>();

        AngleSharp.Dom.IElement navContainer = component.Find("div.nav-scrollable");
        Assert.Contains("collapse", navContainer.GetAttribute("class"), StringComparison.Ordinal);

        component.Find("button.navbar-toggler").Click();

        navContainer = component.Find("div.nav-scrollable");
        Assert.DoesNotContain("collapse", navContainer.GetAttribute("class"), StringComparison.Ordinal);
    }

    [Fact]
    public void NavMenu_WhenUserIsAnonymous_ShowsSignInAndRegisterLinks()
    {
        TestAuthorizationContext authContext = this.AddTestAuthorization();
        authContext.SetNotAuthorized();

        IRenderedComponent<NavMenu> component = RenderComponent<NavMenu>();

        component.Find("a[href='account/login']");
        component.Find("a[href='account/register']");
        Assert.DoesNotContain("users", component.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NavMenu_WhenUserIsAuthenticated_ShowsProtectedLinksWithoutAdminLink()
    {
        TestAuthorizationContext authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("user@example.com");

        IRenderedComponent<NavMenu> component = RenderComponent<NavMenu>();

        component.Find("a[href='products']");
        component.Find("a[href='categories']");
        component.Find("a[href='account/profile']");
        Assert.DoesNotContain("Users (Admin)", component.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void NavMenu_WhenUserIsAdmin_ShowsAdminLink()
    {
        TestAuthorizationContext authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("admin@example.com");
        authContext.SetRoles("Admin");

        IRenderedComponent<NavMenu> component = RenderComponent<NavMenu>();

        component.Find("a[href='users']");
        Assert.Contains("Users (Admin)", component.Markup, StringComparison.Ordinal);
    }
}
