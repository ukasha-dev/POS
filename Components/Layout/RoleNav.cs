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

    public static IReadOnlyList<NavItem> Manager { get; } = new List<NavItem>
    {
        new("Dashboard",       "/manager/dashboard",            "bi-speedometer2",        NavLinkMatch.All),
        new("Inventory",       "/manager/inventory",            "bi-boxes",               NavLinkMatch.All),
        new("Low Stock",       "/manager/inventory/low-stock",  "bi-exclamation-triangle"),
        new("Purchase Orders", "/manager/purchase-orders",      "bi-receipt"),
        new("Promotions",      "/manager/promotions",           "bi-percent"),
    };

    public static IReadOnlyList<NavItem> Cashier { get; } = new List<NavItem>
    {
        new("Dashboard",       "/cashier/dashboard",       "bi-speedometer2", NavLinkMatch.All),
        new("Point of Sale",   "/cashier/pos",             "bi-cart3"),
        new("Sales History",   "/cashier/sales-history",   "bi-clock-history"),
        new("Customer Lookup", "/cashier/customer-lookup", "bi-search"),
    };

    public static IReadOnlyList<NavItem> Customer { get; } = new List<NavItem>
    {
        new("Dashboard",  "/customer/dashboard",  "bi-speedometer2", NavLinkMatch.All),
        new("Home",       "/customer/home",       "bi-house"),
        new("Shop",       "/customer/shop",       "bi-bag"),
        new("Cart",       "/customer/cart",       "bi-cart3"),
        new("Orders",     "/customer/orders",     "bi-bag-check"),
        new("Reviews",    "/customer/reviews",    "bi-star"),
        new("Profile",    "/customer/profile",    "bi-person"),
        new("Promotions", "/customer/promotions", "bi-percent"),
    };
}
