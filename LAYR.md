# Introduction #

Bitmaps are referenced through LAYR definitions which are found referenced in the [materials](MaterialDefinitions.md).

LAYR's can contain a path to an internal bitmap file which is usually a DDS file or in the case of animated textures an OGV file. Even if they do not contain a map path, they may still influence how a material is rendered. For example, alpha map LAYR settings can influence the way standard materials are rendered when alpha blend modes are used. More material information can be found in the [material definitions](MaterialDefinitions.md) documentation.

The previewer contains no information related to bitmaps except the bitmap filename if it is present. This has made documenting the LAYR structure accurately problematic.

# Details #

```

// Size = 356 bytes / 0x164 bytes
// Incomplete
struct LAYR
{
    /*0x00*/ uint32 d1;
    /*0x04*/ Ref mapPath;
    /*0x10*/ Aref_Colour colour; //used if useColour set to true in texFlags
    /*0x24*/ uint32 texFlags; 
    /*0x28*/ uint32 uvmapChannel; //seems to be flags handling UV map channels
    /*0x2C*/ uint32 alphaFlags;
    /*0x30*/ Aref_FLOAT brightness_mult1;
    /*0x44*/ Aref_FLOAT brightness_mult2; //works very similar to the first
    /*0x58*/ uint32 d4;
    /*0x5C*/ int32 d5; //should be set to -1 or else texture not rendered
    /*0x60*/ uint32 d6[2];
    /*0x68*/ int32 d7; //set to -1 in animated bitmaps
    /*0x6C*/ uint32 d8[2];
    /*0x74*/ Aref_UINT32 ar1;
    /*0x88*/ Aref_VEC2D ar2;
    /*0xA4*/ Aref_INT16 ar3;
    /*0xB4*/ Aref_VEC2D ar4;
    /*0xD0*/ Aref_VEC3D uvAngle; //3dsAngle = value * 50 * -1
    /*0xF4*/ Aref_VEC2D uvTiling;
    /*0x110*/ Aref_UINT32 ar5;
    /*0x124*/ Aref_FLOAT ar6;
    /*0x138*/ Aref_FLOAT brightness; //0.0 to 1.0 only?
    /*0x14C*/ int32 d20; //seems to affect UV coords? should be set to -1
    /*0x150*/ uint32 tintFlags;
    /*0x154*/ float   tintStrength; //set to 4.0 by default in Blizzard models
    /*0x158*/ float   tintUnk1; //0.0 to 1.0 only?
    /*0x15C*/ float   tintUnk2[2];
};

```

## Texture Flags ##
Flags that affect the way the bitmap is handled.
| **Bit Index** | **Bit Address** | **Flag** |
|:--------------|:----------------|:---------|
| 3 | 0x4 | Texture Wrap X |
| 4 | 0x8 | Texture Wrap Y |
| 5 | 0x10 | Black Texture |
| 10 | 0x200 | Use [Aref\_Colour](References.md) values |

## Alpha Flags ##
Flags that affect the way the bitmap alpha is handled.
| **Bit Index** | **Bit Address** | **Flag** |
|:--------------|:----------------|:---------|
| 1 | 0x1 | Render texture alpha as team colour |
| 2 | 0x2 | Render alpha of texture only |
| 3 | 0x4 | Alpha based shading? |
| 4 | 0x8 | Unknown, garbles the texture |

# Tint #
Some bitmaps use what I've decided to call tint. I'm not an expert on materials so I'm not sure on the accuracy of calling it tint. It seems to affect how the model responds to light, at least that's what I observe. Further clarification is needed. Good examples of this interesting effect can be found in the CastanarStasisTube models.

## Tint Flags ##
Flags that affect control tint, aswell as a texture flag?
| **Bit Index** | **Bit Address** | **Flag** |
|:--------------|:----------------|:---------|
| 1 | 0x1 | Inverse tint |
| 2 | 0x2 | Tint |
| 3 | 0x4 | Use [Aref\_Colour](References.md) values, just like in Texture Flags |