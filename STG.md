# Introduction #

The number of [STG](STG.md) blocks in a .m3 file equals the number of [SEQS](SEQS.md) blocks, and tells where in the [STC](STC.md) list the animations of the corresponding [SEQS](SEQS.md) block starts.


# Details #

```

// Size = 16 byte / 0x10 byte
// Complete
struct STG
{
    /*0x00*/ Reference name;
    /*0x08*/ Reference stcID;
};

```