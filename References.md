# Introduction #

The .m3 files contain references in a different way than previous versions of the MDX format.


# Details #

The file header contains the number of reference lists in the file, along with the offset where their headers start.

Each reference header contains the data type in the list, the number of entries in that list, along with the offset where that list starts.

There is a fourth value in the reference header, which purpose is unknown. But the size of the block can depend on this value.
For example the MODL block in units/buildings have a different value here compared to the MODL block in skyboxes, and the size of the block / number of fields in the block is different.

```
// MD33
// Size = 8 byte / 0x08 byte
// Complete
struct Reference 
{
    /*0x00*/ uint32 nEntries;
    /*0x04*/ uint32 ref;
};

// MD34
// Size = 12 byte / 0x0C byte
// Complete
struct Reference
{
    /*0x00*/ uint32 nEntries;
    /*0x04*/ uint32 ref;
    /*0x08*/ uint32 d1;
};

// Size = 16 byte / 0x10 byte
// Incomplete
struct ReferenceEntry 
{ 
    /*0x00*/ char id[4]; 
    /*0x04*/ uint32 offset; 
    /*0x08*/ uint32 nEntries; 
    /*0x0C*/ uint32 d1; // Possibly nReferences;
};
```

# Animation References #
Each animation sequence has related SD-blocks which references the data used by the animation. Animation references use a unique uint32 ID to reference sequence animation data found in STC. Following animation references are two values, the initial value before animation and another value of the same type that seems to have no effect. Following these is a uint32 flag that seems to always be 0. The value types depend on the animation reference (i.e. VEC3D, Quat, uint32, etc). I'm willing to bet there's 13 different Animation Reference types based on the fact that [STC](STC.md) has 13 static reference arrays for sequence data.

```

// Size = 8 Byte / 0x08 byte
// Incomplete
struct AnimationReference
{
    /*0x00*/ uint16 flags; //usually 1
    /*0x02*/ uint16 animflag; //6 indicates animation data present
    /*0x04*/ uint32 animid; //a unique uint32 value referenced in STC.animid and STS.animid
};

struct Aref_UINT32
{
    AnimationReference AnimRef; //STC/STS reference
    uint32 value; //initial value
    uint32 unValue; //unused value
    uint32 flag; //seems unused, 0
}; //used in SDFG anim blocks, as FLAG data type

struct Aref_INT16
{
    AnimationReference AnimRef; //STC/STS reference
    int16 value; //initial value
    int16 unValue; //unused value
    uint32 flag; //seems unused, 0
}; //used in SDS6 anim blocks, as I16_ data type

struct Aref_FLOAT
{
    AnimationReference AnimRef; //STC/STS reference
    float value; //initial value
    float unValue; //unused value
    uint32 flags; //seems unused, 0 
}; //used in SDR3 anim blocks, as REAL data type

struct Aref_VEC2D
{
    AnimationReference AnimRef; //STC/STS reference
    Vec2D value; //initial value
    Vec2D unValue; //unused value
    uint32 flag; //seems unused, 0
}; //used in SD2V anim blocks, as VEC2 data type

struct Aref_VEC3D
{
    AnimationReference AnimRef; //STC/STS reference
    Vec3D value; //initial value
    Vec3D unValue; //unused value
    uint32 flag; //seems unused, 0
}; //used in SD3V anim blocks, as VEC3 data type

struct Aref_QUAT
{
    AnimationReference AnimRef; //STC/STS reference
    QUAT value; //initial value
    QUAT unValue; //unused value
    uint32 flags; //seems unused, 0
}; //used in SD4Q anim blocks, as QUAT data type

struct Aref_Colour
{
    AnimationReference AnimRef; //STC/STS reference
    uint8 value[4]; //b, g, r, alpha initial value 
    uint8 unValue[4]; //unused value
    uint32 flags; //seems unused, 0
}; //used in SDCC anim blocks, as COL data type

struct Aref_Sphere
{
    AnimationReference AnimRef; //STC/STS reference
    Sphere value; //initial value
    Sphere unValue; //unused value
    uint32 flags; //seems unused, 0    
}; //used in SDMB anim blocks, as BNDS data type
```