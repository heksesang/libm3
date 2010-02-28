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
    fread(m_buf, m_bufSize, 1, f);
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
    
    error = fopen_s(&f, filepath.string().c_str(), "r");
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

map<string, Model> Model::m_models;