
//////////////////////////////////////////////////////////////////////
// Version.cpp - Implements test for version.cpp                    //
// ver 1.0														    //
//Name:Akash Bhosale                                               //
//Source:Jim Fawcett, CSE687 - Object Oriented Design, Spring 2018 //
/////////////////////////////////////////////////////////////////////
/*
* Package Operations:
* -------------------
* This package provides functions to test the versioning system

* Required Files:
* ---------------
*version.h , StringUtilities.h
* TestUtilities.h

* Maintenance History:
* --------------------
* ver 1.0 : 8th  March 2018
* - added code to manage version
* 
*/

#include "Version.h"
#include <regex>
#include <string>
#include "../Utilities/StringUtilities/StringUtilities.h"
#include "../Utilities/TestUtilities/TestUtilities.h"

using namespace::std;
using namespace NoSqlDb;

//----< reduce the number of characters to type >----------------------

auto putLine = [](size_t n = 1, std::ostream& out = std::cout)
{
	Utilities::putline(n, out);
};

//---------------<function to display text>------------------------------------
void showText(const std::string& text)
{
	std::cout << "\n  " << text;
}


#ifdef TEST_VERSION
//--------------------------------< demo class >-----------------------------------
class DbProvider
{
public:
	DbCore<PayLoad>& db() { return db_; }
private:
	static DbCore<PayLoad> db_;
};

DbCore<PayLoad> DbProvider::db_;


// --------------< demo db creator >------------------------------
void createDemoDb()
{
	DbCore<PayLoad> db;
	DbElement<PayLoad>elem;
	DbProvider dbp;
	elem.name("IPayload.h");
	db["NoSqlDb::IPayload.h.1"] = elem;
	elem.name("IPayload.h");
	elem.setStatus(Open);
	db["NoSqlDb::IPayload.h.2"] = elem;
	elem.name("Ipayload.h");
	db["NoSqlDb::IPayload.h.3"] = elem;
	elem.name("IPayload.h");
	db["NoSqlDb::IPayload.h.4"] = elem;
	dbp.db() = db;
}


//---------------------------Demonstrates requirement 3e>------------------------------------
bool testR3e()
{
	createDemoDb();
	DbProvider dbp;
	DbCore<PayLoad>db = dbp.db();
	Version<PayLoad> v1(db);
	string fileName = "NoSqlDb::IPayload.h";
	int version = v1.getVersion(fileName);
	if (version > 0)
	{
		putLine();
		cout << "Checked-in file version is :" << version;
	}
	return version > 0;
}

int main()
{
	Utilities::Title("Testing Version - manages version numbering for all files held in the Repository");
	putLine();
	TestExecutive ex;
	TestExecutive::TestStr ts1{testR3e , "Generates checked-in file version" };
	ex.registerTest(ts1);
	bool result = ex.doTests();
	if (result == true)
		std::cout << "\n  all tests passed";
	else
		std::cout << "\n  at least one test failed";

	putLine(2);
	return 0;

	return 0;
}
#endif


