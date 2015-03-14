# Introduction #

The MODL block in .m3 files can contain various structs. So far two structs have been identified in the files, type 20 and 23.

These blocks are basically the model header.


# Details #

```

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
    /*0x50*/ Reference views;
    /*0x58*/ Reference bonelookup; // uint16, vertices reference this bonelookup

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
    /*0x50*/ Reference views;
    /*0x58*/ Reference bonelookup; // uint16, vertices reference this bonelookup

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

```