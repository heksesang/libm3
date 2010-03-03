//
// Copyright (C) M3Import
// 
// File: M3ImportCmd.cpp
//
// MEL Command: M3Import
//
// Author: Maya Plug-in Wizard 2.0
//

// Includes everything needed to register a simple MEL command with Maya.
// 
#include <maya/MSimple.h>
#include "../lib/model.h"

// Use helper macro to register a command with Maya.  It creates and
// registers a command that does not support undo or redo.  The 
// created class derives off of MPxCommand.
//
DeclareSimpleCommand( M3Import, "M3Import", "2010");

using namespace m3;

MStatus M3Import::doIt( const MArgList& args )
//
//	Description:
//		implements the MEL M3Import command.
//
//	Arguments:
//		args - the argument list that was passes to the command from MEL
//
//	Return Value:
//		MS::kSuccess - command succeeded
//		MS::kFailure - command failed (returning this value will cause the 
//                     MEL script that is being run to terminate unless the
//                     error is caught using a "catch" statement.
//
{
	MStatus stat = MS::kSuccess;

	// Since this class is derived off of MPxCommand, you can use the 
	// inherited methods to return values and set error messages
	//
	setResult( "M3Import command executed!\n" );

	return stat;
}
