# Pretendard Slide Design System (16:9)

## Overview

This is a **photography-first, chrome-light slide design system** built for academic and product presentations. Every slide is a stack of edge-to-edge "tiles" — alternating light and dark canvases, each centered on a confident headline, a one-line subtitle, and dense, well-organized content. Nothing competes with the content. Typography is confident but quiet; color is either pure white, an off-white parchment, or a near-black tile; interactive/emphasis elements use a single, quiet blue.

Density is balanced — each slide carries meaningful content from top to bottom of the body area, **never leaving a vast empty band at the lower half**. Decorative chrome is forbidden — no borders for borders' sake, no decorative gradients, no decorative frames, no shadows on headlines. Elevation appears only when a product image or figure rests on a surface (a single soft `rgba(0, 0, 0, 0.22) 3px 5px 30px` drop for visual weight). The result feels like a museum gallery wall: the surface disappears and the artifact takes over.

**Key Characteristics:**

- All slides are **16:9** (13.333 × 7.5 inches / 1920 × 1080 px). No other aspect ratio is permitted.
- **Pretendard is the only typeface used** across the entire system. No fallback to SF Pro, Inter, Noto, or system fonts in production output. Pretendard's full weight ladder (100–900) is available; the system uses a deliberate subset (see Typography).
- **No logo on any slide.** No brand mark, no watermark, no presenter mark. The content carries itself.
- **Header zone (chapter / title / subtitle) is positionally locked across every slide.** A reader's eye should land in exactly the same coordinate on every page to find the page's identity.
- **Body area is densely populated.** The lower 60% of every slide must carry content. If a body section is short, expand with supporting structure (sub-points, a small data table, an inline figure caption, a footnote band) — never leave a tall blank band beneath the title.
- Alternating full-bleed tile sections: white/parchment ↔ near-black, with the color change itself acting as the section divider.
- Single accent (`{colors.primary}` — #0066cc) carries every emphasis element. No second brand color exists.
- Two button/chip grammars: pill (`{rounded.pill}`) for "actions/links" and compact rect (`{rounded.sm}`) for "utility/labels."
- Whisper-soft elevation used only when an image needs to breathe — exactly one drop-shadow recipe in the entire system.
- Section rhythm across a deck: cover → light section opener → dark content tile → light content tile → dark tile → parchment summary. A predictable pulse.

---

## The Locked Header Zone (Critical — Read First)

Every content slide carries a three-line header in **the exact same position**, on every page, regardless of which template variant is used. This consistency is the single most important rule of the deck.

### Header coordinates (16:9, 1920 × 1080 px reference grid)

| Element | x (left edge) | y (top edge) | Width | Height | Token |
|---|---|---|---|---|---|
| Chapter name | 80 px | 56 px | 1760 px | 24 px | `{slot.chapter}` |
| Page title | 80 px | 104 px | 1760 px | 80 px | `{slot.title}` |
| Page subtitle | 80 px | 200 px | 1760 px | 36 px | `{slot.subtitle}` |
| Header divider hairline | 80 px | 264 px | 240 px | 1 px | `{slot.divider}` |
| Body region begins | 80 px | 296 px | 1760 px | 720 px | `{slot.body}` |
| Page-number footer | 1840 px | 1024 px | 40 px | 24 px | `{slot.page-num}` |

### Header typography

- **Chapter name** (`{slot.chapter}`) — `{typography.chapter-tag}`: Pretendard SemiBold (600), 18 px, line-height 1.2, letter-spacing -0.36 px, color `{colors.primary}` on light surfaces / `{colors.primary-on-dark}` on dark surfaces. ALL CAPS is **not** used; render exactly as authored (e.g., "Chapter 3 · Camera Intrinsics"). Always one line — truncate or shorten before wrapping.
- **Page title** (`{slot.title}`) — `{typography.title-display}`: Pretendard Bold (700), 56 px, line-height 1.07, letter-spacing -1.12 px, color `{colors.ink}` / `{colors.body-on-dark}`. One line preferred; two-line wrap permitted only when the title is genuinely long, and the second line stays within the 80–1840 horizontal band.
- **Page subtitle** (`{slot.subtitle}`) — `{typography.subtitle-lead}`: Pretendard Regular (400), 24 px, line-height 1.3, letter-spacing -0.24 px, color `{colors.ink-muted-80}` / `{colors.body-muted}`. One line; if absent, the slot stays empty (do **not** push the title down to fill the space — the locked y-coordinate is sacred).
- **Header divider** (`{slot.divider}`) — 1 px hairline, color `{colors.hairline}` on light / `rgba(255,255,255,0.16)` on dark. Always 240 px wide, anchored at x = 80. Acts as the visual baseline of the header zone.

### Header rules (non-negotiable)

1. The y-coordinates above are **locked** on every content slide. If a slide has no chapter name, the chapter slot stays at y=56 and is left blank — the title does **not** rise into it.
2. The chapter name updates only when crossing a chapter boundary; within a chapter, every page repeats the same chapter string.
3. The divider hairline is present on every content slide. It is the visual line that separates header from body.
4. Cover, section-opener, and full-bleed image slides are the only templates where the header zone may be omitted entirely — see Templates below.

---

## Body Density Rule (Critical — Read Second)

The body region (`{slot.body}`) spans y=296 to y=1016 — roughly 720 px tall, occupying the bottom two-thirds of the slide. **This region must feel populated**, not abandoned.

### Density floor

- The body must use **at least 70% of its vertical height** for meaningful content (text, figures, tables, code blocks, diagrams, captions). A body area whose content stops at y=600 with 416 px of empty white below is not acceptable.
- If the primary content is short (e.g., a single quote, a single equation, a 3-bullet list), enrich the slide with supporting structure that earns its place:
  - A right-side caption column or annotation rail
  - An inline figure or schematic
  - A small-print footnote band along the bottom (y≈960, in `{typography.fine-print}`)
  - A horizontal "key takeaways" strip
  - A 2- or 3-column layout instead of a single column
- Do **not** pad with decorative shapes, oversized whitespace, or lorem-ipsum. Density must come from real content.

### Density ceiling

- The body must not feel claustrophobic. Maintain at least 24 px of breathing room between the divider hairline (y=264) and the first body element. Maintain at least 24 px of bottom margin above the page-number footer.
- Body type sizes do not shrink below 18 px. If content does not fit at the minimum size, split across two slides.

### Body composition presets

The body region accepts one of these compositions; the chosen composition is consistent within a slide:

1. **Two-column 60/40** — primary text/figure left (1056 px wide), supporting column right (688 px wide), 16 px gutter.
2. **Three-column 33/33/33** — three equal columns at 565 px wide, 32 px gutters. Used for parallel comparisons (e.g., "NeRF / 3DGS / Gaussian Surfels").
3. **Hero figure + caption strip** — a figure occupying the upper 60% of the body (~432 px tall), with a caption rail below (~288 px) holding a numbered list, references, or annotation.
4. **Stacked bands** — two horizontal bands inside the body, each ~336 px tall, used when two distinct ideas share one slide.
5. **Centered single column** — only used when the content is genuinely a centered statement (e.g., a section quote on a section-opener). On content slides, this is rare.

---

## Colors

### Brand & Accent

- **Action Blue** (`{colors.primary}` — #0066cc): The single accent. All inline links, all emphasis chips, all callout strokes. No second accent exists.
- **Focus Blue** (`{colors.primary-focus}` — #0071e3): A marginally brighter sibling reserved for selected-state borders on configurator-style chips.
- **Sky Link Blue** (`{colors.primary-on-dark}` — #2997ff): The dark-tile variant. Action Blue would lose contrast against `{colors.surface-tile-1}`, so all blue marks on dark surfaces use this brighter blue instead.

### Surface

- **Pure White** (`{colors.canvas}` — #ffffff): The dominant canvas. Most content slides.
- **Parchment** (`{colors.canvas-parchment}` — #f5f5f7): The signature off-white. Used for alternating light slides and section-opener slides. Just different enough from white to create deck-level rhythm.
- **Pearl** (`{colors.surface-pearl}` — #fafafc): Near-white used for "ghost" inline cards on parchment surfaces, so the card still reads as elevated.
- **Near-Black Tile 1** (`{colors.surface-tile-1}` — #272729): The primary dark-tile surface.
- **Near-Black Tile 2** (`{colors.surface-tile-2}` — #2a2a2c): A micro-step lighter — used when two dark tiles sit consecutively in a deck and need separation.
- **Near-Black Tile 3** (`{colors.surface-tile-3}` — #252527): A micro-step darker — used for embedded video/figure frames inside a tile.
- **Pure Black** (`{colors.surface-black}` — #000000): Reserved for full-bleed photographic title overlays only. Not a body surface.

### Text

- **Ink** (`{colors.ink}` — #1d1d1f): Every headline and body paragraph on light surfaces. Chosen instead of pure black to keep the page feeling photographic rather than printed.
- **Body** (`{colors.body}` — #1d1d1f): Same hex as Ink — one near-black tone for all light-surface text.
- **Body On Dark** (`{colors.body-on-dark}` — #ffffff): All headline and body text on dark tiles.
- **Body Muted** (`{colors.body-muted}` — #cccccc): Secondary copy on dark tiles where pure white would be too loud.
- **Ink Muted 80** (`{colors.ink-muted-80}` — #333333): Subtitle text and Pearl-card body. Slightly softer than pure black.
- **Ink Muted 48** (`{colors.ink-muted-48}` — #7a7a7a): Disabled or de-emphasized labels and legal/footnote fine-print.

### Hairlines & Borders

- **Divider Soft** (`{colors.divider-soft}` — #f0f0f0): Functions as a ring shadow rather than a hard line. Often applied as `rgba(0, 0, 0, 0.04)`.
- **Hairline** (`{colors.hairline}` — #e0e0e0): The 1 px hairline used on the header divider, on inline cards, and on table rules.

### Decorative Gradients

**None.** Atmosphere on figure slides comes from the photographic content of the figure itself, not from CSS/PPT gradient overlays. The deck has zero gradient tokens.

---

## Typography

### Font Family

- **Display & UI**: `Pretendard` — the single typeface for the entire system. Pretendard is a hybrid Korean-Latin sans-serif with full coverage at every weight, optimized for legibility at both display and body sizes. Use the full font family (not Pretendard Variable) so weight selection is unambiguous in PowerPoint.
- **Available weights** (installed from the user's font upload):
  - 100 — Thin
  - 200 — ExtraLight
  - 300 — Light
  - 400 — Regular
  - 500 — Medium *(deliberately unused — see Principles)*
  - 600 — SemiBold
  - 700 — Bold
  - 800 — ExtraBold
  - 900 — Black
- **No fallback fonts** appear in production output. SF Pro, Inter, Noto Sans KR, system-ui, etc., are forbidden. If Pretendard fails to load on a target machine, install the font from the supplied OTF/TTF files before exporting.
- **Numerals**: Use Pretendard's default lining figures; tabular figures are activated only on data tables (via `font-feature-settings: "tnum"` where supported).

### Hierarchy

| Token | Size | Weight | Line Height | Letter Spacing | Use |
|---|---|---|---|---|---|
| `{typography.cover-display}` | 96 px | 800 | 1.05 | -2.40 px | Cover-slide title |
| `{typography.section-display}` | 72 px | 700 | 1.07 | -1.44 px | Section-opener slide title |
| `{typography.title-display}` | 56 px | 700 | 1.07 | -1.12 px | Page title (locked header slot) |
| `{typography.display-md}` | 40 px | 700 | 1.10 | -0.80 px | Big in-body callout heading |
| `{typography.subtitle-lead}` | 24 px | 400 | 1.30 | -0.24 px | Page subtitle (locked header slot) |
| `{typography.lead}` | 22 px | 400 | 1.40 | -0.22 px | Lead paragraph at the top of a body section |
| `{typography.body-strong}` | 20 px | 600 | 1.40 | -0.20 px | In-body emphasized run-in or section head |
| `{typography.body}` | 20 px | 400 | 1.50 | -0.20 px | Default body paragraph (minimum readable size) |
| `{typography.body-tight}` | 18 px | 400 | 1.45 | -0.18 px | Dense body paragraph in two- or three-column layouts |
| `{typography.chapter-tag}` | 18 px | 600 | 1.20 | -0.36 px | Chapter name (locked header slot) |
| `{typography.caption-strong}` | 16 px | 600 | 1.30 | -0.16 px | Figure/table caption, emphasized |
| `{typography.caption}` | 16 px | 400 | 1.30 | -0.16 px | Figure/table caption, default |
| `{typography.fine-print}` | 14 px | 400 | 1.40 | -0.14 px | Footnotes, references, equation source labels |
| `{typography.page-num}` | 12 px | 500 | 1.00 | 0 | Page number footer (one of the rare uses of weight 500) |

### Principles

- **Negative letter-spacing at display sizes.** Every Pretendard size at 18 px and up carries a slight tracking tighten (`-0.14 → -2.40 px`). This produces a confident, "set-in-stone" headline cadence. Never apply negative tracking below 18 px; Pretendard's defaults read best at small sizes without modification.
- **Body copy at 20 px minimum.** Slides are read from a distance; the floor for paragraph type is 20 px (or 18 px in dense multi-column layouts). Never go below 18 px for body content. Captions/footnotes can drop to 16/14 px.
- **The weight ladder is 400 / 600 / 700 / 800.** Mid-weight readings (e.g., body-strong, chapter tag) always use 600 (SemiBold). Headlines use 700 (Bold) by default. The cover uses 800 (ExtraBold). Pretendard Medium (500) is deliberately reserved — it appears only on the page-number footer and on tabular figure captions where it adds subtle weight without competing with Bold headlines.
- **Weights 100, 200, 300, 900 are not used.** Pretendard's lighter weights muddy at projection scale; Black (900) is unnecessarily aggressive next to the system's quiet voice.
- **Korean and Latin mix gracefully.** Pretendard is designed for this. Do not switch fonts mid-line for English/Korean — Pretendard handles both. Do not insert manual kerning between Korean and Latin runs.
- **Line-height is context-specific.** Display sizes (40 px+): 1.05–1.10 (tight). Body: 1.40–1.50. Footnotes: 1.40. Page numbers: 1.00.
- **No italics, no underlines.** Emphasis is carried by weight (400 → 600) or by color (`{colors.ink}` → `{colors.primary}`). Italics are absent from the system. Underlines appear only on inline hyperlinks in printed handouts, never on screen.

---

## Layout

### Spacing System

- **Base unit:** 8 px. Sub-base values (4, 6) are used for tight typographic adjustments; structural layout snaps to 8 / 16 / 24 / 32 / 48 / 64 / 80.
- **Tokens:** `{spacing.xxs}` 4 px · `{spacing.xs}` 8 px · `{spacing.sm}` 16 px · `{spacing.md}` 24 px · `{spacing.lg}` 32 px · `{spacing.xl}` 48 px · `{spacing.xxl}` 64 px · `{spacing.edge}` 80 px.
- **Slide edge inset:** `{spacing.edge}` 80 px on left and right; 56 px top and 64 px bottom. The header header begins at the 56 px top inset.
- **Body-element gutters:** 24 px between paragraphs, 32 px between body sections, 48 px between two stacked bands inside the body region.
- **Card padding:** `{spacing.md}` (24 px) inside inline body cards.
- **Universal rhythm constants:** the 24 px subtitle-to-divider gap, the 32 px divider-to-body gap, and the 64 px right-side margin on header text.

### Grid & Container

- **Slide canvas:** 1920 × 1080 px (16:9). All measurements in this spec assume this canvas. For PowerPoint, set slide size to "Widescreen (16:9)" at 13.333 × 7.5 in.
- **Content band:** x = 80 to x = 1840 (1760 px wide). All header text, all body text, all figures live inside this band.
- **Column patterns inside the body region**:
  - 12-column grid spanning x=80 to x=1840 with 16 px gutters → column width = 132 px.
  - **Two-column 60/40**: cols 1–7 (left) and 9–12 (right), 32 px gutter.
  - **Three-column equal**: cols 1–4, 5–8, 9–12, 16 px gutters.
  - **Single column centered**: cols 2–11 (1496 px wide). Used rarely.
- **Vertical baseline:** 8 px baseline; body type aligns to the baseline grid.

### Whitespace Philosophy

Whitespace is the pedestal for content, not the substitute for it. The header zone always has ≥ 24 px breathing room above and below the divider. Figures inside the body region carry ≥ 24 px between themselves and adjacent text. **The lower half of the body region is never just empty whitespace** — it carries either content, supporting annotation, a footnote band, or a figure. If the slide truly has nothing to say in the lower half, the slide is too short and should be merged with the next.

---

## Elevation & Depth

| Level | Treatment | Use |
|---|---|---|
| Flat | No shadow, no border | Headline text, body text, full-bleed tiles, page number, dividers |
| Soft hairline | 1 px `rgba(0, 0, 0, 0.08)` border | Inline body cards, table cell borders |
| Backdrop tone | Solid surface change (`{colors.canvas-parchment}`) | Section-opener and quote-frame backgrounds |
| Figure shadow | `rgba(0, 0, 0, 0.22) 3px 5px 30px 0` | Photographic figures and product renders resting on a surface |

**Shadow philosophy.** The system uses **exactly one** drop-shadow recipe, applied only to photographic figures — never to cards, never to text, never to chips. Elevation in the slide UI comes from (a) surface-color change (light tile ↔ dark tile) and (b) the divider hairline. The single shadow exists to give photographic content visual weight, not to imply UI hierarchy.

### Decorative Depth

- **Edge-to-edge tile alternation** between light and dark slides creates deck-level rhythm without borders or shadows.
- **No glassmorphism, no gradient overlays, no glow effects, no neumorphism.**

---

## Shapes

### Border Radius Scale

| Token | Value | Use |
|---|---|---|
| `{rounded.none}` | 0 px | Full-bleed slides, photographic figure frames, table cells |
| `{rounded.sm}` | 8 px | Inline body cards, code blocks, dark utility chips |
| `{rounded.md}` | 12 px | Pearl ghost cards on parchment surfaces |
| `{rounded.lg}` | 18 px | Inline grid cards (e.g., a 3-up "what is X / Y / Z" row inside the body) |
| `{rounded.pill}` | 9999 px | Emphasis chips, callout pills, the rare slide-level "key takeaway" badge |
| `{rounded.full}` | 50% | Icon badges in the body region |

### Photography Geometry

- **Hero figures**: full-bleed only on cover and section-opener templates. On content slides, figures live inside the body region with internal padding of 24–40 px.
- **Figures**: WebP/PNG with transparent backgrounds where possible; rest on the slide surface and pick up the figure shadow.
- **Inline cards**: 1:1 or 4:3 crops at `{rounded.lg}` (18 px) radius for grid cards; `{rounded.sm}` (8 px) for inline thumbnails inside body text.
- **No rounded full-bleed imagery.** Full-bleed = rectangular, edge to edge. Rounding appears only on inline figures.

---

## Components

### Slide Templates

The deck uses a small, fixed set of templates. Every slide is one of these.

- **`template.cover`** — The first slide of the deck. Background `{colors.canvas-parchment}` or full-bleed photographic. Title in `{typography.cover-display}` (96 px / 800), positioned at x=80, y=400 (vertically centered with the visual mass of the slide). Below the title, a single-line presenter byline in `{typography.body}` (or `{typography.body-tight}`) at color `{colors.ink-muted-80}`. **No logo, no chapter slot, no subtitle slot, no page number, no header divider** — the cover is the one place the locked header zone is omitted.
- **`template.section-opener`** — Opens a new chapter. Background `{colors.canvas-parchment}` or `{colors.surface-tile-1}` (alternating). The chapter slot at y=56 carries "Chapter N" in `{typography.chapter-tag}`. The section title sits in `{typography.section-display}` (72 px / 700), centered vertically inside the body region (y≈540). A one-sentence chapter rationale appears below in `{typography.lead}`. **No subtitle, no body grid** — the section-opener is intentionally airier than a content slide.
- **`template.content-light`** — The default content slide. Background `{colors.canvas}`. Header zone (chapter, title, subtitle, divider) at the locked positions above. Body region populated per the Body Density Rule.
- **`template.content-parchment`** — Same as `template.content-light` but on `{colors.canvas-parchment}`. Used to alternate within a long stretch of light content slides for rhythm.
- **`template.content-dark`** — Same structure as `template.content-light` but on `{colors.surface-tile-1}`. Header text uses `{colors.body-on-dark}`; chapter tag uses `{colors.primary-on-dark}`; divider hairline uses `rgba(255,255,255,0.16)`. Used to mark a major shift in topic or to emphasize a single high-impact slide.
- **`template.figure-hero`** — A content slide whose body region is dominated by a single hero figure (a diagram, a 3DGS render, a system architecture). Header zone unchanged. Body composition uses the "Hero figure + caption strip" preset: figure occupies y=296 to y=752 (456 px tall), caption strip y=776 to y=1016 (240 px) carrying numbered annotations or a reference list.
- **`template.compare-3up`** — A content slide whose body region uses the three-column equal grid for parallel comparison. Header zone unchanged. Each column carries a sub-heading in `{typography.body-strong}`, body in `{typography.body-tight}`, optional small figure or icon at the top.
- **`template.summary`** — Closing slide of a chapter or deck. Background `{colors.canvas-parchment}`. Header zone unchanged but title reads "Summary" or "정리." Body uses a numbered list of 3–7 takeaways in `{typography.body}` with each item in `{typography.body-strong}` followed by a short explanation.

### Chips & Buttons (used inline in body)

- **`chip.emphasis`** — Pill-shaped inline emphasis chip. Background transparent, 1.5 px solid `{colors.primary}` border, text `{colors.primary}` in `{typography.caption-strong}`, rounded `{rounded.pill}`, padding 6 × 14 px. Used to label a key term inside body copy or to badge a "new" idea.
- **`chip.tag`** — Compact rectangular tag. Background `{colors.canvas-parchment}` (on white) or `{colors.surface-tile-2}` (on dark), text `{colors.ink-muted-80}` / `{colors.body-muted}` in `{typography.caption}`, rounded `{rounded.sm}`, padding 4 × 10 px. Used to mark category or status.
- **`chip.selected`** — A chip in selected state. 2 px solid `{colors.primary-focus}` border. Same shape, same content as the unselected chip.

There are **no clickable buttons in slides** — slides are static. Use chips for emphasis, not for "click me."

### Cards & Containers

- **`card.inline`** — A bordered container inside the body region. Background `{colors.canvas}` (or `{colors.surface-pearl}` on parchment slides), 1 px solid `{colors.hairline}`, rounded `{rounded.lg}` (18 px), padding `{spacing.md}` (24 px). Holds a small structured unit: a definition, a sub-figure with caption, a code snippet header.
- **`card.code`** — A code block. Background `#1d1d1f` (matching `{colors.ink}`), text `#f5f5f7` in `Pretendard Regular` mono-style fallback, **but** since Pretendard is sans-serif and this system forbids other fonts, code is presented in `Pretendard Regular` 18 px with explicit white-space preservation rather than a monospace family. Rounded `{rounded.sm}` (8 px), padding `{spacing.md}` (24 px).
- **`card.callout`** — A horizontal accent strip used inside the body to draw attention to a single sentence. Background `rgba(0, 102, 204, 0.06)`, left border 4 px solid `{colors.primary}`, text `{colors.ink}` in `{typography.body-strong}`, rounded `{rounded.sm}`, padding 16 × 24 px.
- **`card.figure`** — Wraps a photographic figure. No background, no border. Figure inside picks up the `{shadow.figure}` recipe. Caption below in `{typography.caption-strong}` for the figure label ("Figure 3.2") and `{typography.caption}` for the description.

### Tables

- **`table.default`** — Used for spec sheets and small data displays. Header row `{typography.body-strong}` 18 px on `{colors.canvas-parchment}`, body rows alternating `{colors.canvas}` and `{colors.canvas-parchment}` for zebra striping (subtle — the parchment is barely off-white). Cell padding 12 × 16 px. Bottom border on each row at `{colors.hairline}`. No vertical rules.

### Footer

- **`footer.page-num`** — Page number slot at the bottom-right of every content slide. Position x=1840, y=1024, right-aligned. Text in `{typography.page-num}` (Pretendard Medium 500, 12 px), color `{colors.ink-muted-48}` on light surfaces / `{colors.body-muted}` on dark. **No logo, no presenter name, no copyright line** — page number alone is the footer.

---

## Slide Construction Workflow

When building a new slide, follow this order. Skipping any step risks breaking the locked-header invariant or the body-density rule.

1. **Pick a template.** One of the eight templates above. The choice is deterministic: cover for slide 1, section-opener at chapter boundaries, content-light/parchment/dark alternating for body slides, figure-hero when the slide centers on a single image, compare-3up for parallel concepts, summary at chapter ends.
2. **Place the locked header.** Chapter tag, title, subtitle, divider. Use the exact y-coordinates. Do not adjust the title's vertical position to "center it" on the slide — the locked position is correct on every page.
3. **Decide body composition.** One of the five compositions in the Body Composition Presets section. The composition is committed before any content is written.
4. **Populate the body to the density floor.** Real content, not filler. If the primary content is shorter than 70% of the body height, add supporting structure (annotation rail, footnote band, sub-figure, key-takeaways strip).
5. **Add the page number.** Always present on content slides. Never on the cover.
6. **Audit against the don'ts** (next section).

---

## Do's and Don'ts

### Do

- Use Pretendard exclusively. Install the supplied font files (Thin–Black, OTF and TTF) on every machine that opens the deck before exporting.
- Lock the header zone (chapter / title / subtitle / divider) to the exact y-coordinates on every content slide.
- Fill the body region to at least 70% of its vertical height with real, meaningful content.
- Use `{colors.primary}` (Action Blue #0066cc) for every emphasis element — chips, callout strokes, key terms — and nothing else.
- Set headlines in `{typography.title-display}` or larger with negative letter-spacing (`-1.12 px` or tighter at 56 px+) for the "set-in-stone" cadence.
- Run body copy at `{typography.body}` (20 px / 400 / 1.50 / -0.20 px) — never below 18 px on a slide.
- Alternate light, parchment, and dark tile backgrounds for deck-level rhythm. The color change IS the divider between sections.
- Reserve `{rounded.pill}` for emphasis chips and the rare key-takeaway badge.
- Apply the figure shadow (`rgba(0, 0, 0, 0.22) 3px 5px 30px`) only to photographic figures resting on a surface — never on cards, chips, or text.
- Keep the cover slide chrome-free: title, single byline, nothing else.

### Don't

- Don't use any font other than Pretendard. No SF Pro, no Noto Sans KR, no Inter, no system fallback. If Pretendard is missing, install it before exporting.
- Don't add a logo to any slide — cover, header, footer, or anywhere else.
- Don't deviate from the 16:9 canvas. No 4:3, no portrait, no custom ratios.
- Don't shift the title's vertical position to "center" it when the subtitle is absent — the locked y-coordinate stays.
- Don't leave a tall empty band beneath the title. If real content is short, expand with supporting structure.
- Don't introduce a second accent color. Every emphasis is `{colors.primary}` (or its on-dark sibling).
- Don't add shadows to cards, buttons, chips, or text. Shadow is reserved for photographic figures.
- Don't use gradients as decorative backgrounds. Atmosphere comes from photography, not CSS effects.
- Don't set body copy at weight 500 — the ladder is 400 / 600 / 700 / 800. Weight 500 is reserved for the page-number footer and tabular figure captions.
- Don't use Pretendard Thin (100), ExtraLight (200), Light (300), or Black (900) anywhere — they muddy at projection scale or fight the system's quiet voice.
- Don't round full-bleed slide backgrounds — slides are rectangular. Rounding only appears on inline figures and cards.
- Don't tighten line-height below 1.40 for body copy — the editorial leading is part of the brand.
- Don't mix radius grammars — `{rounded.sm}` for compact utility, `{rounded.lg}` for inline grid cards, `{rounded.pill}` for emphasis chips, and nothing in between (except the rare `{rounded.md}` Pearl card on parchment surfaces).
- Don't use `{colors.primary-on-dark}` (Sky Link Blue) on light surfaces — it's the dark-tile-only variant.
- Don't insert italics or underlines for emphasis — use weight (400 → 600) or color shift instead.

---

## Iteration Guide

1. Focus on ONE template at a time. Reference its YAML key directly (`{template.content-light}`, `{component.card.callout}`).
2. Variants of a template (`-dark`, `-parchment`) live as separate entries.
3. Use `{token.refs}` everywhere — never inline hex.
4. Display headlines stay Pretendard 700+ with negative letter-spacing. Body stays Pretendard 400 at 20 px (18 px in dense layouts). The boundary is unbreakable.
5. The single drop-shadow (`rgba(0, 0, 0, 0.22) 3px 5px 30px`) is reserved for photographic figures only.
6. When in doubt about emphasis: alternate surface (light → dark tile) before adding chrome.
7. When in doubt about whether a slide is "full enough": measure. If real content occupies less than 70% of the body region's vertical height, the slide is under-built.

---

## Known Gaps

- Animation and slide-transition behavior is not specified — this system covers static layout only.
- Print/handout layouts (multi-slide-per-page) are not specified.
- Equation typesetting (LaTeX rendering) is delegated to the host platform; this spec only governs the text style around the equation.
- Speaker-notes layout is delegated to PowerPoint's notes pane — no design tokens defined.
- Right-to-left language layouts are not specified; the system assumes Korean and Latin text directions.
