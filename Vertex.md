# Introduction #

In the .m3 files, the vertex data seems to be contained within a uint8 block.

MODL-> flags defines some vertex stuff:
  * 0x20000 = has vertices
  * 0x40000 = has 1 extra UV coords
  * 0x80000 = has 2 extra UV coords
  * 0x100000 = has 3 extra UV coords

The UV coordinates and the normals must be converted from their compressed form into their true float values. See below for conversion formulas. In 3ds max, the Y-UV coord must also be flipped (1 - UV.y) for textures to be displayed on the mesh properly. UV coordinates can be **negative** values too, which just means they're using a different tile for their coordinates of the UV map. Multiple UV channels can be used for overlaying bitmaps such as decals and I've seen some models use extra channels for envio maps (reflective surfaces).

Tangents are also present but I don't understand their purpose as they seem unused by the game engine. Consequently, it seems that tangents do not need to be generated on export but should the need arise they should be handled with the same conversion formulas as the normals.

# Details #

```

struct Vertex32 // 32 byte
{
    Vec3D pos;
    char boneWeight[4];
    char boneIndex[4]; //index in boneLookup of vertex submesh
    char normal[4];  // x, y, z, w (w is the scale)
    int16 uv[2];
    char tangents[4];
};

struct Vertex36 // 36 byte
{
    Vec3D pos;
    char boneWeight[4];
    char boneIndex[4]; //index in boneLookup of vertex submesh
    char normal[4];  // x, y, z, w (w is the scale)
    int16 uv1[2];
    int16 uv2[2];
    char tangents[4];
};

struct Vertex40 // 40 byte
{
    Vec3D pos;
    char boneWeight[4];
    char boneIndex[4]; //index in boneLookup of vertex submesh
    char normal[4];  // x, y, z, w (w is the scale)
    int16 uv1[2];
    int16 uv2[2];
    int16 uv3[2];
    char tangents[4];
};

struct Vertex44 // 44 byte
{
    Vec3D pos;
    char boneWeight[4];
    char boneIndex[4]; //index in boneLookup of vertex submesh
    char normal[4];  // x, y, z, w (w is the scale)
    int16 uv1[2];
    int16 uv2[2];
    int16 uv3[2];
    int16 uv4[2];
    char tangents[4];
};

```

# Extra Information #

## Vertex Weighting ##
Each vertex boneIndex is NOT an index into the global bone entries found in [MODL](MODL.md) but rather a reference into the bonelookup. However, it's not just an index into the bonelookup entries either. In order to find the correct bone to weight the vertice to, the boneIndex value uses submesh information found in the [REGN](DIV#REGN.md) indBone value to grab the right bone in the bonelookup entries. So in order to calculate the correct boneIndex:
  1. Find which REGN entry the vertex belongs to
  1. Add the REGN.indBone to the vertex.boneIndex value
  1. Grab the bonelookup value your new index points to
  1. Get the bone the bonelookup value refers to

## Conversion Formulas ##

### UV Coordinates: ###

_Note:_ Important that you read in UV coordinates as signed short values
```
realUV = uv / 2048.0
```

_e.g._ uv = 1771
```
realUV = 1771 / 2048.0
realUV = 0.8647
```

For export, simply multiply by 2048 and round/discard the remainder.
```
convUV = uv * 2048
```

_e.g._ uv = 0.8647
```
convUV = 0.8647 * 2048
convUV = 1770.9 (1771)
```

### Normal Values ###
Three steps which can probably be combined into one by the mathematically minded. A note on the scale (`norm[4]`) value, it seems almost always to be 0 or 1 and nothing inbetween. I'm not sure if the w value need be used at all however it is present in Blizzard generated vertex normals.
```
realNorm = norm / 255.0
realNorm = realNorm * 2
realNorm = realNorm - 1
```

_e.g._ norm = 127
```
realNorm = 127 / 255.0
realNorm = 0.498 * 2
realNorm = 0.996 - 1
realNorm = -0.004
```

For export, the reverse with rounding/discard remainder:
```
convNorm = 1 + norm
convNorm = norm / 2
convNorm = norm * 255
```
_e.g._ norm = -0.004
```
convNorm = 1 + -0.004
convNorm = 0.966 / 2
convNorm = 0.498 * 255
convNorm = 126.9 (127)
```