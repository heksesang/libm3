//**************************************************************************/
// Copyright (c) 1998-2007 Autodesk, Inc.
// All rights reserved.
// 
// These coded instructions, statements, and computer programs contain
// unpublished proprietary information written by Autodesk, Inc., and are
// protected by Federal copyright law. They may not be disclosed to third
// parties or copied or duplicated in any form, in whole or in part, without
// the prior written consent of Autodesk, Inc.
//**************************************************************************/
// DESCRIPTION: M3 Model Loader Plugin
// AUTHOR: Gunnar Lilleaasen
//***************************************************************************/

#include "3dsImport.h"
#include "../lib/model.h"
#include <sstream>

using namespace m3;

#define m3import_CLASS_ID	Class_ID(0xaf819621, 0x608573a4)


class m3import : public SceneImport {
	public:

		static HWND hParams;
		
		int				ExtCount();					// Number of extensions supported
		const TCHAR *	Ext(int n);					// Extension #n (i.e. "3DS")
		const TCHAR *	LongDesc();					// Long ASCII description (i.e. "Autodesk 3D Studio File")
		const TCHAR *	ShortDesc();				// Short ASCII description (i.e. "3D Studio")
		const TCHAR *	AuthorName();				// ASCII Author name
		const TCHAR *	CopyrightMessage();			// ASCII Copyright message
		const TCHAR *	OtherMessage1();			// Other message #1
		const TCHAR *	OtherMessage2();			// Other message #2
		unsigned int	Version();					// Version number * 100 (i.e. v3.01 = 301)
		void			ShowAbout(HWND hWnd);		// Show DLL's "About..." box
		int				DoImport(const TCHAR *name,ImpInterface *i,Interface *gi, BOOL suppressPrompts=FALSE);	// Import file
		
		//Constructor/Destructor
		m3import();
		~m3import();

};



class m3importClassDesc : public ClassDesc2 
{
public:
	virtual int IsPublic() 							{ return TRUE; }
	virtual void* Create(BOOL /*loading = FALSE*/) 		{ return new m3import(); }
	virtual const TCHAR *	ClassName() 			{ return GetString(IDS_CLASS_NAME); }
	virtual SClass_ID SuperClassID() 				{ return SCENE_IMPORT_CLASS_ID; }
	virtual Class_ID ClassID() 						{ return m3import_CLASS_ID; }
	virtual const TCHAR* Category() 				{ return GetString(IDS_CATEGORY); }

	virtual const TCHAR* InternalName() 			{ return _T("m3import"); }	// returns fixed parsable name (scripter-visible name)
	virtual HINSTANCE HInstance() 					{ return hInstance; }					// returns owning module handle
	

};


ClassDesc2* Getm3importDesc() { 
	static m3importClassDesc m3importDesc;
	return &m3importDesc; 
}

enum
{
    m3import_params
};

enum
{
    m3import_filename
};

static ParamBlockDesc2 m3import_param_blk (

   //Required arguments ----------------------------

   0, _T("params"), 0, Getm3importDesc(),

   //flags

   NULL,

 

   //Parameter Specifications ----------------------

   // For each control create a parameter:

   m3import_filename,      _T("filename"),      TYPE_FILENAME,    P_ANIMATABLE,    IDS_SPIN, end,

   end

);


INT_PTR CALLBACK m3importOptionsDlgProc(HWND hWnd,UINT message,WPARAM wParam,LPARAM lParam) {
	static m3import *imp = NULL;

	switch(message) {
		case WM_INITDIALOG:
			imp = (m3import *)lParam;
			CenterWindow(hWnd,GetParent(hWnd));
			return TRUE;

		case WM_CLOSE:
			EndDialog(hWnd, 0);
			return 1;
	}
	return 0;
}


//--- m3import -------------------------------------------------------
m3import::m3import()
{
    LogConsole(LDEBUG, "Constructor\n");
}

m3import::~m3import() 
{
    LogConsole(LDEBUG, "Destructor\n");
}

int m3import::ExtCount()
{
	#pragma message(TODO("Returns the number of file name extensions supported by the plug-in."))
	return 1;
}

const TCHAR *m3import::Ext(int n)
{		
	#pragma message(TODO("Return the 'i-th' file name extension (i.e. \"3DS\")."))
	return _T("M3");
}

const TCHAR *m3import::LongDesc()
{
	#pragma message(TODO("Return long ASCII description (i.e. \"Targa 2.0 Image File\")"))
	return _T("MDX 3.3 Model File");
}
	
const TCHAR *m3import::ShortDesc() 
{			
	#pragma message(TODO("Return short ASCII description (i.e. \"Targa\")"))
	return _T("M3 File");
}

const TCHAR *m3import::AuthorName()
{			
	#pragma message(TODO("Return ASCII Author name"))
	return _T("");
}

const TCHAR *m3import::CopyrightMessage() 
{	
	#pragma message(TODO("Return ASCII Copyright message"))
	return _T("");
}

const TCHAR *m3import::OtherMessage1() 
{		
	//TODO: Return Other message #1 if any
	return _T("");
}

const TCHAR *m3import::OtherMessage2() 
{		
	//TODO: Return other message #2 in any
	return _T("");
}

unsigned int m3import::Version()
{				
	#pragma message(TODO("Return Version number * 100 (i.e. v3.01 = 301)"))
	return 100;
}

void m3import::ShowAbout(HWND hWnd)
{			
	// Optional
}

Point3 normal_4d_to_3d(UCHAR normal[4])
{
    Point3 norm;
    norm.x = (float) 2*normal[0]/UCHAR_MAX - 1;
    norm.y = (float) 2*normal[1]/UCHAR_MAX - 1;
    norm.z = (float) 2*normal[2]/UCHAR_MAX - 1;

    float w = (float) normal[3]/UCHAR_MAX;

    if(w)
    {
        norm.x = norm.x/w;
        norm.y = norm.y/w;
        norm.z = norm.z/w;
    }
    return norm;
}

int LoadMesh(__in  const TCHAR* filename,
             __in  int rnum,         // Region index
             __in  int vfmt,         // Vertex format
             __in  Reference rref,   // Region reference
             __in  Reference vref,   // Vertex reference
             __in  Reference fref,   // Face reference
             __out Mesh* msh)
{   
    int vcnt = 0; // Vertex count
    int fcnt = 0; // Face count

    int vnum = 0; // Vertex number
    int fnum = 0; // Face number

    // Load model
    Model* pModel = Model::LoadModel(filename);

    if(!pModel)
    {
        return FALSE;
    }
    else
    {
        // Number of vertices and faces in the region
        vcnt = pModel->GetEntry<Region>( rref, rnum )->nVertices;
        fcnt = pModel->GetEntry<Region>( rref, rnum )->nIndices;

        // Start vertex and face in the region
        vnum = pModel->GetEntry<Region>( rref, rnum )->ofsVertices;
        fnum = pModel->GetEntry<Region>( rref, rnum )->ofsIndices;

        // Set the size of the vertex array in the mesh
        msh->setNumVerts(vcnt);
        //msh[rnum]->setNumTVerts(vcnt);
        
        switch( vfmt )
        {
        case VERTEX_STANDARD:
            for(int vcount = 0; vcount < vcnt; vnum, vcount++)
            {
                Vertex* v = pModel->GetEntry<Vertex>( vref, vnum+vcount );
                msh->setVert(vcount, v->pos.x, v->pos.y, v->pos.z);
                msh->setNormal(vcount, normal_4d_to_3d(v->normal));
                //msh[region]->setTVert(i, (float) vertex[i].uv[0]/2048, (float) -vertex[i].uv[1]/2048, 0.0f);
            }
            break;

        case VERTEX_EXTENDED:
            for(int vcount = 0; vcount < vcnt; vnum, vcount++)
            {
                VertexExt* v = pModel->GetEntry<VertexExt>( vref, vnum+vcount );
                msh->setVert(vcount, v->pos.x, v->pos.y, v->pos.z);
                msh->setNormal(vcount, normal_4d_to_3d(v->normal));
                //msh[region]->setTVert(i, (float) vertex[i].uv[0]/2048, (float) -vertex[i].uv[1]/2048, 0.0f);
            }
            break;

        default:
            return FALSE;
        }
        
        // Set the size of the face array in the mesh
        msh->setNumFaces(fcnt/3);

        int fcount = 0;
        for(; fcount < fcnt/3; fcount++)
        {
            uint16* f = pModel->GetEntry<uint16>( fref, fnum+fcount*3 );
            msh->faces[fcount].setVerts(f[0]-vnum, f[1]-vnum, f[2]-vnum);
        }
        
        // Build the mesh
        msh->buildNormals();
    }
    return TRUE;
}

int m3import::DoImport(const TCHAR *filename,ImpInterface *i,
						Interface *gi, BOOL suppressPrompts)
{
	#pragma message(TODO("Implement the actual file import here and"))

    int rcnt = 0;
    int rnum = 0;

    int vfmt = 0;
    
    Reference dref = {0};
    Reference vref = {0};
    Reference fref = {0};
    Reference rref = {0};

    Model* pModel = Model::LoadModel(filename);
    MD33* pHead = pModel->GetHeader();
    ReferenceEntry* pRefs = pModel->GetRefs();

    switch( pRefs[pHead->MODL.ref].type )
    {
    case TYPE1:
        vfmt = (pModel->GetEntries<MODL20>( pHead->MODL )->flags & 0x40000) != 0 ? VERTEX_EXTENDED : VERTEX_STANDARD;
        dref = pModel->GetEntries<MODL20>( pHead->MODL )->divisions;
        vref = pModel->GetEntries<MODL20>( pHead->MODL )->vertexData;
        break;

    case TYPE2:
        vfmt = (pModel->GetEntries<MODL23>( pHead->MODL )->flags & 0x40000) != 0 ? VERTEX_EXTENDED : VERTEX_STANDARD;
        dref = pModel->GetEntries<MODL23>( pHead->MODL )->divisions;
        vref = pModel->GetEntries<MODL23>( pHead->MODL )->vertexData;
        break;

    default:
        return FALSE;
    }

    rref = pModel->GetEntry<Division>( dref, 0 )->regions;
    fref = pModel->GetEntry<Division>( dref, 0 )->faces;

    rcnt = rref.nEntries;

    
	if(!suppressPrompts)
		DialogBoxParam(hInstance, 
				MAKEINTRESOURCE(IDD_PANEL), 
				GetActiveWindow(), 
				m3importOptionsDlgProc, (LPARAM)this);
                
	
    // Object ptr
    TriObject* object = NULL;
    
    for(; rnum < rcnt; rnum++)
    {
        object = CreateNewTriObject();

        if( !object )
            continue;

        LoadMesh(filename, rnum, vfmt, rref, vref, fref, &object->GetMesh());

        ImpNode* node = i->CreateNode();

        if( !node )
        {
#ifdef CONSOLE
            LogConsole(LDEBUG, "Failed to create node\n");
#endif
            delete object;
            continue;
        }

        Matrix3 tm;
		tm.IdentityMatrix();

        node->Reference(object);
        node->SetTransform(0,tm);

        std::stringstream ss;
        ss << "Geoset #" << rnum;

        i->AddNodeToScene(node);
        node->SetName( ss.str().c_str() );
    }
    i->RedrawViews();

    return TRUE;

	#pragma message(TODO("return TRUE If the file is imported properly"))
	return FALSE;
}
	
