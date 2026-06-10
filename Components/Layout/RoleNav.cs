using Microsoft.AspNetCore.Components.Routing;

namespace SmartPOS.Components.Layout;

/// <summary>Centralized per-role sidebar navigation config (single source of truth).</summary>
public static class RoleNav
{
    public static IReadOnlyList<NavItem> Admin { get; } = new List<NavItem>
    {
        new("Dashboard",  "/admin/dashboard",  "bi-speedometer2", NavLinkMatch.All),
        new("Analytics",  "/admin/analytics",  "bi-graph-up"),
        new("Products",   "/admin/products",   "bi-box-seam"),
        new("Categories", "/admin/categories", "bi-tags"),
        new("Suppliers",  "/admin/suppliers",  "bi-truck"),
        new("Users",      "/admin/users",      "bi-people"),
        new("Roles",      "/admin/roles",      "bi-shield-lock"),
        new("Customers",  "/admin/customers",  "bi-person-badge"),
        new("Audit Logs", "/admin/audit-logs", "bi-journal-text"),
        new("Sentiment",  "/admin/sentiment",  "bi-emoji-smile"),
    };
}
