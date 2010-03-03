/****************************************************************************
*                                                                           *
*   ObjConv - M3 to OBJ converter                                           *
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

#include <iostream>
#include "../model.h"

using namespace std;
using namespace boost::filesystem;
using namespace m3;

int main(int argc, char* argv[])
{
    if(argc == 1)
    {
        cout << "ERROR: No args supplied!" << endl;
        return -1;
    }

    path p(argv[1]);

    if( !exists(p) )
    {
        cout << "ERROR: Invalid filepath!" << endl;
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
                cout << "Conversion failed: " << iter->path().string() << endl;
            else
                cout << "Conversion succeeded: " << iter->path().string() << endl;
        }
    }
    else
    {
        if( p.extension().compare(".m3") )
        {
            cout << "ERROR! Wrong file format!" << endl;
            return -1;
        }

        return Model::Convert(argv[1]);
    }
}