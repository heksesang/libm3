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
        /*0x00*/ uint32 entries;
        /*0x04*/ uint32 referenceid;
    };

    struct ReferenceEntry
    {
        /*0x00*/ char id[4];
        /*0x04*/ uint32 offset;
        /*0x08*/ uint32 entries;
        /*0x0C*/ uint32 d1;
    };

    struct ModelHeader
    {
        /*0x00*/ char id[4];
        /*0x04*/ uint32 ofsRefs;
        /*0x08*/ uint32 nRefs;
        /*0x0C*/ Reference MODL;
    };

    struct MODL
    {
        uint32 nName;
        uint32 refName;
        uint32 d1; // Flags?
        uint32 nAnimations; // SEQS (88 byte)
        uint32 refAnimations;
        uint32 nB; // STC_ (140 byte)
        uint32 refB;
        uint32 nC; // STG_ (16 byte)
        uint32 refC;
        uint32 d2;
        uint32 d3;
        uint32 d4;
        uint32 nD; // STS_ (24 byte)
        uint32 refD;
        uint32 nBones;
        uint32 refBones;
        uint32 d5;
        uint32 d6;
        uint32 nF; // uint8
        uint32 refF;
        uint32 nG; // DIV_ (36 byte)
        uint32 refG;
        uint32 nH; // uint16
        uint32 refH;

        Vec3D extents[2];
        float radius;

        uint32 d7;
        uint32 d8;
        uint32 d9;
        uint32 d10;
        uint32 d11;
        uint32 d12;
        uint32 d13;
        uint32 d14;
        uint32 d15;
        uint32 d16;
        uint32 d17;
        uint32 d18;
        uint32 d19;
        
        uint32 nAttachments; // ATT_
        uint32 refAttachments;
        uint32 nI; // uint16
        uint32 refI;
        uint32 nLights; // LITE
        uint32 refLights;

        uint32 d20;
        uint32 d21;
        uint32 d22;
        uint32 d23;
        uint32 d24;
        uint32 d25;

        uint32 nJ; // MATM
        uint32 refJ;
        uint32 nK; // MAT_
        uint32 refK;
        uint32 nL; // DIS_
        uint32 refL;

        
        uint32 d26;
        uint32 d27;
        uint32 d28;
        uint32 d29;
        uint32 d30;
        uint32 d31;
        uint32 d32;
        uint32 d33;
        uint32 d34;
        uint32 d35;

        uint32 nM; // PAR_
        uint32 refM;
        uint32 nN; // PARC
        uint32 refN;
        
        uint32 d36;
        uint32 d37;
        uint32 d38;
        uint32 d39;
        uint32 d40;
        uint32 d41;
        uint32 d42;
        uint32 d43;
        uint32 d44;
        uint32 d45;

        uint32 nO; // PHRB
        uint32 refO;

        uint32 d46;
        uint32 d47;
        uint32 d48;
        uint32 d49;
        uint32 d50;
        uint32 d51;
        uint32 d52;
        uint32 d53;
        uint32 d54;
        uint32 d55;
        uint32 d56;
        uint32 d57;
        uint32 d58;
        uint32 d59;

        uint32 nP; // IREF
        uint32 refP;

        uint32 d60;
        uint32 d61;

        float floats[23];

        uint32 nQ; // SSGS
        uint32 refQ;
        uint32 nR; // ATVL
        uint32 refR;
        uint32 nS; // uint16
        uint32 refS;
        uint32 nT; // uint16
        uint32 refT;
        uint32 nU; // BBSC
        uint32 refU;

        uint32 d62;
        uint32 d63;
        float f1;
        uint32 d64;
    };

}

#endif // M3HEADER_H_