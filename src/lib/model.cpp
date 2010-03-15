/****************************************************************************
*                                                                           *
*   libm3                                                                   *
*   Copyright (C) 2010  Gunnar Lilleaasen                                   *
*                                                                           *
*   This program is free software; you can redistribute it and/or modify    *
*   it under the terms of the GNU General Public License as published by    *
*   the Free Software Foundation; either version 2 of the License, or       *
*   (at your option) any later version.                                     *
*                                                                           *
*   This program is distributed in the hope that it will be useful,         *
*   but WITHOUT ANY WARRANTY; without even the implied warranty of          *
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the           *
*   GNU General Public License for more details.                            *
*                                                                           *
*   You should have received a copy of the GNU General Public License along *
*   with this program; if not, write to the Free Software Foundation, Inc., *
*   51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.             *
*                                                                           *
****************************************************************************/

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
    {
        return NULL;
    }

    map<string, Model>::iterator iter = m_models.find(filepath.filename() );
    if( iter != m_models.end() ) // File already loaded
    {
        return Model::GetModel(filename);
    }

    FILE* f = NULL;
    int error = 0;
    
    error = fopen_s(&f, filepath.string().c_str(), "rb");
    if(error) // Failed to open the file
    {
        return NULL;
    }

    Model m3(f);
    pair<string, Model> entry(filepath.filename(), m3);
    
    if(m3.GetHeader()->id[0] == '3' && m3.GetHeader()->id[1] == '3'
        && m3.GetHeader()->id[2] == 'D' && m3.GetHeader()->id[3] == 'M')
    {
        m_models.insert(entry);
    }
    else
    {
        return NULL;
    }
    
    return Model::GetModel(filename); // Everything went fine
};

void Model::UnloadModel(string filename)
{
    path filepath(filename);

    map<string, Model>::iterator iter = m_models.find(filepath.filename() );
    if( iter != m_models.end() )
    {
        m_models.erase( iter );
    }
    else
    {
    }
};

Model* Model::GetModel(string filename)
{
    path filepath(filename);

    map<string, Model>::iterator iter = m_models.find( filepath.filename() );
    if( iter == m_models.end() ) // Model isn't loaded
    {
        Model* pModel = LoadModel(filename);
        return pModel;
    }

    return &iter->second;
};

map<string, Model> Model::m_models;