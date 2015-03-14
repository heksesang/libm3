# Introduction #

These are the structures found so far in the .m3 files.


# Details #

```
// Size = 8 byte / 0x08 byte
// Complete
struct Reference 
{
    /*0x00*/ uint32 nEntries;
    /*0x04*/ uint32 ref;
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

// Size = 8 Byte / 0x08 byte
// Incomplete
struct AnimRef
{
    /*0x00*/ uint16 flags; //usually 1
    /*0x02*/ uint16 animflag; //6 indicates animation data present
    /*0x04*/ uint32 animid; //a unique uint32 value referenced in STC.animid and STS.animid
};

// Size = 20 byte / 0x14 byte
// Complete
struct MD33
{ 
    /*0x00*/ char id[4]; 
    /*0x04*/ uint32 ofsRefs; 
    /*0x08*/ uint32 nRefs; 
    /*0x0C*/ Reference MODL; 
};

// Size = 156 byte / 0x9C byte
// Incomplete
struct BONE
{
    /*0x00*/ int32 d1; // Keybone?
    /*0x04*/ Reference name;
    /*0x0C*/ uint32 flags; //2560 = Weighted vertices rendered, 512 = not rendered
    /*0x10*/ int16 parent;
    /*0x12*/ uint16 s1;
    /*0x14*/ Aref_VEC3D trans;
    /*0x38*/ Aref_QUAT rot;
    /*0x64*/ Aref_VEC3D scale;
    //appears to have another animation ref at 0x8C but not sure what for
};

// Size = 8 byte / 0x08 byte
// Complete
struct MATM
{
    /*0x00*/ uint32 nmat; //usually only 1
    /*0x04*/ uint32 matind; //MAT index
};

// Size = 212 bytes / 0xD4 bytes
// Incomplete
struct MAT
{
    Reference name;
    int32 d1[8];
    float x, y;
    Reference layers[13];//0 - Diffuse, 1 - Decal, 2 - Specular, 3 - Emissive, 9 - Normal
    int32 d2[15];
};

// Size = 352 bytes / 0x160 bytes
// Incomplete
struct LAYR
{
    uint32 d1;
    Reference name;
    Aref_Colour colour;
    uint32 uvFlags;
    uint32 uvChannel; //used like flags in decal maps so they use a different uv channel
    uint32 alphaFlags;
    Aref_FLOAT bright_mult1;
    Aref_FLOAT bright_mult2;
    uint32 d1;
    uint8 col2[4];
    uint32 d2[2];
    int32 d3;
    uint32 d4[2];
    Aref_UINT32 ar1;
    Aref_VEC2D ar2;
    Aref_UINT16 ar3;
    Aref_VEC2D ar4;
    Aref_VEC3D uvAngle; //max angle = uvAngle * 50 * -1
    Aref_VEC2D uvTiling;
    Aref_UINT32 ar5;
    Aref_FLOAT ar6;
    Aref_FLOAT brightness;
    int32 d5; //unknown purpose, seems to affect uv coords, should be set to -1
    uint32 tintFlags;
    float tintStrength; //set to 4 by default in Blizzard models
    float tintUnk; //unknown purpose relating to tint
    float f1[2]; //seems to be more settings for tint
};

// Size = 32 byte / 0x20 byte
// Incomplete
struct DIV
{
    /*0x00*/ Reference faces;
    /*0x08*/ Reference REGN;
    /*0x10*/ Reference BAT;
    /*0x18*/ Reference MSEC;
};

// Size = 14 byte / 0x0E byte
// Incomplete
struct BAT
{
    /*0x00*/ uint32 d1;
    /*0x04*/ uint16 subid; //REGN index
    /*0x06*/ uint16 s1[2];
    /*0x0A*/ uint16 matid; //MATM index (MATM is a material lookup table)
    /*0x0C*/ int16 s2; //usually -1
};

//Size = 28 byte / 0x1C byte
// Incomplete
struct REGN
{
    /*0x00*/ uint32 d1; //always 0?
    /*0x04*/ uint16 indVert;
    /*0x06*/ uint16 numVert;
    /*0x08*/ uint32 indFaces;
    /*0x0C*/ uint32 numFaces;
    /*0x10*/ uint16 boneCount; //boneLookup total count (redundant?)
    /*0x12*/ uint16 indBone; //indice into boneLookup
    /*0x14*/ uint16 numBone; //number of bones used from boneLookup
    /*0x16*/ uint16 s1;
    /*0x20*/ uint8 b1[2]; //flags? should be set to 1
    /*0x22*/ uint16 rootBone;
};

// Size = 72 byte / 0x48 byte
// Incomplete
struct MSEC
{
    /*0x00*/ uint32 d1; //always 0?
    /*0x04*/ Aref_Sphere bndSphere;
};

// Size = 176 byte / 0xB0 byte
// Incomplete
struct CAM
{
    /*0x00*/ int32 d1;
    /*0x04*/ Reference name;
    /*0x0C*/ uint16 flags1;
    /*0x0E*/ uint16 flags2;
};

// Size = 388 byte / 0x184 byte
// Incomplete
struct PROJ
{
};

// Size = 96 byte/ 0x60 byte
// Incomplete
struct EVNT
{
    /*0x00*/ Reference name;
    /*0x08*/ int16 unk1[4];
    /*0x10*/ float matrix[4][4];
    /*0x50*/ int32 unk2[4];
};

// Size = 16 byte / 0x10 byte
// Incomplete
struct ATT
{
    /*0x00*/ int32 unk;
    /*0x04*/ Reference name;
    /*0x0C*/ int32 bone;
};

// Size = 24 byte / 0x18 byte
// Complete
struct SD
{
    /*0x00*/ Reference timeline;
    /*0x08*/ uint32 flags;
    /*0x0C*/ uint32 length;
    /*0x08*/ Reference data;
};

// Size = 88 byte / 0x58 byte
// Incomplete
struct SEQS
{
    /*0x00*/ int32 d1[2]; //usually -1
    /*0x08*/ Reference name;
    /*0x14*/ uint32 animStart; //start frame
    /*0x18*/ uint32 animEnd; //end frame
    /*0x1C*/ float moveSpeed; //used in movement animations (walk, run)
    /*0x20*/ uint32 flags;
    /*0x24*/ uint32 frequency; //play frequency ratio
    /*0x28*/ uint32 replayStart; //in most Blizzard models set to 1
    /*0x2C*/ uint32 replayEnd; //in most Blizzard models also set to 1
    /*0x30*/ uint32 d2[2]; //usually 0
    /*0x38*/ Sphere bndSphere
    /*0x58*/ uint32 d3[2]; //usually 0
};

// Size = 4 byte / 0x04 byte
// Incomplete
struct AnimationIndex
{
    /*0x00*/ uint16 aind; //anim ind in seq data
    /*0x02*/ uint16 sdind; //seq data array index
};

// Size = 140 byte / 0x8C byte
// Incomplete
struct STC
{
    /*0x00*/ Reference name;
    /*0x08*/ uint32 d1;
    /*0x0C*/ uint16 indSEQ[2]; //points to animation in SEQS chunk, twice for some reason
    /*0x10*/ Reference animid; //list of unique uint32s used in chunks with animation. The index of these correspond with the data in the next reference.
    /*0x18*/ Reference animindex; //lookup table, connects animid with it's animation data, nEntries of AnimationIndex reference using U32_ id
    /*0x20*/ uint32 d2;
    /*0x24*/ Reference SeqData[13]; //SD3V - Trans, SD4Q - Rotation, SDR3 - Scale?, SDFG - Flags, SDMB - Bounding Boxes?
};

// Size = 24 byte / 0x18 byte
// Incomplete
struct STS
{
    /*0x00*/ Reference animid; //uint32 values, same as what's in STC?
    /*0x08*/ int32 d1[3]; //usually -1
    /*0x14*/ int16 s1; //usually -1
    /*0x16*/ uint16 s2; //usually 0
};

// Size = 16 byte / 0x10 byte
// Complete
struct STG
{
    /*0x00*/ Reference name;
    /*0x08*/ Reference stcID;
};

// Size = 28 byte / 0x1C byte
// Incomplete
struct BNDS
{
    /*0x00*/ Sphere bndSphere;
};

// Size = 64 byte / 0x40 byte
// Complete
struct IREF
{
    float matrix[4][4];
};

struct VEC2
{
    float x, y;
};

struct VEC3
{
    float x, y, z;
};

struct VEC4
{
    float x, y, z, w;
};

struct QUAT
{
    float, x, y, z, w;
};

struct Sphere
{
    Vec3D min;
    Vec3D max;
    float radius;
};
```