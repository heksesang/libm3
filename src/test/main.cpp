#include "../model.h"

using namespace std;
using namespace m3;

int main(int argc, char* argv[])
{
    if( argc == 1 )
        return -1; // No filename given

    Model::LoadModel(argv[1]);
    
    Model* pModel = Model::GetModel(argv[1]);
    MD33* pHead = pModel->GetHeader();
    ReferenceEntry* pRef = pModel->GetRefs();

    MODL20* pMODL20 = NULL;
    MODL23* pMODL23 = NULL;

    switch(pRef[pHead->MODL.ref].type)
    {
    case 20:
        pMODL20 = pModel->GetEntries<MODL20>(pHead->MODL);
        break;

    case 23:
        pMODL23 = pModel->GetEntries<MODL23>(pHead->MODL);
        break;
    }

    return 0;
}