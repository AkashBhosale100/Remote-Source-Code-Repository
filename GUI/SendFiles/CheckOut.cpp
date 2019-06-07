

/////////////////////////////////////////////////////////////////////////
//Author: Akash Bhosale	, aabhosal@syr.edu							  //	
//Source: Prof. Jim Fawcett, CSE 687 -Object Oriented Design     	 //
//Course:CSE 687 Object Oriented Design								//
//Application: CSE 687 Project# 2  - CheckOut.cpp				   //
//Environment : C++ Console										  //	
////////////////////////////////////////////////////////////////////
/*
*Package Operation
*------------------
*This package provided functions to checkout files from repository
*
*
*Required files:
*----------------
*Dbcore.h
*Version.h
*Payload.h
*FileSystem.h
*StringUtilites.h
*TestUtilities.h
*
*Build Process
*--------------
*devenv Project 1.sln /rebuild debug
*
*
*Maintenance History:
*---------------------
*ver 1.0 : 6th March 2018
*Created and implemented functions to perform checkout
*/

#include "CheckOut.h"
#include "../Version/Version.h"
#include "../Payload/Payload.h"
#include "../DbCore/DbCore.h"
#include "../Payload/Payload.h"
#include "../FileSystem-Windows/FileSystemDemo/FileSystem.h"
#include "../Utilities/StringUtilities/StringUtilities.h"
#include "../Utilities/TestUtilities/TestUtilities.h"
using namespace NoSqlDb;


//---------------< db Provider class >----------------------------------------
class DbProvider2
{
public:
	DbCore<PayLoad>& db() { return db_; }
private:
	static DbCore<PayLoad> db_;
};
DbCore<PayLoad> DbProvider2::db_;

//----< reduce the number of characters to type >----------------------

void createDemoDb2()
{
	DbCore<PayLoad> db;
	DbElement<PayLoad>elem;
	DbProvider2 dbp;
	elem.name("IPayload.h");
	db["NoSqlDb::IPayload.h.1"] = elem;
	elem.name("IPayload.h");
	db["NoSqlDb::IPayload.h.2"] = elem;
	elem.name("Ipayload.h");
	db["NoSqlDb::IPayload.h.3"] = elem;
	elem.name("IPayload.h");
	db["NoSqlDb::IPayload.h.4"] = elem;
	dbp.db() = db;
}

//--------------------- < function to display text >-----------------------------------
void showText1(const std::string& text)
{
	std::cout << "\n  " << text;
}

// ---------------------< lambda to add line and show data >--------------------------------------
auto putLine1 = [](size_t n = 1, std::ostream& out = std::cout)
{
	Utilities::putline(n, out);
};

//---------------------------------<Testing check-out functionality>-------------------------------
bool testR3d()
{
	createDemoDb2();
	DbProvider2 dbp;
	DbCore<PayLoad>db = dbp.db();
	CheckOut<PayLoad> chkOut(db);
	bool result=chkOut.doChkOut("NoSqlDb", "IPayload.h");
	return result;
}

//----------------------< test stub >--------------------------------------------------------------------
#ifdef TEST_CHECKOUT
int main()
{
	Utilities::Title("Testing Version - manages version numbering for all files held in the Repository");
	putLine1();
	TestExecutive ex;
	TestExecutive::TestStr ts1{ testR3d , "Generates checked-in file version" };
	ex.registerTest(ts1);
	bool result = ex.doTests();
	if (result == true)
		std::cout << "\n  all tests passed";
	else
		std::cout << "\n  at least one test failed";

	putLine1(2);
	getchar();
	return 0;
	
}
#endif 







