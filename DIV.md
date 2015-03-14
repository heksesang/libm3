# Introduction #

Geosets (divisions/parts) of the model.


# Details #

## DIV ##
```

// Size = 36 byte / 0x24 byte
// Incomplete
struct DIV
{
    /*0x00*/ Reference triangles;
    /*0x08*/ Reference REGN;
    /*0x10*/ Reference BAT;
    /*0x18*/ Reference MSEC;
    /*0x20*/ uint32 d1; //always 1?
};

```


## REGN ##
```

//Size = 28 byte / 0x1C byte
// Incomplete
struct REGN
{
    /*0x00*/ uint32 d1; //always 0?
    /*0x08*/ uint32 indVert;
    /*0x0C*/ uint32 numVert;
    /*0x10*/ uint32 indFaces;
    /*0x14*/ uint32 numFaces;
    /*0x18*/ uint16 boneCount; //boneLookup total count (redundant?)
    /*0x1A*/ uint16 indBone; //indice into boneLookup
    /*0x1C*/ uint16 numBone; //number of bones used from boneLookup
    /*0x1E*/ uint16 s1; 
    /*0x20*/ uint8 b1[2]; //should be set to 1
    /*0x22*/ uint16 rootBone; //root bone for the skin of the geometry
};

```

## BAT ##
```

// Size = 14 byte / 0x0E byte
// Incomplete
struct BAT
{
    /*0x00*/ uint32 d1; //always 0?
    /*0x04*/ uint16 subid; //REGN index
    /*0x06*/ uint16 s1[2];
    /*0x0A*/ uint16 matid; //MATM index (MATM = material lookup table)
    /*0x0C*/ int16 s2; //usually -1
};

```

## MSEC ##

```

// Size = 72 byte / 0x48 byte
// Incomplete
struct MSEC
{
    /*0x00*/ uint32 d1; //always 0?
    /*0x04*/ Aref_Sphere bndSphere;
};

```