# Hierarchy Customizer

A free, lightweight alternative to vHierarchy2: assign a color and/or icon to
any GameObject in the Hierarchy, or any folder in the Project window, to keep
big scenes and projects organized.

## Installation

1. Copy the `HierarchyCustomizer` folder into your project's `Assets` folder
   (e.g. `Assets/HierarchyCustomizer`). It contains its own `Editor` folder
   with an editor-only assembly definition, so it will never end up in your
   builds.
2. Let Unity recompile. No further setup needed — it activates automatically.

## Usage

- **Hierarchy**: hover over any GameObject's row. A small dot button (●)
  appears at the far right — click it to open the color/icon picker.
  - Click a color swatch to tint the whole row.
  - Click a built-in icon to replace the object's icon in the Hierarchy.
  - Drag any `Texture2D` into the "Custom icon" field to use your own icon.
  - Click the ✕ at the start of either row to clear that color or icon.
  - Once an object has a color, its dot button stays visible even without
    hovering, so you can always see and re-open it.
- **Project window**: same idea, but only shows up on **folders** (works in
  both list view and grid/icon view). The color re-draws the actual folder
  icon texture tinted with your chosen color (so it still looks like a
  folder, just recolored — pick a color with alpha < 1 for a lighter tint),
  and a custom icon is drawn as a small badge in the bottom-right corner
  rather than replacing the whole icon.
- **Tools > Hierarchy Customizer**:
  - `Select Database Asset` — jumps to the data asset holding every
    assignment. It's created automatically in the same folder as
    `CustomizerDatabase.cs` — wherever you've placed the HierarchyCustomizer
    package in your project (e.g. `Assets/Plugins/HierarchyCustomizer/Editor/`)
    — so it travels with the tool instead of living at a fixed path. Commit
    this file to source control if you want your team to share the same
    colors/icons.
  - `Clear All Customizations` — wipes every assignment (with a confirmation).

## How it works

- Colors/icons for GameObjects are keyed by a `GlobalObjectId`, which stays
  stable across editor sessions and scene reloads (as long as the object
  itself isn't deleted and recreated).
- Colors/icons for folders are keyed by the folder's asset GUID, so they
  survive renames and moves.
- Everything is drawn via `EditorApplication.hierarchyWindowItemOnGUI` and
  `EditorApplication.projectWindowItemOnGUI` — no changes to your actual
  scene data or assets, it's purely an editor-side overlay.

## Customizing further

- **More icons**: add any built-in icon name to the `BuiltinIconNames` array
  in `IconLibrary.cs`. Any name recognized by
  `EditorGUIUtility.IconContent(name)` works.
- **More preset colors**: edit the `PresetColors` array in
  `IconColorPickerPopup.cs`.
- **Layout tweaks**: the row-tint start position, icon size, and button size
  are all simple constants at the top of `HierarchyCustomizerHook.cs` and
  `ProjectCustomizerHook.cs` if you want to nudge alignment for your Unity
  version or editor theme.

## Performance notes

- Computing a GameObject's stable ID (`GlobalObjectId.GetGlobalObjectIdSlow`)
  is genuinely expensive, so it's cached per instance ID and only
  recomputed when the hierarchy structurally changes or an instance ID gets
  reused by a different object.
- Rows/folders with nothing assigned and no mouse hover skip all drawing
  work entirely, and label styles are cached instead of being rebuilt every
  repaint, so the overhead scales with how many items you've actually
  customized, not with the total size of your scene or project.
- The Project window fires its GUI callback for every visible item, most of
  which are files, not folders. Whether a given GUID is a folder is cached
  per-GUID (invalidated only when the project's asset structure changes),
  so files are skipped instantly instead of re-querying AssetDatabase every
  repaint.
- Custom icon textures (the ones you drag into the "Custom icon" field) are
  now cached by GUID too, so using a custom icon doesn't cost an
  AssetDatabase lookup every repaint the way it used to.
- The small customize button, the clear-icon "✕", and the picker's icon
  grid buttons all reuse cached `GUIContent`/`GUIStyle` objects instead of
  allocating new ones each repaint - implicit string-to-GUIContent
  conversions in IMGUI allocate every time, which adds up when a row/folder
  is visibly hovered or colored continuously.
- `EditorUtility.InstanceIDToObject` is version-guarded to use
  `EditorUtility.EntityIdToObject` on Unity 6+ where the former is obsolete,
  so you shouldn't see that compiler warning anymore.

## Known limitations

- If you'd already used an earlier version of this tool, your old data
  asset was created at `Assets/Editor/HierarchyCustomizer/CustomizerDatabase.asset`.
  To keep those assignments, just drag that file into the same folder as
  `CustomizerDatabase.cs` in the new package location — it'll be picked up
  automatically since it's found by filename and folder, not by GUID.

- Very deeply nested/indented objects, and unusual Project window layouts,
  may need the offset constants above tweaked slightly — exact pixel
  positions can shift a little between Unity versions.
- Colors are drawn as flat overlays (not blended with selection highlight),
  so a selected + colored row will show the color on top of the selection
  tint, which is normal and matches most similar tools.
