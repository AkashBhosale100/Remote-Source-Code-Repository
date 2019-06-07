#pragma once
///////////////////////////////////////////////////////////////////////
// Persist.h - persist DbCore<P> to and from XML file                //
// ver 1.0                                                           //
// Jim Fawcett, CSE687 - Object Oriented Design, Spring 2018         //
///////////////////////////////////////////////////////////////////////
/*
*  Package Operations:
*  -------------------
*  This package defines a single Persist class that:
*  - accepts a DbCore<P> instance when constructed
*  - persists its database to an XML string
*  - creates an instance of DbCore<P> from a persisted XML string
*
*  Required Files:
*  ---------------
*  Persist.h, Persist.cpp
*  DbCore.h, DbCore.cpp
*  Query.h, Query.cpp
*  PayLoad.h
*  XmlDocument.h, XmlDocument.cpp
*  XmlElement.h, XmlElement.cpp
*
*  Maintenance History:
*  --------------------
*  ver 1.0 : 12 Feb 2018
*  - first release
*/
#include "../DbCore/DbCore.h"
#include "../Query/Query.h"
#include "../DateTime/DateTime.h"
#include "../XML Document/XmlDocument/XmlDocument.h"
#include "../XML Document/XmlElement/XmlElement.h"
#include "../FileSystem-Windows/FileSystemDemo/FileSystem.h"
#include <string>
#include <iostream>
#include <fstream>
#include<sstream>

namespace NoSqlDb
{
	using namespace XmlProcessing;
	using Xml = std::string;
	using Sptr = std::shared_ptr<AbstractXmlElement>;

	const bool augment = true;   // do augment
	const bool rebuild = false;  // don't augment


  /////////////////////////////////////////////////////////////////////
  // Persist<P> class
  // - persist DbCore<P> to XML string

	template<typename P>
	class Persist
	{
	public:
		Persist(DbCore<P>&db):db_(db){}
		static void identify(std::ostream& out = std::cout);
		Persist<P>& shard(const Keys& keys);
		Persist<P>& addShardKey(const Key& key);
		Persist<P>& removeShard();
		Xml toXml();
		void saveXml(Xml& xml);
		Xml loadXml();
		bool fromXml(const Xml& xml, bool augment = true);  // will clear and reload db if augment is false !!!
		DbCore<P>& getDb() { return db_; }
	private:
		DbCore<P>& db_;
		Keys shardKeys_;
		bool containsKey(const Key& key);
		void toXmlRecord(Sptr pDb, const Key& key, DbElement<P>& dbElem);
		std::string xmlPath = "../Database";
		std::string xmlFile = "Database.xml";
		std::string fileName = "../Database/Database.xml";
	};

    
	//-------------< show file name >---------------------
	
	template<typename P>
	void Persist<P>::identify(std::ostream& out)
	{
		out << "\n  \"" << __FILE__ << "\"";
	}

	//--------------< set the shard keys collection >----------------
	template<typename P>
	Persist<P>& Persist<P>::shard(const Keys& keys)
	{
		shardKeys_ = keys;
		return *this;
	}

	//----< add key to shard collection >--------------------------------
	/*
	* - when sharding, a db record is persisted if, and only if, its key
	is contained in the shard collection
	*/
	template<typename P>
	Persist<P>& Persist<P>::addShardKey(const Key & key)
	{
		shardKeys_.push_back(key);
		return *this;
	}

	//----< empty the shardKeys collection >-----------------------------
	template<typename P>
	Persist<P>& Persist<P>::removeShard()
	{
		shardKeys_.clear();
	}

	//----< persist database record to XML string >----------------------
	template<typename P>
	void Persist<P>::toXmlRecord(Sptr pDb, const Key & key, DbElement<P>& dbElem)
	{
		Sptr pRecord = makeTaggedElement("dbRecord");
		pDb->addChild(pRecord);
		Sptr Key = makeTaggedElement("key", key);
		pRecord->addChild(Key);
		Sptr pValue = makeTaggedElement("value");
		pRecord->addChild(pValue);
		Sptr name = makeTaggedElement("name", dbElem.name());
		pValue->addChild(name);
		Sptr Author = makeTaggedElement("author", dbElem.author());
		pValue->addChild(Author);
		Sptr descrip = makeTaggedElement("description", dbElem.descrip());
		pValue->addChild(descrip);
		Sptr dateTime = makeTaggedElement("dateTime", dbElem.dateTime());
		pValue->addChild(dateTime);
		if (dbElem.getStatus() == Open)
		{
			Sptr status = makeTaggedElement("status", "Open");
			pValue->addChild(status);
		}
		else
		{
			Sptr status = makeTaggedElement("status", "Close");
			pValue->addChild(status);
		}

		Sptr pChildren = makeTaggedElement("children");
		pValue->addChild(pChildren);
		for (auto child : dbElem.children())
		{
			Sptr pChild = makeTaggedElement("child", child);
			pChildren->addChild(pChild);
		}

		Sptr pPayload = dbElem.payLoad().toXmlElement();
		pValue->addChild(pPayload);

	
	}
	//----< persist, possibly sharded, database to XML string >----------
	/*
	* - database is sharded if the shardKeys collection is non-empty
	*/
	template<typename P>
	inline Xml Persist<P>::toXml()
	{
		Sptr pDb = makeTaggedElement("db");
		pDb->addAttrib("type", "fromQuery");
		Sptr pDocElem = makeDocElement(pDb);
		XmlDocument xDoc(pDocElem);

		if (shardKeys_.size() > 0)
		{
			for (auto key : shardKeys_)
			{
				DbElement<P> elem = db_[key];
				toXmlRecord(pDb, key, elem);
			}
		}
		else
		{
			for (auto item : db_)
			{
				toXmlRecord(pDb, item.first, item.second);
			}
		}
		std::string xml = xDoc.toString();
		return xml;
	}


	//--------------<  function to save xml >----------------------------------
	template<typename P>
	inline void Persist<P>::saveXml(Xml & xml)
	{
		
		if (!FileSystem::Directory::exists(FileSystem::Path::getFullFileSpec(xmlPath)))
		{
			FileSystem::Directory::create(xmlPath);
		}
		std::ofstream out(FileSystem::Path::getFullFileSpec(fileName));
		XmlDocument newXDoc(xml, XmlDocument::str);
		out << newXDoc.toString();
		out.close();
	}

	//--------- < code to load xml >------------------------------

	template<typename P>
	inline Xml Persist<P>::loadXml()
	{
		if (!FileSystem::Directory::exists(xmlPath)) {
			FileSystem::Directory::create(xmlPath);
			std::ofstream outFile(fileName);
			outFile.close();
		}
		std::stringstream buffer;
		std::ifstream file(fileName.c_str());
		buffer << file.rdbuf();
		std::string Xml = buffer.str();
		return Xml;
	}
	
	//----< does the shard key collection contain key? >-----------------

	/*template<typename P>
	bool Persist<P>::containsKey(const Key& key)
	{
		Keys::iterator start = shardKeys_.begin();
		Keys::iterator end = keys_.end();
		return std::find(start, end, key) != end;
	}*/
	//----< retrieve database from XML string >--------------------------
	/*
	* - Will clear db and reload if augment is false
	*/
	template<typename P>
	bool Persist<P>::fromXml(const Xml & xml, bool augment)
	{
		XmlProcessing::XmlDocument doc(xml);
		if (!augment)db_.dbStore().clear();
		std::vector<Sptr> pRecords = doc.descendents("dbRecord").select();
		for (auto pRecord : pRecords){
			Key key;DbElement<P> elem;P pl;
			std::vector<Sptr> pChildren = pRecord->children();
			for (auto pChild : pChildren){
				if (pChild->tag() == "key")
					key = pChild->children()[0]->value();
				else{
					std::vector<Sptr> pValueChildren = pChild->children();
					std::string valueOfTextNode;
					for (auto pValueChild : pValueChildren){
						std::string tag = pValueChild->tag();
						if (pValueChild->children().size() > 0)
							valueOfTextNode = pValueChild->children()[0]->value();
						else
							valueOfTextNode = "";
						if (tag == "name")elem.name(valueOfTextNode);
						else if (tag == "author")elem.author(valueOfTextNode);
						else if (tag == "description")elem.descrip(valueOfTextNode);
						else if (tag == "dateTime")elem.dateTime(valueOfTextNode);
						else if (tag == "status"){
							if (valueOfTextNode == "Close")
								elem.setStatus(Close);
							else
								elem.setStatus(Open);
						}
						else if (tag == "children"){
							for (auto pChild : pValueChild->children()){
								valueOfTextNode = pChild->children()[0]->value();
								elem.children().push_back(valueOfTextNode);
							}
						}
						else if (tag == "payload"){
							pl = PayLoad::fromXmlElement(pValueChild);
							elem.payLoad(pl);
						}
					}
				}
				db_[key] = elem;
			}
		}
		return true;
	}
}