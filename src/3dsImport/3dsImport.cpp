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
    norm.x = (float) 2*normal[0]/255.0f - 1;
    norm.y = (float) 2*normal[1]/255.0f - 1;
    norm.z = (float) 2*normal[2]/255.0f - 1;

    float w = (float) normal[3]/255.0f;

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
    // Load model
    Model* pModel = Model::LoadModel(filename);
    if(!pModel)
    {
        return FALSE;
    }
    else
    {
        // Get pointers to head and refs
        MD33* pHead = pModel->GetHeader();
        ReferenceEntry* pRefs = pModel->GetRefs();

        // Get vertex reference and format
        bool hasVertices = false;

        Reference refDivs  = {0};
        Reference refVerts = {0};
        int vertexFormat = 0;

        switch( pRefs[pHead->MODL.ref].type )
        {
        case TYPE1:
            LogPrintf(LGREEN, "Model type: %d", pModel->GetType());
            hasVertices  = (pModel->GetEntries<MODL20>( pHead->MODL )->flags & 0x20000) != 0;
            vertexFormat = (pModel->GetEntries<MODL20>( pHead->MODL )->flags & 0x40000) != 0 ? VERTEX_EXTENDED : VERTEX_STANDARD;
            refVerts    = pModel->GetEntries<MODL20>( pHead->MODL )->vertexData;
            refDivs     = pModel->GetEntries<MODL20>( pHead->MODL )->divisions;
            break;

        case TYPE2:
            LogPrintf(LGREEN, "Model type: %d", pModel->GetType());
            hasVertices  = (pModel->GetEntries<MODL23>( pHead->MODL )->flags & 0x20000) != 0;
            vertexFormat = (pModel->GetEntries<MODL23>( pHead->MODL )->flags & 0x40000) != 0 ? VERTEX_EXTENDED : VERTEX_STANDARD;
            refVerts    = pModel->GetEntries<MODL23>( pHead->MODL )->vertexData;
            refDivs     = pModel->GetEntries<MODL23>( pHead->MODL )->divisions;
            break;

        default:
            LogPrintf(LYELLOW, "Unknown model type: %d", pModel->GetType());
            return FALSE;
        }
        
        DIV* divs = pModel->GetEntries<DIV>( refDivs );

        // Faces
        for(int region = 0; region < 1/*divs->regions.nEntries*/; region++)
        {
            msh[region] = new Mesh();
            Region* geoset = pModel->GetEntries<Region>( divs->regions ) + sizeof(Region)*region;

            // Vertices
            LogPrintf(LGREEN, "Starting to parse vertices");
            
            uint8* vertexData = pModel->GetEntries<uint8>( refVerts );
            msh[region]->setNumVerts(geoset->nVertices);
            msh[region]->setNumTVerts(geoset->nVertices);
            
            switch( vertexFormat )
            {
            case VERTEX_STANDARD:
                if( msh[region]->setNumVerts(geoset->nVertices) )
                {
                    //LogPrintf(LGREEN, "Set: Number of vertices: %d", msh[region]->getNumVerts());
                    Vertex* vertex = pModel->GetEntries<Vertex>( refVerts ) + sizeof(Vertex)*geoset->ofsVertices;
                    for(int i = 0; i < msh[region]->getNumVerts(); i++)
                    {
                        msh[region]->setVert(i, vertex[i].pos.x, vertex[i].pos.y, vertex[i].pos.z);
                        //msh[region]->setNormal(i, normal_4d_to_3d(vertex[i].normal));
                        //msh[region]->setTVert(i, (float) vertex[i].uv[0]/2048, (float) -vertex[i].uv[1]/2048, 0.0f);
                        //LogPrintf(LGREEN, "Set: Vertex #%d", i);
                    }
                }
                break;

            case VERTEX_EXTENDED:
                if( msh[region]->getNumVerts() )
                {
                    //LogPrintf(LGREEN, "Set: Number of vertices: %d", msh[region]->getNumVerts());
                    VertexExt* vertex = pModel->GetEntries<VertexExt>( refVerts ) + sizeof(VertexExt)*geoset->ofsVertices;
                    for(int i = 0; i < msh[region]->getNumVerts(); i++)
                    {
                        msh[region]->setVert(i, vertex[i].pos.x, vertex[i].pos.y, vertex[i].pos.z);
                        msh[region]->setNormal(i, normal_4d_to_3d(vertex[i].normal));
                        msh[region]->setTVert(i, (float) vertex[i].uv[0]/2048, (float) -vertex[i].uv[1]/2048, 0.0f);
                        //LogPrintf(LGREEN, "Set: Vertex #%d", i);
                    }
                }
                break;
            }
            
            LogPrintf(LGREEN, "Starting to parse faces");
            if( msh[region]->setNumFaces(geoset->nIndices/3) )
            {
                //LogPrintf(LGREEN, "Set: Number of faces: %d", msh[region]->getNumFaces());
                uint16* faces = pModel->GetEntries<uint16>( divs->faces ) + sizeof(uint16)*geoset->ofsIndices;
                for(int i = 0, j = 0; i < msh[region]->getNumFaces(); i++, j+=3)
                {
                    msh[region]->faces[i].setVerts(faces[j+0], faces[j+1], faces[j+2]);
                    //LogPrintf(LDEBUG, "Set: Face #%d", i);
                }
            }
        }

        Model::UnloadModel(filename);
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
	
    // TriObject...
    TriObject* object[MAX_REGIONS] = {0};
    Mesh* mshArray[MAX_REGIONS] = {0};

    if( !object )
    {
        return FALSE;
    }

    if( LoadFromFile(filename, mshArray) )
    {
        LogPrintf(LDEBUG, "Adding nodes to the scene");
        for(int region = 0; region < MAX_REGIONS; region++)
        {
            if(!mshArray[region])
            {
                LogPrintf(LDEBUG, "%d geosets parsed", region);
                break;
            }

            object[region] = CreateNewTriObject();
            object[region]->GetMesh() = *mshArray[region];

            ImpNode* node = i->CreateNode();

            if( !node )
            {
                LogPrintf(LYELLOW, "Failed to create node for geoset #%d", region);
                delete object[region];
                continue;
            }
    		
            Matrix3 tm;
    		tm.IdentityMatrix();
            node->Reference(object[region]);
            node->SetTransform(0,tm);

            std::stringstream ss;
            ss << "Geoset #" << region;

            i->AddNodeToScene(node);
            node->SetName( ss.str().c_str() );

            LogPrintf(LDEBUG, "Added node '%s' to the scene", ss.str());
        }
        LogPrintf(LDEBUG, "Finished adding nodes");
        i->RedrawViews();

        return TRUE;
    }

	#pragma message(TODO("return TRUE If the file is imported properly"))
	return FALSE;
}
	
