#ifndef MODEL_H_
#define MODEL_H_

// STL
#include <vector>
#include <map>

// Boost
#include <boost/filesystem.hpp>

// M3
#include "m3header.h"

namespace m3
{
    class Model
    {
        static std::map<std::string, Model> m_models;
        
        char* m_buf;
        int   m_bufSize;
        
        MD33* m_head;
        ReferenceEntry* m_refs;

    public:
        Model(FILE* f);
        Model(const Model& m);
        ~Model();

        Model& operator=(const Model& m);

        // Static functions
        static Model* LoadModel(std::string filename);
        static void   UnloadModel(std::string filename);
        static Model* GetModel(std::string filename);
        
        static int Convert(std::string filename);

        // Non-static functions
        template <typename T>
        T* GetEntries(Reference ref);

        MD33* GetHeader();
        ReferenceEntry* GetRefs();
    };

    template <typename T>
    T* Model::GetEntries(Reference ref)
    {
        return (T*) ( m_buf + m_refs[ref.ref].offset );
    }
}

#endif // MODEL_H_