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

#ifndef MODEL_H_
#define MODEL_H_

// STL
#include <vector>
#include <map>

// Boost
#include <boost/filesystem.hpp>

// M3
#include "m3header.h"
#include "log.hpp"

namespace m3
{
    class Model
    {
        static std::map<std::string, Model> m_models;
        
        char* m_buf;
        int   m_bufSize;
        
        MD33* m_head;
        ReferenceEntry* m_refs;

        
        int flags;

    public:
        Model(FILE* f);
        Model(const Model& m);
        ~Model();

        Model& operator=(const Model& m);

        static Model* LoadModel(std::string filename);
        static void   UnloadModel(std::string filename);
        static Model* GetModel(std::string filename);

        template <typename T>
        T* GetEntries(Reference ref);
        template <typename T>
        T* GetEntry(Reference ref, int index);

        MD33* GetHeader() { return m_head; };
        ReferenceEntry* GetRefs() { return m_refs; };
        int GetType() { return m_refs[m_head->MODL.ref].type; };
    };

    template <typename T>
    T* Model::GetEntries(Reference ref)
    {
        return (T*) ( m_buf + m_refs[ref.ref].offset );
    }

    template <typename T>
    T* Model::GetEntry(Reference ref, int index)
    {
        return (T*) ( m_buf + m_refs[ref.ref].offset + sizeof(T)*index );
    }
}

#endif // MODEL_H_