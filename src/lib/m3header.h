/****************************************************************************
*                                                                           *
*   libm3                                                                   *
*   Copyright (C) 2010  Gunnar Lilleaasen                                   *
*                                                                           *
*   This program is free software; you can redistribute it and/or modify    *
*   it under the terms of the GNU General Public License as published by    *
*   the Free Software Foundation; either version 2 of the License, or       *
*   (at your option) any later version.                                     *
*                                                                           *
*   This program is distributed in the hope that it will be useful,         *
*   but WITHOUT ANY WARRANTY; without even the implied warranty of          *
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the           *
*   GNU General Public License for more details.                            *
*                                                                           *
*   You should have received a copy of the GNU General Public License along *
*   with this program; if not, write to the Free Software Foundation, Inc., *
*   51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.             *
*                                                                           *
****************************************************************************/

#ifndef M3HEADER_H_
#define M3HEADER_H_

typedef unsigned char uint8;
typedef char int8;
typedef unsigned short uint16;
typedef short int16;
typedef unsigned int uint32;
typedef int int32;

#include "vec3d.h"
#include "quaternion.h"

namespace m3
{
    struct Reference 
    {
        /*0x00*/ uint32 nEntries;
        /*0x04*/ uint32 ref;
    };

    struct ReferenceEntry
    {
        /*0x00*/ char id[4];
        /*0x04*/ uint32 offset;
        /*0x08*/ uint32 nEntries;
        /*0x0C*/ uint32 type;
    };

    struct MD33
    {
        /*0x00*/ char id[4];
        /*0x04*/ uint32 ofsRefs;
        /*0x08*/ uint32 nRefs;
        /*0x0C*/ Reference MODL;
    };

    enum ModelType
    {
        TYPE1 = 20,
        TYPE2 = 23
    };

    enum VertexFormat
    {
        VERTEX_STANDARD,
        VERTEX_EXTENDED
    };

    struct MODL23
    {
        /*0x00*/ Reference name;
        /*0x08*/ uint32 version;
        /*0x0C*/ Reference sequenceHeader;
        /*0x14*/ Reference sequenceData;
        /*0x1C*/ Reference sequenceLookup;
        /*0x24*/ uint32 d2;
        /*0x28*/ uint32 d3;
        /*0x2C*/ uint32 d4;
        /*0x30*/ Reference STS;
        /*0x38*/ Reference bones;
        /*0x40*/ uint32 d5;
        /*0x44*/ uint32 flags;
        /*0x48*/ Reference vertexData; // uint8
        /*0x50*/ Reference divisions;
        /*0x58*/ Reference B; // uint16

        /*0x60*/ Vec3D extents[2];
        /*0x78*/ float radius;

        /*0x7C*/ uint32 d7;
        /*0x80*/ uint32 d8;
        /*0x84*/ uint32 d9;
        /*0x88*/ uint32 d10;
        /*0x8C*/ uint32 d11;
        /*0x90*/ uint32 d12;
        /*0x94*/ uint32 d13;
        /*0x98*/ uint32 d14;
        /*0x9C*/ uint32 d15;
        /*0xA0*/ uint32 d16;
        /*0xA4*/ uint32 d17;
        /*0xA8*/ uint32 d18;
        /*0xAC*/ uint32 d19;

        /*0xB0*/ Reference attachments;
        /*0xB8*/ Reference attachmentLookup; // uint16
        /*0xC0*/ Reference lights;
        /*0xC8*/ Reference SHBX;
        /*0xD0*/ Reference cameras;
        /*0xD8*/ Reference D; // uint16
        /*0xE0*/ Reference materialLookup;
        /*0xE8*/ Reference materials;
        /*0xF0*/ Reference DIS;
        /*0xF8*/ Reference CMP;

        /*0x100*/ Reference TER;
        /*0x108*/ Reference VOL;
        /*0x110*/ uint32 d21;
        /*0x114*/ uint32 d22;
        /*0x118*/ Reference CREP;
        /*0x120*/ Reference PAR;
        /*0x128*/ Reference PARC;
        /*0x130*/ Reference RIB;
        /*0x138*/ Reference PROJ;
        /*0x140*/ Reference FOR;
        /*0x148*/ Reference WRP;
        /*0x150*/ uint32 d24;
        /*0x154*/ uint32 d25;
        /*0x158*/ Reference PHRB;
        /*0x160*/ uint32 d27;
        /*0x164*/ uint32 d28;
        /*0x168*/ uint32 d29;
        /*0x16C*/ uint32 d30;
        /*0x170*/ uint32 d32;
        /*0x174*/ uint32 d33;
        /*0x178*/ Reference IKJT;
        /*0x180*/ uint32 d35;
        /*0x184*/ uint32 d36;
        /*0x188*/ Reference PATU;
        /*0x190*/ Reference TRGD;
        /*0x198*/ Reference IREF;
        /*0x1A0*/ Reference E; // uint32
        /*0x1A8*/ float matrix[4][4];
        /*0x1E8*/ Vec3D extent[2];
        /*0x200*/ float rad;
        /*0x204*/ Reference SSGS;
        /*0x20C*/ Reference ATVL;
        /*0x210*/ uint32 d61;
        /*0x214*/ Reference F; // uint16
        /*0x21C*/ Reference G; // uint16
        /*0x224*/ Reference BBSC;
        /*0x22C*/ Reference TMD;
        /*0x234*/ uint32 d62;
        /*0x238*/ uint32 d63;
        /*0x23C*/ uint32 d64;
    };

    struct MODL20
    {
        /*0x00*/ Reference name;
        /*0x08*/ uint32 version;
        /*0x0C*/ Reference sequenceHeader;
        /*0x14*/ Reference sequenceData;
        /*0x1C*/ Reference sequenceLookup;
        /*0x24*/ uint32 d2;
        /*0x28*/ uint32 d3;
        /*0x2C*/ uint32 d4;
        /*0x30*/ Reference STS;
        /*0x38*/ Reference bones;
        /*0x44*/ uint32 d5;
        /*0x44*/ uint32 flags;
        /*0x48*/ Reference vertexData; // uint8
        /*0x50*/ Reference divisions;
        /*0x58*/ Reference B; // uint16

        /*0x60*/ Vec3D extents[2];
        /*0x78*/ float radius;

        /*0x7C*/ uint32 d7;
        /*0x80*/ uint32 d8;
        /*0x84*/ uint32 d9;
        /*0x88*/ uint32 d10;
        /*0x8C*/ uint32 d11;
        /*0x90*/ uint32 d12;
        /*0x94*/ uint32 d13;
        /*0x98*/ uint32 d14;
        /*0x9C*/ uint32 d15;
        /*0xA0*/ uint32 d16;
        /*0xA4*/ uint32 d17;
        /*0xA8*/ uint32 d18;
        /*0xAC*/ uint32 d19;
        
        /*0xB0*/ Reference attachments;
        /*0xB8*/ Reference attachmentLookup; // uint16
        /*0xC0*/ Reference lights;
        /*0xC8*/ Reference cameras;
        /*0xD0*/ Reference D; // uint16
        /*0xD8*/ Reference materialLookup;
        /*0xE0*/ Reference materials;
        /*0xE8*/ Reference DIS;
        /*0xF0*/ Reference CMP;
        /*0xF8*/ Reference TER;

        /*0x100*/ uint32 d20;
        /*0x104*/ uint32 d21;
        /*0x108*/ uint32 d22;
        /*0x10C*/ uint32 d23;
        /*0x110*/ Reference CREP;
        /*0x118*/ Reference PAR;
        /*0x120*/ Reference PARC;
        /*0x128*/ Reference RIB;
        /*0x130*/ Reference PROJ;
        /*0x138*/ Reference FOR;
        /*0x140*/ uint32 d25;
        /*0x144*/ uint32 d26;
        /*0x148*/ uint32 d27;
        /*0x14C*/ uint32 d28;
        /*0x150*/ Reference PHRB;
        /*0x158*/ uint32 d30;
        /*0x15C*/ uint32 d31;
        /*0x160*/ uint32 d32;
        /*0x164*/ uint32 d33;
        /*0x168*/ uint32 d34;
        /*0x16C*/ uint32 d35;
        /*0x170*/ Reference IKJT;
        /*0x178*/ uint32 d36;
        /*0x17C*/ uint32 d37;
        /*0x180*/ Reference PATU;
        /*0x188*/ Reference TRGD;
        /*0x190*/ Reference IREF;
        /*0x198*/ Reference E; // int32
        
        /*0x1A0*/ float matrix[4][4];
        /*0x1E0*/ Vec3D extent[2];
        /*0x1F8*/ float rad;

        /*0x1FC*/ Reference SSGS;
        /*0x204*/ uint32 d38;
        /*0x208*/ uint32 d39;
        /*0x20C*/ Reference BBSC;

        /*0x214*/ uint32 d40;
        /*0x218*/ uint32 d41;
        /*0x21C*/ uint32 d42;
        /*0x220*/ uint32 d43;
        /*0x224*/ uint32 d44;
    };

    struct Bone
    {
        int32 d1; // Keybone?
        Reference name;
        uint32 flags;
        int16 parent;
        int16 s1;

        float floats[34];
    };

    struct VertexExt // 36 byte
    {
        Vec3D pos;
        uint8 boneWeight[4];
        uint8 boneIndex[4];
        uint8 normal[4];  //normal_x = (float)normal[0]/255.0f...
        int16 uv[2];
        uint32 d1;
        uint8 tangent[4];
    };

    struct Vertex // 32 byte
    {
        Vec3D pos;
        uint8 boneWeight[4];
        uint8 boneIndex[4];
        uint8 normal[4];  //normal_x = (float)normal[0]/255.0f...
        uint16 uv[2];
        uint8 tangent[4];
    };


    struct MATM
    {
        uint32 d1; // Index into geosets?
        uint32 d2; // Index into MAT-table?
    };

    struct MAT
    {
        Reference name;
        int ukn1[8];
        float x, y;  //always 1.0f
        Reference layers[13];
        int ukn2[15];
    };

    struct LAYR
    {
        int unk;
        Reference name;
        float unk2[85];
    };

    struct Division
    {
        /*0x00*/ Reference faces; // U16
        /*0x08*/ Reference regions; // REGN - Region
        /*0x10*/ Reference BAT;
        /*0x18*/ Reference MSEC;
    };

    struct Region {
        uint32 unk;
        uint16 ofsVertices;
        uint16 nVertices;
        uint32 ofsIndices;
        uint32 nIndices; // reference into DIV.faces
        uint8 unknown[12];
    };

    struct CAM
    {
        /*0x00*/ int32 d1;
        /*0x04*/ Reference name;
        /*0x0C*/ uint16 flags1;
        /*0x0E*/ uint16 flags2;
    };

    struct EVNT
    {
        /*0x00*/ Reference name;
        /*0x08*/ int16 unk1[4];
        /*0x10*/ float matrix[4][4];
        /*0x50*/ int32 unk2[4];
    };

    struct ATT
    {
        /*0x00*/ int32 unk;
        /*0x04*/ Reference name;
        /*0x0C*/ int32 bone;
    };

    struct PHSH
    {
        float m[4][4];
        float f1;
        float f2;
        Reference refs[5];
        float f3;
    };

    struct SEQS
    {
        /*0x00*/ int32 d1;
        /*0x04*/ int32 d2;
        /*0x08*/ Reference name;
        /*0x10*/ int32 d3;
        /*0x14*/ uint32 length;
        /*0x18*/ int32 d4;
        /*0x1C*/ uint32 flags;
        /*0x20*/ int32 unk[5];
        /*0x34*/ Vec3D extents[2];
        /*0x4C*/ float radius;
        /*0x50*/ int32 d5;
        /*0x54*/ int32 d6;
    };

    struct STC
    {
        /*0x00*/ Reference name;
        /*0x08*/ uint16 s1;
        /*0x0A*/ uint16 s2;
        /*0x0C*/ uint16 s3;
        /*0x0E*/ uint16 s4;
        /*0x12*/ Reference unk2; // uint32
        /*0x1A*/ Reference unk3; // uint32
        /*0x22*/ uint32 d3;
        /*0x24*/ Reference evt;
        /*0x2C*/ Reference unk4[11]; // Seems to be transformation data
        /*0x84*/ Reference bnds;
    };

    struct STS
    {
        /*0x00*/ Reference unk1; // uint32
        /*0x08*/ int32 unk[3];
        /*0x14*/ int16 s1;
        /*0x16*/ int16 s2;
    };

    struct STG
    {
        /*0x00*/ Reference name;
        /*0x08*/ Reference stcID;
    };
    
    struct SD
    {
        /*0x00*/ Reference timeline;
        /*0x08*/ uint32 flags;
        /*0x0C*/ uint32 length;
        /*0x10*/ Reference data;
    };

    struct BNDS
    {
        /*0x00*/ Vec3D extents1[2];
        /*0x18*/ float radius1;
        /*0x1C*/ Vec3D extents2[2];
        /*0x34*/ float radius2;
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
        float x, y, z, w;
    };
}

#endif // M3HEADER_H_