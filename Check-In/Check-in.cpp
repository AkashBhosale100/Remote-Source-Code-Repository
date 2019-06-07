
/////////////////////////////////////////////////////////////////////////
//Author: Akash Bhosale	, aabhosal@syr.edu							  //	
//Source: Prof. Jim Fawcett, CSE 687 -Object Oriented Design     	 //
//Course:CSE 687 Object Oriented Design								//
//Application: CSE 687 Project# 2  - Check-in.cpp				   //
//Environment : C++ Console										  //	
////////////////////////////////////////////////////////////////////
/*
*Package Operation
*------------------
*This package implements all the necessary functionality required to
query the database depending on name, description,dateTime,category.
*This package also implements functions to perform 'and'ing and
*'or'ing of queries.
*
*Required files:
*----------------
*FileSystem.h
*StringUtilites.h
*TestUtilities.h
*Version.h
*
*Build Process
*--------------
*devenv Project 1.sln /rebuild debug
*
**Maintenance History:
*---------------------
*ver 1.0 : 6th March 2018
*Test stub for Check-in
*/


#include "Check-in.h"
#include "../Version/Version.h"
#include "../Utilities/StringUtilities/StringUtilities.h"
#include "../Utilities/TestUtilities/TestUtilities.h"
#include <iostream>

using namespace::NoSqlDb;
using namespace::std;

#ifdef TEST_CHECKIN
//----------< function to display text >---------------------------------------
void showText1(const std::string& text)
{
	std::cout << "\n  " << text;
}
auto putLine1 = [](size_t n = 1, std::ostream& out = std::cout)
{
	Utilities::putline(n, out);
};
//--------------< db provider class >----------------------------------------
class DbProvider1
{
public:
	DbCore<PayLoad>& db() { return db_; }
private:
	static DbCore<PayLoad> db_;
};

DbCore<PayLoad> DbProvider1::db_;

<---------------< demo db creator >---------------------------------------------
void createDemoDb()
{
	DbCore<PayLoad> db;
	DbElement<PayLoad>elem;
	DbProvider1 dbp;
	
	db["NoSqlDb::IPayload.h.1"] = elem;
	elem.name("IPayload.h");
	db["NoSqlDb::IPayload.h.2"] = elem;
	elem.name("Ipayload.h");
	db["NoSqlDb::IPayload.h.3"] = elem;
	elem.name("IPayload.h");
	elem.setStatus(Open);
	db["NoSqlDb::IPayload.h.4"] = elem;
	dbp.db() = db;
}

< -------------------------< test function >---------------------------------------------
	bool testR3c()
{
	createDemoDb();
	DbProvider1 dbp;
	DbCore<PayLoad>db = dbp.db();
	CheckIn<PayLoad> ckIn(db);
	string clientPath = "../ClientStore/CheckIn";
	string fileName = "IPayload.h";
	string namespace_ = "NoSqlDb";
	bool result = ckIn.doCheckIn(clientPath, fileName, namespace_);
	if (result)
	{
		showText1("Ready for next check in");
		putLine1();
		return true;
	}
	return false;
}

< -----------------------< test stub >-------------------------------------------------
int main()
{
	Utilities::Title("Testing Check-In  - Checks-in files provided by  client");
	putLine1();
	TestExecutive ex;
	TestExecutive::TestStr ts1{ testR3c , "Generates checked-in file version" };
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
