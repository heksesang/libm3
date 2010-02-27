#include "model.h"

using namespace std;
using namespace boost::filesystem;
using namespace m3;

Model::Model(FILE* f)
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

Model::Model(const Model& m)
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

int Model::LoadModel(string filename)
{
    path filepath(filename);

    if( !exists(filepath) ) // Invalid filepath
        return -1;

    map<string, Model>::iterator iter = m_models.find(filepath.filename() );
    if( iter != m_models.end() ) // File already loaded
        return -1;

    FILE* f = NULL;
    int error = 0;
    
    error = fopen_s(&f, filepath.string().c_str(), "r");
    if(error) // Failed to open the file
        return error;

    Model m3(f);
    pair<string, Model> entry(filepath.filename(), m3);
    
    m_models.insert(entry);
    
    return 0; // Everything went fine
};

int Model::UnloadModel(string filename)
{
    path filepath(filename);

    map<string, Model>::iterator iter = m_models.find(filepath.filename() );
    if( iter != m_models.end() )
        m_models.erase( iter );
    else
        return -1; // File is not loaded

    return 0; // Everything went fine
};

// Needs some sort of security-mechanism
Model& Model::GetModel(string filename)
{
    path filepath(filename);

    map<string, Model>::iterator iter = m_models.find( filepath.filename() );
    if( iter == m_models.end() ) // Model isn't loaded
    {
        int error = LoadModel(filename);
        if(error)
            throw std::runtime_error("ERROR: Model not available"); // Failed to load model
    }

    return iter->second;
};