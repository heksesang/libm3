#include "filebuffer.h"


FileBuffer::FileBuffer(const char* filename) : 
m_buffer(0)
{
    Load(filename);
}

FileBuffer::FileBuffer(uint32 size) : 
m_buffer(size)
{
};

FileBuffer::~FileBuffer()
{
}

void FileBuffer::Load(const char* filename)
{
    if(filename == NULL)
        return;

    struct stat results;

    int error = stat(filename, &results);
    if(!error)
    {
        m_buffer.resize(results.st_size);

        std::ifstream fs(filename, std::ios::in|std::ios::binary);
        fs.read(GetBuffer(), GetSize());
        fs.close();

        Log::Write(LL_INFO, "Loaded file: %s\n", filename);
    }
    else
    {
        Log::Write(LL_WARNING, "Failed to load file: %s\n", filename);
    }
}

void FileBuffer::Save(const char* filename)
{
    if(filename == NULL)
        return;

    std::fstream fs(filename, std::ios::out|std::ios::binary);
    if(fs)
    {
        fs.write(GetBuffer(), GetSize());
        fs.close();

        Log::Write(LL_INFO, "Saved the file: %s\n", filename);
    }
    else
    {
        Log::Write(LL_WARNING, "Failed to save the file: %s\n", filename);
    }
}

// Getters
char* FileBuffer::GetBuffer()
{
    return &m_buffer[0];
}

size_t FileBuffer::GetSize() const
{
    return m_buffer.size();
}

// Extends the buffer with 'size' bytes
size_t FileBuffer::Extend(size_t size, int32 offset)
{

    if( offset > -1 )
    {
        m_buffer.insert( m_buffer.begin() + offset, size, '\0' );
    }
    else
    {
        m_buffer.resize( GetSize() + size, '\0' );
    }
    return GetSize();
}

// Detracts the buffer with 'size' bytes
size_t FileBuffer::Detract(size_t size, int32 offset)
{

    if( offset > -1 )
    {
        std::vector<char>::iterator it = m_buffer.begin() += offset;
        
        if( size > GetSize() )
            return GetSize();

        m_buffer.erase(it, it + size);
    }
    else
    {
        m_buffer.resize( GetSize() - size, '\0' );
    }
    
    return GetSize();
}