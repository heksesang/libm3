#include "model.h"

using namespace std;
using namespace boost::filesystem;
using namespace m3;

Model::Model(FILE* f) : m_buf(NULL)
{
    fseek(f, 0, SEEK_END);
    m_bufSize = ftell(f);
    m_buf = new char[m_bufSize];
    fseek(f, 0, SEEK_SET);
    fread(m_buf, sizeof(char), m_bufSize, f);
    fclose(f);

    m_head = (MD33*)( m_buf );
    m_refs = (ReferenceEntry*)( m_buf + m_head->ofsRefs );
}

Model::Model(const Model& m) : m_buf(NULL)
{
    *this = m;
}

Model::~Model()
{
    if(m_buf)
        delete [] m_buf;
}

Model& Model::operator=(const Model& m)
{
    m_bufSize = m.m_bufSize;
    if(m_buf)
        delete [] m_buf;
    m_buf = new char[m_bufSize];
    memcpy(m_buf, m.m_buf, m_bufSize);

    m_head = (MD33*)( m_buf );
    m_refs = (ReferenceEntry*)( m_buf + m_head->ofsRefs );
    
    return *this;
}

Model* Model::LoadModel(string filename)
{
    path filepath(filename);

    if( !exists(filepath) ) // Invalid filepath
        return NULL;

    map<string, Model>::iterator iter = m_models.find(filepath.filename() );
    if( iter != m_models.end() ) // File already loaded
        return Model::GetModel(filename);

    FILE* f = NULL;
    int error = 0;
    
    error = fopen_s(&f, filepath.string().c_str(), "rb");
    if(error) // Failed to open the file
        return NULL;

    Model m3(f);
    pair<string, Model> entry(filepath.filename(), m3);
    
    m_models.insert(entry);
    
    return Model::GetModel(filename); // Everything went fine
};

void Model::UnloadModel(string filename)
{
    path filepath(filename);

    map<string, Model>::iterator iter = m_models.find(filepath.filename() );
    if( iter != m_models.end() )
        m_models.erase( iter );
};

Model* Model::GetModel(string filename)
{
    path filepath(filename);

    map<string, Model>::iterator iter = m_models.find( filepath.filename() );
    if( iter == m_models.end() ) // Model isn't loaded
    {
        Model* pModel = LoadModel(filename);
        if(!pModel)
            throw std::runtime_error("ERROR: Model not available"); // Failed to load model
        else
            return pModel;
    }

    return &iter->second;
};

MD33* Model::GetHeader()
{
    return m_head;
}

ReferenceEntry* Model::GetRefs()
{
    return m_refs;
}

int Model::Convert(std::string filename)
{
    Model* pModel = Model::LoadModel(filename);

    if(!pModel)
        return -1;

    MD33* pHead = pModel->GetHeader();
    ReferenceEntry* pRefs = pModel->GetRefs();

    MODL20* pMODL20 = NULL;
    MODL23* pMODL23 = NULL;

    Vertex1* pVerts1 = NULL;
    Vertex2* pVerts2 = NULL;
    DIV* geosets = NULL;
    uint16* faces = NULL;

    uint32 nVertices = 0;
    uint32 nFaces = 0;

    path p(filename);
    p.replace_extension(".obj");

    FILE* f = NULL;
    if( fopen_s(&f, p.string().c_str(), "w") )
        return -1;

    switch(pRefs[pHead->MODL.ref].type)
    {
    case 20:
        pMODL20 = pModel->GetEntries<MODL20>(pHead->MODL);
        if( (pMODL20->flags & 0x20000) != 0 ) // Has vertices
        {
            if( (pMODL20->flags & 0x40000) != 0 ) // Has extra 4 byte
            {
                pVerts1 = pModel->GetEntries<Vertex1>(pMODL20->A);
                nVertices = pMODL20->A.nEntries/sizeof(Vertex1);
            }
            else
            {
                pVerts2 = pModel->GetEntries<Vertex2>(pMODL20->A);
                nVertices = pMODL20->A.nEntries/sizeof(Vertex2);
            }
        }
        geosets = pModel->GetEntries<DIV>( pMODL20->DIV );
        faces = pModel->GetEntries<uint16>( geosets->U16 );
        nFaces = geosets->U16.nEntries;
        break;

    case 23:
        pMODL23 = pModel->GetEntries<MODL23>(pHead->MODL);
        if( (pMODL23->flags & 0x20000) != 0 ) // Has vertices
        {
            if( (pMODL23->flags & 0x40000) != 0 ) // Has extra 4 byte
            {
                pVerts1 = pModel->GetEntries<Vertex1>(pMODL23->A);
                nVertices = pMODL23->A.nEntries/sizeof(Vertex1);
            }
            else
            {
                pVerts2 = pModel->GetEntries<Vertex2>(pMODL23->A);
                nVertices = pMODL23->A.nEntries/sizeof(Vertex2);
            }
        }
        geosets = pModel->GetEntries<DIV>( pMODL23->DIV );
        faces = pModel->GetEntries<uint16>( geosets->U16 );
        nFaces = geosets->U16.nEntries;
        break;

    default:
        return -1;
    }

    // Write vertices
    for(uint32 i = 0; i < nVertices; i++)
    {
        if(pVerts1)
        {
            fprintf_s(f, "v %f %f %f\n", pVerts1[i].pos.x, pVerts1[i].pos.y, pVerts1[i].pos.z);
        }

        if(pVerts2)
        {
            fprintf_s(f, "v %f %f %f\n", pVerts2[i].pos.x, pVerts2[i].pos.y, pVerts2[i].pos.z);
        }
    }
    
    // Write UV coords
    for(uint32 i = 0; i < nVertices; i++)
    {
        if(pVerts1)
        {
            float u = pVerts1[i].uv[0] > 2046 ? float(pVerts1[i].uv[0])/65536.0f : float(pVerts1[i].uv[0])/2046.0f;
            float v = pVerts1[i].uv[1] > 2046 ? float(pVerts1[i].uv[1])/65536.0f : float(pVerts1[i].uv[1])/2046.0f;
            fprintf_s(f, "vt %f %f\n", u, v);
        }

        if(pVerts2)
        {
            float u = pVerts2[i].uv[0] > 2046 ? float(pVerts2[i].uv[0])/65532.0f : float(pVerts2[i].uv[0])/2046.0f;
            float v = pVerts2[i].uv[1] > 2046 ? float(pVerts2[i].uv[1])/65532.0f : float(pVerts2[i].uv[1])/2046.0f;
            fprintf_s(f, "vt %f %f\n", u, v);
        }
    }
    
    // Write normals
    for(uint32 i = 0; i < nVertices; i++)
    {
        if(pVerts1)
        {
            Vec3D norm;
            norm.x = (float) pVerts1[i].normal[0]/255.0f;
            norm.y = (float) pVerts1[i].normal[1]/255.0f;
            norm.z = (float) pVerts1[i].normal[2]/255.0f;
            fprintf_s(f, "vn %f %f %f\n", norm.x, norm.y, norm.z);
        }

        if(pVerts2)
        {
            Vec3D norm;
            norm.x = (float) pVerts2[i].normal[0]/255.0f;
            norm.y = (float) pVerts2[i].normal[1]/255.0f;
            norm.z = (float) pVerts2[i].normal[2]/255.0f;
            fprintf_s(f, "vn %f %f %f\n", norm.x, norm.y, norm.z);
        }
    }

    // Write faces
    for(uint32 i = 0; i < nFaces; i += 3)
    {
        fprintf_s(f, "f %d/%d/%d %d/%d/%d %d/%d/%d\n", faces[i]+1, faces[i]+1, faces[i]+1,
                                                       faces[i+1]+1, faces[i+1]+1, faces[i+1]+1,
                                                       faces[i+2]+1, faces[i+2]+1, faces[i+2]+1);
    }

    fclose(f);

    return 0;
}

map<string, Model> Model::m_models;