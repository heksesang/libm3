# Introduction #

Material definitions for the M3 model. Materials act as a container for a group of bitmaps such as diffuse, specular, emissive, etc. The material type and its general properties play a role in how the bitmaps are ultimately rendered on the model. [MATM](MATM.md) entries are referenced in the `DIV.BAT.matid` reference for a submesh and function as a lookup table into material definitions based on the material type. For example, a `MODL.MATM[0]` entry with the values `MODL.MATM[0].matType` as 2 (Displacement material type) and `MODL.MATM[0].matIndex` as 2 would point to the the `MODL.DIS[2]` material definition.

Further information can be found in the [material lookup](MATM.md) documentation.

# Details #
Alot of the details have been determined through the previewer.

```
// Standard Material
// Size = 268 bytes / 0x10C bytes
// Incomplete
struct MAT
{
    /*0x00*/ Reference name;
    /*0x0C*/ uint32 d1;
    /*0x10*/ uint32 flags; //see material flags
    /*0x14*/ uint32 blendMode;
    /*0x18*/ uint32 priority;
    /*0x1C*/ uint32 d2;
    /*0x20*/ float specularity;
    /*0x24*/ float f1;
    /*0x28*/ uint32 cutoutThresh;
    /*0x2C*/ float SpecMult;
    /*0x30*/ float EmisMult;
    /*0x34*/ Reference layers[13];
    /*0xD0*/ uint32 d4;
    /*0xD4*/ uint32 layerBlend;
    /*0xD8*/ uint32 emisBlend;
    /*0xDC*/ uint32 d5; //controls emissive blending in someway, should be set to 2
    /*0xE0*/ uint32 specType;
    /*0xE4*/ Aref_UINT32 ar1;
    /*0xF8*/ Aref_UINT32 ar2;
};

// Displacement Material
// Size = 68 bytes / 0x44 bytes
// Incomplete
struct DIS
{
    /*0x00*/ Reference name;
    /*0x0C*/ uint32 d1;
    /*0x10*/ Aref_FLOAT ar1;
    /*0x24*/ Reference normalMap; //LAYR ref
    /*0x30*/ Reference strengthMap; //LAYR ref
    /*0x3C*/ uint32 flags; //see material flags
    /*0x40*/ uint32 priority;
};

// Composite Material Reference
// Size = 24 bytes / 0x18 bytes
// Incomplete
struct CMS
{
    /*0x00*/ uint32 matmIndex;
    /*0x04*/ Aref_FLOAT ar1; //blend amount?
};

// Composite Material
// Size = 12 bytes / 0x0C bytes
// Incomplete
struct CMP
{
    /*0x00*/ Reference name;
    /*0x0C*/ uint32 d1;
    /*0x10*/ Reference compositeMats; //CMS ref
};

// Terrain (Null) Material
// Size = 24 bytes / 0x18 bytes
// Complete
struct TER
{
    /*0x00*/ Reference name;
    /*0x0C*/ Reference nullMap; //usually blank LAYR ref
};

```

## Standard Material Layers ##
In the standard material definition, each layer index corresponds to a specific type of map. Some don't appear to do anything, even the previewer doesn't disclose their purpose. It's been found that some of the [LAYR](LAYR.md) references don't need to point to a real map to still have an effect on the material, such as Alpha Map properties when the alpha blend modes are used. Each layer index must have a referenced LAYR chunk even if it's blank or else the previewer will crash.

| **Layer Index** | **Map Type** |
|:----------------|:-------------|
| 0 | Diffuse|
| 1 | Decal |
| 2 | Specular |
| 3 | Emissive (1) |
| 4 | Emissive (2) |
| 6 | Envio (Reflective) |
| 7 | Envio Mask |
| 8 | Alpha Mask |
| 10 | Normal |
| 11 | Height |

## Material Flags ##
Most of these determined through the previewer. They're used in multiple material types (Standard, Displacement).

| **Bit Index** | **Bit Address** | **Flag** |
|:--------------|:----------------|:---------|
| 3 | 0x4 | Unfogged (1) |
| 4 | 0x8 | Two-sided |
| 5 | 0x10 | Unshaded |
| 6 | 0x20 | No shadows cast |
| 7 | 0x40 | No hit test |
| 8 | 0x80 | No shadows received |
| 9 | 0x100 | Depth prepass |
| 10 | 0x200 | Use terrain HDR |
| 12 | 0x800 | Splat UV fix |
| 13 | 0x1000 | Soft blending |
| 14 | 0x2000 | Unfogged (2) |

## Blendmodes ##
Used in the standard material type.
| **Value** | **Mode** |
|:----------|:---------|
| 0 | Opaque |
| 1 | Alpha Blend |
| 2 | Add |
| 3 | Alpha Add |
| 4 | Mod |
| 5 | Mod 2x |

## Layer/Emissive Blendmodes ##
Used in the standard material type.
| **Value** | **Mode** |
|:----------|:---------|
| 0 | Mod |
| 1 | Mod 2x |
| 2 | Add |
| 3 | Blend |
| 4 | Team Colour Emissive Add |
| 5 | Team Colour Diffuse Add |

## Specular Types ##
Used in the standard material type.
| **Value** | **Type** |
|:----------|:---------|
| 0 | RGB |
| 1 | Alpha Only |

# Composite Materials #
Composite materials are, just as you'd assume, a composite of multiple materials. Instead of directly referencing bitmaps through [LAYR](LAYR.md) references, composite materials instead reference multiple materials through [CMS](CMS.md) references. [CMS](CMS.md) structures contain an index into the [material lookup table](MATM.md) to create the composite material out of multiple material definitions.

# Terrain (Null) Materials #
Terrain materials appear as type Null in the previewer. The effect on the model appears to be that the material bitmap used is one of the terrain bitmaps from the surrounding tiles of where the object is placed. I suppose this is useful for models that you want to appear embedded in the terrain. They contain a map reference that is usually blank. After some testing, this material type only appears to work properly on geometry that is strictly planar, any kind of three dimensional structure to the geometry will cause the terrain texture to be distorted. Most terrain objects provide examples of the use of this material type for those interested.