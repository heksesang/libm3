# Introduction #

Header for animation sequences defined in the .m3 files.

# Details #

```

// Size = 96 byte / 0x60 byte
// Incomplete
struct SEQS
{
    /*0x00*/ int32 d1[2]; // always -1?
    /*0x08*/ Reference name;
    /*0x14*/ int32 SeqStart;
    /*0x18*/ int32 SeqEnd;
    /*0x1C*/ int32 moveSpeed; //used in movement sequences (Walk, Run)
    /*0x20*/ uint32 flags; //see below for flags
    /*0x24*/ uint32 frequency; //how often it plays
    /*0x28*/ uint32 ReplayStart;
    /*0x2C*/ uint32 ReplayEnd;
    /*0x30*/ int32 unk[2];
    /*0x38*/ Bounds bndSphere;
    /*0x58*/ int32 d5[2];
};

```
## Sequence Description ##
Main header for the various sequences of a model. The STG chunk contains the same amount of indices as this block and functions as a lookup table that connects STC transformation data to a particular SEQS entry. Some models (Marine.m3) have multiple STC blocks for a single SEQS entry, the purpose of which is poorly understood.

## Sequence Flags ##
| **Bit Index** | **Bit Address** | **Flag** | **Description** |
|:--------------|:----------------|:---------|:----------------|
| 1| 0x1 | Non-Looping | If set true, sequence is non-looping |
| 2| 0x2 | Global Sequence (hard) | Forces sequence to play regardless of other sequences |
| 4| 0x8 | Global Sequence (previewer) | Sets Global Sequence true in previewer but otherwise has no effect |

The looping flag works inversely, so that if it's set to true it is non-looping. The hard global sequence flag forces the model to play the sequence endlessly and it would seem no other sequence can affect the model. The previewer global sequence flag sets the Global Animation boolean to true in the previewer but seems to have no apparent affect on the model. Further testing with the global flags is required.

### Sequence ID's ###
Names function as an ID within the engine to call certain sequences in response to particular game events (_Walk_ when moving, _Stand_ when idle, _Attack_ when attacking, etc). Multiple sequences for the same sequence ID need to have numbers appended after a space to avoid sequence calling conflicts (i.e. _Stand 01_, _Attack 03_).

### Frequency ###
Frequency determines the probability a sequence will be called. It's only relevant if multiple sequences are defined for the same sequence ID. What's interesting is that this is a uint32 value and not a float. From what I have observed, the values function as a ratio in comparison to the other sequence frequencies in the group. This is different from the WoW format which used percentages. For example, say _Attack 01_ has a frequency of 3 while _Attack 02_ has a frequency of 1. I believe this means _Attack 01_ is three times more likely to be called than _Attack 02_. Frequency values can then be arbitrary however in Blizzard models it is common to see either values around 1 or values around 100.

### Start and End frames ###
Start and end frames can be set for sequences that determine ultimately at what frame the sequence begins and ends along the sequences timeline. Almost all Blizzard models I have looked at start at frame 0 and end at the final frame of the sequence.

There is additional Start and End frames for a Replay setting (seen in the previewer). I have not done any testing but I assume that this is used to play a different timeframe range when the sequence has been called multiple times within a loop. Most Blizzard models I have looked at set their replay start and end frames to 1, which results in a length of 0 indicated in the previewer. I believe if the length is 0, the animation will be played from start to finish as normal regardless of it being looped. I may follow up with further tests to see if this is correct.

