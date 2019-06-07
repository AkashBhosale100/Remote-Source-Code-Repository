////////////////////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs : This package implements GUI handling functionality            //
// ver 1.0                                                                            //
//                                                                                    //
//Language:      C#                                                                   //
//Platform    : Lenovo Z50, Win 10, Visual Studio 2017                                //
//Application : CSE-687 OOD Project 3                                                 //
//Author      : Akash Bhosale	, aabhosal@syr.edu                                    //
//Source      : Prof. Jim Fawcett, CSE 687 -Object Oriented Design                    //
//Environment : C# Console                                                            //
////////////////////////////////////////////////////////////////////////////////////////

/*
 * Package Operation
 * ------------------------
 * This file implements functions for handling user interface
 * 
 * Required files:
 *----------------
 *MainWindow.xaml
 *CsMessage.h
 *Translater.h
 *
 * Build Process
 *--------------
 *devenv Project 1.sln /rebuild debug
 *
 * 
 * Maintainance History
 * ------------------------
 * ver 1.1 May 01,2018
 * ver 1.0, April 10, 2018
 *
 * 
 * public interface
 * ------------------------
 *MainWindow()----Initializes UI component
 *processMessage()-----processes incomming message on child thread
 *clearDirs()---------clears dir list
 *clearFiles()--------clears file list
 *add Dir()-----------add Dir
 *insert Parent()------adds parent 
 *DispatcherLoadGetFiles()------dispatcher for getting files
 *addClientProc()--------------add client messaging for message key
 *addFile()-------------------adds file to fileList
 *DispatcherLoadGetFiles()------ load getFiles processing into dispatcher dictionary
 *ConnectButton_Click()-----------event for button click
 *Window_Loaded(object sender, RoutedEventArgs e)---start Comm, fill window display with dirs and files 
 *DispatcherViewMetadata()---------Dispatcher for viewing metadata  
 *DispatcherCheckOutComplete()-------Check Out dispatcher
 *DispatcherCheckInComplete()---------Check In dispatcher
 * GetVersionBtn_Click()-------------Function to get versions
 *startTests()-------------------Performs automated tests
 *Test1()-----------------Implements requirement 1 functions
 *Test2()-----------------Implements requirement 2 functions
 *Test2a()-----------------Implements requirement functions
 *Test2b()----------------Implements requirement functions
 *TestR1()----------------Implements requirement functions
 *TestR2()----------------Implements requirement functions
 *TestR3()----------------Implements requirement functions
 *TestR4()----------------Implements requirement functions
 *viewFileContent()-------Implements requirement for viewing file content
 *populateDep()-----------Implements requirement for adding data to depedency list
 *show(CsMsessage)--------Implements functionality for showing msg
 *InitiateBrowse()--------Implements initiate browse functionality 
 *sendButtonClick()-------Implements send button functionality
 *depedenciesView()-------Implements functionality to view depedencies
 *statusView()------------Implements functionality to view status
 *InitiateCheckIn()-------Implements functionality to check in file 
 *InitiateCheckOut()------Starts check out process
 *PopulateList()----------Populates dir and files list
 *getFiles()--------------Retrieves files() 
 *getDirs()---------------Retrieves dirs()
 *AddDepHandler()---------add depedency handler
 *AddCategory_Click()-----adds new category 
 *DirList_MouseDoubleClick()----------event for directory list double click
 *checkInSetup()------------prepares for check In
 *DispatcherLoadGetFile()--display received file
 *DispatcherMakeConnection()---make Connection function
 *DisplayQueryConsole()--display query on console
 *DispatcherFilesNoParent()----dispatcher for files with no parent
 *startTests()-----start test sequence
 *checkOutHandler()-----------Handles checkOut functions
 *generateQueryMsg()---generates a Query msg
 *viewMetadataSetup()----setup function for view Metadata tab
 *demoCheckIn()------------function to perform demoCheckIn operations
 *sendDbShowMsg()--------send Db show msg
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes; 
using System.Threading;
using System.Collections.ObjectModel;
using MsgPassingCommunication;
using System.IO;

namespace GUI
{
    public partial class MainWindow : Window
    {
        public class StringClass
        {
            public string TheText { get; set; }
            public bool IsSelected { get; set; }
            public bool InList { get; set; }
        }
        public ObservableCollection<StringClass> TheList { get; set; }
        public ObservableCollection<StringClass> CatList { get; set; }
        public ObservableCollection<StringClass> VersionList { get; set; }
        private static readonly object ConsoleWriterLock = new object();

        public MainWindow()
        {

            Console.Title = "GUI Window";
            TheList = new ObservableCollection<StringClass>();
            CatList = new ObservableCollection<StringClass>();
            VersionList = new ObservableCollection<StringClass>();
            InitializeComponent();
            this.DataContext = this;
        }
        private Stack<string> pathStack_ = new Stack<string>();
        private Translater translater;
        private CsEndPoint endPoint_;
        private Thread rcvThrd = null;
        private Dictionary<string, Action<CsMessage>> dispatcher_
          = new Dictionary<string, Action<CsMessage>>();
        private bool connected_ = false;
        private List<string> ClientFiles = new List<string>();
        private List<string> ClientDirs = new List<string>();
        private List<string> CheckInFiles = new List<string>();
        private HashSet<string> depedencyList = new HashSet<string>();
        private HashSet<string> catLists = new HashSet<string>();
        private List<string> DirList1 = new List<string>();
        private List<string> FileLists = new List<string>();
        private string saveFilesPath = "../CppCommWithFileXfer/saveFiles";
        private string sendFilesPath = "../CppCommWithFileXfer/sendFiles";
        bool addDep = false;
        bool ChkOutTab = false;
        bool viewMetadataTab = false;

        //----< process incoming messages on child thread >----------------
        private void processMessages()
        {
            ThreadStart thrdProc = () => {
                while (true)
                {
                    CsMessage msg = translater.getMessage();
                    string msgId = msg.value("command");
                    if (dispatcher_.ContainsKey(msgId))
                        dispatcher_[msgId].Invoke(msg);
                }
            };
            rcvThrd = new Thread(thrdProc);
            rcvThrd.IsBackground = true;
            rcvThrd.Start();
        }
        //----< function dispatched by child thread to main thread >-------

        private void clearDirs()
        {
            DirList.Items.Clear();
        }
        //----< function dispatched by child thread to main thread >-------

        private void addDir(string dir)
        {
            DirList.Items.Add(dir);
        }
        //----< function dispatched by child thread to main thread >-------

        private void insertParent()
        {
            DirList.Items.Insert(0, "..");
        }
        //----< function dispatched by child thread to main thread >-------

        private void clearFiles()
        {
            FileList1.Items.Clear();
        }


        //---------------------------------------<clear checkOut files>-------------------------------------
        private void clearFilesCheckOut()
        {
            FileListCheckOut.Items.Clear();
        }

        //----< function dispatched by child thread to main thread >-------

        private void addFile(string file)
        {
            FileList1.Items.Add(file);
        }

        //----< add client processing for message with key >---------------

        private void addClientProc(string key, Action<CsMessage> clientProc)
        {
            dispatcher_[key] = clientProc;
        }
        //----< load getDirs processing into dispatcher dictionary >-------

        private void DispatcherLoadGetDirs()
        {
            Action<CsMessage> getDirs = (CsMessage rcvMsg) =>
            {
                Action clrDirs = () => {
                    if (addDep == true)
                        clearDirs1();
                    else clearDirs();
                    if (ChkOutTab == true)
                        clearDirsChkOut();
                    if (viewMetadataTab == true)
                        clearViewDirs();
                };
                Dispatcher.Invoke(clrDirs, new Object[] { });
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("dir"))
                    {
                        Action<string> doDir = (string dir) => {
                            if (addDep == true)
                                addDir1(dir);
                            else addDir(dir);
                            if (ChkOutTab == true)
                                addDirChkOut(dir);
                            if (viewMetadataTab == true)
                                addDirViewMetadata(dir);
                        };
                        Dispatcher.Invoke(doDir, new Object[] { enumer.Current.Value });
                    }
                }
                Action insertUp = () => {
                    if (addDep == true)
                        AddParent();
                    else insertParent();
                    if (ChkOutTab == true) addCheckOutDirParent();
                    if (viewMetadataTab == true) addViewTabParent();
                };
                Dispatcher.Invoke(insertUp, new Object[] { });
            };
            addClientProc("getDirs", getDirs);
        }

        //----< load getFiles processing into dispatcher dictionary >------

        private void DispatcherLoadGetFiles()
        {
            Action<CsMessage> getFiles = (CsMessage rcvMsg) =>
            {
                Action clrFiles = () =>
                {
                    if (addDep == true)
                        clearFiles1();
                    else clearFiles();
                    if (ChkOutTab == true)
                        clearFilesCheckOut();
                    if (viewMetadataTab == true)
                        clearFilesView();
                };
                Dispatcher.Invoke(clrFiles, new Object[] { });
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("file"))
                    {
                        Action<string> doFile = (string file) =>
                        {
                            if (addDep == true)
                                addFile1(file);
                            else addFile(file);
                            if (ChkOutTab == true)
                                addFileCheckOut(file);
                            if (viewMetadataTab == true)
                                addFileViewMetadata(file);
                        };
                        Dispatcher.Invoke(doFile, new Object[] { enumer.Current.Value });
                    }
                }
            };
            addClientProc("getFiles", getFiles);
        }

        //-----------------------------<dispatcher view make connection>--------------------------------------
        private void DispatcherMakeConnection()
        {
            Action<CsMessage> connection = (CsMessage rcvMsg) =>
            {
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("message"))
                    {
                        Action<string> connect = (string status) =>
                        {
                            if (status == "Connected")
                            {
                                connected_ = true;
                                this.MessageText.Text = "Connection Successfull" + "..";
                                CheckIn.IsEnabled = true;
                                CheckOut.IsEnabled = true;
                                Browse.IsEnabled = true;
                                ViewMetadata.IsEnabled = true;
                                QueryTab.IsEnabled= true;
                                this.statusBarText.Text = "Connected..";
                            }
                        };
                        Dispatcher.Invoke(connect, new Object[] { enumer.Current.Value });
                    }
                }
            };
            addClientProc("connection", connection);
        }

        //-------------------------------<dispatcher for check in complete>--------------------------------------------
        private void DispatcherCheckInComplete()
        {
            Action<CsMessage> checkInComplete = (CsMessage rcvMsg) =>
               {
                   var enumer = rcvMsg.attributes.GetEnumerator();
                   while (enumer.MoveNext())
                   {
                       string key = enumer.Current.Key;
                       if (key.Contains("message"))
                       {
                           Action<string> mess = (string value) =>
                              {
                                  this.statusBarText.Text = enumer.Current.Value;
                              };
                           Dispatcher.Invoke(mess, new Object[] { enumer.Current.Value });
                       }
                   }
               };
            addClientProc("checkIn", checkInComplete);
        }

        //---------------------------------<checkOutHandler code>----------------------------------------------------
        private void checkOutHandler(CsMessage rcvMsg)
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "sendFile");
            msg.add("path", rcvMsg.value("parentPath"));
            msg.add("fileName", rcvMsg.value("parentFile"));
            translater.postMessage(msg);
            int count = Int32.Parse(rcvMsg.value("depCount"));
                for(int i = 0; i < count; i++)
            {
                CsMessage newMsg = new CsMessage();
                newMsg.add("to", CsEndPoint.toString(serverEndPoint));
                newMsg.add("from", CsEndPoint.toString(endPoint_));
                newMsg.add("command", "sendFile");
                newMsg.add("path", rcvMsg.value("path" + i.ToString()));
                newMsg.add("fileName", rcvMsg.value("fileName" + i.ToString()));
                translater.postMessage(newMsg);
            }
        }


        //----------------------------------------<dispatcher for check out complete>----------------------------------
        private void DispatcherCheckOutComplete()
        {
            Action<CsMessage> checkOutComplete = (CsMessage rcvMsg) =>
            {
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("message"))
                    {
                        Action<string> mess = (string value) =>
                        {
                            this.statusBarText.Text = enumer.Current.Value;
                           checkOutHandler(rcvMsg);
                        };
                        Dispatcher.Invoke(mess, new Object[] { enumer.Current.Value });
                    }
                }
            };
            addClientProc("checkOut", checkOutComplete);
        }

        //----------------------<Helper function for extracting metadata>------------------------------------

        private void extractMetadata(Dictionary<string,string>.Enumerator enumer,CsMessage rcvMsg)
        {
            enumer = rcvMsg.attributes.GetEnumerator();
            while(enumer.MoveNext())
            {
                string key = enumer.Current.Key;
                switch (key)
                {
                    case "version":
                        Action<string> act4 = (string value) =>
                        {
                            VersionBox.Text = rcvMsg.value(key);
                        };
                        Dispatcher.Invoke(act4, new Object[] { enumer.Current.Value });
                        break;
                    case "status":
                        Action<string> act5 = (string value) =>
                        {
                            StatusViewBox.Text = rcvMsg.value(key);
                        };
                        Dispatcher.Invoke(act5, new Object[] { enumer.Current.Value });
                        break;
                    case "dateTime":
                        Action<string> act6 = (string value) =>
                        {
                            DateTimeViewBox.Text = rcvMsg.value(key);
                        };
                        Dispatcher.Invoke(act6, new Object[] { enumer.Current.Value });
                        break;
                }
            }
        }

        //----------------------------------------------<Dispatcher for viewing metadata>---------------------------------
        private void DispatcherViewMetadata()
        {
            Action<CsMessage> viewMetadata = (CsMessage rcvMsg) =>
            {
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    switch (key)
                    {
                        case "description":
                            Action<string> act3 = (string value) =>
                            {
                                DescriptionBox.Text = rcvMsg.value(key);
                            };
                            Dispatcher.Invoke(act3, new Object[] { enumer.Current.Value });
                            break;
                        case "depedencies":
                            Action<string> act7 = (string value) =>
                            {
                                DepedencyViewBox.Text = rcvMsg.value(key).Trim(',');
                            };
                            Dispatcher.Invoke(act7, new Object[] { enumer.Current.Value });
                            break;
                        case "category":
                            Action<string> act8 = (string value) =>
                            {
                                CategoryViewBox.Text = rcvMsg.value(key).Trim(',');
                            };
                            Dispatcher.Invoke(act8, new Object[] { enumer.Current.Value });
                            break;
                        default:
                            extractMetadata(enumer, rcvMsg);
                            break;
                    }
                }
            };
            addClientProc("viewMetadata", viewMetadata);
        }

        //--------------------------<show header on console>--------------------------------------------------------
        void showHeader()
        {
            string Heading = "\nKey \t\t\t\t" + "FileName \t\t\t" + "DateTime \t\t     " + "Description \t\t\t" + "Payload \t\t\t\t  " + "Status";
            Console.WriteLine(Heading);
            Console.Write("\n------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
        }

        //------------------------------------<show Db>---------------------------------------------------------------------

        void showDb(CsMessage rcvMsg)
        {
            int dbCount = Int32.Parse(rcvMsg.value("dbCount"));
            List<string> catList = new List<string>();
            List<string> depList = new List<string>();
            for (int i = 0; i < dbCount; i++)
            {
                int catCount = Int32.Parse(rcvMsg.value("catCount" + i.ToString()));
                int depCount = Int32.Parse(rcvMsg.value("depCount" + i.ToString()));
                string key = rcvMsg.value("key" + i.ToString());
                string fileName = rcvMsg.value("fileName" + i.ToString());
                string dateTime = rcvMsg.value("dateTime" + i.ToString());
                string description = rcvMsg.value("description" + i.ToString());
                string path = rcvMsg.value("path" + i.ToString());
                string status_ = rcvMsg.value("status" + i.ToString());
                for (int j = 0; j < catCount; j++)
                {
                    catList.Add(rcvMsg.value("cat" + i.ToString() + "_" + j.ToString()));
                }
                for (int k = 0; k < depCount; k++)
                {
                    depList.Add(rcvMsg.value("dep" + i.ToString() + "_" + k.ToString()));
                }
                Console.Write(Environment.NewLine);
                Console.Write(key);
                Console.Write(fileName.PadLeft(15, ' '));
                Console.Write(dateTime.PadLeft(40, ' '));
                Console.Write(description.Substring(0, 20).PadLeft(38, ' '));
                Console.Write(path.Substring(0, 30).PadLeft(40, ' '));
                Console.Write(status_.PadLeft(20, ' '));
                Console.Write("\nCategories:");
                foreach (string cat in catList)
                {
                    Console.Write(cat + " ");
                }
                Console.Write("\nDepedecies:");
                foreach (string dep in depList)
                {
                    Console.Write(dep + " ");
                }
                catList.Clear();
                depList.Clear();
                Console.Write(Environment.NewLine);
            }
        }
        //---------------------------------------<Dispatcher for displaying db>--------------------------------------
        private void DispatcherDisplayDb()
        {
            Action<CsMessage> displayDb = (CsMessage rcvMsg) =>
            {
                var enumer = rcvMsg.attributes.GetEnumerator();
                showHeader();
                string key = enumer.Current.Key;
                Action<string> message = (string value) =>
                {
                    this.statusBarText.Text = "Database shown on GUI console";
                };
                Dispatcher.Invoke(message, new Object[] { enumer.Current.Value });
                showDb(rcvMsg);
            };
            addClientProc("displayDb", displayDb);
        }

        //----------------------------<display query on console>---------------------------------------------------------------------------------------------------
        private void DisplayQueryConsole(string fileName,string dateTime,string description,string path,string status_, List<string> catList, List<string> depList)
        {
            Console.Write(Environment.NewLine);
            Console.Write(fileName);
            Console.Write(dateTime.PadLeft(40, ' '));
            Console.Write(description.Substring(0, 30).PadLeft(38, ' '));
            Console.Write(path.Substring(0, 30).PadLeft(40, ' '));
            Console.Write(status_.PadLeft(20, ' '));
            Console.Write("\nCategories:");
            foreach (string cat in catList)
            {
                Console.Write(cat + " ");
            }
            Console.Write("\nDepedecies:");
            foreach (string dep in depList)
            {
                Console.Write(dep + " ");
            }
            Console.Write(Environment.NewLine);
        }
        
        //----------------------------------<show Header on console>-----------------------------------------------
        public void showHeader1()
        {
            Console.Write(Environment.NewLine);
            string Heading="FileName \t\t\t" + "DateTime \t\t     " + "Description \t\t\t" + "Payload \t\t\t\t  " + "Status";
            Console.WriteLine(Heading);
            Console.Write("\n------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
        }


        //-------------------------------------<show Query Db Message>--------------------------------------------------
        private void showQueryDb(CsMessage rcvMsg)
        {
            QueryOutputBox.Clear();
            string Heading = "FileName\t" + "DateTime\t\t " + "Description \t\t" + "Payload\t\t" + "Status";
            HeadingBox.Text = Heading;
            showHeader1();
            int dbCount = Int32.Parse(rcvMsg.value("dbCount"));
            List<string> catList = new List<string>();
            List<string> depList = new List<string>();
            for (int i = 0; i < dbCount; i++)
            {
                int catCount = Int32.Parse(rcvMsg.value("catCount" + i.ToString()));
                int depCount = Int32.Parse(rcvMsg.value("depCount" + i.ToString()));
                string key = rcvMsg.value("key" + i.ToString());
                string fileName = rcvMsg.value("fileName" + i.ToString());
                string dateTime = rcvMsg.value("dateTime" + i.ToString());
                string description = rcvMsg.value("description" + i.ToString());
                string path = rcvMsg.value("path" + i.ToString());
                string status_ = rcvMsg.value("status" + i.ToString());
                for (int j = 0; j < catCount; j++)
                {
                    catList.Add(rcvMsg.value("cat" + i.ToString() + "_" + j.ToString()));
                }
                for (int k = 0; k < depCount; k++)
                {
                    depList.Add(rcvMsg.value("dep" + i.ToString() + "_" + k.ToString()));
                }
                DisplayQueryConsole(fileName, dateTime, description, path, status_, catList, depList);
                QueryOutputBox.AppendText(fileName);
                QueryOutputBox.AppendText(dateTime.PadLeft(40, ' '));
                QueryOutputBox.AppendText(description.Substring(0, 20).PadLeft(32, ' '));
                QueryOutputBox.AppendText(path.Substring(0, 30).PadLeft(35, ' '));
                QueryOutputBox.AppendText((status_.PadLeft(20, ' ')));
                QueryOutputBox.AppendText("\nCategories:");
                foreach (string cat in catList)
                {
                    QueryOutputBox.AppendText(cat + " ");
                }
                QueryOutputBox.AppendText("\nDepedecies:");
                foreach (string dep in depList)
                {
                    QueryOutputBox.AppendText(dep + " ");
                }
                QueryOutputBox.AppendText(Environment.NewLine);
                catList.Clear();
                depList.Clear();
            }
        }

        //-------------------------------------------------<Dispatcher for Query Db>--------------------------------

        private void DispatcherQueryDb()
        {
            Action<CsMessage> queryDb = (CsMessage rcvMsg) =>
            {
                var enumer = rcvMsg.attributes.GetEnumerator();
                string key = enumer.Current.Key;
                Action<string> message = (string value) =>
                {
                    this.statusBarText.Text = "Query shown in above window";
                   
                    showQueryDb(rcvMsg);
                };
                Dispatcher.Invoke(message, new Object[] { enumer.Current.Value });
                
            };
            addClientProc("query", queryDb);
        }

        //--------------------------------------<show Output>--------------------------------------------------
        private void showOutput(CsMessage rcvMsg)
        {
            showQueryDb(rcvMsg);
        }

        //------------------------------------<Files with no parent proc>-------------------------------------------
        private void DispatcherFilesNoParent()
        {
            Action<CsMessage> filesWithNoParent = (CsMessage rcvMsg) =>
            {
                var enumer = rcvMsg.attributes.GetEnumerator();
                string key = enumer.Current.Key;
                Action<string> message = (string value) =>
                {
                    this.statusBarText.Text = "Output shown in above window";
                    showOutput(rcvMsg);
                };
                Dispatcher.Invoke(message, new Object[] { enumer.Current.Value });
            };
            addClientProc("filesWithNoParent", filesWithNoParent);
        }


        //--------------------------------------<check in dispatcher>-------------------------------------
        private void checkInDispatcher()
        {
            DispatcherCheckInComplete();
        }

        //----------------------------------<check out dispatcher>------------------------------------------
        private void CheckOutDispatcher()
        {
            DispatcherCheckOutComplete();
        }

        //----< load all dispatcher processing >---------------------------
        private void loadDispatcher()
        {
            DispatcherLoadGetDirs();
            DispatcherLoadGetFiles();
            DispatcherLoadSendFile();
            DispatcherViewMetadata();
            checkInDispatcher();
            CheckOutDispatcher();
            DispatcherDisplayDb();
            DispatcherQueryDb();
            DispatcherFilesNoParent();
        }

        //----< start Comm, fill window display with dirs and files >------

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Console.SetWindowSize(170, 44);
            loadDispatcher();
            endPoint_ = new CsEndPoint();
            endPoint_.machineAddress = "localhost";
            endPoint_.port = 8082;
            translater = new Translater();
            translater.listen(endPoint_);
            processMessages();
            Console.Title = "GUI.exe";
            saveFilesPath = translater.setSaveFilePath("../../../SaveFiles");
            sendFilesPath = translater.setSendFilePath("../../../SendFiles");
            startTests();

            if (!connected_)
                return;
        }

        //----<  getFile processing >------

        private void DispatcherLoadSendFile()
        {
            Action<CsMessage> sendFile = (CsMessage rcvMsg) =>
            {
                Console.Write("\n  processing incoming file");
                string fileName = "";
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("sendingFile"))
                    {
                        fileName = enumer.Current.Value;
                        break;
                    }
                }
                if (ChkOutTab == true)
                {
                    Action<string> updateUi = (string fileNm) => { updateChkOut(fileNm); };
                    Dispatcher.Invoke(updateUi, new object[] { fileName });
                }
                else if (fileName.Length > 0)
                {
                        Action<string> act = (string fileNm) => { showFile(fileNm); };
                        Dispatcher.Invoke(act, new object[] { fileName });
                }
            };
            addClientProc("sendFile", sendFile);
        }


        //--------------------------------------<function to update chkOut>---------------------------------------------
        private void updateChkOut(string fileName)
        {
            statusBarText.Text = "Check Out complete";
            CheckOutPath.Text = System.IO.Path.GetFullPath(System.IO.Path.Combine(saveFilesPath, fileName));
        }

        //----< show file text >-------------------------------------------

        private void showFile(string fileName)
        {
            Paragraph paragraph = new Paragraph();
            string fileSpec = saveFilesPath + "\\" + fileName;
            string fileText = File.ReadAllText(fileSpec);
            paragraph.Inlines.Add(new Run(fileText));
            ViewWindow popUp = new ViewWindow();
            popUp.Title = fileName;
            popUp.codeView.Blocks.Clear();
            popUp.codeView.Blocks.Add(paragraph);
            popUp.Show();
        }

        //----< strip off name of first part of path >---------------------

        private string removeFirstDir(string path)
        {
            string modifiedPath = path;
            int pos = path.IndexOf("/");
            modifiedPath = path.Substring(pos + 1, path.Length - pos - 1);
            return modifiedPath;
        }
        //----< respond to mouse double-click on dir name >----------------

        private void DirList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DirList.SelectedItem == null)
                return;
            string selectedDir = (string)DirList.SelectedItem;
            string path;
            if (selectedDir == "..")
            {
                clearFiles1();
                if (pathStack_.Count > 1)
                    pathStack_.Pop();
                else
                    return;
            }
            else
            {
                clearFiles1();
                path = pathStack_.Peek() + "/" + selectedDir;
                pathStack_.Push(path);
            }

            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "getDirs");
            msg.add("path", pathStack_.Peek());
            translater.postMessage(msg);
            msg.remove("command");
            msg.add("command", "getFiles");
            translater.postMessage(msg);
        }

        //-----------------------------------------<Connect Button Click>----------------------------------------------
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            DispatcherMakeConnection();
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = this.IPAddrName.Text;
            serverEndPoint.port = Int32.Parse(this.PortName.Text);
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "connection");
            translater.postMessage(msg);
            MessageText.Text = "Attempting connection to " + endPoint_.machineAddress + ":" + endPoint_.port + "..";
            msg.remove("command");
        }

        //-------------------------------------<Browse tab function>-----------------------------------
        public void browseTab()
        {
            ChkOutTab = false;
            loadDispatcher();
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            pathStack_.Clear();
            pathStack_.Push("../Storage");
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "getDirs");
            msg.add("path", pathStack_.Peek());
            show(msg);
            translater.postMessage(msg);
            msg.remove("command");
            msg.add("command", "getFiles");
            show(msg);
            translater.postMessage(msg);
            msg.remove("command");
        }

        //-------------------------------<Browse button>----------------------------------------------------------
        private void Browse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            addDep = false;
            ChkOutTab = false;
            browseTab();
        }

        //------------------------------------<Send button event>-----------------------------------------------
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!connected_)
                return;

            Action<CsMessage> echo = (CsMessage rcvMsg) =>
            {
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("message"))
                    {
                        Action<string> mess = (string value) =>
                        {

                        };
                        Dispatcher.Invoke(mess, new Object[] { enumer.Current.Value });
                    }
                }
            };
            addClientProc("echo", echo);
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "echo");
            msg.add("message", this.MessageText.Text);
            translater.postMessage(msg);
        }

        //--------------------------------<function to add parent>-----------------------------
        private void AddParent()
        {
            CLientDirsList.Items.Insert(0, "..");
        }

        private void addCheckOutDirParent()
        {
            DirListCheckOut.Items.Insert(0, "..");
        }
        //----< function dispatched by child thread to main thread >---------------------------

        private void clearFiles1()
        {
            TheList.Clear();
        }
        
        private void clearFilesView()
        {
            ClientFilesListView.Items.Clear();
        }


        //----------------------------------<add parent to view tab>----------------------
        private void addViewTabParent()
        {
            CLientDirsListView.Items.Insert(0, "..");
        }

        //----< function dispatched by child thread to main thread >-------
        private void addFile1(string file)
        {
            string listFile = pathStack_.Peek() + "/" + file;
            if (depedencyList.Contains(listFile))
            {
                TheList.Add(new StringClass { IsSelected = true, TheText = file, InList = false });
            }

            TheList.Add(new StringClass { IsSelected = false, TheText = file, InList = false });
        }

        //-----------------------------------------------------------<Add file to checkOut Window>-----------------------------------
        private void addFileCheckOut(string file)
        {
            FileListCheckOut.Items.Add(file);
        }

        //--------------------------------------------------<add file to view metadata tab>---------------------------------------
        private void addFileViewMetadata(string file)
        {
            ClientFilesListView.Items.Add(file);
        }

        //----------------<function to clear ClientDirsList>--------------------------------
        private void clearDirs1()
        {
            CLientDirsList.Items.Clear();

        }

        //--------------------<Clear Dirs View>-----------------------------------------------------
        private void clearViewDirs()
        {
            CLientDirsListView.Items.Clear();
        }
        private void clearDirsChkOut()
        {
            DirListCheckOut.Items.Clear();
        }

        //----< function dispatched by child thread to main thread >-------
        private void addDir1(string dir)
        {
            CLientDirsList.Items.Add(dir);
        }

        //-----------------------<function to add Dirs to checkout window>-----------------------------
        private void addDirChkOut(string dir)
        {
            DirListCheckOut.Items.Add(dir);
        }

        //-----------------------------<function to add dir to view Metadata window>----------------------

        private void addDirViewMetadata(string dir)
        {
            CLientDirsListView.Items.Add(dir);
        }

        //<----------------------------<Add dep event>-------------------------------
        private void AddDep_Click(object sender, RoutedEventArgs e)
        {
            if (!connected_)
                return;

            addDep = true;
            loadDispatcher();

            UserMessages.Text = "Sending message to server to fetch dirs and files";
            TheList.Clear();
            CLientDirsList.Items.Clear();
            AddParent();
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            pathStack_.Clear();
            pathStack_.Push("../Storage");
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "getDirs");
            msg.add("path", pathStack_.Peek());
            translater.postMessage(msg);
            msg.remove("command");
            msg.add("command", "getFiles");
            translater.postMessage(msg);
            msg.remove("command");
        }

        //---------------------------------<checkIn setup>------------------------------------------------------- 
        private void checkInSetup()
        {
            ConfirmSelectionBtn.IsEnabled = true;
            UserMessages.Text = "Select file to checkIn";
            AddCategory.IsEnabled = true;
            CheckInStatus.IsEnabled = true;
            AddDepBtn.IsEnabled = false;
            CheckInBtn.IsEnabled = false;
            depedencyList.Clear();

            pathStack_.Clear();
            pathStack_.Push("../../../../");
            PopulateList();
            ClientDirs = getDirsList();
            ClientFiles = getFilesList();
            CLientDirsList.Items.Clear();
            TheList.Clear();
            AddParent();
            foreach (string dir in ClientDirs)
            {
                string dirName = new System.IO.DirectoryInfo(dir).Name;
                CLientDirsList.Items.Add(dirName);
            }
            foreach (string file in ClientFiles)
                TheList.Add(new StringClass { IsSelected = false, TheText = file, InList = false });
            populateCategory();
        }

        //----------------------------------<populate category>----------------------------------------------
        private void populateCategory()
        {
            CatList.Clear();
            CatList.Add(new StringClass { IsSelected = false, TheText = "User Interface", InList = false });
            CatList.Add(new StringClass { IsSelected = false, TheText = "File System", InList = false });
            CatList.Add(new StringClass { IsSelected = false, TheText = "Server", InList = false });
            CatList.Add(new StringClass { IsSelected = false, TheText = "Client", InList = false });
            CatList.Add(new StringClass { IsSelected = false, TheText = "Socket Implementer", InList = false });
            CatList.Add(new StringClass { IsSelected = false, TheText = "Repository", InList = false });
            CatList.Add(new StringClass { IsSelected = false, TheText = "MsgPassingComm", InList = false });
            CatList.Add(new StringClass { IsSelected = false, TheText = "Check-in", InList = false });
            CatList.Add(new StringClass { IsSelected = false, TheText = "DateTime", InList = false });
        }


        //-------------------------------------<Check In button>----------------------------------------------------
        private void CheckIn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            checkInSetup();
            
        }

        //------------------------------------<Add depedency>-------------------------------------------------------
        private void addDependency()
        {
            if (CLientDirsList.SelectedItem == null)
                return;
            string selectedDir = (string)CLientDirsList.SelectedItem;
            string path;
            if (selectedDir == "..")
            {
                if (pathStack_.Count > 1)  
                    pathStack_.Pop();
                else
                    return;
            }
            else
            {
                path = pathStack_.Peek() + "/" + selectedDir;
                pathStack_.Push(path);
            }
            
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "getDirs");
            msg.add("path", pathStack_.Peek());
            translater.postMessage(msg);
            msg.remove("command");
            msg.add("command", "getFiles");
            translater.postMessage(msg);
        }

        //-----------------------------------<ClientDirs List>-----------------------------------------
         private void CLientDirsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            if (CLientDirsList.SelectedItem == null)
                return;
            if (addDep == true)
            {
                addDependency();
                return;
            }
            string selectedDir = (string)CLientDirsList.SelectedItem;
            string path="";
            if (selectedDir == "..")
            {
                if (pathStack_.Count > 1)  
                {
                    pathStack_.Pop();
                    path = pathStack_.Peek();
                }
                else
                    return;
            }
            else
            {
                path = pathStack_.Peek() + "/" + selectedDir;
                pathStack_.Push(path);
            }

            CLientDirsList.Items.Clear();
            TheList.Clear();
            AddParent();
            PopulateList(path);
            ClientDirs = getDirsList();
            ClientFiles = getFilesList();
            foreach (string dir in ClientDirs)
            {
                string dirName = new System.IO.DirectoryInfo(dir).Name;
                CLientDirsList.Items.Add(dirName);
            }
            foreach (string file in ClientFiles)
                TheList.Add(new StringClass { IsSelected = false, TheText = System.IO.Path.GetFileName(file), InList = false });
         }

        //----------------------------<Add Category event>--------------------------------------------------
        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            string category = CategoryBox.Text;
            CatList.Add(new StringClass { IsSelected = false, TheText = category, InList = false });
        }

        //----------------------------------------------<Initiate checkIn>-------------------------------------
        public void InitiateCheckIn()
        {
            int count = 0;
            string fullSrcPath = System.IO.Path.GetFullPath(CheckInFiles[0]);
            string fullDstPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(sendFilesPath,System.IO.Path.GetFileName(CheckInFiles[0])));
            System.IO.File.Copy(fullSrcPath, fullDstPath,true);
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "checkIn");
            msg.add("namespace", NamespaceText.Text);
            msg.add("description", DescriptionText.Text);
            msg.add("filePath", FilePath.Text);
            msg.add("checkInStatus", CheckInStatus.Text);
            foreach (var item in depedencyList)
            {
                msg.add("dep" + count, item);
                count++;
            }
            msg.add("totalDep", count.ToString());
            count = 0;
            foreach(var item in catLists)
            {
                msg.add("cat" + count, item);
                count++;
            }
            msg.add("totalCat", count.ToString());
            msg.add("sendingFile", System.IO.Path.GetFileName(CheckInFiles[0]));
            msg.add("fileName", CheckInFiles[0]);
            show(msg);
            translater.postMessage(msg);
        }
        
        //--------------------------------------<Initiate view metadata>-----------------------------------------
        private void InitiateViewMetadata() 
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "viewMetadata");
            show(msg);
            translater.postMessage(msg);
        }


        //--------------------------------<perform Check in>-----------------------------------------------------------
        public void performCheckIn()
        {
            int catCount = 0;
            if (string.IsNullOrEmpty(NamespaceText.Text))
            {
                UserMessages.Text = "Enter namespace. Namespace cannot be empty";
                return;
            }
            if (string.IsNullOrEmpty(DescriptionText.Text))
            {
                UserMessages.Text = "Enter description. Description cannot be empty";
                return;
            }
            if (string.IsNullOrEmpty(FileNameBox.Text))
            {
                UserMessages.Text = "Enter file name. File Name cannot be empty";
                return;
            }
            if (string.IsNullOrEmpty(FilePath.Text))
            {
                UserMessages.Text = "Enter File Path. File Path cannot be empty";
                return;
            }
            foreach (var item in CatList)
            {
                if (item.IsSelected == true) catCount++;
            }
            if (catCount == 0)
            {
                UserMessages.Text = "Select atleast one category";
                return;
            }
            UserMessages.Text = "Checking file " + System.IO.Path.GetFullPath(CheckInFiles[0]) + " with depedencies: " + Environment.NewLine;
            foreach (string file in depedencyList)
            {
                UserMessages.Text += System.IO.Path.GetFileName(file);
                UserMessages.Text += Environment.NewLine;
            }
            UserMessages.Text += " and following categories: " + Environment.NewLine;
            foreach (string cat in catLists)
            {
                UserMessages.Text += cat;
                UserMessages.Text += Environment.NewLine;
            }
            InitiateCheckIn();
        }

        //--------------------------<CheckBtn event>-----------------------------------------------------
        private void CheckInBtn_Click(object sender, RoutedEventArgs e)
        {
            AddDepBtn.IsEnabled = false;
            ConfirmSelectionBtn.IsEnabled = false;
            performCheckIn();
        }

        //--------------------------<New Check-inBtn event>-----------------------------------------------------
        private void PerformNewCheckIn_Click(object sender, RoutedEventArgs e)
        {
            addDep = false;
            catLists.Clear();
            checkInSetup();
        }

        //------------------------------------<get new status>------------------------------------------------
        private void CheckInStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            string text = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content.ToString();
            CheckInStatus.Text = text;
        }        
         //--------------------------------------------<Initiate Browse>--------------------------------------
        public void InitiateBrowse()
        {
            pathStack_.Clear();
            pathStack_.Push("../../..");
           PopulateList();
            ClientDirs =getDirsList();
            ClientFiles =getFilesList();
            CLientDirsList.Items.Clear();
            ClientFilesList.Items.Clear();
            AddParent();
            foreach (string dir in ClientDirs)
            {
                string dirName = new System.IO.DirectoryInfo(dir).Name;
                CLientDirsList.Items.Add(dirName);
            }
            foreach (string file in ClientFiles)
                ClientFilesList.Items.Add(System.IO.Path.GetFileName(file));
        }

        //-----------------------------<Browse CheckIn>---------------------------------------------
        private void BrowseCheckIn_Click(object sender, RoutedEventArgs e)
        {
            addDep = false;
            InitiateBrowse();
        }

        //-----------------------------------<CheckBox unchecked>-------------------------------------------
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ClientFilesList.Items.Refresh();
            ClientFilesList.UnselectAll();
        }

        //---------------------------------<Client Dirs List View Handler>---------------------------------------------
        private void CLientDirsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CLientDirsListView.SelectedItem == null)
                return;
            string selectedDir = (string)CLientDirsListView.SelectedItem;
            string path;
            if (selectedDir == "..")
            {
                clearFilesCheckOut();
                if (pathStack_.Count > 1)
                    pathStack_.Pop();
                else
                    return;
            }
            else
            {
                clearFilesCheckOut();
                path = pathStack_.Peek() + "/" + selectedDir;
                pathStack_.Push(path);
            }

            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "getDirs");
            msg.add("path", pathStack_.Peek());
            translater.postMessage(msg);
            msg.remove("command");
            msg.add("command", "getFiles");
            translater.postMessage(msg);
        }


        //---------------------------<A show Db msg generator>-----------------------------------------------------------
        void  sendDbShowMsg()
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "displayDb");
            show(msg);
            translater.postMessage(msg);
        }

        //------------------------------------< Show Database btn>-------------------------------------------
        private void ShowDatabase_Click(object sender, RoutedEventArgs e)
        {
            sendDbShowMsg();
        }

        //------------------------------<FileList1 double click event>-------------------------------------------
        private void FileList1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!connected_)
                return;
            string fileName =(string)FileList1.SelectedItem;
            Console.WriteLine("++" + (string)FileList1.SelectedItem);
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost"; 
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "sendFile");
            msg.add("path", pathStack_.Peek()+"/"+(string)DirList.SelectedItem);
            msg.add("fileName", fileName);
            translater.postMessage(msg);
        }

        //----------------------------------<ConfirmSelection button>---------------------------------
        private void ConfirmSelectionBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckInStatus.IsEnabled = true;
            CheckInBtn.IsEnabled = true;
            int count = 0;
            foreach (var item in TheList)
            {
                if (item.IsSelected == true)
                    count++;
            }
            if (count > 1)
            {
                UserMessages.Text = "Please select one file. Multiple files selected for check-in";
                return;
            }
            else if (count == 0)
            {
                UserMessages.Text = "No file selected for check-in";
                return;
            }
            else
            {
                CheckInFiles.Clear();
                CheckInStatus.IsEditable = true;
                foreach (var item in TheList)
                {
                    if (item.IsSelected == true)
                    {
                        string selectedFile = pathStack_.Peek() + "/" + (string)CLientDirsList.SelectedItem + item.TheText;
                        CheckInFiles.Add(selectedFile);
                        UserMessages.Text = "File " + item.TheText + " selected for check-in";
                        FileNameBox.Text = (string)item.TheText;
                        FilePath.Text = System.IO.Path.GetFullPath(pathStack_.Peek() + "/" + (string)CLientDirsList.SelectedItem);
                        AddDepBtn.IsEnabled = true;
                        if (string.IsNullOrEmpty(FileNameBox.Text))
                        {
                            UserMessages.Text = "No File selected. Select one file to check-in";
                        }
                    }
                }
            }
            ConfirmSelectionBtn.IsEnabled = false;
        }

        //------------------------------------<CheckInBox checked>--------------------------------------------
        private void CheckInBox_Checked(object sender, RoutedEventArgs e)
        {
            ConfirmSelectionBtn.IsEnabled = false;
        }

        //--------------------------------------<remove File Handler>--------------------------------------
        private void removeFile()
        {
            foreach(var item in TheList)
            {
                string file = pathStack_.Peek() + "/" + item.TheText;

                if (item.IsSelected == false)
                {
                    if (depedencyList.Contains(file))
                    {
                        if (item.IsSelected == false)
                            depedencyList.Remove(file);
                    }
                }
            }
        }

        //------------------------------------------<clientFiles selected unchecked>---------------------------------------
        private void ClientFilesSelectedBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (addDep == true)
                removeFile();
        }

        //-------------------------<add depedency handler>----------------------------------------------
        private void addDependencyHandler()
        {
            foreach(var item in TheList)
            {
                if(item.IsSelected==true)
                {
                    int pos = pathStack_.Peek().LastIndexOf("/");
                    string namespace_ = pathStack_.Peek().Substring(pos + 1);
                    string selectedFile =namespace_ +"::"+ item.TheText;
                    depedencyList.Add(selectedFile);
                }
            }
        }

        //-------------------------------<clientFiles selected box event>--------------------------------
        private void ClientFilesSelectedBox_Checked(object sender, RoutedEventArgs e)
        {
            if (addDep == true)
            {
                addDependencyHandler();
                ConfirmSelectionBtn.IsEnabled = false;
            }
        }

        //-------------------------------<cat unchecked event>-------------------------------------------
        private void CatCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach(var item in CatList)
            {
                if(item.IsSelected==false)
                {
                    if(catLists.Contains(item.TheText))
                    {
                        catLists.Remove(item.TheText);
                    }
                }
            }
        }

        //-----------------------<catChecked event>------------------------------------
        private void CatCheckBox_Checked(object sender, RoutedEventArgs e)
        {
           foreach(var item in CatList)
           {
                if (item.IsSelected == true)
                    catLists.Add(item.TheText);
           }
        }

        //--------------------<getVersion button event>-----------------------------------
        private void GetVersionBtn_Click(object sender, RoutedEventArgs e)
        {
            VersionList.Add(new StringClass { IsSelected = false, TheText = "1", InList = false });
            VersionList.Add(new StringClass { IsSelected = false, TheText = "2", InList = false });
            VersionList.Add(new StringClass { IsSelected = false, TheText = "3", InList = false });
            VersionList.Add(new StringClass { IsSelected = false, TheText = "4", InList = false });
            VersionList.Add(new StringClass { IsSelected = true, TheText = "5", InList = false });
        }

        //-------------------------<button click for metadata>----------------------------------------------
        private void ViewMetadataBtn_Click(object sender, RoutedEventArgs e)
        {
            InitiateViewMetadata();
        }


        //----------------------------------------<Handler function for Directory List Double Click>---------------------
        private void DirListCheckOut_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DirListCheckOut.SelectedItem == null)
                return;
            string selectedDir = (string)DirListCheckOut.SelectedItem;
            string path;
            if (selectedDir == "..")
            {
                clearFilesCheckOut();
                if (pathStack_.Count > 1)
                    pathStack_.Pop();
                else
                    return;
            }
            else
            {
                clearFilesCheckOut();
                path = pathStack_.Peek() + "/" + selectedDir;
                pathStack_.Push(path);
            }

            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "getDirs");
            msg.add("path", pathStack_.Peek());
            translater.postMessage(msg);
            msg.remove("command");
            msg.add("command", "getFiles");
            translater.postMessage(msg);
        }

        //------------------------------<Function to perform checkOut operation>------------------------------------
        private void InitiateCheckOut(string fileNm=" ")
        {
            string fileName;
            if (FileListCheckOut.SelectedItem == null) fileName = fileNm;
            else
            fileName = (string)FileListCheckOut.SelectedItem;
            Console.WriteLine("++" + (string)FileListCheckOut.SelectedItem);
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "checkOut");
            msg.add("path", pathStack_.Peek() + "/" + (string)DirList.SelectedItem);
            msg.add("fileName", fileName);
            translater.postMessage(msg);
        }

        //--------------------------------<FileList CheckOut Handler>---------------------------------
        private void FileListCheckOut_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            InitiateCheckOut();
        }
        
        //-----------------------------<CheckOut Handler>---------------------------------------------------
        private void CheckOut_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChkOutTab = true;
            DirListCheckOut.Items.Clear();
            FileListCheckOut.Items.Clear();
            loadDispatcher();
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            pathStack_.Clear();
            pathStack_.Push("../Storage");
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "getDirs");
            msg.add("path", pathStack_.Peek());
            show(msg);
            translater.postMessage(msg);
            msg.remove("command");
            msg.add("command", "getFiles");
            show(msg);
            translater.postMessage(msg);
            msg.remove("command");
        }

        //---------------------------------<Setup View Metadata tab>-------------------------------------------------
        private void viewMetadataSetup()
        {
            loadDispatcher();
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            pathStack_.Clear();
            pathStack_.Push("../Storage");
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "getDirs");
            msg.add("path", pathStack_.Peek());
            show(msg);
            translater.postMessage(msg);
            msg.remove("command");
            msg.add("command", "getFiles");
            show(msg);
            translater.postMessage(msg);
            msg.remove("command");
        }

        //----------------------------------------<View metadata Handler function>---------------------------------------
        private void ViewMetadata_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            viewMetadataTab = true;
            viewMetadataSetup();
        }


        //------------------------------------------<Handler function>-----------------------------------------------
        private void ClientFilesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string dirPath = pathStack_.Peek();
            int pos = dirPath.LastIndexOf("/");
            string fileName =(string)ClientFilesListView.SelectedItem;
            int pos1 = fileName.LastIndexOf(".");
            string version = (string)fileName.Substring(fileName.Length-1);
            string namespace_ = dirPath.Substring(pos + 1);
            string key = namespace_ + "::" + fileName;
            fileName = fileName.Substring(0,pos1);
            dirPath.Remove(pos);
            pos = dirPath.LastIndexOf("/");
            string package = dirPath.Substring(pos + 1);
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            PackageViewMetadata.Text = package;
            NamespaceBox.Text = namespace_;
            FileNameView.Text = fileName;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "viewMetadata");
            msg.add("namespace", namespace_);
            msg.add("package", package);
            msg.add("path", pathStack_.Peek());
            msg.add("key", key);
            msg.add("fileName", fileName);
            msg.add("version", version);
            translater.postMessage(msg);
        }

        //--------------------------------------<Helper function for generating Query msg>----------------------------------------------
        private void generateQueryMsg()
        {
            loadDispatcher();
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "query");
            msg.add("fileName", NameQueryBox.Text);
            msg.add("description", DescriptionQueryBox.Text);
            msg.add("dateFrom", FromQueryBox.Text);
            msg.add("dateTo", ToQueryBox.Text);
            msg.add("categories", CategoryQueryBox.Text);
            msg.add("depedencies", DepedencyQueryBox.Text);
            msg.add("version", VersionQueryBox.Text);
            translater.postMessage(msg);
        }

        //-------------------------------------------<helper function for generating msg>--------------------------------
        private void generateFilesNoParentMsg()
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "filesWithNoParent");
            translater.postMessage(msg);
        }

        //----------------------------------<On Query click handler>----------------------------------------
        private void Query_Click(object sender, RoutedEventArgs e)
        {
            generateQueryMsg();
        }

        //----------------------------------------------<Function to populateTest>------------------------------------------
        public void PopulateList(string path = "../../../../")
        {
            DirList1 = getDirs(path);
            FileLists = getFiles(path);
        }
        private void FilesNoParent_Click(object sender, RoutedEventArgs e)
        {
            generateFilesNoParentMsg();
        }

        //-------------------------------------------<getDirs>-----------------------------------------------------------------
        public List<string> getDirs(string path)
        {
            List<string> dirList = new List<string>();
            string[] Dirs = System.IO.Directory.GetDirectories(path);
            foreach (string dir in Dirs)
                dirList.Add(dir);
            return dirList;
        }

        //--------------------------------------------------<getFiles>--------------------------------------------------------
        public List<string> getFiles(string path)
        {
            List<string> fileList = new List<string>();
            var ext = new List<string> { "jpg", "gif", "png" };
            var files = System.IO.Directory.GetFiles(path, "*", System.IO.SearchOption.TopDirectoryOnly)
                            .Where(s => s.EndsWith(".cpp", StringComparison.OrdinalIgnoreCase) ||
                                      s.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                                      s.EndsWith(".h", StringComparison.OrdinalIgnoreCase));

            foreach (var file in files)
                fileList.Add(file);

            return fileList;
        }

        //----------------------------------------<Function getDirList>---------------------------------------------------
        public List<string> getDirsList()
        {
            return DirList1;
        }
        //---------------------------------------------<Function for getlist>-----------------------------------------------
        public List<string> getFilesList()
        {
            return FileLists;
        }

        //----------------------------------------------<Function for showing message>------------------------------------------
        public void show (CsMessage msg)
        {
            lock (ConsoleWriterLock)
            {
               Console.WriteLine("\nClient:\n");
               msg.show();
            }
        }

        //---------------------------------------------<Function for populating Depedency>----------------------------------------
        public void populateDep()
        {
            depedencyList.Add("FileSystem");
            depedencyList.Add("Message.h");
        }

        //----------------------------------------<Function to perform demoCheckIn>--------------------------------------------------------------------------------
        public void demoCheckIn(string namespace_, string description, string fileName, string path, string status, HashSet<string> catList, HashSet<string> depList)
        {
            NamespaceText.Text = namespace_;
            DescriptionText.Text = description;
            FileNameBox.Text = fileName;
            FilePath.Text = System.IO.Path.GetFullPath(path);
            CheckInStatus.Text = status;
            depedencyList = depList;
            catLists = catList;
            CheckInFiles.Clear();
            CheckInFiles.Add(System.IO.Path.Combine(FilePath.Text, fileName));
            InitiateCheckIn();
        }

        //-----------------------------------------------<Function for viewing file>------------------------------------------------
        public void viewFileContent()
        {
            string fileName = "Process.h.1";
            string selectedFile = fileName;
            Console.WriteLine("++" + selectedFile);
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "sendFile");
            msg.add("path", pathStack_.Peek());
            msg.add("fileName", fileName);
            translater.postMessage(msg);
        }
        
        //-----------------------------<Test1>------------------------------------------------------------
        public void Test1()
        {
                Thread.Sleep(1000);
                Console.WriteLine("\n---------------Demonstrating Requirement 1--------------------------------------------------------");
                Console.WriteLine("Server application is developed in C++ using Visual Studio 2017");
                Console.WriteLine("Graphical User Interface is developed in C#.NET using Windows Presentation Foundation framework and the translator in C++_CLI \n");
        }
                
        //----------------------------------<Test2>--------------------------------------------------------------------------
        public void Test2()
        {
            Thread.Sleep(1000);
            Console.WriteLine("\n---------------Demonstrating Requirement 2a Checkin a file --------------------------------------------------------");
            Console.WriteLine("\n Checking in a file from the client into the remote code repository");
            Console.WriteLine("This demonstration satisfies requirement 3 of being able to upload files in the repository");
            Console.WriteLine("This demonstration satisfies the requirement of having a communication channel that can support passing HTTP style messages using asynchronous one-way messaging while passing messages from client to repository");
            Console.WriteLine("\nA check-in updates the NoSql database with the updated value and the database can be found in /Database/Database.xml");
            Console.Write("\n The updations to the database can also be seen on the GUI console window");
            Console.WriteLine("\n1.Executing an open check-in of file 'check-in.h' with category:check-in with no depedencies");
            HashSet<string> categoryList=new HashSet<string>();
            HashSet<string> depedencyList = new HashSet<string>();
            categoryList.Add("Check-in");
            demoCheckIn("Repository","Implements check-in","check-in.h","../../../../Check-in","Open", categoryList, depedencyList);
            sendDbShowMsg();
            Console.WriteLine("\n1.Executing an close check-in of file 'check-in.cpp' with category:check-in and the above checked-in file('check-in.h') as depedency");
            Console.WriteLine("But since the depedency 'check-in.h' is open, 'check-in.cpp' will be a open check-in");
            depedencyList.Add("Repository::check-in.h");
            demoCheckIn("Repository", "Implements check-in", "check-in.cpp", "../../../../Check-in", "Close", categoryList, depedencyList);
            sendDbShowMsg();
            depedencyList.Clear();
            Console.WriteLine("\n1.Executing an close check-in of file 'check-in.h' with category:check-in and the above checked-in file('check-in.h') as depedency");
            demoCheckIn("Repository", "Implements check-in", "check-in.h", "../../../../Check-in", "Close", categoryList, depedencyList);
            Console.WriteLine("\n1.Executing an close check-in of file 'check-in.cpp' with category:check-in and the above checked-in file('check-in.h') as depedency");
            depedencyList.Add("Repository::check-in.h.1");
            demoCheckIn("Repository", "Implements check-in", "check-in.cpp", "../../../../Check-in", "Close", categoryList, depedencyList);
            sendDbShowMsg();
            categoryList.Clear();
            depedencyList.Clear();
            categoryList.Add("DateTime");
            Console.WriteLine("\n1.Executing an close check-in of file 'DateTime.h' with category:DateTime and no depedency");
            demoCheckIn("DateTime", "Implements dateTime functionality", "DateTime.h", "../../../../DateTime", "Open", categoryList, depedencyList);
            Console.WriteLine("\n1.Executing an close check-in of file 'Process.h' with category:Process");
            categoryList.Clear();
            depedencyList.Clear();
            categoryList.Add("Process");
            demoCheckIn("Process", "Implements process functionality", "Process.h", "../../../../Process/Process", "Open", categoryList, depedencyList);
            sendDbShowMsg();
        }

        //----------------------------------<Test3>--------------------------------------------------------------------------

        public void Test3()
        {
            Thread.Sleep(1000);
            Console.WriteLine("\n---------------Demonstrating Requirement 2b CheckOut a file --------------------------------------------------------");
            Console.WriteLine("\nChecking out a file from the remote code repository");
            Console.WriteLine("\nContents can also be seen through a popped-up window");
            Console.WriteLine("\nFile transferred through the communication channel which satisfies requirement 7 for sending and receiving blocks of bytes to support file transfer");
            Console.WriteLine("\nChecking-out file 'Repository::check-in.cpp.1'");
            Console.WriteLine("\nDependencies 'Repository::check-in.h.1' are also checked out");
            Console.WriteLine("The file can be seen in: "+ (string)saveFilesPath);
            pathStack_.Push("../Storage/check-in/Repository");
            ChkOutTab = true;
            InitiateCheckOut("check-in.cpp.1");
            pathStack_.Clear();
        }

        //----------------------------------<Test4>--------------------------------------------------------------------------

        public void Test4()
        {
            Thread.Sleep(1000);
            ChkOutTab = false;
            Console.WriteLine("\n-----------------------------Demonstrating requirement of viewing file content----------------------------");
            Console.WriteLine("\nDemonstrating Requirement of 'viewing full file text'");
            Console.WriteLine("File can be found in: " + (string)saveFilesPath);
            Console.WriteLine("\nA file is opened with file contents\n");
            Console.WriteLine("Check server console for server side messages");
            pathStack_.Push("../Storage/Process/Process");
            viewFileContent();
            pathStack_.Clear();
        }

        //----------------------------------<Test5>--------------------------------------------------------------------------
        public void Test5()
        {
            Thread.Sleep(1000);
            Console.WriteLine("\n-----------------------------Demonstrating requirement to query based on category ----------------------------");
            Console.WriteLine("\nDemonstrating query based on categories");
            Console.WriteLine("\nQuerying for all files belonging to category: check-in and DateTime");
            CategoryQueryBox.Text = "Check-in|DateTime";
            generateQueryMsg();
        }

        //----------------------------------<Test6>--------------------------------------------------------------------------
        public void Test6()
        {
            Thread.Sleep(1000);
            CategoryQueryBox.Clear();
            Thread.Sleep(100);
            Console.WriteLine("\n-----------------------------Demonstrating requirement to query based on fileName and description ----------------------------");
            HashSet<string> categoryList = new HashSet<string>();
            HashSet<string> depedencyList = new HashSet<string>();
            categoryList.Add("Check-in");
            depedencyList.Add("Repository::check-in.h.1");
            Console.WriteLine("\n1.Executing an close check-in of file 'check-in.cpp' with category:check-in and the checked-in file 'check-in.h' as depedency");
            demoCheckIn("Repository", "Implements check-in", "check-in.cpp", "../../../../Check-in", "Close", categoryList, depedencyList);
            Console.WriteLine("\nDemonstrating compound Query on fileName and description");
            Console.WriteLine("\nQuerying for all files with name 'check-in.cpp' and description containing word 'Repository'");
            DescriptionQueryBox.Text = "Repository";
            NameQueryBox.Text = "check-in.cpp";
            generateQueryMsg();
        }

        //----------------------------------<Test7>--------------------------------------------------------------------------

        public void Test7()
        {
            Thread.Sleep(1000);
            DescriptionQueryBox.Clear();
            NameQueryBox.Clear();
            Console.WriteLine("\n-----------------------------Demonstrating requirement to display all of the files in any category that have no parents ----------------------------");
            generateFilesNoParentMsg();
            QueryOutputBox.Text = String.Empty;
            viewMetadataSetup(); 
        }

        //----------------------------------<Test8>--------------------------------------------------------------------------
        public void Test8()
        {
            Thread.Sleep(1000);
            CategoryQueryBox.Clear();
            Thread.Sleep(100);
            Console.WriteLine("\n-----------------------------Demonstrating requirement to query based on depedencies ----------------------------");
            QueryOutputBox.Text = String.Empty;
            Console.WriteLine("\nDemonstrating Query on depedency:'Repository::check -in.h.1'");
            DepedencyQueryBox.Text = "Repository::check-in.h.1";
            generateQueryMsg();
        }

        //-----------------------------<Test9>----------------------------------------
        public void Test9()
        {
            Thread.Sleep(1000);
            CategoryQueryBox.Clear();
            Thread.Sleep(100);
            Console.WriteLine("\n-----------------------------Demonstrating requirement to query based on versions ----------------------------");
            DepedencyQueryBox.Text = String.Empty;
            Console.WriteLine("\nDemonstrating Query for all files having version 1");
            VersionQueryBox.Text = "1";
            generateQueryMsg();
            VersionQueryBox.Text = String.Empty;
        }
        

        //--------------------------<test automation sequence>--------------------------------
        public void startTests()
        {
            Console.WriteLine("\n------------------------Demonstrating tests-----------------------------------------------\n");
            Test1();
            Test2();
            Test3();
            Test4();
            Test5();
            Test6();
            Test7();
            Test8();
            Test9();
        }
    }
}