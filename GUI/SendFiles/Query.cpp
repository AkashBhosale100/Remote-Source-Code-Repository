
/////////////////////////////////////////////////////////////////////////
//Author: Akash Bhosale	, aabhosal@syr.edu							  //	
//Source: Prof. Jim Fawcett, CSE 687 -Object Oriented Design     	 //
//Course:CSE 687 Object Oriented Design								//
//Application: CSE 687 Project# 1  - Query.cpp      			   //
//Environment : C++ Console										  //	
///////////////////////////////////////////////////////////////////
/*
Required files :
*----------------
*Dbcore.h
*
*Build Process
*--------------
*devenv Project 1.sln / rebuild debug
*
*
*Maintenance History :
*-------------------- -
*ver 1.0 : 6th February 2018
* Created and implemented functions to perform query on database
*/

#include"Query.h"

#ifdef QUERY_TEST
int main()
{
	Utilities::Title("-------------------------------Testing Query---------------------------");
	putLine();
	TestExecutive ex;
	TestExecutive::TestStr ts6a{ Req6a," Query for a value of a specified key" };
	TestExecutive::TestStr ts6b{ Req6b, "Query for children of a specified key" };
	TestExecutive::TestStr ts6c{ Req6c, "Query for all keys that contain a specified string in their metadata section,\n where the specification is based on a regular expression" };
	TestExecutive::TestStr ts6d{ Req6d, "Query for all keys written within a specified time-date interval" };
	TestExecutive::TestStr ts7a{ Req7a,"Query on set of keys returned by a previous query " };
	TestExecutive::TestStr ts7b{ Req7b,"Performing 'oring' of queries" };
	TestExecutive::TestStr ts7c{ Req7c,"Performing query based on category" };
	ex.registerTest(ts6a);
	ex.registerTest(ts6b);
	ex.registerTest(ts6c);
	ex.registerTest(ts6d);
	ex.registerTest(ts7a);
	ex.registerTest(ts7b);
	ex.registerTest(ts7c);
	// run tests

	bool result = ex.doTests();
	if (result == true)
		std::cout << "\n  all tests passed";
	else
		std::cout << "\n  at least one test failed";

	putLine(2);
	return 0;

}
#endif

