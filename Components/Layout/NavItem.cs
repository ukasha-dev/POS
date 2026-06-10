using Microsoft.AspNetCore.Components.Routing;

namespace SmartPOS.Components.Layout;

/// <summary>One link in a role's sidebar navigation.</summary>
public record NavItem(string Label, string Href, string Icon, NavLinkMatch Match = NavLinkMatch.Prefix);
