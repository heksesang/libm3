/*!
 *  @file filebuffer.h
 *  @author Gunnar Lilleaasen
 *  @date 27/01/2010
 */
#ifndef FILEBUFFER_H_
#define FILEBUFFER_H_

#include <stdio.h>
#include <string.h>
#include <stdlib.h>

typedef unsigned char uint8;
typedef char int8;
typedef unsigned __int16 uint16;
typedef __int16 int16;
typedef unsigned __int32 uint32;
typedef __int32 int32;

#include <sys/stat.h>
#include <cassert>
#include <vector>
#include <deque>
#include <list>
#include <iostream>
#include <iomanip>
#include <sstream>
#include <string>
#include <fstream>
#include <exception>

#include "log.h"

/*!
 *  @brief A class to contain/buffer a file
 */
class FileBuffer
{
public:
    /*!
     *  @brief Constructs a filebuffer from a file
     *
     *  @param filename file to load into buffer
     */
    FileBuffer(const char* filename);

    /*!
     *  @brief Constructs a filebuffer of a specific size
     *
     *  @param size the size of the buffer in bytes
     */
    FileBuffer(uint32 size = NULL);

    /*!
     *  @brief Destructor
     */
    virtual ~FileBuffer();

    /*!
     *  @brief Loads a file
     *
     *  @param filename the filename of the file
     */
    virtual void Load(const char* filename);

    /*!
     *  @brief Saves a file
     *
     *  @param filename the filename of the file
     */
    virtual void Save(const char* filename);


    /*!
     *  @brief Gets a pointer of datatype T into the buffer
     *
     *  @param offset the offset where the pointer points to
     *  @return the pointer
     */
    template <typename T>
    T* GetPointer(uint32 offset);

    template <typename T>
    const T* GetPointer(uint32 offset) const;

    /*!
     *  @brief Gets a value of datatype T from the buffer
     *
     *  @param offset the offset into the buffer
     *  @return the object
     */
    template <typename T>
    T GetValue(uint32 offset) const;

    /*!
     *  @brief Sets a value of datatype T into the buffer
     *
     *  @param value the value to insert
     *  @param offset the offset into the buffer
     *  @return the pointer
     */
    template <typename T>
    void SetValue(T value, uint32 offset);

    /*!
     *  @brief Gets a pointer to the start of the buffer
     *
     *  @return the pointer
     */
    char* GetBuffer();

    /*!
     *  @brief Gets the size of the buffer
     *
     *  @return buffer size
     */
    size_t GetSize() const;

    /*!
     *  @brief Increases the size of the buffer
     *
     *  @param size the amount of bytes that should be added
     *  @return the new size
     */
    virtual size_t Extend(size_t size, int32 offset = -1);

    /*!
     *  @brief Decreases the size of the buffer
     *
     *  @param size the amount of bytes that should be removed
     *  @return the new size
     */
    virtual size_t Detract(size_t size, int32 offset = -1);

protected:
    std::vector<char>    m_buffer;

private:
    /*!
     *  @brief Copy constructor (disabled)
     */
    FileBuffer(FileBuffer& f);

    /*!
     *  @brief Assignment operator (disabled)
     */
    void operator=(const FileBuffer &f);
};

// TEMPLATE FUNCTIONS

template <typename T>
T* FileBuffer::GetPointer(uint32 offset)
{
    return reinterpret_cast<T*>( &m_buffer[offset] );
};

template <typename T>
const T* FileBuffer::GetPointer(uint32 offset) const
{
    return reinterpret_cast<const T*>( &m_buffer[offset] );
};

template <typename T>
T FileBuffer::GetValue(uint32 offset) const
{
    return *GetPointer<T>( offset );
};

template <typename T>
void FileBuffer::SetValue(T value, uint32 offset)
{
    for(uint32 i = 0; i < sizeof(T); i++)
        GetBuffer()[offset + i] = *reinterpret_cast<char*>( &value )[i];
};

#endif // FILEBUFFER_H_