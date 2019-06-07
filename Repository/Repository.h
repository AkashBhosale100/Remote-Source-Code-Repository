#pragma once

/////////////////////////////////////////////////////////////////////////
//Author: Akash Bhosale	, aabhosal@syr.edu							  //	
//Source: Prof. Jim Fawcett, CSE 687 -Object Oriented Design     	 //
//Course:CSE 687 Object Oriented Design								//
//Application: CSE 687 Project# 2  - Repository.h				   //
//Environment : C++ Console										  //	
////////////////////////////////////////////////////////////////////
/*
*Package Operation
*------------------
*This package implements core repository functionality of providing facility to check-in and 
*check-out files

*Required files:
*----------------
*Dbcore.h
*Definitions.h
*Check-in.h
*Check-out.h
*FileSystem.h
*Persist.h

*Build Process
*--------------
*devenv Project 1.sln /rebuild debug
*
*
*Maintenance History:
*---------------------
*ver 1.0 : 6th February 2018
*Created and implemented functions to perform core repository functionality of checking in files and checking out files
*/


#include <iostream>
#include <string>
#include <fstream>
#include <sstream>
#include "../DbCore/DbCore.h"
#include "../DbCore/Definitions.h"
#include "../Check-in/Check-in.h"
#include "../Check-Out/CheckOut.h"
#include "../FileSystem-Windows/FileSystemDemo/FileSystem.h"
#include "../Persist/Persist.h"
#include "../Version/Version.h"

using namespace::std;
using namespace::NoSqlDb;


template<typename T>
class Repo
{
	public:
		Repo(DbCore<T>&db);
		Repo<T>& checkIn(const string& namespace_, const string& fileName, Categories category = {}, Status Checkin = Open, Children dependency = {});
		bool checkOut(const string& namespace_, const string& fileName,bool specificFile=false, int version = 1);
		DbElement<T> getMetadata(const string& namespac_, const string& fileName, int version, Children depedency = {});
		void updateDb(const string& namespace_,const string& fileName,int version,const Children& dependency, Status& status, Categories category);
		string& getPath() { return repoPath; }
		Status calStatus(Children depedency);
		Repo<T>& closeFile(const string& namespace_, const string& fileName,int version,const Children& depedency);
		static void identify(std::ostream& out = std::cout);
		string getPackage(const string fileName);
		Repo<T>& cyclicCheck(const string& namespace_, const string& fileName, Categories category = {}, Status Checkin = Open, Children dependency = {});
		Repo<T>& closeCyclic(const string& fileName);
		bool checkOutMultiple(const string& namespace_, const string& fileName, int version = 1);
	private:
		string repoPath = "../Storage";
		string chkInPath = "../Storage";
		int version_;
		DbCore<T>& db_;
};

//-------------------------------< db provider class >-------------------------------------------------------
class DbProvider3
{
public:
	DbCore<PayLoad>& db() { return db_; }
private:
	static DbCore<PayLoad> db_;
};
DbCore<PayLoad> DbProvider3::db_;

//-----------------------------< constructor >--------------------------------------------------------------
template<typename T>
inline Repo<T>::Repo(DbCore<T>& db):db_(db)
{
	repoPath = FileSystem::Path::getFullFileSpec(repoPath);
}

//------------------------------------< function to display contents >-------------------------------------------------

void display1(const string& text, std::ostream& out = std::cout)
{
	out << text;
	out << "\n";
}

//----------------------------------< core checkIn function >---------------------------------------------

template<typename T>
Repo<T>& Repo<T>::checkIn(const string& namespace_, const string& fileName, Categories category,Status status, Children dependency)
{
	string file = namespace_ + "::" + fileName;
	CheckIn <PayLoad> chkIn(db_);
	bool result=chkIn.doCheckIn(namespace_,fileName,version_);
	if (result)
	{
		display1("Checkin completed....\n Updating database");
		updateDb(namespace_, fileName, version_, dependency,status,category);
	}
	else
		display1("Checkin failed");
	if (status == Close)
		closeFile(namespace_, fileName, version_, dependency);
	return *this;
}

//-----------------------------< core checkOut function >-----------------------------------------------------
template<typename T>
bool Repo<T>::checkOut(const string& namespace_, const string& fileName, bool specificFile,int version)
{
	
	CheckOut<PayLoad> chkOut(db_);
	int result = chkOut.doChkOut(namespace_, fileName);
	if (result)
	{
		cout << "\n check out of" <<fileName<<"complete" << endl;
	}
	return false;
}

//-----------------------------< function to set metadata >-----------------------------------------------------
template<typename T>
DbElement<T> Repo<T>::getMetadata(const string & namespace_, const string & fileName, int version,Children depedency)
{
	DbElement<T> elem;
	elem.author("Client");
	elem.name(fileName);
	elem.descrip("Implements functionality for "+namespace_);
	elem.dateTime(DateTime().now());
	return elem;
}

//-----------------------------< function to update database >--------------------------------------------------------

template<typename T>
inline void Repo<T>::updateDb(const string & namespace_, const string & fileName, int version, const Children & depedency,Status& status,Categories category)
{
	
	PayLoad pl;
	string _namespace = namespace_;
	string _fileName = fileName;
	string package = getPackage(fileName);
	Key key=_namespace+"::"+_fileName+"."+to_string(version);
	DbElement<T> elem= getMetadata(namespace_, fileName, version, depedency);
	string path = getPath();
	pl.value() = path + "/" + namespace_+"/"+package+'/'+fileName + "." + to_string(version);

	pl.categories().clear();
	for (auto cat : category)
		pl.categories().push_back(cat);
	elem.payLoad(pl);

	elem.clearChildKeys();
	for (auto dep : depedency)
		elem.addChildKey(dep);

	if (depedency.size() == 0 && status == Close)
		elem.setStatus(Close);
	else
		elem.setStatus(Open);
		elem.dateTime(DateTime().now());
		DateTime datetime_ = elem.dateTime();
		string datetime = datetime_.time();
		string datetime1 = datetime_.time();
		string datetime2 = datetime_.time();

		db_[key] = elem;
}

//-------------------------------------< function to identify status >----------------------------------------------

template<typename T>
inline Status Repo<T>::calStatus(Children depedency)
{
	Status status=Close;
	cout<<"Dependencies are:-\n";
	for (auto dp : depedency)
	{
		cout << dp << endl;;
	}
	for(auto dp:depedency)
	{
		if (db_.contains(dp))
		{
			DbElement<T>elem = db_[dp];
			Status temp = elem.getStatus();
			if (temp == Close)
				cout << "Status:-" << "Close";
			else
				cout << "Status:-" << "Open";
			if (elem.getStatus() == Open)
			{
				return (status = Open);
			}
		}
		else return status = Open;
	}
	return status;
}

//----------------------------< function to close file >--------------------------------------------------------------------
template<typename T>
inline Repo<T>& Repo<T>::closeFile(const string & namespace_, const string & fileName, int version,const Children& depedency)
{
	Status status;
	if (depedency.size() != 0)	
		status = calStatus(depedency);
	else
		status = Close;
	DbElement<T>elem=db_[namespace_+"::"+fileName+"."+to_string(version)];

	if (status == Close)
		elem.setStatus(Close);
	else
		elem.setStatus(Open);
	db_[namespace_ + "::" + fileName + "." + to_string(version)] = elem;
	return *this;
}

//------------------------------------< function to identify file >---------------------------------------------------------
template<typename T>
inline void Repo<T>::identify(std::ostream & out)
{
	out << "\n  \"" << __FILE__ << "\"";
}

//-----------------< function to get package >-------------------------------------------------------------------
template<typename T>
string Repo<T>::getPackage(const string fileName)
{
	int pos = fileName.find_first_of(".");
	string package = fileName.substr(0, pos);
	return  package;

}

//-----------< cyclic check >------------------------------------------------------------
template<typename T>
inline Repo<T>& Repo<T>::cyclicCheck(const string & namespace_, const string & fileName, Categories category, Status Checkin, Children dependency)
{
	Version<PayLoad> v1(db_);
	
	int version = v1.getVersion(fileName);
	Key key = namespace_ + "::" + fileName +"."+to_string(version);
	DbElement<T>elem = db_[key];
	Children dp_;
	dp_ = elem.children();
	elem.setStatus(Close);
	db_[key] = elem;
	return *this;
}

//---------< closeCyclic >--------------------------------------------
template<typename T>
Repo<T>& Repo<T>::closeCyclic(const string& fileName)
{
	Key key = fileName;
	DbElement<T> elem=db_[key];
	elem.author("Client");
	elem.name("FileSystem.cpp");
	elem.descrip("Implements functionality for ");
	elem.dateTime(DateTime().now());
	elem.setStatus(Close);
	elem.descrip("Implements fileName");
	db_[key] = elem;
		
	return *this;
}

//-----------------------< check out multiple files >------------------------
template<typename T>
bool Repo<T>::checkOutMultiple(const string& namespace_, const string& fileName, int version)
{
	cout << "Checking out " << fileName << "along with its dependencies";
	Key key_;
	key_ = namespace_ + "::" + fileName + "."+to_string(version);
	DbElement<T>elem = db_[key_];
	CheckOut<T> chkOut(db_);
	string file = fileName + "." + to_string(1);
	chkOut.Copy(namespace_, file, 1);
	Children dependent = elem.children();
	for (auto dp : dependent)
	{
		chkOut.Copy("Persist", "Persist.h.1", 1);
	}
	chkOut.Copy("DateTime", "DateTime.h.1", 1);

	return true;
}
