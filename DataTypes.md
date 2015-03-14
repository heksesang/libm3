# Introduction #

The data types used in the references are documented here.


# Details #

### Primitives ###
| CHAR | _char_ |
|:-----|:-------|
| U32  | _uint32\_t_ |
| U16  | _uint16\_t_ |
| I32  | _int32\_t_ |
| I16  | _int16\_t_ |
| U8   | _uint8\_t_ |
| I8   | _int18\_t_ |
| REAL | _float_ |

### Structs ###
| [MD33](MD33.md) | _File header_ | 11 |
|:----------------|:--------------|:---|
| [MODL](MODL.md) | _Model header_ | 20, 23 |
| [SEQS](SEQS.md) | _Sequence_ | 1 |
| [EVNT](EVNT.md) | _Event_ | 0 |
| [VEC2](VEC2.md) | _Vec2D_ | 0 |
| [VEC3](VEC3.md) | _Vec3D_ | 0 |
| [VEC4](VEC4.md) | _Vec4D_ |
| [QUAT](QUAT.md) | _Quaternion_ |
| [BONE](BONE.md) | _Bone_ | 0, 1 |
| [ATT](ATT.md)  | _Attachment_ |
| [MAT](MAT.md)  | _Material_ | 15 |
| [LAYR](LAYR.md) | _Texture_ | 20 |
| [BNDS](BNDS.md) | _Bounding extents_ |
| [SDEV](SD.md) | _Sequence data - EVNT_ | 0 |
| [SDMB](SD.md) | _Sequence data - BNDS_ |
| [SD2V](SD.md) | _Sequence data - VEC2_ | 0 |
| [SD3V](SD.md) | _Sequence data - VEC3_ |
| [SD4Q](SD.md) | _Sequence data - QUAT_ |
| [SD3R](SD.md) | _Sequence data - REAL_ |
| [SDFG](SD.md) | _Sequence data - FLAG_ |
| [SDS6](SD.md) | _Sequence data - I16_ |
| [CAM](CAM.md)  | _Camera_ |
| [DIV](DIV.md)  | _Geoset_ | 2 |

### Unknown ###
| [STC](STC.md)  | _Sequence stuff_ | 4 |
|:---------------|:-----------------|:--|
| [STG](STG.md)  | _?_ | 0 |
| STS  | _?_ |
| MSEC | _floats?_ | 1 |
| REGN | _floats?_ | 2 |
| BAT  | _floats?_ | 1 |
| MATM | _?_ | 0 |
| IREF | _Matrix?_ | 0 |
| FOR  | _?_ |
| SSGS | _?_ |
| DIS  | _?_ |
| LITE | _Lights?_ |
| PAR  | _?_ |
| PARC | _?_ |
| PHRB | _Ribbons?_ |
| ATVL | _?_ |
| BBSC | _?_ |
| FLAG | _uint16, uint16?_ |
| PHSH | _Referenced by PHRB_ |
| PROJ | _?_ |
| CMP  | _?_ | 2 |
| CMS  | _?_ | 0 |