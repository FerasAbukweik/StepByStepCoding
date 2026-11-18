---
name: Technical Precision IDE
colors:
  surface: '#10141a'
  surface-dim: '#10141a'
  surface-bright: '#353940'
  surface-container-lowest: '#0a0e14'
  surface-container-low: '#181c22'
  surface-container: '#1c2026'
  surface-container-high: '#262a31'
  surface-container-highest: '#31353c'
  on-surface: '#dfe2eb'
  on-surface-variant: '#c0c7d4'
  inverse-surface: '#dfe2eb'
  inverse-on-surface: '#2d3137'
  outline: '#8b919d'
  outline-variant: '#414752'
  surface-tint: '#a2c9ff'
  primary: '#a2c9ff'
  on-primary: '#00315c'
  primary-container: '#58a6ff'
  on-primary-container: '#003a6b'
  inverse-primary: '#0060aa'
  secondary: '#b1c5ff'
  on-secondary: '#002c71'
  secondary-container: '#0059d4'
  on-secondary-container: '#d1dcff'
  tertiary: '#67df70'
  on-tertiary: '#00390d'
  tertiary-container: '#40ba51'
  on-tertiary-container: '#004411'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#d3e4ff'
  primary-fixed-dim: '#a2c9ff'
  on-primary-fixed: '#001c38'
  on-primary-fixed-variant: '#004882'
  secondary-fixed: '#dae2ff'
  secondary-fixed-dim: '#b1c5ff'
  on-secondary-fixed: '#001947'
  on-secondary-fixed-variant: '#00419f'
  tertiary-fixed: '#83fc89'
  tertiary-fixed-dim: '#67df70'
  on-tertiary-fixed: '#002105'
  on-tertiary-fixed-variant: '#005317'
  background: '#10141a'
  on-background: '#dfe2eb'
  surface-variant: '#31353c'
typography:
  headline-lg:
    fontFamily: Geist
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 28px
    letterSpacing: -0.015em
  headline-md:
    fontFamily: Geist
    fontSize: 16px
    fontWeight: '600'
    lineHeight: 24px
    letterSpacing: -0.01em
  headline-sm:
    fontFamily: Geist
    fontSize: 14px
    fontWeight: '600'
    lineHeight: 20px
    letterSpacing: -0.005em
  title-md:
    fontFamily: Geist
    fontSize: 13px
    fontWeight: '600'
    lineHeight: 18px
  body-lg:
    fontFamily: Geist
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  body-md:
    fontFamily: Geist
    fontSize: 13px
    fontWeight: '400'
    lineHeight: 18px
  body-sm:
    fontFamily: Geist
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 16px
  label-md:
    fontFamily: Geist
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 16px
  label-sm:
    fontFamily: Geist
    fontSize: 11px
    fontWeight: '500'
    lineHeight: 14px
    letterSpacing: 0.02em
  code-md:
    fontFamily: JetBrains Mono
    fontSize: 13px
    fontWeight: '400'
    lineHeight: 20px
  code-sm:
    fontFamily: JetBrains Mono
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 18px
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  gutter: 0.5rem
  gutter-compact: 0.25rem
  margin: 0.75rem
  margin-panel: 0.5rem
  space-xs: 0.25rem
  space-sm: 0.5rem
  space-md: 0.75rem
  space-lg: 1rem
  space-xl: 1.5rem
---

## Brand & Style

This design system is engineered specifically for university computer science instruction, live code execution, and proctored technical evaluations. The visual language rejects decorative trends, consumer-grade whitespace inflation, and distracting ambient glows in favor of high-density utility, predictable layout structures, and rigorous visual ergonomics.

Targeting engineering students, teaching assistants, and university faculty, the aesthetic reflects the functional austerity of tools like GitHub, VS Code, and Linear. The interface operates as an invisible, zero-latency instrument where syntax readability, execution telemetry, and grading state take absolute precedence.

### Core Principles
- **Utilitarian Density:** Tight vertical rhythm (4px grid baseline) and compact data presentation maximize viewport space for split-pane editors, side-by-side terminal buffers, and test case tables.
- **Surface Hierarchy as Architecture:** Rather than relying on drop shadows, spatial depth is established purely through surface luminosity gradations—anchoring focus to the code editing canvas while receding navigation chrome.
- **Low-Fatigue Dark Contrast:** Specially balanced dark surfaces reduce ocular strain during multi-hour programming exams under varying ambient lighting conditions.

## Colors

The system uses a dark palette defined by precise luminosity steps designed to isolate editing planes from structural panels.

### Surface Tiers
- **Editor & Terminal Canvas:** `#0B0F14` — Deepest sink for zero-distraction syntax reading.
- **Application Canvas Base:** `#0D1117` — Global shell background.
- **Panels, Sidebars, and Tables:** `#161B22` — Raised structural surfaces.
- **Interactive Hover & Elevated Rows:** `#1C232C` — State feedback for transient selections.
- **Active Navigation / Active Row:** `#1B2740` with an accent anchor.

### Boundaries & Rules
- **Subtle Dividers:** `#21262D` — For non-interactive data row separators and internal component sectioning.
- **Structural Borders:** `#30363D` — 1px borders delineating resizable split panes and modal containers.

### Typography & Content
- **Text Primary:** `#E6EDF3` — Highest contrast readability for active code and primary labels.
- **Text Secondary:** `#8B949E` — Metadata, gutter line numbers, and inactive panel headers.
- **Text Muted / Disabled:** `#6E7681` — Inactive commands and unselected tabs.

### Accent & Action
- **Selection & Focus Ring:** `#58A6FF` — High-visibility keyboard accessibility indicators and cursor caret.
- **Interactive Action:** `#2F6FEB` (Hover: `#4A83F0`) — Primary submission and execution buttons.

### Status Telemetry
- **Success / Live Evaluation:** `#3FB950` with `#12261A` container tint.
- **Warning / Non-Fatal Compiler Alerts:** `#D29922` with `#261C08` container tint.
- **Error / Failing Test Cases:** `#F85149` with `#281014` container tint.
- **Neutral / Concluded:** `#8B949E` with `#161B22` container tint.

## Typography

The type scale is optimized around a compact 13px base body text size, catering to multi-pane split layouts where vertical and horizontal space are constrained.

### Application Rules
- **UI Elements:** All interface chrome, breadcrumbs, modal headers, and table cells utilize `Geist` to maintain compact geometric precision.
- **Code & Terminals:** All code execution output, editor text, test inputs, and diffs must render using a monospace stack (`JetBrains Mono`, `SFMono-Regular`, `Menlo`, `Consolas`). Tab size is strictly locked to 2 or 4 characters depending on the active language configuration.
- **Numbers & Metrics:** Use tabular numbers (`font-variant-numeric: tabular-nums`) across all test results, execution timers, line counters, and submission point allocations to preserve column alignment.

## Layout & Spacing

The layout model enforces a strict, rigid 4px metric rhythm designed to support a multi-pane split-screen architecture (IDE editor, interactive execution terminal, automated test output panel, lecture video/prompt sidebar).

### Layout & Resizing
- **Splitter Architecture:** Canvas divisions use explicit 1px boundary tracks (`#30363D`) with dynamic drag handles. Zero margin buffers exist between sibling operational panels.
- **Table Density:** Submission grids and test matrices enforce a strict row height between 32px and 36px to maximize the number of visible records above the fold.
- **Grid Disciplines:** Mobile transitions collapse split panes into persistent bottom tab navigation where code editors and terminal sessions exist as isolated full-screen views. Desktop panes prioritize a horizontal left-to-right hierarchy: Sidebar Navigation (compact 48px or expanded 240px) → Specification / Instructions → Code Buffer → Output / Feedback.

## Elevation & Depth

This design system intentionally eliminates diffuse drop shadows and soft skeuomorphic blurs. Depth is communicated strictly via structural 1px borders and surface luminosity steps.

### Layer Mechanics
- **Ground Floor (`#0B0F14`):** Code editor canvases, input terminals, and shell execution zones.
- **Mid Deck (`#0D1117`):** The primary view container and structural grid framework.
- **Top Deck (`#161B22`):** Persistent sidebars, table toolbars, pane headers, and unselected status tabs.
- **Overlays & Popovers:** Autocomplete dropdowns, context menus, and diagnostic tooltips share the `#161B22` surface, bound by a crisp 1px border of `#30363D` with a high-falloff directional shadow (`0 4px 12px rgba(0, 0, 0, 0.5)`).

### Focus & Active Signaling
- Active split panes do not elevate via shadows; instead, their tab header presents a persistent 1px accent underline or top stripe in `#2F6FEB`.
- Keyboard focus is delineated strictly by a non-blurry, high-visibility 2px outer outline in `#58A6FF` with a 1px offset.

## Shapes

The design system employs a soft, disciplined corner radius profile (level 1).

### Radius Hierarchy
- **Panels, Split Dividers, and Full Bleed Screens:** `0px` radius. Flush edges prevent visual distraction and maximize usable pixel area.
- **Interactive Controls (Buttons, Inputs, Selectors):** `0.25rem` (4px). Standardized for inputs, run buttons, and interactive badges.
- **Floating Modals, Code Cards, and Menus:** `0.5rem` (8px). Softens floating structural boundaries without consuming internal padding space.
- **Pill Badges:** Never used for major structural containers. Status dots remain 6px or 8px circular shapes (`border-radius: 9999px`) inside tabular rows.

## Components

### Buttons
- **Primary Action (Run Code / Submit):** Surface `#2F6FEB`, text `#FFFFFF`, border `1px solid transparent`, corner radius `4px`. Hover: `#4A83F0`. Active: `#2658BE`.
- **Secondary Action (Stash, Format, Reset):** Surface `#161B22`, text `#E6EDF3`, border `1px solid #30363D`. Hover: surface `#1C232C`, border `#8B949E`.
- **Destructive (Terminate Process, Reset Exam):** Surface `#281014`, text `#F85149`, border `1px solid rgba(248, 81, 73, 0.4)`. Hover: surface `#F85149`, text `#FFFFFF`.
- **Dimensions:** Compact 28px height for secondary controls; 32px for primary header CTAs. Font: `label-md`.

### Navigation & List Rows
- **List Items (Files, Submission History, Questions):** Height 32px–36px. Default background transparent, text `#8B949E`.
- **Hover Item:** Background `#1C232C`, text `#E6EDF3`.
- **Selected Item:** Background `#1B2740`, text `#E6EDF3`, with a solid `2px` left-edge indicator in `#2F6FEB`.

### Chips & Badges
- **Status (Live / Active Exam):** Surface `#12261A`, border `1px solid #3FB950`, text `#3FB950`. Includes a 6px pulse dot `#3FB950`.
- **Test Passed:** Surface `#12261A`, text `#3FB950`, border `1px solid rgba(63, 185, 80, 0.3)`.
- **Test Failed:** Surface `#281014`, text `#F85149`, border `1px solid rgba(248, 81, 73, 0.3)`.
- **Badge Shape:** Compact padding `2px 6px`, radius `4px`, font `label-sm`.

### Form Controls & Inputs
- **Text & Search Fields:** Height 28px–32px. Background `#0B0F14`, border `1px solid #30363D`, text `#E6EDF3`, placeholder `#6E7681`. Focus: border `#58A6FF`, outline `2px solid rgba(88, 166, 255, 0.3)`.
- **Checkboxes & Radios:** 14px × 14px footprint. Background `#0B0F14`, border `1px solid #30363D`, radius `3px` (checkbox) or circular (radio). Checked state: `#2F6FEB` with white checkmark icon.

### Cards & Tables (Gradescope/Submission Tables)
- **Table Container:** Surface `#161B22`, border `1px solid #30363D`, radius `4px`.
- **Table Headers:** Height 32px, background `#0D1117`, text `#8B949E`, font `label-sm`, border-bottom `1px solid #30363D`.
- **Table Rows:** Height 32px–36px, border-bottom `1px solid #21262D`. Monospace text for runtime/memory usage metrics (`code-sm`). Hover background: `#1C232C`.

### Specialized IDE Elements
- **Editor Gutter:** Background `#0B0F14`, text `#484F58`, font `code-md`, right border `1px solid #21262D`. Current execution line number highlights in `#E6EDF3`.
- **Active Cursor:** Width 2px, color `#58A6FF`.
- **Terminal Buffer:** Monospace text, zero margins, `#0B0F14` background with colored exit code chips (Code 0 in `#3FB950`, non-zero in `#F85149`).