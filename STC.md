# Introduction #

These blocks works like some sort of headers for sequences, referencing [Sequence Data](SD.md) blocks and some other stuff.

Seems there can be more than one [STC](STC.md) block per sequence if there are several versions of an animation sequence. [STG](STG.md) represents an [STC](STC.md) lookup table for each [SEQS](SEQS.md) entry.
Example: Cover Full, Cover Shield

Bones, among other structures, reference the [Sequence Data](SD.md) located in these structs for animation.

# Details #
```
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
    /*0x0C*/ uint16 indSTC[2]; //seems to be the STC index, twice
    /*0x10*/ Reference animid; //list of unique uint32s used in chunks with animation. The index of these correspond with the data in the next reference.
    /*0x18*/ Reference animindex; //lookup table, connects animid with it's animation data, nEntries of AnimationIndex reference using U32_ id
    /*0x20*/ uint32 d2;
    /*0x24*/ Reference SeqData[13]; //SD3V - Trans/Scale, SD4Q - Rotation, SDFG - Flags, SDMB - Bounding Boxes?
};

```

# Sequence Data Types #
Known Sequence Data types:
| **SD ID** | **Index** | **Type** | **Data ID** | **Found In** | **Anim. Ref Type** | **Description** |
|:----------|:----------|:---------|:------------|:-------------|:-------------------|:----------------|
| SDEV | 0 | _Event_ | [EVNT](SD#EVNT.md) | Unknown | Unknown | Event Animation? |
| SD2V | 1 | _Vector 2D_ | [VEC2](SD#VEC2.md) | [PAR](PAR.md) | Aref\_VEC2D | Unknown |
| SD3V | 2 | _Vector 3D_ | [VEC3](SD#VEC3.md) | [BONE](BONE.md) | Aref\_VEC3D | Translation/Scale |
| SD4Q | 3 | _Quaternion_ | [QUAT](SD#QUAT.md) | [BONE](BONE.md) | Aref\_QUAT | Rotation |
| SDCC | 4 | _Colour (4 byte floats)_ | [COL](SD#COL.md) | [RIB](RIB.md) | Aref\_Colour | Colour (Blue, Green, Red, Alpha) |
| SDR3 | 5 | _Float_ | [REAL](SD#REAL.md) | [PAR](PAR.md) | Aref\_FLOAT | Unknown |
| SDS6 | 7 | _int16_ | I16 | Unknown | Aref\_INT16 | Unknown |
| SDFG | 11 | _int32_ | [FLAG](SD#FLAG.md) | [RIB](RIB.md) | Aref\_UINT32 | Flags? |
| SDMB | 12 | _Extent_ | [BNDS](SD#BNDS.md) | [MSEC](DIV#MSEC.md) | Aref\_Sphere | Bounding Sphere |

## Sequence Data Information ##
Translation and Scaling animation data use the same [Sequence Data](SD.md) entry, SD3V which uses [VEC3](SD#VEC3.md)'s for keyframes. [EVNT](SD#EVNT.md) animations determine the end of an animation sequence. Sequences that are part of a looped series require `Evt_SeqEnd` [EVNT](SD#EVNT.md) keyframes so that they only loop once in the sequence. Examples of these animations are Stand animations, which typically all loop, but each only plays once in the loop and are picked based on their frequency setting of their corresponding SEQS entry.

## Animation References ##
Animation references indirectly reference the data found in the Sequence Data arrays. See the [BONE](BONE.md) animation data description to get a handle on how STC data is referenced using animation references. I believe there will be a total of 13 different types of animation references, one for each index of the sequence data array. So far only some have been discovered within M3 files.