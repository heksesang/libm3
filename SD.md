# Introduction #

References sequence data.


# SD Header #

## SD ##
```

// Size = 24 byte / 0x18 byte
// Complete
struct SD
{
    /*0x00*/ Reference timeline;
    /*0x08*/ uint32 flags;
    /*0x0C*/ uint32 length;
    /*0x08*/ Reference data;
};

```

# SD Types #
Known Sequence Data types:
| **SD ID** | **Index** | **Type** | **Data ID** | **Found In** | **Description** |
|:----------|:----------|:---------|:------------|:-------------|:----------------|
| SDEV | 0 | _Event_ | [EVNT](SD#EVNT.md) | Unknown | Event Animation? |
| SD2V | 1 | _Vector 2D_ | [VEC2](SD#VEC2.md) | [PAR](PAR.md) | Unknown |
| SD3V | 2 | _Vector 3D_ | [VEC3](SD#VEC3.md) | [BONE](BONE.md) | Translation |
| SD4Q | 3 | _Quaternion_ | [QUAT](SD#QUAT.md) | [BONE](BONE.md) | Rotation |
| SDR3 | 5 | _Float_ | [REAL](SD#REAL.md) | [BONE](BONE.md) | Scale? |
| SDS6 | 7 | _int16_ | I16 | Unknown | Unknown |
| SDFG | 11 | _int32_ | [FLAG](SD#FLAG.md) | Unknown | Unknown |
| SDMB | 12 | _Extent_ | [BNDS](SD#BNDS.md) | [MSEC](DIV#MSEC.md) | Bounding Box |

## EVNT ##
```

// Size = 96 byte / 0x60 byte
// Incomplete
struct EVNT
{
    /*0x00*/ Reference name;
    /*0x08*/ int32 d1; //usually -1
    /*0x0C*/ int16 s1[2];
    /*0x10*/ float matrix[4][4];
    /*0x50*/ int32 d2[4];
};

```

## VEC2 ##
```
struct VEC2
{
    float x, y;
};
```

## VEC3 ##
```
struct VEC3
{
    float x, y, z;
};
```

## QUAT ##
```
struct QUAT
{
    float x, y, z, w;
};
```

## REAL ##
```
struct REAL
{
    float f1;
};
```

## FLAG ##
```
struct FLAG
{
    ulong flg;
};
```

## BNDS ##
```
// Size = 28 byte / 0x1C byte
// Complete
struct BNDS
{
    /*0x00*/ VEC3 min;
    /*0x0C*/ VEC3 max;
    /*0x18*/ float radius;
}
```