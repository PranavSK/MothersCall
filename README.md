# A Mother's Call

## Introduction

A narrative sidescroller set in the Antarctica. This is a game being developed as an entry to [Women in games Jam](https://itch.io/jam/women-in-games-jam).

## Workflow Conventions

### Folder Naming

 - Never use Spaces in names.
 - Use PascalCase.
 - Prefer a deep folder structure over having long asset names. Directory names should be as concise as possible, prefer one or two words. If a directory name is too long, it probably makes sense to split it into sub directories.
 - Try to have only one file type per folder. Use Textures/Trees, Models/Trees and not Trees/Textures, Trees/Models. That way its easy to set up root directories for the different software involved, for example, Substance Painter would always be set to save to the Textures directory.
 - Use the asset type for the parent directory: Trees/Jungle, Trees/City not Jungle/Trees, City/Trees. Since it makes it easier to compare similar assets from different art sets to ensure continuity across art sets.
 
### Asset Naming

 - Keep the most specific descriptor to the last: VampireDark and not DarkVampire, TreeSmall and not SmallTree.
 - For textures use the following suffixes:

    Suffix | Texture
    :------|:-----------------
    `_D`   | Albedo
    `_S`   | Specular
    `_R`   | Roughness
    `_M`   | Metallic
    `_N`   | Normal
    `_H`   | Height
    `_B`   | Displacement/Bump
    `_E`   | Emission
    `_AO`  | Ambient Occlusion
    `_Mask`| Mask
    
    It is good practice to use a single texture to combine black and white masks in a single texture split by each RGB channel. For such texture combine the suffixes in the order of RGB.

### Directory Structure

```
Assets
+---Art
|   +---Materials
|   +---Models      # FBX and BLEND files
|   +---Textures    # PNG files
+---Audio
|   +---Music
|   \---Sound       # Samples and sound effects
+---Code
|   +---Scripts     # C# scripts
|   \---Shaders     # Shader files and shader graphs
+---Narrative       # The narrative arc and ink scripts.
+---Docs            # Wiki, concept art, marketing material
+---Level           # Anything related to game design in Unity
|   +---Prefabs
|   +---Scenes
|   \---UI
\---Resources       # Configuration files, localization text and other user files.
```


## Credits

### Third-party assets

The following assets were used as-is or with modifications in compliance to the licenses attached to them.
 - ["Female Base Mesh"](https://skfb.ly/onVon) by kosmosovich is licensed under [Creative Commons Attribution](http://creativecommons.org/licenses/by/4.0/).
 - ["Stylized Low Poly Sci-Fi Buildings"](https://skfb.ly/6ztoo) by Robin Butler is licensed under [Creative Commons Attribution-NonCommercial](http://creativecommons.org/licenses/by-nc/4.0/).