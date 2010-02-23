#include "log.h"

/********************************
*                               *
*         CONSTRUCTORS          *
*                               *
********************************/
Log::Log()
{
    fopen_s(&log_file, "log.txt", "w");
}
Log::~Log()
{
    fclose(log_file);
}

/********************************
*                               *
*       STATIC FUNCTIONS        *
*                               *
********************************/
Log& Log::Instance()
{
    static Log log;
    return log;
}

void Log::SetLevel(int level)
{
    Log& log = Log::Instance();
    log.level = level;
}

void Log::SetFlags(int flags)
{
    Log& log = Log::Instance();

    log.flags = 0;
    log.flags |= flags;
}

int Log::Write(int level, const char* format, ...)
{
    Log& log = Log::Instance();

    if(level >= log.level)
    {
        va_list args;
        va_start(args, format);

        char szBuf[1024];

        time_t t;
        tm timeptr;

        time(&t);
        gmtime_s(&timeptr, &t);

        strftime(szBuf, 1024, "[%H:%M:%S] ", &timeptr);

        switch(level)
        {
        case LL_DEBUG:
            sprintf_s(&szBuf[strlen(szBuf)], 1024 - strlen(szBuf), "debug: ");
            vsprintf_s(&szBuf[strlen(szBuf)], 1024 - strlen(szBuf), format, args);
            break;

        case LL_INFO:
            sprintf_s(&szBuf[strlen(szBuf)], 1024 - strlen(szBuf), "info: ");
            vsprintf_s(&szBuf[strlen(szBuf)], 1024 - strlen(szBuf), format, args);
            break;

        case LL_WARNING:
            sprintf_s(&szBuf[strlen(szBuf)], 1024 - strlen(szBuf), "warning: ");
            vsprintf_s(&szBuf[strlen(szBuf)], 1024 - strlen(szBuf), format, args);
            break;

        case LL_ERROR:
            sprintf_s(&szBuf[strlen(szBuf)], 1024 - strlen(szBuf), "error: ");
            vsprintf_s(&szBuf[strlen(szBuf)], 1024 - strlen(szBuf), format, args);
            break;

        default:
            break;
        }

        int error = 0;

        if((log.flags & LOG_CONSOLE) == LOG_CONSOLE)
            std::cout << szBuf;
        if((log.flags & LOG_FILE) == LOG_FILE)
        {
            fwrite(szBuf, sizeof(char), strnlen(szBuf, 1024), log.log_file);
            error = fflush(log.log_file);
        }

        return error;
    }

    return -1;
}