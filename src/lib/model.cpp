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
        LogPrintf(LYELLOW, "The file %s does not exist", filename);
        return NULL;
    }

    map<string, Model>::iterator iter = m_models.find(filepath.filename() );
    if( iter != m_models.end() ) // File already loaded
    {
        LogPrintf(LDEBUG, "The file %s is already loaded", filename);
        return Model::GetModel(filename);
    }

    FILE* f = NULL;
    int error = 0;
    
    error = fopen_s(&f, filepath.string().c_str(), "rb");
    if(error) // Failed to open the file
    {
        LogPrintf(LYELLOW, "The file %s could not be opened", filename);
        return NULL;
    }

    Model m3(f);
    pair<string, Model> entry(filepath.filename(), m3);
    
    if(m3.GetHeader()->id[0] == '3' && m3.GetHeader()->id[1] == '3'
        && m3.GetHeader()->id[2] == 'D' && m3.GetHeader()->id[3] == 'M')
    {
        m_models.insert(entry);
        LogPrintf(LGREEN, "The file %s was loaded", filename);
    }
    else
    {
        LogPrintf(LYELLOW, "Identifier does not match '33DM'");
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
        LogPrintf(LGREEN, "The file %s was unloaded", filename);
        m_models.erase( iter );
    }
    else
    {
        LogPrintf(LGREEN, "The file %s was not loaded", filename);
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
/*
int Model::Convert(std::string filename)
{
    Model* pModel = Model::LoadModel(filename);

    if(!pModel)
        return -1;

    MD33* pHead = pModel->GetHeader();
    ReferenceEntry* pRefs = pModel->GetRefs();

    MODL20* pMODL20 = NULL;
    MODL23* pMODL23 = NULL;

    VertexExt* pVerts1 = NULL;
    Vertex* pVerts2 = NULL;
    
    DIV* views = NULL;
    Region* regions = NULL;
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
                pVerts1 = pModel->GetEntries<VertexExt>(pMODL20->vertexData);
                nVertices = pMODL20->vertexData.nEntries/sizeof(VertexExt);
            }
            else
            {
                pVerts2 = pModel->GetEntries<Vertex>(pMODL20->vertexData);
                nVertices = pMODL20->vertexData.nEntries/sizeof(Vertex);
            }
        }
        views = pModel->GetEntries<DIV>( pMODL20->divisions );
        break;

    case 23:
        pMODL23 = pModel->GetEntries<MODL23>(pHead->MODL);
        if( (pMODL23->flags & 0x20000) != 0 ) // Has vertices
        {
            if( (pMODL23->flags & 0x40000) != 0 ) // Has extra 4 byte
            {
                pVerts1 = pModel->GetEntries<VertexExt>(pMODL23->vertexData);
                nVertices = pMODL23->vertexData.nEntries/sizeof(VertexExt);
            }
            else
            {
                pVerts2 = pModel->GetEntries<Vertex>(pMODL23->vertexData);
                nVertices = pMODL23->vertexData.nEntries/sizeof(Vertex);
            }
        }
        views = pModel->GetEntries<DIV>( pMODL23->divisions );
        break;

    default:
        return -1;
    }

    regions = pModel->GetEntries<Region>( views->regions );
    faces = pModel->GetEntries<uint16>( views->faces );
    nFaces = views->faces.nEntries;

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
            float u = pVerts1[i].uv[0] > 4094 ? float(pVerts1[i].uv[0])/32767.0f : float(pVerts1[i].uv[0])/2047.0f;
            float v = pVerts1[i].uv[1] > 4094 ? 1-float(pVerts1[i].uv[1])/32767.0f : float(pVerts1[i].uv[1])/2047.0f;
            //float u = (float) pVerts1[i].uv[0] / 2047.0f;
            //float v = (float) pVerts1[i].uv[1] / 2047.0f;

            fprintf_s(f, "vt %f %f\n", u, 1-v);
        }

        if(pVerts2)
        {
            float u = pVerts2[i].uv[0] > 2047 ? float(pVerts2[i].uv[0])/32767.0f : float(pVerts2[i].uv[0])/2047.0f;
            float v = pVerts2[i].uv[1] > 2047 ? float(pVerts2[i].uv[1])/32767.0f : float(pVerts2[i].uv[1])/2047.0f;
            //float u = (float) pVerts2[i].uv[0] / 2047.0f;
            //float v = (float) pVerts2[i].uv[1] / 2047.0f;

            fprintf_s(f, "vt %f %f\n", u, 1-v);
        }
    }
    
    // Write normals
    for(uint32 i = 0; i < nVertices; i++)
    {
        Vec3D norm;
        float w = 0.0f;

        if(pVerts1)
        {
            norm.x = (float) 2*pVerts1[i].normal[0]/255.0f - 1;
            norm.y = (float) 2*pVerts1[i].normal[1]/255.0f - 1;
            norm.z = (float) 2*pVerts1[i].normal[2]/255.0f - 1;

            w = (float) pVerts1[i].normal[3]/255.0f;
        }

        if(pVerts2)
        {
            norm.x = (float) 2*pVerts2[i].normal[0]/255.0f - 1;
            norm.y = (float) 2*pVerts2[i].normal[1]/255.0f - 1;
            norm.z = (float) 2*pVerts2[i].normal[2]/255.0f - 1;

            w = (float) pVerts2[i].normal[3]/255.0f;
        }

        if(w)
        {
            norm.x = norm.x/w;
            norm.y = norm.y/w;
            norm.z = norm.z/w;
        }

        fprintf_s(f, "vn %f %f %f\n", norm.x, norm.y, norm.z);
    }

    // Write geosets
    for(uint32 i = 0; i < views->regions.nEntries; i++)
    {
        fprintf_s(f, "g %s %d\n", "geoset", i);
        for(uint32 j = regions[i].ofsIndices; j < (regions[i].ofsIndices + regions[i].nIndices); j +=3)
        {
            fprintf_s(f, "f %d/%d/%d %d/%d/%d %d/%d/%d\n", faces[j]+1, faces[j]+1, faces[j]+1,
                                                           faces[j+1]+1, faces[j+1]+1, faces[j+1]+1,
                                                           faces[j+2]+1, faces[j+2]+1, faces[j+2]+1);
        }
    }

    fclose(f);

    return 0;
}
*/

map<string, Model> Model::m_models;