/*!
 *  @file log.h
 *  @author Gunnar Lilleaasen
 *  @date 27/01/2010
 */
#ifndef LOG_H_
#define LOG_H_

#include <iostream>
#include <cstdarg>
#include <ctime>

enum LogLevel
{
    LL_DEBUG,
    LL_INFO,
    LL_WARNING,
    LL_ERROR
};

#define LOG_CONSOLE 0x00000001
#define LOG_FILE    0x00000002

class Log
{
public:
    /*!
     *  @brief Sets the log-level
     *
     *  @param level the log-level (use enum LogLevel)
     *  @return void
     */
    static void SetLevel(int level);

    /*!
     *  @brief Sets the output-flags
     *
     *  @param flags the flags
     *  @return void
     */
    static void SetFlags(int flags);

    /*!
     *  @brief Writes a message to the active outputs
     *
     *  @param level the log-level of this message
     *  @param format the format-string
     *  @param ... variable arguements for the format-string
     *  @return 0 at success
     */
    static int Write(int level, const char* format, ...);

private:
    /*!
     *  @brief Returns the singleton Log object
     *
     *  @return reference to the singleton object
     */
    static Log& Instance();

    /*!
     *  @brief Default constructor
     */
    Log();

    /*!
     *  @brief Copy-constructor (disabled)
     *
     *  @param log log object
     */
    Log(const Log& log);

    /*!
     *  @brief Assignment operator (disabled)
     *
     *  @param log log object
     */
    void operator= (const Log& log);

    /*!
     *  @brief Destructor
     */
    ~Log();

    int     flags;
    int     level;
    FILE*   log_file;
};

#endif // LOG_H_