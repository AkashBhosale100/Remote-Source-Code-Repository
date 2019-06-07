
/////////////////////////////////////////////////////////////////////////
//Author: Akash Bhosale	, aabhosal@syr.edu							  //	
//Source: Prof. Jim Fawcett, CSE 687 -Object Oriented Design     	 //
//Course:CSE 687 Object Oriented Design								//
//Application: CSE 687 Project# 2  - Repository.cpp				   //
//Environment : C++ Console										  //	
////////////////////////////////////////////////////////////////////
/*
*Package Operation
*------------------
*Implements functions to test core repository functionality
*
*Required files:
*----------------
*Repository.h
*Version.h

*Build Process
*--------------
*devenv Project 1.sln /rebuild debug
*
*
*Maintenance History:
*---------------------
*ver 1.0 : 6th March 2018
*Created and implemented functions to test repository core
*/
#include "Repository.h"
#include "../Version/Version.h"

//----------< function to display data >-----------------------------------------
void showText2(const std::string& text)
{
	std::cout << "\n  " << text;
}

//-------------------< function to create demo database >--------------------------
void createDemoDb3()
{
	DbCore<PayLoad> db;
	DbElement<PayLoad>elem;
	DbProvider3 dbp;
	elem.name("IPayload.h");
	db["NoSqlDb::IPayload.h.1"] = elem;
	elem.name("IPayload.h");
	db["NoSqlDb::IPayload.h.2"] = elem;
	elem.name("Ipayload.h");
	db["NoSqlDb::IPayload.h.3"] = elem;
	elem.name("IPayload.h");
	elem.setStatus(Close);
	db["NoSqlDb::IPayload.h.4"] = elem;

	
	elem.name("Depedency1");
	elem.setStatus(Close);
	db["NoSqlDb::Depedency1"] = elem;
	elem.name("Depedency2");
	elem.setStatus(Close);
	db["NoSqlDb::Depedency2"] = elem;
	elem.name("Depedency3");
	elem.setStatus(Close);
	db["NoSqlDb::Depedency3"] = elem;

	db["NoSqlDb::IPayload.h.4"].addChildKey("NoSqlDb::Depedency1");
	db["NoSqlDb::IPayload.h.4"].addChildKey("NoSqlDb::Depedency2");
	db["NoSqlDb::IPayload.h.4"].addChildKey("NoSqlDb::Depedency3");
	dbp.db() = db;
}

// -------------------------------< test stub >-------------------------------------------------------------
#ifdef TEST_REPOSITORY
int main()
{
	createDemoDb3();
	DbProvider3 dbp;
	DbCore<PayLoad>db = dbp.db();
	Repo<PayLoad> Rp(db);
	Categories category = {};
	Children Depedency = { "NoSqlDb::Depedency1","NoSqlDb::Depedency2","NoSqlDb::Depedency3" };
	Rp.checkIn("NoSqlDb","IPayload.h",category,Close, Depedency);
	Rp.checkOut("NoSqlDb", "IPayload.h");
}
#endif



