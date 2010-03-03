#include "../model.h"

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

        switch(pRefs[pHead->MODL.ref].type)
        {
        case 20:
            pMODL20 = pModel->GetEntries<MODL20>(pHead->MODL);
            view = pModel->GetEntries<DIV>(pMODL20->views);
            break;

        case 23:
            pMODL23 = pModel->GetEntries<MODL23>(pHead->MODL);
            view = pModel->GetEntries<DIV>(pMODL23->views);
            break;

        default:
            system("pause");
            break;
        }

        vector<ReferenceEntry> refs;
        for(uint32 i = 0; i < pHead->nRefs; i++)
            refs.push_back( pRefs[i] );
    }

    return 0;
}