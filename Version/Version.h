#pragma once

/////////////////////////////////////////////////////////////////////////
//Author: Akash Bhosale	, aabhosal@syr.edu							  //	
//Source: Prof. Jim Fawcett, CSE 687 -Object Oriented Design     	 //
//Course:CSE 687 Object Oriented Design								//
//Application: CSE 687 Project# 1  - Version.h					   //
//Environment : C++ Console										  //	
////////////////////////////////////////////////////////////////////
/*
*Package Operation
*------------------
*This package implements the versioning system of the repository

*Required files:
*----------------
*Dbcore.h
*FileSystem.h
*Payload.h
*Build Process
*--------------
*devenv Project 1.sln /rebuild debug
*
*
*Maintenance History:
*---------------------
*ver 1.0 : 6th February 2018
*Created and implemented functions to perform query on database
*/



#include <iostream>
#include "../FileSystem-Windows/FileSystemDemo/FileSystem.h"
#include "../Payload/Payload.h"
#include "../DbCore/DbCore.h"
#include <string>
#include <vector>
#include <algorithm>
#include <regex>

using namespace::std;
using namespace NoSqlDb;


	template<typename P>
	class Version
	{
	public:
		Version(DbCore<P>& db) : db_(db) { keys_ = db_.keys(); }
		int getVersion(const string& namespace_,const string& fileName);
		int getLatest(const string& fileName);
		vector<string> matchFile(const string& fileName);
		vector<int> getFileVersions(const vector<string>& files);
		bool isExists(const string& namespace_, const string& filename, int version);
		static void identity(std::ostream& out = std::cout);
		bool checkStatus(const string& fileName,int version);

	private:
		DbCore<P>& db_;
		Keys keys_;
		int version = 0;
	};

	//---------------------< function to get version >-----------------------------------------------
	template<typename P>
	int Version<P>::getVersion(const string& namespace_,const string& fileName)
	{
		int version;
		std::vector<std::string>matchFiles = matchFile(fileName);
		if (matchFiles.size() == 0)
			return 1;
		std::vector<int>versions = getFileVersions(matchFiles);
		if (versions.size() == 0)
			return 1;
		 version=*std::max_element(versions.begin(), versions.end()) ;
		 bool status = checkStatus(fileName,version);
		 if (status)
		 {
			 return version + 1;
		 }
		 else
		 {
			 return version;
		 }

	}
	//-----------------------< function to find latest version >----------------------------------------------------
	template<typename P>
	int Version<P>::getLatest(const string& fileName)
	{
		int version;
		std::vector<std::string>matchFiles = matchFile(fileName);
		if (matchFiles.size() == 0)
			return 1;
		std::vector<int>versions = getFileVersions(matchFiles);
		if (versions.size() == 0)
			return 0;
		version = *std::max_element(versions.begin(), versions.end());
		return version;
	}

	//---------------------------< function to match file >-------------------------------------------------------
	template<typename P>
	vector<string> Version<P>::matchFile(const string& fileName)
	{

		vector<string>matchFiles;
		for (auto key : keys_)
		{
			std::regex re(fileName);
			if (std::regex_search(key, re))
				matchFiles.push_back(key);
		}
		return matchFiles;
	}
	//----------------------------------------< function to get file version >---------------------------------------
	template<typename P>
	vector<int> Version<P>::getFileVersions(const vector<string>& files)
	{
		vector<int>versions;
		for (auto file : files)
		{
			size_t pos = file.find_last_of(".");
			if (isdigit(file[pos + 1]))
				versions.push_back(stoi(file.substr(pos + 1)));
		}
		return versions;
	}

	//-------------------------------< function to check if file exists >-------------------------------------------

	template<typename P>
	inline bool Version<P>::isExists(const string& namespace_, const string& fileName, int version)
	{
		string key = namespace_ + "::" + fileName + "." + to_string(version);
		bool result = db_.contains(key);
		if (result)
			return true;
		return false;
	}

	//---------------------------------< function to identify if file exists >-----------------------------------------

	template<typename P>
	inline void Version<P>::identity(std::ostream & out)
	{
		out << "\n  \"" << __FILE__ << "\"";
	}

	//------------------------------< function to check status of file >-------------------------------------------------
	template<typename P>
	inline bool Version<P>::checkStatus(const string& fileName,int version)
	{
		string key =fileName + "." + to_string(version);
		DbElement<P>elem = db_[key];
		if (elem.getStatus() == Close)
			return true;
		return false;
	}
