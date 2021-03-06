/////////////////////////////////////////////////////////////////////////
// ServerPrototype.cpp - Console App that processes incoming messages  //
// ver 1.1                                                             //
// Jim Fawcett, CSE687 - Object Oriented Design, Spring 2018           //
/////////////////////////////////////////////////////////////////////////

#include "ServerPrototype.h"
#include "../FileSystem-Windows/FileSystemDemo/FileSystem.h"
#include "../Process/Process/Process.h"
#include "../Repository/Repository.h"
#include <chrono>

namespace MsgPassComm = MsgPassingCommunication;

using namespace Repository;
using namespace FileSystem;
using Msg = MsgPassingCommunication::Message;

//----< return name of every file on path >----------------------------

Files Server::getFiles(const Repository::SearchPath& path)
{
  return Directory::getFiles(path);
}
//----< return name of every subdirectory on path >--------------------

Dirs Server::getDirs(const Repository::SearchPath& path)
{
  return Directory::getDirectories(path);
}

namespace MsgPassingCommunication
{
  // These paths, global to MsgPassingCommunication, are needed by 
  // several of the ServerProcs, below.
  // - should make them const and make copies for ServerProc usage

  std::string sendFilePath;
  std::string saveFilePath;

  //----< show message contents >--------------------------------------
  template<typename T>
  void show(const T& t, const std::string& msg)
  {
    std::cout << "\n  " << msg.c_str();
    for (auto item : t)
    {
      std::cout << "\n    " << item.c_str();
    }
  }

  //----< test ServerProc simply echos message back to sender >--------
  std::function<Msg(Msg)> echo = [](Msg msg) {
    Msg reply = msg;
    reply.to(msg.from());
    reply.from(msg.to());
    return reply;
  };

  //-------------------------------------------<connection proc>---------------------------------------------
  std::function<Msg(Msg)> connection = [](Msg msg) {
	  Msg reply = msg;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.attribute("message", "Connected");
	  return reply;
  };

  //--------------------------------------------<readFile proc>---------------------------------------------------
  std::function<Msg(Msg)> readFile = [](Msg msg) {
	  Msg reply = msg;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.remove("content");
	  reply.attribute("content", "File sent from repository server| File name: " + msg.value("filename"));
	  return reply;
  };

  //---------------------------------<helper function to perform checkIn>------------------------------------
  void performCheckIn(Msg msg)
  {
	  int totalDep, totalCat;
	  NoSqlDb::Children depedencyList;
	  NoSqlDb::Categories categoryList;
	  NoSqlDb::Status status = Open;
	  string namespace_ = msg.value("namespace");
	  string description = msg.value("description");
	  string filePath = msg.value("filePath");
	  string checkInStatus = msg.value("checkInStatus");
	  string fileName = msg.value("sendingFile");
	  if (checkInStatus == "Close")status = Close;
	  totalDep = std::stoi(msg.value("totalDep"));
	  totalCat = std::stoi(msg.value("totalCat"));
	  for (int i = 0; i < totalDep; i++) {
		  depedencyList.push_back(msg.value("dep" + to_string(i)));
	  }
	  for (int i = 0; i < totalCat; i++) {
		  categoryList.push_back(msg.value("cat" + to_string(i)));
	  }
	  DbCore<PayLoad> db_;
	  Persist<PayLoad> persist(db_);
	  Xml xml = persist.loadXml();
	  if (xml.size() != 0) {
		  persist.fromXml(xml);
	  }
	  Repo<PayLoad> Rp(db_);
	  Rp.checkIn(namespace_, fileName, categoryList, status, depedencyList);
	  xml = persist.toXml();
	  persist.saveXml(xml);
  }

  //-----------------------------------<checkIn proc>----------------------------------------------------

  std::function<Msg(Msg)> checkIn = [](Msg msg) {
	  performCheckIn(msg);
	  msg.show();
	  Msg reply;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.command("checkIn");
	    reply.attribute("message", "check-in completed");
	  return reply;
  };
  
  //--------------------------------------<checkOut proc>-------------------------------------------------
  std::function<Msg(Msg)> checkOut = [](Msg msg) {
	  Msg reply;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.command("checkOut");
	  reply.attribute("message", "received check-out message");
	  DbCore<PayLoad> db_;
	  Persist<PayLoad> persist(db_);
	  Xml xml = persist.loadXml();
	  if (xml.size() != 0) {
		  persist.fromXml(xml);
	  }
	  string path = msg.value("path");
	  int pos = path.find_last_of("/");
	  path.erase(pos);
	  pos = path.find_last_of("/");
	  string namespace_ = path.substr(pos + 1);
	  string parentKey = namespace_ + "::" + msg.value("fileName");
	  DbElement<PayLoad> elem = db_[parentKey];
	  Children children = elem.children();
	  int count = 0;
	  reply.attribute("parentFile", msg.value("fileName"));
	  reply.attribute("parentPath", msg.value("path"));
	  for (auto child : children) {
		  int pos1 = child.find_first_of("::");
		  string fileName = child.substr(pos1+2, child.length());

		  reply.attribute("fileName" + to_string(count), fileName);
		  reply.attribute("path"+ to_string(count), path);
		  count++;
	  }
	  reply.attribute("depCount", to_string(count));
	  return reply;
  };

  //--------------------------------------------<viewMetadata proc>--------------------------------------
  std::function<Msg(Msg)> viewMetadata = [](Msg msg) {
	  Msg reply = msg;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.command("viewMetadata");
	  DbCore<PayLoad> db_;
	  DbCore<PayLoad>::iterator itr1;
	  Persist<PayLoad> persist(db_);
	  Xml xml = persist.loadXml();
	  if (xml.size() != 0) {
		  persist.fromXml(xml);
	  }
	  DbElement<PayLoad> elem=db_[msg.value("key")];
	  PayLoad payload = elem.payLoad();
	  Children depedencies = elem.children();
	  Categories categories = payload.categories();
	  string cat;
	  string dep;
	  Status status = elem.getStatus();
	  string status_= status == Open ? "Open" : "Close";
	  int catSize = categories.size();
	  int depSize = depedencies.size();
	  for (int i = 0; i < catSize; i++) {
		  cat += categories[i];
		  cat += ',';
	  }
	  reply.attribute("description", elem.descrip());
	  reply.attribute("dateTime", elem.dateTime());
	  reply.attribute("path", payload);
	  reply.attribute("category", cat);
	  reply.attribute("status", status_);
	  for (int i = 0; i < depSize; i++)
	  {
		  dep += depedencies[i];
		  dep += ',';
	  }
	  reply.attribute("depedencies", dep);
	  return reply;
  };

  //----< getFiles ServerProc returns list of files on path >----------

  std::function<Msg(Msg)> getFiles = [](Msg msg) {
    Msg reply;
    reply.to(msg.from());
    reply.from(msg.to());
    reply.command("getFiles");
    std::string path = msg.value("path");
    if (path != "")
    {
      std::string searchPath = storageRoot;
      if (path != ".")
        searchPath = searchPath + "\\" + path;
      Files files = Server::getFiles(searchPath);
      size_t count = 0;
      for (auto item : files)
      {
        std::string countStr = Utilities::Converter<size_t>::toString(++count);
        reply.attribute("file" + countStr, item);
      }
    }
    else
    {
      std::cout << "\n  getFiles message did not define a path attribute";
    }
    return reply;
  };

  //----< getDirs ServerProc returns list of directories on path >-----

  std::function<Msg(Msg)> getDirs = [](Msg msg) {
    Msg reply;
    reply.to(msg.from());
    reply.from(msg.to());
    reply.command("getDirs");
    std::string path = msg.value("path");
    if (path != "")
    {
      std::string searchPath = storageRoot;
      if (path != ".")
        searchPath = searchPath + "\\" + path;
      Files dirs = Server::getDirs(searchPath);
      size_t count = 0;
      for (auto item : dirs)
      {
        if (item != ".." && item != ".")
        {
          std::string countStr = Utilities::Converter<size_t>::toString(++count);
          reply.attribute("dir" + countStr, item);
        }
      }
    }
    else
    {
      std::cout << "\n  getDirs message did not define a path attribute";
    }
    return reply;
  };

  //----< sendFile ServerProc sends file to requester >----------------
  /*
  *  - Comm sends bodies of messages with sendingFile attribute >------
  */
  std::function<Msg(Msg)> sendFile = [](Msg msg) {
    Msg reply;
    reply.to(msg.from());
    reply.from(msg.to());
    reply.command("sendFile");
    reply.attribute("sendingFile", msg.value("fileName"));
    reply.attribute("fileName", msg.value("fileName"));
    reply.attribute("verbose", "blah blah");
    std::string path = msg.value("path");
    if (path != "")
    {
      std::string searchPath = storageRoot;
      if (path != "." && path != searchPath)
        searchPath = searchPath + "\\" + path;
      if (!FileSystem::Directory::exists(searchPath))
      {
        std::cout << "\n  file source path does not exist";
        return reply;
      }
      std::string filePath = searchPath + "/" + msg.value("fileName");
      std::string fullSrcPath = FileSystem::Path::getFullFileSpec(filePath);
      std::string fullDstPath = sendFilePath;
      if (!FileSystem::Directory::exists(fullDstPath))
      {
        std::cout << "\n  file destination path does not exist";
        return reply;
      }
      fullDstPath += "/" + msg.value("fileName");
      FileSystem::File::copy(fullSrcPath, fullDstPath);
    }
    else
    {
      std::cout << "\n  getDirs message did not define a path attribute";
    }
    return reply;
  };

  //----< analyze code on current server path >--------------------------
  /*
  *  - Creates process to run CodeAnalyzer on specified path
  *  - Won't return until analysis is done and logfile.txt
  *    is copied to sendFiles directory
  */
  std::function<Msg(Msg)> codeAnalyze = [](Msg msg) {
    Msg reply;
    reply.to(msg.from());
    reply.from(msg.to());
    reply.command("sendFile");
    reply.attribute("sendingFile", "logfile.txt");
    reply.attribute("fileName", "logfile.txt");
    reply.attribute("verbose", "blah blah");
    std::string path = msg.value("path");
    if (path != "")
    {
      std::string searchPath = storageRoot;
      if (path != "." && path != searchPath)
        searchPath = searchPath + "\\" + path;
      if (!FileSystem::Directory::exists(searchPath))
      {
        std::cout << "\n  file source path does not exist";
        return reply;
      }
      Process p;
      p.title("test application");
      std::string appPath = "CodeAnalyzer.exe";
      p.application(appPath);

      std::string cmdLine = "CodeAnalyzer.exe ";
      cmdLine += searchPath + " ";
      cmdLine += "*.h *.cpp /m /r /f";
      p.commandLine(cmdLine);

      std::cout << "\n  starting process: \"" << appPath << "\"";
      std::cout << "\n  with this cmdlne: \"" << cmdLine << "\"";

      CBP callback = []() { std::cout << "\n  --- child process exited ---"; };
      p.setCallBackProcessing(callback);

      if (!p.create())
      {
        std::cout << "\n  can't start process";
      }
      p.registerCallback();

      std::string filePath = searchPath + "\\" + /*msg.value("codeAnalysis")*/ "logfile.txt";
      std::string fullSrcPath = FileSystem::Path::getFullFileSpec(filePath);
      std::string fullDstPath = sendFilePath;
      if (!FileSystem::Directory::exists(fullDstPath))
      {
        std::cout << "\n  file destination path does not exist";
        return reply;
      }
      fullDstPath += std::string("\\") + /*msg.value("codeAnalysis")*/ "logfile.txt";
      FileSystem::File::copy(fullSrcPath, fullDstPath);
    }
    else
    {
      std::cout << "\n  getDirs message did not define a path attribute";
    }
    return reply;
  };

  //----------------------------<shows database>---------------------------------------------------------
  std::function<Msg(Msg)> displayDb = [](Msg msg) {
	  Msg reply;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.command("displayDb");
	  DbCore<PayLoad> db_;
	  DbCore<PayLoad>::iterator itr1;
	  Persist<PayLoad> persist(db_);
	  Xml xml = persist.loadXml();
	  if (xml.size() != 0) {
		  persist.fromXml(xml);
	  }
	  int count = 0;
	  PayLoad pl;
	  NoSqlDb::Categories categoryList;
	  NoSqlDb::Children depedencyList;
	  for (itr1 = db_.begin(); itr1 != db_.end(); ++itr1) {
		  int catCount = 0, depCount = 0;
		  string key = itr1->first;
		  string  fileName = itr1->second.name();
		  string description = itr1->second.descrip();
		  string dateTime = itr1->second.dateTime();
		  Status status = itr1->second.getStatus();
		  string status_ = status == Open ? "Open" : "Close";
		  pl= itr1->second.payLoad();
		  categoryList = pl.categories();
		  depedencyList = itr1->second.children();
		  catCount = categoryList.size();
		  depCount = depedencyList.size();
		  reply.attribute("catCount"+std::to_string(count),std::to_string( catCount));
		  reply.attribute("depCount"+std::to_string(count), std::to_string(depCount));
		  string path = pl.value();
		  reply.attribute("key" + std::to_string(count), key);
		  reply.attribute("fileName" + std::to_string(count),fileName);
		  reply.attribute("dateTime" + std::to_string(count), dateTime);
		  reply.attribute("description" + std::to_string(count), description);
		  reply.attribute("path" + std::to_string(count), path);
		  reply.attribute("status"+std::to_string(count),status_);
		  for (int i = 0; i < catCount; i++) {
			  reply.attribute("cat" + std::to_string(count) + "_"+ std::to_string(i), categoryList[i]);
		  }
		  for (int i = 0; i < depCount; i++) {
			  reply.attribute("dep" + std::to_string(count) + "_"+ std::to_string(i), depedencyList[i]);
		  }
		  count++;
	  }
	  reply.attribute("dbCount", std::to_string(count));
	  return reply;
  };

  //----------------<converter for string to vector>--------------------------------------
  std::vector<std::string>stringToVec(std::string value) {

	  std::vector<std::string> values;
	  std::stringstream keys(value);
	  while (keys.good())
	  {
		  std::string substr;
		  getline(keys, substr, '|');
		  values.push_back(substr);
	  }
	  return values;
  }

  //------------------<for versions query>----------------------------------------------
  vector<string>getVersionsFile(string fileName)
  {
	  vector<string>tempKeys;
	  DbCore<PayLoad> db_;
	  Persist<PayLoad> persist(db_);
	  Xml xml = persist.loadXml();
	  if (xml.size() != 0) {
		  persist.fromXml(xml);
	  }
	  for (auto item : db_) {
		  string key = item.first;
		  if (key.find(fileName) != std::string::npos)
			  tempKeys.push_back(key);
	  }
	  return tempKeys;
  }

  //-------------------------------------------------<proc for querying Db>---------------------------------------
   vector<string> queryDb(Msg msg)
  {
	  vector<string>keys; vector<string>tempKeys;
	  DbCore<PayLoad> db_;Conditions<PayLoad>cond;
	  Persist<PayLoad> persist(db_);
	  Xml xml = persist.loadXml();
	  if (xml.size() != 0) {
		  persist.fromXml(xml);
	  }
	  Query<PayLoad> q1(db_);

	  if (msg.value("dateTo") != "To" && msg.value("dateFrom") != "From") {
		  int val = q1.allCheck_InsWithinMonthInterval(
			  DateTime(msg.value("dateFrom")),
			  DateTime(msg.value("dateTo")));

		  return vector<string>{std::to_string(val)};
	  }


	  if (msg.value("fileName") != "Enter file Name" && msg.value("fileName") !="" ) {
		  if (msg.value("version") != "Enter version as a numeric value") {
			  std::string fileName = msg.value("fileName") + "." + msg.value("version");
			  cond.name(fileName);
		  }
		  else {
			  cond.name(msg.value("fileName"));
		   }
	  }
	  else if (msg.value("version") != "Enter version as a numeric value" && msg.value("version")!="") {
		  std::string filename = "." + msg.value("version");
		  vector<string>keys=getVersionsFile(filename);
		  return keys;
	  }
	  if (msg.value("depedencies") != "Enter depedencies" &&msg.value("depedencies") !="" ) {
		  cond.children(stringToVec(msg.value("depedencies")));
	  }
	  if (msg.value("description") != "Enter description" && msg.value("description") !="" )cond.description(msg.value("description"));
	  vector<string>categories;
	  if (msg.value("categories") != "Enter categories" && msg.value("categories") != "") {
		  categories = stringToVec(msg.value("categories"));
		  for (auto cat : categories) {
			  auto hasCategory = [&cat](DbElement<PayLoad>& elem) {
				  return (elem.payLoad()).hasCategory(cat);
			  };
			  q1.select(hasCategory);
			  tempKeys = q1.keys();
			  for (auto key : tempKeys)
				  keys.push_back(key);
		  }
		  return keys;
	  }
	  if (msg.value("fileName")== "Enter file Name"&& msg.value("depedencies") == "Enter depedencies"&&msg.value("description") == "Enter description"
		  && msg.value("categories") == "Enter categories") return tempKeys;

	  tempKeys.clear();
	  q1.select(cond);
	  tempKeys= q1.keys();

	  for (auto key : tempKeys)
		  keys.push_back(key);
	  return keys;
  }

   //-----------------------<proc for query>-------------------------------------------------------------
  std::function<Msg(Msg)> query = [](Msg msg) {
	  Msg reply; reply.to(msg.from());
	  reply.from(msg.to());
	  reply.command("query");
	  vector<string>keys=queryDb(msg);
	  DbCore<PayLoad> db_;
	  Conditions<PayLoad>cond;
	  Persist<PayLoad> persist(db_);
	  Xml xml = persist.loadXml();
	  if (xml.size() != 0) {
		  persist.fromXml(xml);
	  }
	  int count = 0;
	  PayLoad pl;
	  NoSqlDb::Categories categoryList;
	  NoSqlDb::Children depedencyList;
	  for(auto key:keys){
		  int catCount = 0, depCount = 0;
		  DbElement<PayLoad>elem = db_[key];
		  string  fileName = elem.name();
		  string description = elem.descrip();
		  string dateTime = elem.dateTime();
		  Status status = elem.getStatus();
		  string status_ = status == Open ? "Open" : "Close";
		  pl = elem.payLoad();
		  categoryList = pl.categories();
		  depedencyList = elem.children();
		  catCount = categoryList.size();
		  depCount = depedencyList.size();
		  reply.attribute("catCount" + std::to_string(count), std::to_string(catCount));
		  reply.attribute("depCount" + std::to_string(count), std::to_string(depCount));
		  string path = pl.value();
		  reply.attribute("key" + std::to_string(count), key);
		  reply.attribute("fileName" + std::to_string(count), fileName);
		  reply.attribute("dateTime" + std::to_string(count), dateTime);
		  reply.attribute("description" + std::to_string(count), description);
		  reply.attribute("path" + std::to_string(count), path);
		  reply.attribute("status" + std::to_string(count), status_);
		  for (int i = 0; i < catCount; i++) {
			  reply.attribute("cat" + std::to_string(count) + "_" + std::to_string(i), categoryList[i]);
		  }
		  for (int i = 0; i < depCount; i++) {
			  reply.attribute("dep" + std::to_string(count) + "_" + std::to_string(i), depedencyList[i]);
		  }
		  count++;
	  }
	  reply.attribute("dbCount", std::to_string(count)); 
	  return reply;
  };

	 //--------------------------<proc for files with no parents>--------------------------------
	  std::function<Msg(Msg)>filesWithNoParent= [](Msg msg) {
	  Msg reply;
	  reply.to(msg.from());
	  reply.from(msg.to());
	  reply.command("filesWithNoParent");
	  unordered_map < string,DbElement<PayLoad>> FilesNoParent;
	  DbCore<PayLoad> db_;
	  Persist<PayLoad> persist(db_);
	  Xml xml = persist.loadXml();
	  if (xml.size() != 0) {
		  persist.fromXml(xml);
		  for (auto item : db_) {
			  FilesNoParent.insert({ item.first,item.second });
		  }
	  }
		  for (auto value : FilesNoParent) {
			  string key = value.first;
			  DbElement<PayLoad> elem = value.second;
			  Children depedency = elem.children();
			  if (depedency.size() != 0) {

				  for (auto dep : depedency)
				  {
					  FilesNoParent.erase(dep);
				  }
			  }
		  }
		  int count = 0;
		  PayLoad pl;
		  NoSqlDb::Categories categoryList;
		  NoSqlDb::Children depedencyList;
		  for (auto item : FilesNoParent) {
			  int catCount = 0, depCount = 0;
			  DbElement<PayLoad>elem =item.second;
			  string key = item.first;
			  string  fileName = elem.name();
			  string description = elem.descrip();
			  string dateTime = elem.dateTime();
			  Status status = elem.getStatus();
			  string status_ = status == Open ? "Open" : "Close";
			  pl = elem.payLoad();
			  categoryList = pl.categories();
			  depedencyList = elem.children();
			  catCount = categoryList.size();
			  depCount = depedencyList.size();
			  reply.attribute("catCount" + std::to_string(count), std::to_string(catCount));
			  reply.attribute("depCount" + std::to_string(count), std::to_string(depCount));
			  string path = pl.value();
			  reply.attribute("key" + std::to_string(count), key);
			  reply.attribute("fileName" + std::to_string(count), fileName);
			  reply.attribute("dateTime" + std::to_string(count), dateTime);
			  reply.attribute("description" + std::to_string(count), description);
			  reply.attribute("path" + std::to_string(count), path);
			  reply.attribute("status" + std::to_string(count), status_);
			  for (int i = 0; i < catCount; i++) {
				  reply.attribute("cat" + std::to_string(count) + "_" + std::to_string(i), categoryList[i]);
			  }
			  for (int i = 0; i < depCount; i++) {
				  reply.attribute("dep" + std::to_string(count) + "_" + std::to_string(i), depedencyList[i]);
			  }
			  count++;
		  }
		  reply.attribute("dbCount", std::to_string(count));
		  return reply;
};
  
	  //--------<function to initialize server>----------------------------
  void initializeServer()
  {
	   SetConsoleTitleA("Project4 Server Console");
	   std::cout << "\n  Testing Server Prototype";
	   std::cout << "\n ==========================";
	   std::cout << "\n";
  }
}


using namespace MsgPassingCommunication;

int main()
{
  initializeServer();
  sendFilePath = FileSystem::Directory::createOnPath("./SendFiles");
  saveFilePath = FileSystem::Directory::createOnPath("./SaveFiles");
  Server server(serverEndPoint, "ServerPrototype");
  MsgPassingCommunication::Context* pCtx = server.getContext();
  pCtx->saveFilePath = saveFilePath;
  pCtx->sendFilePath = sendFilePath;
  server.start();
  std::cout << "\n  testing getFiles and getDirs methods";
  std::cout << "\n --------------------------------------";
  Files files = server.getFiles();
  show(files, "Files:");
  Dirs dirs = server.getDirs();
  show(dirs, "Dirs:");
  std::cout << "\n";
  std::cout << "\n  testing message processing";
  std::cout << "\n ----------------------------";
  server.addMsgProc("echo", echo);
  server.addMsgProc("getFiles", getFiles);
  server.addMsgProc("getDirs", getDirs);
  server.addMsgProc("sendFile", sendFile);
  server.addMsgProc("codeAnalyze", codeAnalyze);
  server.addMsgProc("serverQuit", echo);
  server.addMsgProc("connection", connection);	
  server.addMsgProc("sendFile", sendFile);
  server.addMsgProc("checkIn", checkIn);
  server.addMsgProc("checkOut", checkOut);
  server.addMsgProc("viewMetadata", viewMetadata);
  server.addMsgProc("displayDb", displayDb);
  server.addMsgProc("query", query);
  server.addMsgProc("filesWithNoParent", filesWithNoParent);
  server.processMessages();
  Msg msg(serverEndPoint, serverEndPoint);  
  std::cout << "\n  press enter to exit\n";
  std::cin.get();
  std::cout << "\n";
  msg.command("serverQuit");
  server.stop();
  return 0;
}

