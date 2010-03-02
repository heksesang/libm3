/************************************************************************
*                                                                       *
*   Application: ObjConv                                                *
*   Description: Converts .m3 files into .obj files.                    *
*                PS: .obj files cannot hold animations                  *
*                                                                       *
*   Usage: ObjConv <filename>                                           *
*                                                                       *
*   Author: Gunnar Lilleaasen                                           *
*                                                                       *
************************************************************************/
#include <iostream>
#include "../model.h"

using namespace std;
using namespace boost::filesystem;
using namespace m3;

int main(int argc, char* argv[])
{
    if(argc == 1)
    {
        cout << "No args supplied" << endl;
        return -1;
    }

    path p(argv[1]);

    if( !exists(p) )
    {
        cout << "Invalid filepath" << endl;
        return -1;
    }

    p.extension();

    if( is_directory(p) )
    {
        int error = 0;
        for(directory_iterator iter(p), end; iter != end; ++iter)
        {
            if( iter->path().extension().compare(".m3") )
                continue;

            error = Model::Convert( iter->path().string() );
            if(error)
                cout << "Conversion failed for file: " << iter->path().string() << endl;
        }
    }
    else
    {
        if( p.extension().compare(".m3") )
        {
            cout << "This is no .m3 file" << endl;
            return -1;
        }

        return Model::Convert(argv[1]);
    }
}