using AdvancedDevSample.Frontend.Layout;
using Bunit;
using Bunit.TestDoubles;

namespace AdvancedDevSample.Test.Frontend.Components;

public class NavMenuComponentTests : TestContext
{
    [Fact]
    public void NavMenu_WhenUserIsAnonymous_ShowsSignInAndRegisterLinks()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetNotAuthorized();

        var component = RenderComponent<NavMenu>();

        component.Find("a[href='account/login']");
        component.Find("a[href='account/register']");
        Assert.DoesNotContain("users", component.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NavMenu_WhenUserIsAuthenticated_ShowsProtectedLinksWithoutAdminLink()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("user@example.com");

        var component = RenderComponent<NavMenu>();

        component.Find("a[href='products']");
        component.Find("a[href='categories']");
        component.Find("a[href='account/profile']");
        Assert.DoesNotContain("Users (Admin)", component.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void NavMenu_WhenUserIsAdmin_ShowsAdminLink()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("admin@example.com");
        authContext.SetRoles("Admin");

        var component = RenderComponent<NavMenu>();

        component.Find("a[href='users']");
        Assert.Contains("Users (Admin)", component.Markup, StringComparison.Ordinal);
    }
}
