# UI Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the shared design-system foundation (light/dark tokens, reusable component CSS, a left-sidebar `AppShell`, theme toggle, real-user identity) and prove it by rebuilding the Admin Dashboard.

**Architecture:** One global token stylesheet (`theme.css`) drives light + dark via a `data-theme` attribute on `<html>`. A component stylesheet (`components.css`) supplies reusable classes. A single `AppShell.razor` (sidebar + top bar) replaces the per-role navbars; each role layout passes a nav config from `RoleNav`. Only `AdminLayout` migrates in this phase; legacy stylesheets stay linked so other roles keep working. UI is verified by building clean and visually inspecting the running app (no unit tests for CSS/markup).

**Tech Stack:** Blazor Server (.NET 10), Razor components, CSS custom properties, Bootstrap 5.3 (retained) + Bootstrap Icons (added), Blazor `NavLink`, `CascadingAuthenticationState`.

**Spec:** `docs/superpowers/specs/2026-06-10-ui-foundation-design.md`

**Conventions for this plan:** Since CSS/markup has no unit tests, each task's verification is `dotnet build` (must be 0 errors) plus, in the final task, a visual checklist against the running app. The app must be stopped before building (the running process locks output DLLs): `Get-Process SmartPOS -ErrorAction SilentlyContinue | Stop-Process -Force`.

---

### Task 1: Design tokens (`theme.css`)

**Files:**
- Create: `wwwroot/css/theme.css`

- [ ] **Step 1: Create the token stylesheet**

```css
/* ============================================================
   SmartPOS — Design Tokens (single source of truth)
   Light = :root ; Dark = [data-theme="dark"]
   New token names (--bg, --surface, --accent, …) intentionally
   do NOT collide with legacy colors.css names.
   ============================================================ */
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap');

:root {
    /* Surfaces */
    --bg:            #f1f5f9;
    --surface:       #ffffff;
    --surface-2:     #f8fafc;
    --surface-3:     #f1f5f9;
    --border:        #e2e8f0;
    --border-strong: #cbd5e1;

    /* Text */
    --text:          #0f172a;
    --text-muted:    #64748b;
    --text-subtle:   #94a3b8;
    --text-on-accent:#ffffff;

    /* Accent (emerald/teal) */
    --accent:        #0d9488;
    --accent-hover:  #0f766e;
    --accent-soft:   rgba(13, 148, 136, 0.10);
    --accent-ring:   rgba(13, 148, 136, 0.35);

    /* Semantic */
    --success: #16a34a; --success-soft: rgba(22, 163, 74, 0.12);
    --warning: #d97706; --warning-soft: rgba(217, 119, 6, 0.12);
    --danger:  #dc2626; --danger-soft:  rgba(220, 38, 38, 0.12);
    --info:    #2563eb; --info-soft:    rgba(37, 99, 235, 0.12);

    /* Role accents (subtle per-role tint) */
    --role-admin:    #0d9488;
    --role-manager:  #2563eb;
    --role-cashier:  #7c3aed;
    --role-customer: #d97706;

    /* Typography */
    --font-sans: 'Inter', 'Segoe UI', system-ui, -apple-system, sans-serif;
    --fs-xs: 0.75rem; --fs-sm: 0.85rem; --fs-base: 0.9375rem;
    --fs-lg: 1.0625rem; --fs-xl: 1.25rem; --fs-2xl: 1.5rem; --fs-3xl: 2rem;

    /* Spacing (4px base, balanced) */
    --sp-1: 0.25rem; --sp-2: 0.5rem; --sp-3: 0.75rem; --sp-4: 1rem;
    --sp-5: 1.25rem; --sp-6: 1.5rem; --sp-8: 2rem;

    /* Radius */
    --radius-sm: 0.375rem; --radius-md: 0.625rem; --radius-lg: 1rem;
    --radius-xl: 1.5rem; --radius-full: 9999px;

    /* Shadow */
    --shadow-xs: 0 1px 2px rgba(15, 23, 42, 0.06);
    --shadow-sm: 0 1px 3px rgba(15, 23, 42, 0.10), 0 1px 2px rgba(15, 23, 42, 0.06);
    --shadow-md: 0 4px 12px rgba(15, 23, 42, 0.10);
    --shadow-lg: 0 12px 32px rgba(15, 23, 42, 0.14);

    /* Chrome sizing */
    --sidebar-w: 256px;
    --sidebar-w-collapsed: 72px;
    --topbar-h: 60px;

    /* Z-index */
    --z-sidebar: 800; --z-topbar: 700; --z-overlay: 1000;
    --z-modal: 1100; --z-toast: 1200;

    /* Transitions */
    --t-fast: 0.15s ease; --t-base: 0.25s ease;
}

[data-theme="dark"] {
    --bg:            #0b1220;
    --surface:       #111a2b;
    --surface-2:     #16213a;
    --surface-3:     #1e293b;
    --border:        #243049;
    --border-strong: #334155;

    --text:          #e8eef7;
    --text-muted:    #94a3b8;
    --text-subtle:   #64748b;
    --text-on-accent:#04201d;

    --accent:        #2dd4bf;
    --accent-hover:  #5eead4;
    --accent-soft:   rgba(45, 212, 191, 0.14);
    --accent-ring:   rgba(45, 212, 191, 0.40);

    --success: #34d399; --success-soft: rgba(52, 211, 153, 0.15);
    --warning: #fbbf24; --warning-soft: rgba(251, 191, 36, 0.15);
    --danger:  #f87171; --danger-soft:  rgba(248, 113, 113, 0.15);
    --info:    #60a5fa; --info-soft:    rgba(96, 165, 250, 0.15);

    --role-admin:    #2dd4bf;
    --role-manager:  #60a5fa;
    --role-cashier:  #a78bfa;
    --role-customer: #fbbf24;

    --shadow-xs: 0 1px 2px rgba(0, 0, 0, 0.40);
    --shadow-sm: 0 1px 3px rgba(0, 0, 0, 0.45);
    --shadow-md: 0 4px 12px rgba(0, 0, 0, 0.45);
    --shadow-lg: 0 12px 32px rgba(0, 0, 0, 0.55);
}

html, body {
    background: var(--bg);
    color: var(--text);
    font-family: var(--font-sans);
    font-size: var(--fs-base);
}
```

- [ ] **Step 2: Build to verify nothing breaks**

Run: `Get-Process SmartPOS -ErrorAction SilentlyContinue | Stop-Process -Force; dotnet build --nologo -v q`
Expected: `0 Error(s)` (the file is not yet linked; this just confirms the project still compiles).

- [ ] **Step 3: Commit**

```bash
git add wwwroot/css/theme.css
git commit -m "feat(ui): add light/dark design tokens (theme.css)"
```

---

### Task 2: Component library (`components.css`)

**Files:**
- Create: `wwwroot/css/components.css`

- [ ] **Step 1: Create the component stylesheet**

```css
/* ============================================================
   SmartPOS — Reusable component classes (token-driven)
   ============================================================ */

/* ── Page header ─────────────────────────────────────────── */
.page-header { display:flex; justify-content:space-between; align-items:flex-end;
    gap:var(--sp-4); margin-bottom:var(--sp-6); flex-wrap:wrap; }
.page-header__title { font-size:var(--fs-2xl); font-weight:700; color:var(--text); margin:0; letter-spacing:-0.02em; }
.page-header__subtitle { color:var(--text-muted); font-size:var(--fs-sm); margin:var(--sp-1) 0 0; }
.page-header__actions { display:flex; gap:var(--sp-2); align-items:center; }

/* ── Card ────────────────────────────────────────────────── */
.app-card { background:var(--surface); border:1px solid var(--border);
    border-radius:var(--radius-lg); box-shadow:var(--shadow-xs); overflow:hidden; }
.app-card__header { display:flex; justify-content:space-between; align-items:center;
    padding:var(--sp-4) var(--sp-5); border-bottom:1px solid var(--border); }
.app-card__title { font-size:var(--fs-base); font-weight:600; color:var(--text); margin:0; }
.app-card__body { padding:var(--sp-5); }
.app-card__body--flush { padding:0; }

/* ── Stat card ───────────────────────────────────────────── */
.stat-card { background:var(--surface); border:1px solid var(--border);
    border-radius:var(--radius-lg); box-shadow:var(--shadow-xs);
    padding:var(--sp-5); display:flex; align-items:center; gap:var(--sp-4); }
.stat-card__icon { width:46px; height:46px; border-radius:var(--radius-md);
    display:flex; align-items:center; justify-content:center; flex-shrink:0;
    background:var(--accent-soft); color:var(--accent); font-size:1.25rem; }
.stat-card__icon--success { background:var(--success-soft); color:var(--success); }
.stat-card__icon--warning { background:var(--warning-soft); color:var(--warning); }
.stat-card__icon--danger  { background:var(--danger-soft);  color:var(--danger); }
.stat-card__label { font-size:var(--fs-xs); font-weight:600; color:var(--text-muted);
    text-transform:uppercase; letter-spacing:0.04em; }
.stat-card__value { font-size:var(--fs-2xl); font-weight:700; color:var(--text); line-height:1.1; }

/* ── Data table ──────────────────────────────────────────── */
.data-table-wrap { width:100%; overflow-x:auto; }
.data-table { width:100%; border-collapse:collapse; font-size:var(--fs-sm); }
.data-table thead th { position:sticky; top:0; background:var(--surface-2);
    color:var(--text-muted); font-weight:600; text-transform:uppercase;
    font-size:var(--fs-xs); letter-spacing:0.03em; text-align:left;
    padding:var(--sp-3) var(--sp-4); border-bottom:1px solid var(--border); white-space:nowrap; }
.data-table tbody td { padding:var(--sp-3) var(--sp-4); border-bottom:1px solid var(--border); color:var(--text); }
.data-table tbody tr:last-child td { border-bottom:none; }
.data-table tbody tr:hover { background:var(--surface-2); }
.data-table .text-end { text-align:right; }

/* ── Buttons ─────────────────────────────────────────────── */
.btn-app { display:inline-flex; align-items:center; justify-content:center; gap:var(--sp-2);
    font-family:var(--font-sans); font-size:var(--fs-sm); font-weight:600;
    padding:0.5rem 0.9rem; border-radius:var(--radius-md); border:1px solid transparent;
    cursor:pointer; text-decoration:none; transition:background var(--t-fast), border-color var(--t-fast), color var(--t-fast); white-space:nowrap; }
.btn-app:disabled { opacity:0.55; cursor:not-allowed; }
.btn-primary-app { background:var(--accent); color:var(--text-on-accent); }
.btn-primary-app:hover { background:var(--accent-hover); }
.btn-secondary-app { background:var(--surface); color:var(--text); border-color:var(--border-strong); }
.btn-secondary-app:hover { background:var(--surface-2); }
.btn-ghost-app { background:transparent; color:var(--text-muted); }
.btn-ghost-app:hover { background:var(--surface-2); color:var(--text); }
.btn-danger-app { background:var(--danger); color:#fff; }
.btn-danger-app:hover { filter:brightness(0.93); }
.btn-sm-app { padding:0.3rem 0.6rem; font-size:var(--fs-xs); }
.btn-icon-app { padding:0.4rem; width:36px; height:36px; }

/* ── Badges / status pills ───────────────────────────────── */
.pill { display:inline-flex; align-items:center; gap:0.3rem; font-size:var(--fs-xs);
    font-weight:600; padding:0.2rem 0.6rem; border-radius:var(--radius-full); line-height:1.4; }
.pill-success { background:var(--success-soft); color:var(--success); }
.pill-warning { background:var(--warning-soft); color:var(--warning); }
.pill-danger  { background:var(--danger-soft);  color:var(--danger); }
.pill-info    { background:var(--info-soft);    color:var(--info); }
.pill-neutral { background:var(--surface-3);    color:var(--text-muted); }
.badge-role { font-size:var(--fs-xs); font-weight:700; padding:0.2rem 0.6rem;
    border-radius:var(--radius-full); text-transform:capitalize; }
.badge-role-admin    { background:var(--accent-soft);  color:var(--role-admin); }
.badge-role-manager  { background:var(--info-soft);    color:var(--role-manager); }
.badge-role-cashier  { background:rgba(124,58,237,0.12); color:var(--role-cashier); }
.badge-role-customer { background:var(--warning-soft); color:var(--role-customer); }

/* ── Forms ───────────────────────────────────────────────── */
.form-group-app { display:flex; flex-direction:column; gap:var(--sp-2); margin-bottom:var(--sp-4); }
.form-label-app { font-size:var(--fs-sm); font-weight:600; color:var(--text); }
.form-control-app, .select-app { width:100%; font-family:var(--font-sans); font-size:var(--fs-sm);
    color:var(--text); background:var(--surface); border:1px solid var(--border-strong);
    border-radius:var(--radius-md); padding:0.55rem 0.75rem; transition:border-color var(--t-fast), box-shadow var(--t-fast); }
.form-control-app:focus, .select-app:focus { outline:none; border-color:var(--accent);
    box-shadow:0 0 0 3px var(--accent-ring); }
.form-control-app::placeholder { color:var(--text-subtle); }

/* ── Modal ───────────────────────────────────────────────── */
.modal-overlay-app { position:fixed; inset:0; background:rgba(2,6,23,0.55);
    backdrop-filter:blur(2px); z-index:var(--z-modal); display:flex; align-items:center;
    justify-content:center; padding:var(--sp-4); }
.modal-panel-app { background:var(--surface); border:1px solid var(--border);
    border-radius:var(--radius-lg); box-shadow:var(--shadow-lg); width:100%; max-width:560px;
    max-height:90vh; display:flex; flex-direction:column; overflow:hidden; }
.modal-header-app { display:flex; justify-content:space-between; align-items:center;
    padding:var(--sp-4) var(--sp-5); border-bottom:1px solid var(--border); }
.modal-body-app { padding:var(--sp-5); overflow-y:auto; }
.modal-footer-app { display:flex; justify-content:flex-end; gap:var(--sp-2);
    padding:var(--sp-4) var(--sp-5); border-top:1px solid var(--border); }

/* ── Tabs ────────────────────────────────────────────────── */
.tabs-app { display:flex; gap:var(--sp-1); border-bottom:1px solid var(--border); margin-bottom:var(--sp-5); }
.tab-app { padding:0.6rem 0.9rem; font-size:var(--fs-sm); font-weight:600; color:var(--text-muted);
    background:none; border:none; border-bottom:2px solid transparent; cursor:pointer; }
.tab-app:hover { color:var(--text); }
.tab-app--active { color:var(--accent); border-bottom-color:var(--accent); }

/* ── Empty state ─────────────────────────────────────────── */
.empty-state { text-align:center; padding:var(--sp-8) var(--sp-4); color:var(--text-muted); }
.empty-state__icon { font-size:2rem; color:var(--text-subtle); margin-bottom:var(--sp-2); }
.empty-state__title { font-weight:600; color:var(--text); margin-bottom:var(--sp-1); }

/* ── Avatar ──────────────────────────────────────────────── */
.avatar-app { display:inline-flex; align-items:center; justify-content:center;
    border-radius:var(--radius-full); background:var(--accent-soft); color:var(--accent);
    font-weight:700; font-size:var(--fs-sm); width:36px; height:36px; flex-shrink:0; }

/* ── Skeleton ────────────────────────────────────────────── */
.skeleton { background:linear-gradient(90deg, var(--surface-2) 25%, var(--surface-3) 37%, var(--surface-2) 63%);
    background-size:400% 100%; animation:skeleton 1.4s ease infinite; border-radius:var(--radius-sm); }
@keyframes skeleton { 0%{background-position:100% 50%} 100%{background-position:0 50%} }

/* ── Utilities ───────────────────────────────────────────── */
.app-container { max-width:1500px; margin:0 auto; padding:var(--sp-6); }
.grid-cards { display:grid; gap:var(--sp-4); grid-template-columns:repeat(auto-fit, minmax(220px, 1fr)); }
```

- [ ] **Step 2: Build to verify the project still compiles**

Run: `Get-Process SmartPOS -ErrorAction SilentlyContinue | Stop-Process -Force; dotnet build --nologo -v q`
Expected: `0 Error(s)`

- [ ] **Step 3: Commit**

```bash
git add wwwroot/css/components.css
git commit -m "feat(ui): add reusable component classes (components.css)"
```

---

### Task 3: Wire stylesheets, icons, and theme toggle into `App.razor`

**Files:**
- Modify: `Components/App.razor`

- [ ] **Step 1: Add the new stylesheets + Bootstrap Icons + pre-paint theme script in `<head>`**

In `Components/App.razor`, the `<head>` currently ends with existing `<link>`s and `<ImportMap />` / `<HeadOutlet />`. Add the new links **after** the existing `modern-ui-v4.css` link (keep all existing links). Insert:

```html
    <link rel="stylesheet" href="css/theme.css" />
    <link rel="stylesheet" href="css/components.css" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" />
    <script>
        (function () {
            try {
                var t = localStorage.getItem('smartpos-theme');
                if (!t) t = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
                document.documentElement.setAttribute('data-theme', t);
            } catch (e) { document.documentElement.setAttribute('data-theme', 'light'); }
        })();
    </script>
```

> Note: keep `css/colors.css`, `app.css`, `css/modern-ui-v4.css`, Bootstrap, and the Outfit font link exactly as they are — other roles still depend on them during the transition.

- [ ] **Step 2: Add the theme-toggle + sidebar JS helpers in `<body>`**

In `Components/App.razor`, inside the existing inline `<script>` block in `<body>` (the one defining `window.toggleSidebar`/`window.signOut`), append these functions before its closing `</script>`:

```javascript
        window.smartposToggleTheme = function () {
            var r = document.documentElement;
            var next = (r.getAttribute('data-theme') === 'dark') ? 'light' : 'dark';
            r.setAttribute('data-theme', next);
            try { localStorage.setItem('smartpos-theme', next); } catch (e) {}
        };
        window.smartposGetSidebarCollapsed = function () {
            try { return localStorage.getItem('smartpos-sidebar') === '1'; } catch (e) { return false; }
        };
        window.smartposSetSidebarCollapsed = function (v) {
            try { localStorage.setItem('smartpos-sidebar', v ? '1' : '0'); } catch (e) {}
        };
```

- [ ] **Step 3: Build to verify**

Run: `Get-Process SmartPOS -ErrorAction SilentlyContinue | Stop-Process -Force; dotnet build --nologo -v q`
Expected: `0 Error(s)`

- [ ] **Step 4: Commit**

```bash
git add Components/App.razor
git commit -m "feat(ui): link theme/components css, bootstrap-icons, theme toggle JS"
```

---

### Task 4: Navigation config (`NavItem`, `RoleNav`)

**Files:**
- Create: `Components/Layout/NavItem.cs`
- Create: `Components/Layout/RoleNav.cs`

- [ ] **Step 1: Create `NavItem.cs`**

```csharp
using Microsoft.AspNetCore.Components.Routing;

namespace SmartPOS.Components.Layout;

/// <summary>One link in a role's sidebar navigation.</summary>
public record NavItem(string Label, string Href, string Icon, NavLinkMatch Match = NavLinkMatch.Prefix);
```

- [ ] **Step 2: Create `RoleNav.cs`**

```csharp
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
```

- [ ] **Step 3: Build to verify**

Run: `Get-Process SmartPOS -ErrorAction SilentlyContinue | Stop-Process -Force; dotnet build --nologo -v q`
Expected: `0 Error(s)`

- [ ] **Step 4: Commit**

```bash
git add Components/Layout/NavItem.cs Components/Layout/RoleNav.cs
git commit -m "feat(ui): add NavItem + RoleNav sidebar config"
```

---

### Task 5: Shared `AppShell` (sidebar + top bar)

**Files:**
- Create: `Components/Layout/AppShell.razor`
- Create: `Components/Layout/AppShell.razor.css`

- [ ] **Step 1: Create `AppShell.razor`**

```razor
@namespace SmartPOS.Components.Layout
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.Routing
@inject IJSRuntime JS
@implements IDisposable

<div class="shell @(_collapsed ? "shell--collapsed" : "") @(_mobileOpen ? "shell--mobile-open" : "")">

    <aside class="shell__sidebar" style="--role-accent: @RoleAccent;">
        <div class="shell__brand">
            <span class="shell__brand-mark"><i class="bi bi-shop"></i></span>
            <span class="shell__brand-text">SmartPOS<span class="shell__brand-plus">+</span></span>
            <span class="shell__role-tag">@RoleLabel</span>
        </div>

        <nav class="shell__nav" aria-label="@RoleLabel navigation">
            @foreach (var item in NavItems)
            {
                <NavLink class="shell__nav-link" href="@item.Href" Match="item.Match" @onclick="CloseMobile">
                    <i class="bi @item.Icon shell__nav-icon"></i>
                    <span class="shell__nav-label">@item.Label</span>
                </NavLink>
            }
        </nav>

        <button class="shell__collapse-btn" @onclick="ToggleCollapse" aria-label="Toggle sidebar">
            <i class="bi @(_collapsed ? "bi-chevron-right" : "bi-chevron-left")"></i>
        </button>
    </aside>

    <div class="shell__overlay" @onclick="CloseMobile"></div>

    <div class="shell__main">
        <header class="shell__topbar">
            <button class="shell__icon-btn shell__menu-btn" @onclick="ToggleMobile" aria-label="Open menu">
                <i class="bi bi-list"></i>
            </button>
            <div class="shell__title">@PageTitle</div>
            <div class="shell__topbar-right">
                <button class="shell__icon-btn shell__theme-btn" onclick="smartposToggleTheme()" aria-label="Toggle theme">
                    <i class="bi bi-sun-fill shell__icon-light"></i>
                    <i class="bi bi-moon-stars-fill shell__icon-dark"></i>
                </button>
                <div class="shell__profile">
                    <button class="shell__profile-btn" @onclick="ToggleProfile" aria-label="Account">
                        <span class="avatar-app">@_initials</span>
                    </button>
                    @if (_profileOpen)
                    {
                        <div class="shell__profile-menu">
                            <div class="shell__profile-head">
                                <div class="shell__profile-name">@_name</div>
                                <span class="badge-role badge-role-@_roleClass">@_role</span>
                            </div>
                            <button class="shell__profile-item" onclick="signOut()">
                                <i class="bi bi-box-arrow-right"></i> Sign Out
                            </button>
                        </div>
                    }
                </div>
            </div>
        </header>

        <main class="shell__content">
            <div class="app-container">
                @Body
            </div>
        </main>
    </div>
</div>

@code {
    [Parameter] public RenderFragment? Body { get; set; }
    [Parameter] public string RoleLabel { get; set; } = "Staff";
    [Parameter] public string RoleAccent { get; set; } = "var(--accent)";
    [Parameter] public string RoleClass { get; set; } = "admin";
    [Parameter] public string PageTitle { get; set; } = "";
    [Parameter] public IReadOnlyList<NavItem> NavItems { get; set; } = new List<NavItem>();

    [CascadingParameter] private Task<AuthenticationState>? AuthState { get; set; }

    private bool _collapsed;
    private bool _mobileOpen;
    private bool _profileOpen;
    private string _name = "User";
    private string _role = "Staff";
    private string _roleClass = "admin";
    private string _initials = "U";

    protected override async Task OnInitializedAsync()
    {
        _roleClass = RoleClass;
        if (AuthState is not null)
        {
            var state = await AuthState;
            var user = state.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                _name = user.Identity.Name
                        ?? user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                        ?? "User";
                _role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? RoleLabel;
                _initials = GetInitials(_name);
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _collapsed = await JS.InvokeAsync<bool>("smartposGetSidebarCollapsed");
            StateHasChanged();
        }
    }

    private async Task ToggleCollapse()
    {
        _collapsed = !_collapsed;
        await JS.InvokeVoidAsync("smartposSetSidebarCollapsed", _collapsed);
    }

    private void ToggleMobile()  => _mobileOpen = !_mobileOpen;
    private void CloseMobile()   => _mobileOpen = false;
    private void ToggleProfile() => _profileOpen = !_profileOpen;

    private static string GetInitials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2) return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        return name.Length >= 2 ? name[..2].ToUpper() : name.ToUpper();
    }

    public void Dispose() { }
}
```

- [ ] **Step 2: Create `AppShell.razor.css` (scoped)**

```css
.shell { display:flex; min-height:100vh; background:var(--bg); }

/* Sidebar */
.shell__sidebar { width:var(--sidebar-w); flex-shrink:0; background:var(--surface);
    border-right:1px solid var(--border); display:flex; flex-direction:column;
    position:sticky; top:0; height:100vh; z-index:var(--z-sidebar);
    transition:width var(--t-base); }
.shell--collapsed .shell__sidebar { width:var(--sidebar-w-collapsed); }

.shell__brand { display:flex; align-items:center; gap:0.5rem; height:var(--topbar-h);
    padding:0 1rem; border-bottom:1px solid var(--border); }
.shell__brand-mark { width:32px; height:32px; border-radius:var(--radius-md);
    background:var(--role-accent); color:#fff; display:flex; align-items:center;
    justify-content:center; flex-shrink:0; font-size:1rem; }
.shell__brand-text { font-weight:800; color:var(--text); letter-spacing:-0.02em; white-space:nowrap; }
.shell__brand-plus { color:var(--role-accent); }
.shell__role-tag { font-size:0.6rem; font-weight:700; letter-spacing:0.08em; text-transform:uppercase;
    color:var(--role-accent); background:var(--accent-soft); padding:0.1rem 0.4rem;
    border-radius:var(--radius-full); margin-left:auto; }
.shell--collapsed .shell__brand-text,
.shell--collapsed .shell__role-tag { display:none; }

.shell__nav { flex:1; overflow-y:auto; padding:var(--sp-3); display:flex; flex-direction:column; gap:2px; }
.shell__nav-link { display:flex; align-items:center; gap:0.75rem; padding:0.6rem 0.7rem;
    border-radius:var(--radius-md); color:var(--text-muted); text-decoration:none;
    font-size:var(--fs-sm); font-weight:500; white-space:nowrap; transition:background var(--t-fast), color var(--t-fast); }
.shell__nav-link:hover { background:var(--surface-2); color:var(--text); }
.shell__nav-link.active { background:var(--accent-soft); color:var(--accent); font-weight:600; }
.shell__nav-icon { font-size:1.05rem; width:20px; text-align:center; flex-shrink:0; }
.shell--collapsed .shell__nav-label { display:none; }
.shell--collapsed .shell__nav-link { justify-content:center; }

.shell__collapse-btn { display:flex; align-items:center; justify-content:center;
    height:40px; border:none; border-top:1px solid var(--border); background:var(--surface);
    color:var(--text-muted); cursor:pointer; }
.shell__collapse-btn:hover { background:var(--surface-2); color:var(--text); }

/* Main */
.shell__main { flex:1; display:flex; flex-direction:column; min-width:0; }
.shell__topbar { height:var(--topbar-h); position:sticky; top:0; z-index:var(--z-topbar);
    background:var(--surface); border-bottom:1px solid var(--border);
    display:flex; align-items:center; gap:var(--sp-3); padding:0 var(--sp-5); }
.shell__title { font-weight:600; color:var(--text); }
.shell__topbar-right { margin-left:auto; display:flex; align-items:center; gap:var(--sp-2); }

.shell__icon-btn { width:38px; height:38px; border-radius:var(--radius-md); border:1px solid var(--border);
    background:var(--surface); color:var(--text-muted); cursor:pointer; display:flex;
    align-items:center; justify-content:center; font-size:1rem; transition:background var(--t-fast); }
.shell__icon-btn:hover { background:var(--surface-2); color:var(--text); }
.shell__menu-btn { display:none; }

/* Theme icon swap */
.shell__icon-dark { display:none; }
:global([data-theme="dark"]) .shell__icon-light { display:none; }
:global([data-theme="dark"]) .shell__icon-dark { display:inline; }

/* Profile */
.shell__profile { position:relative; }
.shell__profile-btn { background:none; border:none; cursor:pointer; padding:0; }
.shell__profile-menu { position:absolute; right:0; top:46px; width:220px; background:var(--surface);
    border:1px solid var(--border); border-radius:var(--radius-md); box-shadow:var(--shadow-lg);
    overflow:hidden; z-index:var(--z-overlay); }
.shell__profile-head { padding:var(--sp-4); border-bottom:1px solid var(--border); display:flex;
    flex-direction:column; gap:var(--sp-2); }
.shell__profile-name { font-weight:600; color:var(--text); font-size:var(--fs-sm); }
.shell__profile-item { width:100%; display:flex; align-items:center; gap:0.6rem; padding:0.7rem var(--sp-4);
    background:none; border:none; cursor:pointer; color:var(--danger); font-size:var(--fs-sm);
    font-weight:600; text-align:left; }
.shell__profile-item:hover { background:var(--danger-soft); }

.shell__content { flex:1; }

/* Overlay (mobile) */
.shell__overlay { display:none; position:fixed; inset:0; background:rgba(2,6,23,0.45);
    z-index:calc(var(--z-sidebar) - 1); }

@media (max-width: 900px) {
    .shell__sidebar { position:fixed; left:0; top:0; transform:translateX(-100%); transition:transform var(--t-base); width:var(--sidebar-w); }
    .shell--mobile-open .shell__sidebar { transform:translateX(0); }
    .shell--mobile-open .shell__overlay { display:block; }
    .shell__menu-btn { display:flex; }
    .shell__collapse-btn { display:none; }
}
```

- [ ] **Step 3: Build to verify**

Run: `Get-Process SmartPOS -ErrorAction SilentlyContinue | Stop-Process -Force; dotnet build --nologo -v q`
Expected: `0 Error(s)`

- [ ] **Step 4: Commit**

```bash
git add Components/Layout/AppShell.razor Components/Layout/AppShell.razor.css
git commit -m "feat(ui): add shared AppShell (sidebar + top bar + theme/profile)"
```

---

### Task 6: Convert `AdminLayout` to use `AppShell`

**Files:**
- Modify: `Components/Layout/Admin/AdminLayout.razor`

- [ ] **Step 1: Replace the entire file contents**

```razor
@namespace SmartPOS.Components.Layout
@inherits LayoutComponentBase

<AppShell RoleLabel="Admin"
          RoleClass="admin"
          RoleAccent="var(--role-admin)"
          NavItems="RoleNav.Admin"
          Body="@Body" />
```

> This removes the old `admin-layout-shell` markup, the `AdminNavbar`, the `ProfileDrawer`, and the inline `<style>`. `AdminNavbar.razor` and the old layout CSS are no longer referenced by Admin but are left on disk (other roles' navbars are separate files and are untouched).

- [ ] **Step 2: Build to verify**

Run: `Get-Process SmartPOS -ErrorAction SilentlyContinue | Stop-Process -Force; dotnet build --nologo -v q`
Expected: `0 Error(s)`

- [ ] **Step 3: Commit**

```bash
git add Components/Layout/Admin/AdminLayout.razor
git commit -m "feat(ui): AdminLayout now uses shared AppShell"
```

---

### Task 7: Rebuild the Admin Dashboard on the new components

**Files:**
- Modify: `Components/Pages/Admin/Dashboard.razor`

**IMPORTANT:** Keep the entire `@code { … }` block (lines 291–367 of the current file) **exactly as-is** — only the directives at top and the markup between them and `@code` change. Also fix the broken Quick Link: `/admin/promotions` → `/manager/promotions`.

- [ ] **Step 1: Replace the markup (directives + everything before `@code`)**

Replace from the top of the file through the line immediately before `@code {` with:

```razor
@page "/admin/dashboard"
@layout AdminLayout
@using SmartPOS.Shared.Interfaces
@using SmartPOS.Shared.DTOs.Users
@using SmartPOS.Shared.DTOs.Customers
@using SmartPOS.Shared.DTOs.Inventory
@using SmartPOS.Shared.DTOs.PurchaseOrders
@using SmartPOS.Shared.DTOs.Weather
@using SmartPOS.Shared.Common
@using SmartPOS.Shared.Enums
@inject IUserService UserService
@inject ICustomerService CustomerService
@inject IInventoryService InventoryService
@inject IPurchaseOrderService PurchaseOrderService
@inject IWeatherService WeatherService
@rendermode InteractiveServer

<div class="page-header">
    <div>
        <h1 class="page-header__title">Admin Dashboard</h1>
        <p class="page-header__subtitle">System overview and key business metrics at a glance.</p>
    </div>
    @if (Weather != null)
    {
        <div class="page-header__actions">
            <span class="pill pill-neutral"><i class="bi bi-geo-alt"></i> @Weather.City</span>
            <span class="pill pill-info">@Weather.Temperature°C · @Weather.Condition</span>
        </div>
    }
</div>

@if (IsLoading)
{
    <div class="grid-cards">
        @for (var i = 0; i < 4; i++)
        {
            <div class="stat-card"><div class="skeleton" style="width:100%;height:46px;"></div></div>
        }
    </div>
}
else
{
    <div class="grid-cards" style="margin-bottom:var(--sp-6);">
        <div class="stat-card">
            <div class="stat-card__icon"><i class="bi bi-people-fill"></i></div>
            <div>
                <div class="stat-card__label">Total Users</div>
                <div class="stat-card__value">@TotalUsers</div>
            </div>
        </div>
        <div class="stat-card">
            <div class="stat-card__icon"><i class="bi bi-box-seam-fill"></i></div>
            <div>
                <div class="stat-card__label">Total Stock Items</div>
                <div class="stat-card__value">@TotalStockItems</div>
            </div>
        </div>
        <div class="stat-card">
            <div class="stat-card__icon stat-card__icon--danger"><i class="bi bi-exclamation-triangle-fill"></i></div>
            <div>
                <div class="stat-card__label">Low Stock Items</div>
                <div class="stat-card__value">@LowStockCount</div>
            </div>
        </div>
        <div class="stat-card">
            <div class="stat-card__icon stat-card__icon--warning"><i class="bi bi-receipt"></i></div>
            <div>
                <div class="stat-card__label">Pending POs</div>
                <div class="stat-card__value">@PendingPOCount</div>
            </div>
        </div>
    </div>

    <div class="row g-4">
        <div class="col-lg-6">
            <div class="app-card h-100">
                <div class="app-card__header">
                    <h2 class="app-card__title">Recent Users</h2>
                    <a href="/admin/users" class="btn-app btn-secondary-app btn-sm-app">View All</a>
                </div>
                <div class="app-card__body app-card__body--flush">
                    @if (RecentUsers == null || !RecentUsers.Any())
                    {
                        <div class="empty-state"><div class="empty-state__icon"><i class="bi bi-people"></i></div>No users found.</div>
                    }
                    else
                    {
                        <div class="data-table-wrap">
                            <table class="data-table">
                                <thead><tr><th>Name</th><th>Email</th><th>Role</th><th class="text-end">Status</th></tr></thead>
                                <tbody>
                                    @foreach (var user in RecentUsers)
                                    {
                                        <tr>
                                            <td><strong>@user.Name</strong></td>
                                            <td>@user.Email</td>
                                            <td><span class="badge-role badge-role-@user.RoleName.ToLower()">@user.RoleName</span></td>
                                            <td class="text-end">
                                                @if (user.IsActive)
                                                {
                                                    <span class="pill pill-success">Active</span>
                                                }
                                                else
                                                {
                                                    <span class="pill pill-neutral">Inactive</span>
                                                }
                                            </td>
                                        </tr>
                                    }
                                </tbody>
                            </table>
                        </div>
                    }
                </div>
            </div>
        </div>
        <div class="col-lg-6">
            <div class="app-card h-100">
                <div class="app-card__header">
                    <h2 class="app-card__title">Inventory Status</h2>
                    <a href="/admin/products" class="btn-app btn-secondary-app btn-sm-app">Manage</a>
                </div>
                <div class="app-card__body app-card__body--flush">
                    @if (LowStockItems == null || !LowStockItems.Any())
                    {
                        <div class="empty-state"><div class="empty-state__icon"><i class="bi bi-check-circle"></i></div>All items are well-stocked.</div>
                    }
                    else
                    {
                        <div class="data-table-wrap">
                            <table class="data-table">
                                <thead><tr><th>Product</th><th>Qty</th><th>Reorder Level</th><th class="text-end">Status</th></tr></thead>
                                <tbody>
                                    @foreach (var item in LowStockItems.Take(5))
                                    {
                                        <tr>
                                            <td><strong>@item.ProductName</strong></td>
                                            <td>@item.Quantity</td>
                                            <td>@item.ReorderLevel</td>
                                            <td class="text-end">
                                                @if (item.Quantity == 0)
                                                {
                                                    <span class="pill pill-danger">Out of Stock</span>
                                                }
                                                else
                                                {
                                                    <span class="pill pill-warning">Low Stock</span>
                                                }
                                            </td>
                                        </tr>
                                    }
                                </tbody>
                            </table>
                        </div>
                    }
                </div>
            </div>
        </div>
    </div>

    <div class="row g-4" style="margin-top:var(--sp-2);">
        <div class="col-lg-6">
            <div class="app-card h-100">
                <div class="app-card__header">
                    <h2 class="app-card__title">Recent Purchase Orders</h2>
                    <a href="/manager/purchase-orders" class="btn-app btn-secondary-app btn-sm-app">View All</a>
                </div>
                <div class="app-card__body app-card__body--flush">
                    @if (RecentPOs == null || !RecentPOs.Any())
                    {
                        <div class="empty-state"><div class="empty-state__icon"><i class="bi bi-receipt"></i></div>No purchase orders yet.</div>
                    }
                    else
                    {
                        <div class="data-table-wrap">
                            <table class="data-table">
                                <thead><tr><th>PO #</th><th>Supplier</th><th>Total</th><th class="text-end">Status</th></tr></thead>
                                <tbody>
                                    @foreach (var po in RecentPOs)
                                    {
                                        <tr>
                                            <td><strong>#@po.Id</strong></td>
                                            <td>@po.SupplierName</td>
                                            <td>$@po.TotalCost.ToString("N2")</td>
                                            <td class="text-end">
                                                <span class="pill @(po.Status == POStatus.Received ? "pill-success" : po.Status == POStatus.Cancelled ? "pill-danger" : po.Status == POStatus.Draft ? "pill-neutral" : "pill-warning")">@po.Status</span>
                                            </td>
                                        </tr>
                                    }
                                </tbody>
                            </table>
                        </div>
                    }
                </div>
            </div>
        </div>
        <div class="col-lg-6">
            <div class="app-card h-100">
                <div class="app-card__header"><h2 class="app-card__title">Quick Links</h2></div>
                <div class="app-card__body">
                    <div class="row g-2">
                        <div class="col-6"><a href="/admin/users" class="btn-app btn-secondary-app w-100"><i class="bi bi-people"></i> User Management</a></div>
                        <div class="col-6"><a href="/admin/roles" class="btn-app btn-secondary-app w-100"><i class="bi bi-shield-lock"></i> Roles &amp; Permissions</a></div>
                        <div class="col-6"><a href="/admin/customers" class="btn-app btn-secondary-app w-100"><i class="bi bi-person-badge"></i> Customers</a></div>
                        <div class="col-6"><a href="/admin/products" class="btn-app btn-secondary-app w-100"><i class="bi bi-box-seam"></i> Products</a></div>
                        <div class="col-6"><a href="/manager/inventory" class="btn-app btn-secondary-app w-100"><i class="bi bi-clipboard-data"></i> Inventory</a></div>
                        <div class="col-6"><a href="/manager/promotions" class="btn-app btn-secondary-app w-100"><i class="bi bi-percent"></i> Promotions</a></div>
                    </div>
                </div>
            </div>
        </div>
    </div>
}
```

- [ ] **Step 2: Confirm the `@code { … }` block is unchanged**

Verify the file still ends with the original `@code` block (fields `IsLoading`, `TotalUsers`, … and `LoadDashboard()`), unchanged from the current version.

- [ ] **Step 3: Build to verify**

Run: `Get-Process SmartPOS -ErrorAction SilentlyContinue | Stop-Process -Force; dotnet build --nologo -v q`
Expected: `0 Error(s)`

- [ ] **Step 4: Commit**

```bash
git add Components/Pages/Admin/Dashboard.razor
git commit -m "feat(ui): rebuild Admin Dashboard on new component library; fix promotions link"
```

---

### Task 8: Run and visually verify (approval gate)

**Files:** none (verification only)

- [ ] **Step 1: Start the app**

Run:
```
Get-Process SmartPOS -ErrorAction SilentlyContinue | Stop-Process -Force
$env:ASPNETCORE_ENVIRONMENT="Development"
Start-Process dotnet -ArgumentList "run --project SmartPOS.csproj" -RedirectStandardOutput shell_out.txt -RedirectStandardError shell_err.txt -NoNewWindow
```
Then poll `http://localhost:5062/` until it responds.

- [ ] **Step 2: Log in as admin and verify the checklist**

Log in (`admin@smartpos.com` / `Admin@123`) and confirm on `/admin/dashboard`:
- Left sidebar renders with the 10 Admin nav items; active item highlighted (teal).
- Top bar shows page area, theme toggle, and a profile avatar with the **real** admin initials.
- Profile menu opens and shows the real name + an `Admin` role badge; Sign Out returns to `/login`.
- Theme toggle flips light ⇄ dark; reload the page → the chosen theme **persists** with no flash.
- Sidebar collapse button collapses to an icon rail; reload → collapsed state **persists**.
- Stat cards, both data tables, status pills, and empty states all look correct in **both** themes.
- Quick Links "Promotions" now navigates to `/manager/promotions` (no dead link).
- Resize to mobile width (<900px): sidebar becomes off-canvas via the menu button; overlay closes it.

- [ ] **Step 3: Verify no regression in other roles**

Log in as manager (`manager@smartpos.com` / `Manager@123`) and cashier (`cashier@smartpos.com` / `Cashier@123`); confirm their existing pages still load (old top-nav styling intact — they migrate in later sub-projects).

- [ ] **Step 4: Stop the app and clean up temp logs**

Run: `Get-Process SmartPOS -ErrorAction SilentlyContinue | Stop-Process -Force; Remove-Item shell_out.txt,shell_err.txt -ErrorAction SilentlyContinue`

- [ ] **Step 5: Report for approval**

Summarize the result (with what was observed in both themes) and ask the user to approve the look before rolling out to the remaining role areas. **Do not proceed to other sub-projects without approval.**

---

## Self-Review Notes

- **Spec coverage:** tokens/light+dark (T1) ✓; component library incl. `badge-role-*` (T2) ✓; App.razor wiring + theme script + no-FOUC + toggle JS (T3) ✓; one nav source of truth (T4) ✓; sidebar+topbar shell, collapse persistence, mobile off-canvas, real identity, sign-out (T5) ✓; AdminLayout→AppShell (T6) ✓; exemplar rebuild + broken-link fix (T7) ✓; build/visual verification incl. dark mode + no-regression (T8) ✓. Legacy CSS retained per transition note ✓.
- **Type consistency:** `NavItem(Label, Href, Icon, Match)` used identically in `RoleNav` and `AppShell`; JS helpers `smartposToggleTheme` / `smartposGetSidebarCollapsed` / `smartposSetSidebarCollapsed` defined in T3 and called in T5; `RoleNav.Admin` consumed in T6.
- **Non-goals respected:** no DB/model/service changes; only AdminLayout migrated; Customer auth-wiring deferred.
```
