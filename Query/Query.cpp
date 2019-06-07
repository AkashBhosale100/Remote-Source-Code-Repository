
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

#include"../Query/Query.h"
#include "../DbCore/DbCore.h"
#include "../Payload/Payload.h"
#include "../Persist/Persist.h"
#include "../XML Document/XmlDocument/XmlDocument.h"


#ifdef QUERY_TEST
int main()
{
	NoSqlDb::DbCore<NoSqlDb::PayLoad> db_; 
	
	NoSqlDb::Persist<NoSqlDb::PayLoad> persist(db_);
	NoSqlDb::Xml xml;
	xml = persist.loadXml();
	if (xml.size() != 0) {
		persist.fromXml(xml);
	}
	NoSqlDb::Query<NoSqlDb::PayLoad> q1(db_);
	DateTime from, to;
	int allCheckIns = q1.allCheck_InsWithinMonthInterval(from, to);
	return 0;
}
#endif

