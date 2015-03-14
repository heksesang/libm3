# Introduction #

This article has been started by [Richard Geslot](http://www.richard-geslot.com). It’s based on the Wiki of [dot.qwerty](http://code.google.com/u/dot.qwerty/) and the 010 editor template & the 3DSmax importer of [madyavic](http://code.google.com/u/madyavic/).

I won’t write a simple copy-paste of all my code, I prefer to publish the difficult samples of my code and comment them.
All M3 structures variables names are copied from the 010 editor template 0.95 of madyavic.

Prerequisites:
  * Good knowledge of C++ & DirectX.
  * notions of 3D models animation: indices, vertices, bones, transform quaternion...
  * Read the 010 template of madyavic (because I nearly took the same variables names)


# First: Load the M3 from file #

This section is divided into 2 parts: The load and the pre-compute.

## Load ##

The Load consist in reading the M3 file and set data into structures. I won’t stay a lot on this part, this is a long but simple code that I won't give in order to concentrate in the hard stuff.

My import code is nearly a copy past of the 010 template.
For example, the DIV structure is :

```
struct DIV_
{
   Ref faces;
   Ref submesh;
   Ref BAT;
   Ref MSEC;
   unsigned long unk;
};
```

I made exceptions for some structures. Indeed, it’s sometime easier for coding to add some extra-informations in the struct.

For example, a Ref is a reference to a chunk of data, so it’s easier to get the pointer of the data in the Ref struct :

```
struct Ref
{
//the real struc file :
   HeadRef HRef;
   Tag chunk;
//for the program (not in the struct file) :
   unsigned int nbElement;//nb of elements in pData 
                   //(HRef.nData already contains this value BUT it's not always true...)
   void* pData;//points to the malloc data reference by this ref struct
};
```

For example, if I want to access to bones data:

```
//MODL m_mdata is a member of my M3 class
BONE* bone = (BONE*)(m_mdata.mBone.pData); //the data
m_mdata.mBone.nbElement; //number of bones
```



here are all shortcuts that I will use along this document :

```
BONE* bone = (BONE*)(m_mdata.mBone.pData);
STC_* stc = (STC_*)(m_mdata.mSTC.pData);
unsigned int nbStc = m_mdata.mSTC.nbElement;
DIV_* div = (DIV_*)(m_mdata.mDIV.pData);
REGN* regn = (REGN*)(div[0].submesh.pData);
unsigned int nbRegn = div[0].submesh.nbElement;
BAT_* bat = (BAT_*)(div[0].BAT.pData);
unsigned short* boneLU = (unsigned short*)(m_mdata.mBoneLU.pData);
unsigned int nbBoneLU = m_mdata.mBoneLU.nbElement;
D3DXMATRIXA16* iref = (D3DXMATRIXA16*)(m_mdata.mIREF.pData);
unsigned int nbIref = m_mdata.mIREF.nbElement;
LPDIRECT3DVERTEXBUFFER9 vertexBuffer = (LPDIRECT3DVERTEXBUFFER9)(m_mdata.mVert.pData);
LPDIRECT3DINDEXBUFFER9 indexBuffer = (LPDIRECT3DINDEXBUFFER9)(div[0].faces.pData);
unsigned int nbVertex = m_mdata.mVert.nbElement;
unsigned int nbIndex = div[0].faces.nbElement;
MATM* matm = (MATM*)(m_mdata.mMatLU.pData);
MAT_* material = (MAT_*)(m_mdata.mMat.pData);
```


About the vertex buffer : I stocked it into 2 buffers:
  * The LPDIRECT3DVERTEXBUFFER9 that is here:
```
//This is the buffer that I use for rendering
LPDIRECT3DVERTEXBUFFER9 vertexBuffer = (LPDIRECT3DVERTEXBUFFER9)(m_mdata.mVert.pData);
```

  * And the Vertex structure that is directly a member of my class:
```
//It’s the struct as it is read in the M3 file.
Vertex* m_StaticMeshVB;
```

## Pre-compute for optimization ##

Ok now the pre-compute (for optimization, check errors…):

```

//my code is made for element with 1 DIV... for the moment, I have never seen M3 with more than 1 DIV...
//ERROR_MESSAGE is just a function that display error message.
if ( m_mdata.mDIV.nbElement != 1 ) { ERROR_MESSAGE("nbElement != 1"); }

//check that number of submesh = number of BAT
if ( div[0].submesh.nbElement != div[0].BAT.nbElement ) { ERROR_MESSAGE("submesh.nbElement != BAT.nbElement"); }

//for each REGN, find its BAT (avoid to do that at each frame)
for(unsigned int iRegion=0; iRegion<div[0].submesh.nbElement; iRegion++)
{
   regn[iRegion].linkBat = NULL;
   //for each BAT
   for(unsigned int iBat=0; iBat<div[0].BAT.nbElement; iBat++)
   {
      if ( bat[iBat].subid == iRegion ) 
      {  
         //BAT_* linkBat is a member of the struc REGN that isn’t in the real file structure. 
         //I add it in my structure in order to save this optimization.
         regn[iRegion].linkBat = &bat[iBat];
      }
   }
   if ( regn[iRegion].linkBat == NULL ) { ERROR_MESSAGE("no link bat for this region ?"); }
}

//pre-compute for each bones
for(unsigned int iBone=0; iBone<m_mdata.mBone.nbElement; iBone++)
{

   //you could see my EXmemory::Malloc() function as a simple malloc()
   bone[iBone].transid.goodStcAnimid = (unsigned long*)EXmemory::Malloc(nbStc*sizeof(unsigned long));
   bone[iBone].rotid.goodStcAnimid   = (unsigned long*)EXmemory::Malloc(nbStc*sizeof(unsigned long));
   bone[iBone].scaleid.goodStcAnimid = (unsigned long*)EXmemory::Malloc(nbStc*sizeof(unsigned long));

   //for each stc
   for(unsigned int iSTC=0; iSTC<nbStc; iSTC++)
   {
      //not found yet
      bone[iBone].transid.goodStcAnimid[iSTC] = 0xFFffFFff;
      bone[iBone].rotid.goodStcAnimid[iSTC] = 0xFFffFFff;
      bone[iBone].scaleid.goodStcAnimid[iSTC] = 0xFFffFFff;

      unsigned long* animid = (unsigned long*)(stc[iSTC].animid.pData);
      unsigned int nbAnimid = stc[iSTC].animid.nbElement;

      //for each stc.animid
      for(unsigned int iAnimid=0; iAnimid<nbAnimid; iAnimid++)
      {
         //unsigned long* goodStcAnimid is a member of the struc Animref that isn’t in the real file structure. 
         //I add it in my structure in order to save this optimization.

         if ( bone[iBone].transid.animid == animid[iAnimid] ) { bone[iBone].transid.goodStcAnimid[iSTC] = iAnimid; }
         if ( bone[iBone].rotid.animid == animid[iAnimid] ) { bone[iBone].rotid.goodStcAnimid[iSTC] = iAnimid; }
         if ( bone[iBone].scaleid.animid == animid[iAnimid] ) { bone[iBone].scaleid.goodStcAnimid[iSTC] = iAnimid; }
      }
   }
}


//for each vertex
for(unsigned int iVert=0; iVert < m_mdata.mVert.nbElement; iVert++)
{
   unsigned short ownerRegion = 0xffFF;

   //search the good REGN
   for(unsigned int iRegn=0; iRegn<nbRegn ; iRegn++)
   {
      unsigned long minv = regn[iRegn].vind;
      unsigned long maxv = minv + regn[iRegn].vnum;
      if ( iVert >= minv && iVert < maxv )
      {
         ownerRegion = iRegn;
         break;
      }
   }
   if ( ownerRegion == 0xFFff ) { ERROR_MESSAGE("ownerRegion ?"); }

   //for each weight
   for(unsigned int iWeight = 0; iWeight < 4; iWeight++)
   {
      //if there is a weight
      if ( m_StaticMeshVB[iVert].weBone[iWeight] > 0.0f ) 
      { 
         //unsigned short realOwnerBones[4] is a member of the struc Vertex that isn’t in the real file structure. 
         //I add it in my structure in order to save this optimization.
         m_StaticMeshVB[iVert].realOwnerBones[iWeight] = boneLU[  m_StaticMeshVB[iVert].weIndice[iWeight] + regn[ownerRegion].bind  ];

         if(m_StaticMeshVB[iVert].realOwnerBones[iWeight] == 0xffFF) { ERROR_MESSAGE("bone not find"); }
         if ( m_StaticMeshVB[iVert].realOwnerBones[iWeight] >= m_mdata.mIREF.nbElement ) { ERROR_MESSAGE("BoneIndice >= nbIref"); }

         //D3DXVECTOR4 posXiref[4] is a member of the struc Vertex that isn’t in the real file structure. 
         //I add it in my structure in order to save this optimization. This save directly the SaticPos * IREF matrix
         
         D3DXVec3Transform(
                    &m_StaticMeshVB[iVert].posXiref[iWeight],
                    &m_StaticMeshVB[iVert].pos,
                    &iref[  m_StaticMeshVB[iVert].realOwnerBones[iWeight]  ]);
      }
   }
		
}

```

# Second: The Update #

The aim of this function is to update all bones’s transform at each frame.

We need these 3 pointers (malloc at the start of the program in function of the number of bone) :

```
D3DXVECTOR3* m_BoneTranslation; 
D3DXQUATERNION* m_BoneRotation;
D3DXVECTOR3* m_BoneScale;
//for example: m_BoneTranslation[x] is the translation of the bone number x
```

We also need these variables:

```
unsigned int currentSTC; //it’s just the number of the animation, set it with 0 if you don’t know how many animation the model have.

DWORD timeOfCurrentAnim;//a timer that will always be between 0 and the lengh of the current animation

DWORD deltaTime; //the delta between this time and the time of the last update
```

And now, let’s update ! :

```
//this is how I get the Lengh of the current animation :
unsigned long currentAnimationLength;
if ( stc[currentSTC].Trans.pData != NULL )
{
   currentAnimationLength = ((Animblock*)(stc[currentSTC].Trans.pData))[0].framelength;
}
else if ( stc[currentSTC].Rot.pData != NULL )
{
   currentAnimationLength = ((Animblock*)(stc[currentSTC].Rot.pData))[0].framelength;
}
else
{
   ERROR_MESSAGE("not enough information about current animation");
}

//update the time of the current animation
timeOfCurrentAnim += deltaTime;
if ( timeOfCurrentAnim >= currentAnimationLength ) 
{
   timeOfCurrentAnim = *timeOfCurrentAnim % currentAnimationLength;
}


//and now, set the matrix of each bones
for(unsigned int iBone=0; iBone<m_mdata.mBone.nbElement; iBone++)
{

   short indiceBoneParent = bone[iBone].boneparent;

   GetBoneTransform(&m_BoneTranslation[iBone], &bone[iBone].transid, 1, *timeOfCurrentAnim,currentAnimationLength, &bone[iBone].pos,currentSTC);	
   GetBoneTransform(&m_BoneRotation[iBone],    &bone[iBone].rotid,   2, *timeOfCurrentAnim,currentAnimationLength, &bone[iBone].rot,currentSTC );		
   GetBoneTransform(&m_BoneScale[iBone],       &bone[iBone].scaleid, 3, *timeOfCurrentAnim,currentAnimationLength, &bone[iBone].scale,currentSTC);

   if ( indiceBoneParent != -1 )
   {
      m_BoneScale[iBone].x *= m_BoneScale[indiceBoneParent].x;
      m_BoneScale[iBone].y *= m_BoneScale[indiceBoneParent].y;
      m_BoneScale[iBone].z *= m_BoneScale[indiceBoneParent].z;

      D3DXQuaternionMultiply(&m_BoneRotation[iBone],&m_BoneRotation[iBone],&m_BoneRotation[indiceBoneParent]);
			
      m_BoneTranslation[iBone].x *= m_BoneScale[indiceBoneParent].x;
      m_BoneTranslation[iBone].y *= m_BoneScale[indiceBoneParent].y;
      m_BoneTranslation[iBone].z *= m_BoneScale[indiceBoneParent].z;
      vecXquat(&m_BoneTranslation[iBone],&m_BoneRotation[indiceBoneParent]);
      m_BoneTranslation[iBone] += m_BoneTranslation[indiceBoneParent];

   }


}
```

Here is the GetBoneTransform function:

```

//outVector.w is not always use.
//transformType: 
//   1: translation(outVector is D3DXVECTOR3*)  
//   2: rotation (outVector is D3DXQUATERNION*)    
//   3: scaling (outVector is D3DXVECTOR3*)  
void ModelM3::GetBoneTransform(void* outVec, 
                               Animref* animReference, 
                               char transformType, 
                               unsigned int currentFrame, 
                               unsigned int animationLength, 
                               void* ifNoAnim, 
                               unsigned int currentSTC)
{
   if ( transformType == 0 || transformType >= 4 )
   {
      ERROR_MESSAGE("transformType = 1, 2 or 3");
   }

   STC_* stc = (STC_*)(m_mdata.mSTC.pData);
   unsigned int nbStc = m_mdata.mSTC.nbElement;
   if ( currentSTC >= nbStc ) { ERROR_MESSAGE("currentSTC >= nbStc"); currentSTC = 0; }
   unsigned long* animid = (unsigned long*)(stc[currentSTC].animid.pData);
   AnimationIndex* animoffs = (AnimationIndex*)(stc[currentSTC].animoffs.pData);

   if ( animReference->goodStcAnimid == NULL ) { ERROR_MESSAGE("NULL"); }
   unsigned long ID = animReference->goodStcAnimid[currentSTC];
	
   if ( ID == 0xFFffFFff ) 
   { 
      if ( transformType == 1 ) { *((D3DXVECTOR3*)outVec)    = *((D3DXVECTOR3*)ifNoAnim); }
      if ( transformType == 2 ) { *((D3DXQUATERNION*)outVec) = *((D3DXQUATERNION*)ifNoAnim); }
      else                      { *((D3DXVECTOR3*)outVec)    = *((D3DXVECTOR3*)ifNoAnim); }
   }
   else
   {

      unsigned short correctID;
      Ref* refAnimBlock;

      if ( transformType == 1 )//translation
      {
         correctID = 2;
         refAnimBlock = & stc[currentSTC].Trans;
      }
      else if ( transformType == 2 )//rotation
      {
         correctID = 3;
         refAnimBlock = & stc[currentSTC].Rot;
      }
      else if ( transformType == 3 )//scaling
      {
         correctID = 2;
         refAnimBlock = & stc[currentSTC].Trans;
      }
      else { ERROR_MESSAGE("transformType ?"); }

      //check that it's a good ID
      if ( animoffs[ID].sdind != correctID ) { ERROR_MESSAGE_1("%d isn't a good ID",animoffs[ID].sdind); }

      //check .aind
      if ( animoffs[ID].aind >= refAnimBlock->nbElement ) { ERROR_MESSAGE("uncorrect ID"); }

      Animblock animblock = ((Animblock*)(refAnimBlock->pData))[animoffs[ID].aind];
		
      //I have never seen M3 with animblock.framelength != animationLength...
      if ( animblock.framelength != animationLength ) { ERROR_MESSAGE("animblock.framelength != animationLength"); }

      unsigned int* frame = (unsigned int*)(animblock.frames.pData);


      //find the good frame
      unsigned int lowerRefFrame = 0;
      for(unsigned int i=0;  i<animblock.frames.nbElement;  i++)
      {
         if ( currentFrame < frame[i] || (i == animblock.frames.nbElement-1 && currentFrame == frame[i] ) ) 
         { 
            lowerRefFrame = i; break; 
         }
      }

      if ( lowerRefFrame >= 1 )
      {
         lowerRefFrame -= 1;
         float lerpValue = ((float)currentFrame - (float)frame[lowerRefFrame]) / ((float)frame[lowerRefFrame+1] - (float)frame[lowerRefFrame]);

         if ( transformType == 2 )
         {
            D3DXVECTOR4* data = (D3DXVECTOR4*)(animblock.data.pData);

            D3DXVec4Lerp(
                (D3DXVECTOR4*)outVec,
                &data[lowerRefFrame  ],  
                &data[lowerRefFrame+1],  
                lerpValue);
         }
         else
         {
            D3DXVECTOR3* data = (D3DXVECTOR3*)(animblock.data.pData);

            D3DXVec3Lerp(
                (D3DXVECTOR3*)outVec,  
                &data[lowerRefFrame  ],  
                &data[lowerRefFrame+1],  
                lerpValue);
         }

      }
      else
      {
         ERROR_MESSAGE("frame not found");
      }

   }
}

```

# Third: Render the model #

The idea of my rendering function is to run each submesh.
For each submesh: set its material, and run each vertex own by this submesh, finally, render this group of vertex.

```

//in most cases M3 must be rendered with CCW mode
//careful : some materials need to be rendered with D3DCULL_NONE if they are "Two-sided"
//look at "Material Flags" in the MaterialDefinitions page for more information.
m_pd3dDevice->SetRenderState( D3DRS_CULLMODE, D3DCULL_CCW );

//if you don’t use vertex shaders, set the general matrix of the model (here I choose identity)
D3DXMATRIXA16 ident;
D3DXMatrixIdentity( &ident );
m_pd3dDevice->SetTransform( D3DTS_WORLD, ident);

//for each submesh
for(unsigned int iSubmesh=0; iSubmesh<nbRegn; iSubmesh++)
{
   //set the marterial
   unsigned long materialUsed =  matm[ regn[iSubmesh].linkBat->matid  ].MatInd;
   if ( matm[ bat[iSubmesh].matid  ].nMat != 1 ) 
   { 
      ERROR_MESSAGE("nb of materials != 1"); 
   }

   //EXtexture is just my texture class. It is nearly the same that a classical LPDIRECT3DTEXTURE9 class.
   //EXtexture* pTexture is a member of the struc LAYR that isn’t in the real file structure. 
   //I add it in my structure in order to save this optimization.
   EXtexture* textureUsed = ((LAYR*)(material[materialUsed].LAYR_Diff.pData))->pTexture;
   if ( textureUsed != NULL )
   {
      m_pd3dDevice->SetTexture(0,textureUsed->GetTexture());
   }

   //now, we update vertices (for the moment, I haven’t find another way that use the lock-unkock... 
   //I think there is faster methods, if you have any suggestion, I would be happy to know them)
   VERTEX_MD3* vg;
   HRESULT hr = vertexBuffer->Lock( 0, 0, (void**)&vg, 0 );
   if ( hr != D3D_OK ) { ERROR_MESSAGE("Lock vertex Buffer"); }

   //for each vertex of the current submesh
   for (unsigned int iVertex=regn[iSubmesh].vind;   iVertex< regn[iSubmesh].vind+regn[iSubmesh].vnum;   iVertex++)
   {
      D3DXVECTOR3 finalPos = D3DXVECTOR3(0.0f,0.0f,0.0f);

      //for each weights
      for(int iWeight=0; iWeight<4; iWeight++)
      {
         //if there is a weight
         if ( m_StaticMeshVB[iVertex].weBone[iWeight] > 0.0f ) 
         { 
            unsigned short boneIndice = m_StaticMeshVB[iVertex].realOwnerBones[iWeight];

            D3DXVECTOR4 pos = m_StaticMeshVB[iVertex].posXiref[iWeight];

            pos.x *= m_BoneScale[boneIndice].x;
            pos.y *= m_BoneScale[boneIndice].y;
            pos.z *= m_BoneScale[boneIndice].z;
				
            vecXquat((D3DXVECTOR3*)(&pos),&m_BoneRotation[boneIndice]);

            finalPos += ((D3DXVECTOR3)(pos) + m_BoneTranslation[boneIndice]) * m_StaticMeshVB[iVertex].weBone[iWeight];
         }

      }

      vg[iVertex].x = finalPos.x;
      vg[iVertex].y = finalPos.y;
      vg[iVertex].z = finalPos.z;
   
   }

   vertexBuffer->Unlock();

	
   m_pd3dDevice->SetStreamSource( 0, vertexBuffer, 0, sizeof(VERTEX_MD3) );
   m_pd3dDevice->SetIndices(  indexBuffer  );
   m_pd3dDevice->SetFVF(D3DFVF_VERTEX_MD3);
		
   m_pd3dDevice->DrawIndexedPrimitive( 
                         D3DPT_TRIANGLELIST,
                         regn[iSubmesh].vind,   // BaseVertexIndex
                         0,                     // MinIndex
                         regn[iSubmesh].vnum,   // NumVertices
                         regn[iSubmesh].find,   // StartIndex
                         regn[iSubmesh].fnum/3  // PrimitiveCount
                         );


}

```

And here is the result! :

![![](http://theolith.com/resources/SCREEN_LQ/screen_warcrave_09.jpg)](http://theolith.com/resources/SCREEN_HQ/screen_warcrave_09.png)


Thanks for reading, I hope this article has been useful for your projects. If you have any questions, remarks... Leave a comment or send me an email.