# Introduction #

The bones as defined in the .m3 files.


# Details #

```

// Size = 156 byte / 0x9C byte
// Incomplete
struct BONE
{
    /*0x00*/ int32 d1; // Keybone? usually -1
    /*0x04*/ Reference name;
    /*0x10*/ uint32 flags; //see below
    /*0x14*/ int16 parent;
    /*0x16*/ uint16 s1;
    /*0x18*/ Aref_VEC3D trans; //Translation animation reference
    /*0x3C*/ Aref_QUAT rot; //Rotation animation reference
    /*0x68*/ Aref_VEC3D scale; //Scale animation reference
    /*0x8C*/ Aref_UINT32 ar1; //Unknown
};

```

## Bone Flags ##
Most flags have been determined through the previewer.

| **Bit Index** | **Bit Address** | **Flag** | **Description** |
|:--------------|:----------------|:---------|:----------------|
| 1 | 0x1 | Inherit translation | Inherit translation from parent bone |
| 2 | 0x2 | Inherit scale | Inherit scale from parent bone |
| 3 | 0x4 | Inherit rotation| Inherit rotation from parent bone |
| 5 | 0x10 | Billboard (1) | Billboard bone |
| 7 | 0x40 | Billboard (2) | Billboard bone (again) |
| 9 | 0x100 | 2D Projection | Projects the bone in 2D? |
| 10 | 0x200 | Animated | Bone has [STC](STC.md) (transform) data. Will ignore [STC](STC.md) data if set to false |
| 11 | 0x400 | Inverse Kinematics | Grants the bone IK properties in the game engine |
| 12 | 0x800 | Skinned | Bone has associated geometry. Necessary for rendering geometry weighted to the bone |
| 14 | 0x2000 | Unknown | Always set to true in Blizzard models. No apparent effect |


# Extra Bone Information #

## Bone Configuration ##
M3 bones have two initial configurations that are in two different formats. The bindpose is first setup using bone matrices which can be found in the IREF chunks. Secondly, once the mesh is weighted to the bones, the initial position, rotation and scale values are applied to a bone in relation to the parent bone if the bone has one. In 3ds Max to apply these transformations appropriately, you have to begin at the deepest bones first and work your way up. This places the model in what I've decided to call the base pose. It's unclear what the purpose of the base pose is, but it seems to have relevance to how the model behaves in the absence of sequence transform data. This is different to WoW's M2 model format which had pivot points for their bones to setup the bindpose and lacked any kind of base pose.

Some of Blizzards models use a terrible feature of 3ds Max that allows you to mirror bones by using negative scales. Examples of this can be found in Immortal.m3 and DarkTemplar.m3 aswell as I'm sure other models where the original artist has decided to mirror the bones through this tool. This can cause serious problems when correctly trying to animate bones outside of the Starcraft engine. In 3ds Max this is particularly a problem as rotations do not factor in the negative scaling resulting in improper orientation of bones during animations. A workaround is to force the bones to use a transform matrix with the negative scaling applied for each rotation keyframe and remove the extra keyframes generated in the scale and translation controllers.

## Animation References ##
M3's use an odd system for referencing their animation data. There is a global list of all animation data represented in the [STC](STC.md) structures for each animation sequence. AnimationReferences found in bones contain animflags of 6 when they contain data for at least one of the animations. Regardless of whether they contain data, they will always have a unique uint32 ID which I suppose is randomly generated. These ID's are referenced in `MODL.STC.animid` list if there is animation data present for that particular animation type in that particular animation sequence, if they are not referenced it indicates there is null animation data for that animation type.

In order to gather the correct animation data from the reference, you must iterate through the `STC.animid` list of uint32 ID's and find the matching ID in the list. Once you find that index, you check the same index in the next section of `STC.animindex` which provides the sequence data index and the index into the sequence data array present in STC.

As an example (all values are arbitrary), if you're trying to find rotation data in a bone, you grab the rot.AnimRef.animid from the bone. You go to the animation sequence data you want to animate this bone with, say `MODL.STC[2]`, iterate through `STC[2].animid` until you find the animid in the list and record its index as 6. You go to `STC[2].animind[6]` and record the `STC[2].animind[6].aind` as 8 and `STC[2].animind[6].sdind` as 3. You'll find the appropriate sequence data for that bone in the `MODL.STC[2].SeqData[3].SD4Q[8]` SD (Sequence Data) chunk and can animate the bone accordingly. I hope this clarifies how animation data is referenced in the M3 file format.