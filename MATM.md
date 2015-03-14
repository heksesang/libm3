# Introduction #
MATM is a material lookup table referenced by [BAT](DIV.md) structures in the [DIV](DIV.md) definition. It functions as a reference to the type and the index of a particular type of material. More information on materials can be found in the [material definitions](MaterialDefinitions.md) documentation.


# Details #
```
// Size = 8 bytes / 0x08 bytes
// Complete
struct MATM
{
    /*0x00*/ ulong matType; //see below for known types
    /*0x04*/ ulong matIndex; //index into matType array
};
```

## Material Types ##
Probably only 5 or 6 material types exist total. I've only come across 4 at the moment but I have not looked particularly hard for the others.
| **Value** | **Material Type** | **Material Definition** |
|:----------|:------------------|:------------------------|
| 0 | Unused | Unused |
| 1 | Standard | [MAT](MaterialDefinitions.md) |
| 2 | Displacement | [DIS](MaterialDefinitions.md) |
| 3 | Composite | [CMP](MaterialDefinitions.md) |
| 4 | Terrain (Null) | [TER](MaterialDefinitions.md) |
| 5 | Unknown | [VOL](MaterialDefinitions.md) |