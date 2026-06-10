# SmartPOS UI Overhaul — Foundation (Design System + App Shell)

**Date:** 2026-06-10
**Status:** Approved design, pending implementation plan
**Sub-project:** 1 of 5 (Foundation → Admin → Manager → Cashier → Customer)

## Context

SmartPOS is a Blazor Server POS app (.NET 10) with ~31 routable pages across four
role areas (Admin, Manager, Cashier, Customer). The current UI suffers from:

- **Three competing CSS systems** — `colors.css` (chocolate-brown/dusty-rose "Bakery"
  tokens), `modern-ui-v4.css` / `modern-ui.css` (a separate Material-Design palette),
  and `auth.css` (a third "vanilla/burnt-orange" palette). Pages pull from whichever,
  so nothing looks cohesive. Two fonts are loaded (Inter + Outfit).
- **Heavy inline styles** (~30 per page in Admin/Manager) with hardcoded hex values
  mixed with CSS variables. No reusable component classes.
- **Duplicated navigation** — four near-identical top-navbar components, each with its
  own copy of the same CSS.
- **Hardcoded identity** — the profile drawer shows "Admin User" instead of the
  logged-in user.

This sub-project builds the shared backbone (design system + app shell + component
library) that every subsequent role redesign depends on. It ends with **one exemplar
page (Admin Dashboard)** rebuilt on the foundation as a live approval gate.

## Decisions (locked with user)

| Decision | Choice |
| --- | --- |
| Visual direction | Modern neutral SaaS |
| Accent color | Emerald / teal |
| Navigation layout | Left sidebar + slim top bar |
| Theme modes | Light **and** dark, with a toggle |
| Density | Balanced |
| Overall scope | Full per-page redesign of all ~31 pages + fix functional gaps |
| Execution | Foundation-first, then role-by-role, with a live preview gate |

## Goals

1. Replace the three competing CSS systems with **one** token system supporting light
   and dark modes, anchored on a neutral slate base + emerald/teal accent.
2. Replace the four duplicated role navbars + per-role layouts with **one shared
   `AppShell`** (collapsible left sidebar + top bar), parameterized by role.
3. Provide a **reusable component-class library** so page redesigns compose from shared
   parts instead of inline styles.
4. Wire a **theme toggle** (persisted) with no flash-of-unstyled-content.
5. Show the **real logged-in user** (name, role, initials) in the shell.
6. Prove the look on **Admin Dashboard** in both themes before any rollout.

## Non-goals (this sub-project)

- No database, model, or service changes.
- No new pages (missing/incomplete pages are handled in their role sub-projects).
- No Customer auth-wiring (the hardcoded `customerId = 1` fix lands in the Customer
  sub-project).
- Bootstrap is **retained** for now (grid/utilities); pages depend on it. It may be
  trimmed in a later pass.
- Redesigning Admin pages other than the Dashboard exemplar (Admin sub-project).

## Architecture

### File structure

New / changed files:

- `wwwroot/css/theme.css` *(new)* — design tokens. `:root` = light theme;
  `[data-theme="dark"]` = dark overrides. Palette, typography, spacing, radius, shadow,
  z-index scales.
- `wwwroot/css/components.css` *(new)* — reusable component classes.
- `Components/App.razor` *(edit)* — **add** `theme.css` + `components.css` and an inline
  pre-paint theme-init script that sets `data-theme` on `<html>`. The legacy stylesheets
  (`colors.css`, `modern-ui.css`, `modern-ui-v4.css`, `auth.css`) and the Outfit font
  link **stay linked during the transition** (see Transition note); they are removed at
  the end of the final role sub-project. New tokens use distinct names (`--bg`,
  `--surface`, `--accent`, …) that do not collide with the legacy tokens
  (`--surface-0`, `--color-button-primary`, …), so the two coexist safely.
- `Components/Layout/AppShell.razor` *(new)* — shared sidebar + top-bar shell.
- `Components/Layout/NavItem.cs` *(new)* — small record describing one sidebar link
  (label, href, icon, optional match-mode).
- `Components/Layout/RoleNav.cs` *(new)* — static config returning the `NavItem` list +
  role accent + brand label for each role.
- `Components/Layout/Admin/AdminLayout.razor` *(edit)* — becomes a thin wrapper that
  passes Admin's nav config to `AppShell`. (Manager/Cashier/Customer layouts get the
  same treatment in their sub-projects; for Foundation, only AdminLayout is converted —
  the others keep working unchanged until their phase.)
- `Components/Pages/Admin/Dashboard.razor` *(edit)* — exemplar rebuild.

Files retired from `App.razor`'s `<head>` (kept on disk until all roles migrate, then
deleted): `colors.css`, `modern-ui.css`, `modern-ui-v4.css`, `auth.css`.

> **Transition note:** Only `AdminLayout` switches to `AppShell` in this phase. The
> other three role layouts and their pages still reference the legacy CSS classes, so
> the legacy stylesheets stay linked **alongside** the new ones throughout the overhaul
> and are deleted only at the end of the final role sub-project. Because the new tokens
> use distinct names, there is no collision. One known limitation during the transition:
> **dark mode fully applies only to migrated areas** (Foundation/Admin first); not-yet-
> migrated Manager/Cashier/Customer pages remain light-styled until their phase. This is
> expected and acceptable.

### Theming

Token groups in `theme.css`:

- **Surfaces:** `--bg`, `--surface`, `--surface-2`, `--surface-3`, `--border`,
  `--border-strong`.
- **Text:** `--text`, `--text-muted`, `--text-subtle`, `--text-on-accent`.
- **Accent:** `--accent`, `--accent-hover`, `--accent-soft` (tinted bg),
  `--accent-contrast`.
- **Semantic:** `--success`, `--warning`, `--danger`, `--info` (+ `*-soft` bg variants).
- **Role accents:** `--role-admin`, `--role-manager`, `--role-cashier`,
  `--role-customer` — used only for the sidebar header tint + role badge.
- **Typography:** `--font-sans: 'Inter', system-ui, sans-serif;` + size scale
  (`--fs-xs` … `--fs-3xl`) and weights.
- **Spacing:** `--sp-1` … `--sp-8` (4px base, balanced density).
- **Radius:** `--radius-sm/md/lg/xl/full`.
- **Shadow:** `--shadow-xs/sm/md/lg` (subtle in light, ring-based in dark).
- **Z-index:** `--z-sidebar`, `--z-topbar`, `--z-overlay`, `--z-modal`, `--z-toast`.

Dark mode redefines only the surface/text/border/shadow tokens (and softens accent
tints); accent hue stays constant.

**Accent palette (concrete starting values, tunable during preview):**
emerald/teal — `--accent: #0d9488` (light), hover `#0f766e`, soft `rgba(13,148,136,.12)`;
dark `--accent: #2dd4bf`. Neutral base — slate (`#f8fafc`/`#ffffff` light surfaces,
`#0b1220`/`#111827` dark surfaces, `#e2e8f0`/slate borders).

**Theme toggle:** key `smartpos-theme` in `localStorage` (`"light"` | `"dark"`; default
follows `prefers-color-scheme`). Inline `<head>` script sets `document.documentElement.dataset.theme`
before first paint. A top-bar button flips it via a tiny JS helper and persists.

### App shell

`AppShell.razor` parameters:

- `Role` (enum/string), `Brand` (text), `NavItems` (`IReadOnlyList<NavItem>`),
  `RoleAccentVar` (which `--role-*` token to tint with), and `ChildContent` (`@Body`).

Structure:

- **Sidebar** (`--z-sidebar`): brand + role label header (tinted with role accent);
  scrollable nav list using `NavLink` with `NavLinkMatch` for active state; collapse
  toggle that switches to an icon-only rail; collapsed state persisted in `localStorage`
  (`smartpos-sidebar`). On mobile (<= 900px) the sidebar is off-canvas with an overlay.
- **Top bar** (`--z-topbar`): left = sidebar/mobile toggle + optional page-title slot;
  right = theme toggle + profile menu (avatar with initials → dropdown showing real
  name, role badge, and Sign Out).
- **Content region:** `<main>` with the routed `@Body`, max-width container, balanced
  padding.

**Identity:** `AppShell` injects `AuthenticationStateProvider` (or uses `<AuthorizeView>`)
to read the user's name and role claims (set by the JWT in `CustomAuthStateProvider`),
deriving initials. Sign Out clears the token and redirects to `/login` (reusing the
existing `signOut()` path).

`RoleNav.cs` centralizes each role's nav items so there is exactly one source of truth.
Admin's list mirrors the current Admin navbar (Dashboard, Analytics, Products,
Categories, Suppliers, Users, Roles, Customers, Audit Logs, Sentiment).

### Component library (`components.css`)

Classes (all theme-token driven, both modes):

- Layout/structure: `.page-header` (title + subtitle + actions slot), `.card`,
  `.card-header`, `.card-body`, `.toolbar` (filter/action bar), `.grid` helpers.
- Data: `.stat-card` (label, value, delta up/down, icon), `.data-table` (sticky header,
  row hover, responsive horizontal scroll, optional zebra), `.empty-state`
  (icon + message + optional action), `.skeleton` (loading shimmer).
- Controls: `.btn` + `.btn-primary` (emerald), `.btn-secondary`, `.btn-ghost`,
  `.btn-danger`, `.btn-sm`/`.btn-lg`, `.btn-icon`; `.form-group`, `.form-label`,
  `.form-control`, `.select`, `.checkbox`, `.switch`, `.input-group`.
- Indicators: `.badge` + `.badge-success/warning/danger/info/neutral`, status pills, and
  **`.badge-role-admin/manager/cashier/customer`** (fixes the currently-undefined
  classes); `.chip`; `.avatar`.
- Overlays: `.modal` (overlay + centered panel + header/body/footer), `.tabs`/`.tab`,
  `.toast`.

## Data flow

- Theme: localStorage ⇄ `<html data-theme>` ⇄ CSS token resolution. Toggle button →
  JS helper → updates attribute + storage.
- Sidebar collapse: localStorage ⇄ CSS class on shell root.
- Identity: `CustomAuthStateProvider` (JWT claims) → `AuthenticationState` → `AppShell`
  renders name/role/initials; Sign Out → clear token → `/login`.

## Error handling / edge cases

- **FOUC:** pre-paint inline script guarantees `data-theme` is set before CSS applies.
- **Unauthenticated render:** `AppShell` falls back to neutral placeholder initials if
  no user (should not occur behind `[Authorize]`, but must not throw).
- **No JS / first interactive render:** initial theme comes from the inline script;
  toggle becomes functional once the circuit connects (acceptable for Blazor Server).
- **Long names / overflow:** name and nav labels truncate with ellipsis.
- **Mobile:** off-canvas sidebar closes on nav click and on overlay click.

## Testing / verification

Manual visual verification (UI work), run against the live app:

1. App builds clean (0 errors) and starts.
2. Admin Dashboard renders on the new shell with sidebar + top bar.
3. Theme toggle flips light ⇄ dark; choice **persists across reload** with no flash.
4. Sidebar collapses to a rail and persists; mobile off-canvas works.
5. Top-bar profile shows the **real** logged-in user's name + role; Sign Out works.
6. Stat cards, a data table, badges, and an empty state all render correctly in **both**
   themes.
7. Manager/Cashier/Customer areas still load (unchanged) — no regression from the shared
   CSS additions.

## Rollout (subsequent sub-projects, for context)

After this foundation is approved on the exemplar: Admin (~13 pages) → Manager (~5) →
Cashier (~4) → Customer (~9, incl. auth-wiring + broken-link fixes). Each converts its
`*Layout` to `AppShell`, redesigns its pages on the component library, and removes its
dependence on the legacy stylesheets. The legacy CSS files are deleted at the end of the
final sub-project.
