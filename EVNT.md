# Introduction #

Event definitions as defined in the .m3 files and found referenced in the STC definitions. It contains something that looks like a matrix, though I am not sure what it is used for.

## Sequence End Event ##
A default event is present for every sequence with the name 'Evt\_SeqEnd' that generally fires on the last frame of the sequence. This event is mainly used in sequences that loop. It indicates at which frame the sequence should end and when the next sequence should begin in the loop. I believe this has been implemented to allow greater control of how the sequences flow in the loop as opposed to how they play out when called alone. If these events are absent, looped sequences that are called will play one of the sequences in the loop and freeze on the last frame without continuing through the loop. These events must be generated on export if you intend to have the model sequences loop properly within the game engine.

# Details #

```

// Size = 96 byte/ 0x60 byte
// Incomplete
struct EVNT
{
    /*0x00*/ Reference name;
    /*0x08*/ int16 unk1[4];
    /*0x10*/ float matrix[4][4];
    /*0x50*/ int32 unk2[4];
};

```