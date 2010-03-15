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
    
}

m3import::~m3import() 
{
    
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
    norm.x = (float) normal[0]/UCHAR_MAX;
    norm.y = (float) normal[1]/UCHAR_MAX;
    norm.z = (float) normal[2]/UCHAR_MAX;

    float w = (float) normal[3]/UCHAR_MAX;

    if(w)
    {
        norm.x = norm.x/w;
        norm.y = norm.y/w;
        norm.z = norm.z/w;
    }
    return norm;
}

int LoadFromFile(const TCHAR* filename, Mesh* msh[])
{   
    int vcnt = 0; // Vertex count
    int fcnt = 0; // Face count
    int rcnt = 0; // Region count

    int vnum = 0; // Vertex number
    int fnum = 0; // Face number
    int rnum = 0; // Region number

    Point3* vlst = NULL, nlst = NULL, tlst = NULL; // Vertex lists
    Face*   flst = NULL; // Face list

    Reference dref = {0}; // Division reference
    Reference vref = {0}; // Vertex reference
    Reference rref = {0}; // Region reference
    Reference fref = {0}; // Face reference

    int vfmt = -1; // Vertex format

    Division* pDiv = NULL;
    Region*  pRegn = NULL;

    // Load model
    Model* pModel = Model::LoadModel(filename);

#ifdef CONSOLE
    LogConsole(LDEBUG, std::string("Loaded model\n"));
#endif

    if(!pModel)
    {
        return FALSE;
    }
    else
    {
        // Get pointers to head and refs
        MD33* pHead = pModel->GetHeader();
        ReferenceEntry* pRefs = pModel->GetRefs();

        // Get references and vertex format
        switch( pRefs[pHead->MODL.ref].type )
        {
        case TYPE1:
            vfmt = (pModel->GetEntries<MODL20>( pHead->MODL )->flags & 0x40000) != 0 ? VERTEX_EXTENDED : VERTEX_STANDARD;
            vref = pModel->GetEntries<MODL20>( pHead->MODL )->vertexData;
            dref = pModel->GetEntries<MODL20>( pHead->MODL )->divisions;
            break;

        case TYPE2:
            vfmt = (pModel->GetEntries<MODL23>( pHead->MODL )->flags & 0x40000) != 0 ? VERTEX_EXTENDED : VERTEX_STANDARD;
            vref = pModel->GetEntries<MODL23>( pHead->MODL )->vertexData;
            dref = pModel->GetEntries<MODL23>( pHead->MODL )->divisions;
            break;

        default:
            return FALSE;
        }
        
        // Region reference and number of regions
        rref = pModel->GetEntries<Division>( dref )->regions;
        rcnt = pModel->GetEntries<Division>( dref )->regions.nEntries;

        // Face reference
        fref = pModel->GetEntries<Division>( dref )->faces;

#ifdef CONSOLE
        LogConsole(LDEBUG, "Starting to parse regions\n");
#endif

        for(; rnum < rcnt; rnum++)
        {
            // Mesh for this region
            msh[rnum] = new Mesh();

            // Number of vertices and faces in the region
            vcnt = pModel->GetEntry<Region>( rref, rnum )->nVertices;
            fcnt = pModel->GetEntry<Region>( rref, rnum )->nIndices;

#ifdef CONSOLE
        LogConsole(LDEBUG, StringFromFormat("vcnt: %d\nfcnt: %d\n", vcnt, fcnt));
#endif

            // Start vertex and face in the region
            vnum = pModel->GetEntry<Region>( rref, rnum )->ofsVertices;
            fnum = pModel->GetEntry<Region>( rref, rnum )->ofsIndices;

            // Set the size of the vertex array in the mesh
            msh[rnum]->setNumVerts(vcnt);
            //msh[rnum]->setNumTVerts(vcnt);
            
            switch( vfmt )
            {
            case VERTEX_STANDARD:
                for(; vnum < vcnt; vnum++)
                {
                    Vertex* v = pModel->GetEntry<Vertex>( vref, vnum );
                    msh[rnum]->setVert(vnum, v->pos.x, v->pos.y, v->pos.z);
                    //msh[region]->setNormal(i, normal_4d_to_3d(vertex[i].normal));
                    //msh[region]->setTVert(i, (float) vertex[i].uv[0]/2048, (float) -vertex[i].uv[1]/2048, 0.0f);
                }
                break;

            case VERTEX_EXTENDED:
                for(; vnum < vcnt; vnum++)
                {
                    VertexExt* v = pModel->GetEntry<VertexExt>( vref, vnum );
                    msh[rnum]->setVert(vnum, v->pos.x, v->pos.y, v->pos.z);
                    //msh[region]->setNormal(i, normal_4d_to_3d(vertex[i].normal));
                    //msh[region]->setTVert(i, (float) vertex[i].uv[0]/2048, (float) -vertex[i].uv[1]/2048, 0.0f);
                }
                break;

            default:
                return FALSE;
            }

#ifdef CONSOLE
            LogConsole(LDEBUG, "Finished adding vertices\n");
#endif            
            
            // Set the size of the face array in the mesh
            msh[rnum]->setNumFaces(fcnt);

            for(; fnum < fcnt; fnum+=3)
            {
                uint16* f = pModel->GetEntry<uint16>( fref, fnum );
                msh[rnum]->faces[fnum].setVerts(f[0], f[1], f[2]);
            }

#ifdef CONSOLE
            LogConsole(LDEBUG, "Finished adding faces\n");
#endif
            
            // Build the mesh
            //msh[rnum]->buildNormals();

#ifdef CONSOLE
            LogConsole(LDEBUG, "Finished building mesh\n");
#endif
        }

        Model::UnloadModel(filename);

#ifdef CONSOLE
            LogConsole(LDEBUG, "Unloaded model\n");
#endif
    }
    return TRUE;
}

int m3import::DoImport(const TCHAR *filename,ImpInterface *i,
						Interface *gi, BOOL suppressPrompts)
{
	#pragma message(TODO("Implement the actual file import here and"))	

    /*
	if(!suppressPrompts)
		DialogBoxParam(hInstance, 
				MAKEINTRESOURCE(IDD_PANEL), 
				GetActiveWindow(), 
				m3importOptionsDlgProc, (LPARAM)this);
                */
	
    // Objects and meshes
    TriObject* object[MAX_REGIONS] = {0};
    Mesh*    mshArray[MAX_REGIONS] = {0};

    if( !object )
    {
        return FALSE;
    }

    if( LoadFromFile(filename, mshArray) )
    {
        for(int rnum = 0; rnum < MAX_REGIONS; rnum++)
        {
            if(!mshArray[rnum])
            {
                break;
            }

            object[rnum] = CreateNewTriObject();
            object[rnum]->GetMesh() = *mshArray[rnum];

            ImpNode* node = i->CreateNode();

            if( !node )
            {
#ifdef CONSOLE
                LogConsole(LDEBUG, "Failed to create node\n");
#endif
                delete object[rnum];
                continue;
            }

            Matrix3 tm;
    		tm.IdentityMatrix();

            node->Reference(object[rnum]);
            node->SetTransform(0,tm);

            std::stringstream ss;
            ss << "Geoset #" << rnum;

            i->AddNodeToScene(node);
            node->SetName( ss.str().c_str() );
#ifdef CONSOLE
            LogConsole(LDEBUG, "Added node to scene\n");
#endif
        }
        i->RedrawViews();

#ifdef CONSOLE
        LogConsole(LDEBUG, "Finished importing model\n");
#endif

        return TRUE;
    }

	#pragma message(TODO("return TRUE If the file is imported properly"))
	return FALSE;
}
	
