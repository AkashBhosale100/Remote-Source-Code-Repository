#pragma once
/////////////////////////////////////////////////////////////////////////////
// Definitions.h - define aliases used throughout NoSqlDb namespace        //
// ver 1.0															      //
// Author: Akash Bhosale											     //
// Source: Dr. Jim Fawcett, CSE687 - Object Oriented Design, Spring 2018//
/////////////////////////////////////////////////////////////////////////


#include<vector>
#include <string>

namespace NoSqlDb
{
	using Key = std::string;
	using Keys = std::vector<Key>;
	using Children = std::vector<Key>;
	using Parents = std::vector<Key>;
	using Categories = std::vector<std::string>;
	enum Status{Open,Close,PendingClose};
	const bool showKey = true;
	const bool doNotShowKey = false;

}