#include "../lib/model.h"

#include <boost/filesystem.hpp>

using namespace std;
using namespace m3;
using namespace boost::filesystem;

int main(int argc, char* argv[])
{
    if( argc == 1 )
        return -1; // No filename given

    path dir(argv[1]);

    if( is_directory(dir) )
    {
        FILE* f = NULL;
        fopen_s(&f, "output.txt", "w");
        for(directory_iterator iter(dir),end; iter != end; ++iter)
        {
            Model* pModel = Model::LoadModel( (*iter).string().c_str() );

            if(!pModel)
                continue;

            MD33* pHead = pModel->GetHeader();
            ReferenceEntry* pRefs = pModel->GetRefs();

            MODL20* pMODL20 = NULL;
            MODL23* pMODL23 = NULL;

            switch(pRefs[pHead->MODL.ref].type)
            {
            case 20:
                pMODL20 = pModel->GetEntries<MODL20>(pHead->MODL);
                break;

            case 23:
                pMODL23 = pModel->GetEntries<MODL23>(pHead->MODL);
                break;
            }
        }

        fclose(f);
    }
    else
    {
        Model::LoadModel(argv[1]);
        
        Model* pModel = Model::GetModel(argv[1]);
        MD33* pHead = pModel->GetHeader();
        ReferenceEntry* pRefs = pModel->GetRefs();

        MODL20* pMODL20 = NULL;
        MODL23* pMODL23 = NULL;

        DIV* view = NULL;
        MAT* materials = NULL;
        Region* regions = NULL;

        VertexExt* verts1 = NULL;
        Vertex* verts2 = NULL;

        uint32 nVertices = 0;

        switch(pRefs[pHead->MODL.ref].type)
        {
        case 20:
            pMODL20 = pModel->GetEntries<MODL20>(pHead->MODL);
            view = pModel->GetEntries<DIV>(pMODL20->views);
            materials = pModel->GetEntries<MAT>(pMODL20->materials);
            if( (pMODL20->flags & 0x40000) != 0 )
            {
                nVertices = pMODL20->vertexData.nEntries/sizeof(VertexExt);
                verts1 = pModel->GetEntries<VertexExt>(pMODL20->vertexData);
            }
            else
            {
                nVertices = pMODL20->vertexData.nEntries/sizeof(Vertex);
                verts2 = pModel->GetEntries<Vertex>(pMODL20->vertexData);
            }
            break;

        case 23:
            pMODL23 = pModel->GetEntries<MODL23>(pHead->MODL);
            view = pModel->GetEntries<DIV>(pMODL23->views);
            materials = pModel->GetEntries<MAT>(pMODL23->materials);
            if( (pMODL23->flags & 0x40000) != 0 )
            {
                nVertices = pMODL23->vertexData.nEntries/sizeof(VertexExt);
                verts1 = pModel->GetEntries<VertexExt>(pMODL23->vertexData);
            }
            else
            {
                nVertices = pMODL23->vertexData.nEntries/sizeof(Vertex);
                verts2 = pModel->GetEntries<Vertex>(pMODL23->vertexData);
            }
            break;

        default:
            system("pause");
            break;
        }

        regions = pModel->GetEntries<Region>(view->regions);

        vector<ReferenceEntry> refs;
        for(uint32 i = 0; i < pHead->nRefs; i++)
            refs.push_back( pRefs[i] );

        vector<VertexExt> vertsExt;
        if(verts1)
        {
            for(uint32 i = 0; i < nVertices; i++)
                vertsExt.push_back(verts1[i]);
        }

        vector<Vertex> verts;
        if(verts2)
        {
            for(uint32 i = 0; i < nVertices; i++)
                verts.push_back(verts2[i]);
        }

        vector<Region> regs;
        for(uint32 i = 0; i < view->regions.nEntries; i++)
            regs.push_back(regions[i]);

    }

    return 0;
}