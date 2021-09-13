using CranchyLib.SaveFile;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SaveFileInjectorRemake
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct OpenFileName
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public string lpstrFile;
        public int nMaxFile;
        public string lpstrFileTitle;
        public int nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int flagsEx;
    }




    class Program
    {
        //            Static Global Values
        //   =======================================
        public static bool IsProgramInitialized = false;
        public static string ProgramCurrentDirectory = Environment.CurrentDirectory;
        public static string SpecifiedGamePlatform = string.Empty;
        public static string SpecifiedbhvrSession = string.Empty;
        public static string SpecifiedSaveFile = string.Empty;
        public static sbyte SpecifiedWorkingMode = 0;


        //             ShowDialog Function
        //   =======================================
        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetOpenFileName(ref OpenFileName ofn);

        private static string ShowDialog()
        {
            var filedialog = new OpenFileName();
            filedialog.lStructSize = Marshal.SizeOf(filedialog);
            filedialog.lpstrFilter = "All Files (*.*)\0*.*\0";
            filedialog.lpstrFile = new string(new char[256]);
            filedialog.nMaxFile = filedialog.lpstrFile.Length;
            filedialog.lpstrFileTitle = new string(new char[64]);
            filedialog.nMaxFileTitle = filedialog.lpstrFileTitle.Length;
            filedialog.lpstrTitle = "Open File Dialog...";
            if (GetOpenFileName(ref filedialog))
                return filedialog.lpstrFile;
            return string.Empty;
        }


        //                Main Program
        //   =======================================
        static void Main()
        {
            if (!IsProgramInitialized)
                IncreaseConsoleBufferSize();

            Console.Clear();
            Console.WriteLine("                SSSSS  EEEEEEE RRRRRR  VV     VV EEEEEEE RRRRRR  NN   NN   AAA   MM    MM EEEEEEE ");
            Console.WriteLine("               SS      EE      RR   RR VV     VV EE      RR   RR NNN  NN  AAAAA  MMM  MMM EE      ");
            Console.WriteLine("                SSSSS  EEEEE   RRRRRR   VV   VV  EEEEE   RRRRRR  NN N NN AA   AA MM MM MM EEEEE   ");
            Console.WriteLine("                    SS EE      RR  RR    VV VV   EE      RR  RR  NN  NNN AAAAAAA MM    MM EE      ");
            Console.WriteLine("                SSSSS  EEEEEEE RR   RR    VVV    EEEEEEE RR   RR NN   NN AA   AA MM    MM EEEEEEE ");
            Console.WriteLine("                                      SaveFile Injector - Injection Tool                         ");

            if (SpecifiedGamePlatform.Length == 0)
            {
                Console.Write("\n\nSelect Your Platform:\n[1] Steam\n[2] Microsoft Store\n> ");
                switch (Console.ReadLine())
                {
                    case "1":
                        SpecifiedGamePlatform = "steam";
                        break;

                    case "2":
                        SpecifiedGamePlatform = "grdk";
                        break;

                    default:
                        Console.WriteLine("Please, Specify Valid platform (1 or 2)\nPress ENTER to continue...");
                        Console.ReadLine();
                        Main();
                        break;
                }
            }


            if (SpecifiedbhvrSession.Length == 0)
            {
                Console.Write("\nbhvrSession=");
                SpecifiedbhvrSession = Console.ReadLine();


                if (SpecifiedbhvrSession.Length > 255)
                {
                    Console.Write("\nSpecify what you wanna do:\n[1] Inject SaveFile\n[2] Reset SaveFile - Zero Progress\n[3] Dump SaveFile\n> ");
                    switch (Console.ReadLine())
                    {
                        case "1":
                            SpecifiedWorkingMode = 0;
                            break;

                        case "2":
                            SpecifiedWorkingMode = 1;
                            break;

                        case "3":
                            SpecifiedWorkingMode = 2;
                            break;

                        default:
                            Console.WriteLine("Please, Specify Valid Action Type (1 or 2)\nPress ENTER to continue...");
                            Console.ReadLine();
                            Main();
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("\n\nERROR: bhvrSession Length can't be less then 256 symbols!\nPress ENTER to continue...");
                    SpecifiedGamePlatform = string.Empty;
                    SpecifiedbhvrSession = string.Empty;
                    Console.ReadLine();
                    Main();
                }



                if (SpecifiedWorkingMode == 0)
                    SpecifiedSaveFile = InitializeSaveFile(string.Empty);
                else if (SpecifiedWorkingMode == 1)
                    SpecifiedSaveFile = Properties.Resources.OFFLINE_SAVEFILE;
                else if (SpecifiedWorkingMode == 2)
                    SpecifiedSaveFile = "NONE";


                if (SpecifiedSaveFile.Length != 0)
                {
                    SpecifiedbhvrSession = SpecifiedbhvrSession.Replace("bhvrSession", "").Replace("=", "").Replace(" ", "");
                    string saveFileVersion = NetServices.REQUEST_GET_HEADER($"https://{SpecifiedGamePlatform}.live.bhvrdbd.com/api/v1/players/me/states/FullProfile/binary", $"bhvrSession={SpecifiedbhvrSession}", "Kraken-State-Version");
                    if (saveFileVersion.Length == 0)
                    {
                        Console.WriteLine("Something went wrong, make sure bhvrSession is valid & properly pasted\nPress ENTER to continue...");
                        SpecifiedbhvrSession = string.Empty;
                        SpecifiedSaveFile = string.Empty;
                        Console.ReadLine();
                        Main();
                    }

                    Console.WriteLine($"Profile Version: {saveFileVersion}\n\nTrying To Obtain playerUID...");
                    string saveFileUserID = NetServices.REQUEST_GET($"https://{SpecifiedGamePlatform}.live.bhvrdbd.com/api/v1/players/me/states/FullProfile/binary", $"bhvrSession={SpecifiedbhvrSession}");
                    if (SpecifiedWorkingMode == 2)
                    {
                        InitializeSaveFileDump(SaveFile.DecryptSavefile(saveFileUserID));
                        SpecifiedbhvrSession = string.Empty;
                        SpecifiedSaveFile = string.Empty;
                        Console.WriteLine("SaveFile was successfully obtained & stored on PC \nPress ENTER to continue...");
                        Console.ReadLine();
                        Main();
                    }
                    InitializeSaveFileBackup(saveFileUserID);
                    if (saveFileUserID.Length == 0)
                    {
                        Console.WriteLine("Something went wrong when program tried to obtain playerUID\nPress ENTER to continue...");
                        SpecifiedbhvrSession = string.Empty;
                        SpecifiedSaveFile = string.Empty;
                        Console.ReadLine();
                        Main();
                    }
                    var JsFullProfile = JObject.Parse(SaveFile.DecryptSavefile(saveFileUserID));
                    saveFileUserID = (string)JsFullProfile["playerUId"];

                    Console.WriteLine($"UserID: {saveFileUserID}\n\nTrying To Inject SaveFile...");
                    string saveFileResponse = NetServices.REQUEST_POST($"https://{SpecifiedGamePlatform}.live.bhvrdbd.com/api/v1/players/me/states/binary?schemaVersion=0&stateName=FullProfile&version={(Convert.ToInt32(saveFileVersion) + 1).ToString()}", $"bhvrSession={SpecifiedbhvrSession}", SaveFile.EncryptSavefile(SaveFile.Ressurect_All(SpecifiedSaveFile, saveFileUserID)));
                    if (saveFileResponse.Length == 0)
                    {
                        Console.WriteLine("Something went wrong, make sure that bhvrSession is validated by EAC\nPress ENTER to continue...");
                        SpecifiedbhvrSession = string.Empty;
                        SpecifiedSaveFile = string.Empty;
                        Console.ReadLine();
                        Main();
                    }
                    Console.Write("Success!\n\nPress ENTER to continue...");
                    Console.ReadLine();
                    SpecifiedbhvrSession = string.Empty;
                    SpecifiedGamePlatform = string.Empty;
                    SpecifiedSaveFile = string.Empty;
                    Main();
                }
            }
        }

        private static string InitializeSaveFile(string input)
        {
            try
            {
                if (input.Length == 0)
                {
                    using (StreamReader sr = new StreamReader(ShowDialog()))
                    {
                        string InputFileContent = sr.ReadToEnd();
                        if (InputFileContent.IsDeadByDaylightCryptoString())
                            return SaveFile.DecryptSavefile(InputFileContent);

                        else if (InputFileContent.IsBase64String())
                            return System.Text.Encoding.ASCII.GetString(
                                System.Convert.FromBase64String(InputFileContent));

                        else
                            return InputFileContent;
                    }
                }
                else
                {
                    string InputFileContent = input;
                    if (InputFileContent.IsDeadByDaylightCryptoString())
                        return SaveFile.DecryptSavefile(InputFileContent);

                    else if (InputFileContent.IsBase64String())
                        return System.Text.Encoding.ASCII.GetString(
                            System.Convert.FromBase64String(InputFileContent));

                    else
                        return InputFileContent;
                }
            }
            catch { return string.Empty; }
        }
        private static void InitializeSaveFileBackup(string input)
        {
            try
            {
                if (!Directory.Exists($"{ProgramCurrentDirectory}\\Backups"))
                    Directory.CreateDirectory($"{ProgramCurrentDirectory}\\Backups");
                File.WriteAllText($"{ProgramCurrentDirectory}\\Backups\\[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString().Replace(":", "։")}] fullProfile.backup.txt", input);
            }
            catch { }
        }
        private static void InitializeSaveFileDump(string input)
        {
            try
            {
                if (!Directory.Exists($"{ProgramCurrentDirectory}\\Dumped SaveFiles"))
                    Directory.CreateDirectory($"{ProgramCurrentDirectory}\\Dumped SaveFiles");
                File.WriteAllText($"{ProgramCurrentDirectory}\\Dumped SaveFiles\\[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString().Replace(":", "։")}] fullProfile.dump.txt", input);
            }
            catch { }
        }

        private static void IncreaseConsoleBufferSize()
        {
            Console.SetIn(new StreamReader(Console.OpenStandardInput(),
                               Console.InputEncoding,
                               false,
                               bufferSize: 1024));

            IsProgramInitialized = true;
        }
    }
}
