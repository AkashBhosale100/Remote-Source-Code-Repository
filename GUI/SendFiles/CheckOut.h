#pragma once
/////////////////////////////////////////////////////////////////////////
//Author: Akash Bhosale	, aabhosal@syr.edu							  //	
//Source: Prof. Jim Fawcett, CSE 687 -Object Oriented Design     	 //
//Course:CSE 687 Object Oriented Design								//
//Application: CSE 687 Project# 2  - CheckOut.h					   //
//Environment : C++ Console										  //	
////////////////////////////////////////////////////////////////////
/*
*Package Operation
*------------------
*This package implements functionality to check out files from repository.
*
*Required files:
*----------------
*Dbcore.h
*Payload.h
*
*Build Process
*--------------
*devenv Project 1.sln /rebuild debug
*
*
*Maintenance History:
*---------------------
*ver 1.0 : 6th March 2018
*Created and implemened functions to check out files from repository
*/


#include <iostream>
#include <string>
#include "../DbCore/DbCore.h"
#include "../Payload/Payload.h"

using namespace::std;
using namespace::NoSqlDb;

template<typename P>
class CheckOut
{
	public:
		CheckOut(DbCore<P>& db) :db_(db) {}
		bool Copy(const string& namespace_, const string& fileName, const int& version);
		bool doChkOut(const string& namespace_, const string& fileName, int version = 1);
		std::string getKey(const std::string& namespace_, const std::string& fileName,int version);
		string rmVersion(const string& fileName);
		string getPackage(const string fileName);
		static void identity(std::ostream& out = std::cout);

		bool checkoutDependants(Key key);
		bool copyFiles(std::string& src, std::string& dest);
		int version() { return ver; }
	private:
		int ver = 1;
		string outDir = "../ClientStore/CheckOut";
		string inDir = "../RepositoryStore";
		void buildGraph(DbCore<P>& db);
		DbCore<P>& db_;
};

//-----------------------< function to identify >---------------------------------------------
template<typename P>
inline void CheckOut<P>::identity(std::ostream & out)
{
	out << "\n  \"" << __FILE__ << "\"";
}

//-----------------------< function to copy file from one location to other location >-----------------
template<typename P>
bool CheckOut<P>::Copy(const string& namespace_, const string& fileName, const int& version)
{
	string inDir_ = inDir + '/' + "NoSqlDb";
	string package = getPackage(fileName);
	string source = inDir + "/" + namespace_ +'/'+ package+"/" + fileName;
	string destPath = outDir + '/' + rmVersion(fileName);
	bool result = FileSystem::File::copy(source, destPath);
	if (result)
	{
		cout << "\n File checked out  to: \n" + FileSystem::Path::getFullFileSpec(destPath);
		return true;
	}
	return false;
}


//---------------------< return key >----------------------------------------------------
template<typename P>
inline std::string CheckOut<P>::getKey(const std::string & namespace_, const std::string & fileName,int version)
{
	return (namespace_ + "::" + fileName+"."+to_string(version));
	return std::string();

}

//Helper function to copy files
template <typename T>
bool CheckOut<T>::copyFiles(std::string& src, std::string& dest)
{
	return FileSystem::File::copy(src, dest);
}

//Helper Function to checkout if there is dependancy
template <typename P>
bool CheckOut<P>::checkoutDependants(Key key)
{
	DbElement<P> elem = db_[key];
	std::string fileName = FileSystem::Path::getName(elem.payLoad().value());
	std::string destPath = outDir + "/" + rmVersion(FileSystem::Path::getName(fileName));
	return copyFiles(elem.payLoad().value(), destPath);
}


//-----------------------------------< function to check out file >---------------------------------------
template<typename P>
bool CheckOut<P>::doChkOut(const string& namespace_, const string& fileName, int version)
{
	Key key;
	string srcFile = "";
	if (!FileSystem::Directory::exists(outDir))
	{
		FileSystem::Directory::create(outDir);
	}
	bool result = true;
	Version<PayLoad> v1(db_);
	if (version == 1)
	{
		string file = namespace_ + "::" + fileName;
		version = v1.getLatest(file);		
		ver = version;
			if (version == -1)
			return false;
	}
	else
	{
		result = v1.isExists(namespace_, fileName, version);	
		if (!result)
			return false;
	}
	srcFile = fileName + "." + to_string(version);
	if (result)
	{
		Copy(namespace_, srcFile, version);
		key = getKey(namespace_, fileName, version);
		DbElement<P> elem = db_[key];
		if (elem.children().size() != 0)
		{
			buildGraph(db_);
		}
		return true;
	}
	return false;
}



//------------------------------------< function to remove version from fileName >------------------------------
template<typename P>
string CheckOut<P>::rmVersion(const string& fileName)
{
	return fileName.substr(0, fileName.find_last_of("."));
}



//--------------< function to get package >-----------------------------------------------
template<typename P>
string CheckOut<P>::getPackage(const string fileName)
{
	int pos = fileName.find_first_of(".");
	string package = fileName.substr(0, pos);
	return  package;
}



