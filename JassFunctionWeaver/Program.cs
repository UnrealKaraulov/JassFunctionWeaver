using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace JassFunctionWeaver
{
    class Program
    {
        struct Function
        {
            public string FunctionName;
            public string FunctionNameWithArgs;
            public string[] FunctionData;
            public string FunctionReferences;
            public bool passtoend;
        }



        static void Main(string[] args)
        {
            List<Function> FunctionList = new List<Function>();
            string RegexGetFuncName = @"^\s*function\s+(.*?)\s+";
            string RegexEndFunction = @"^\s*endfunction";
            string RegexEndGlobals = @"^\s*endglobals";
            string ScriptOutFilePath = string.Empty;
            string ScriptFilePath = string.Empty;
            if (args.Length == 1)
            {
                ScriptFilePath = args[0].Replace("\"", "");
            }
            else
            {
                Console.WriteLine("Jass script Weaver by Absol.");
                Console.WriteLine("usage JassFunctionWeaver.exe FileName or run and enter path.");
                Console.WriteLine("Enter path to script file:");
                ScriptFilePath = Console.ReadLine().Replace("\"", "");
            }

            ScriptOutFilePath = ScriptFilePath + ".new.j";

            List<string> outscriptdata = new List<string>();
            string[] scriptdata = File.ReadAllLines(ScriptFilePath);
            int i = 0;
            for (i = 0; i < scriptdata.Length; i++)
            {
                if (Regex.Match(scriptdata[i], RegexEndGlobals).Success)
                {
                    Console.WriteLine("End globals at " + i + " line. Now read all functions data");
                    break;
                }
            }
            int n = 0;
            for (; n < i + 1; n++)
            {
                outscriptdata.Add(scriptdata[n]);
            }


            Match MatchGetFuncName;
            Function CurrentFunction;
            Match MatchEndFunction;
            List<string> CurrentFunctionData;
            int x = 0;
            for (i = 0; i < scriptdata.Length; i++)
            {
                MatchGetFuncName = Regex.Match(scriptdata[i], RegexGetFuncName);
                if (MatchGetFuncName.Success)
                {
                    x = i;
                    CurrentFunction = new Function();
                    CurrentFunction.FunctionName = MatchGetFuncName.Groups[1].Value;
                    if (CurrentFunction.FunctionName == "main" || CurrentFunction.FunctionName == "config")
                        CurrentFunction.passtoend = true;
                    else
                        CurrentFunction.passtoend = false;
                    CurrentFunction.FunctionNameWithArgs = scriptdata[i];
                    for (; i < scriptdata.Length; i++)
                    {
                        MatchEndFunction = Regex.Match(scriptdata[i], RegexEndFunction);
                        if (MatchEndFunction.Success)
                        {
                            CurrentFunctionData = new List<string>();
                            for (x++; x < i + 1; x++)
                            {
                                CurrentFunctionData.Add(scriptdata[x]);
                            }
                            CurrentFunction.FunctionData = CurrentFunctionData.ToArray();
                            FunctionList.Add(CurrentFunction);
                            break;
                        }
                    }

                }
            }

            Console.WriteLine("Functions found:" + FunctionList.Count);
            Console.WriteLine("Start search references...");

            bool needbreak = false;
            for (i = 0; i < FunctionList.Count; i++)
            {

                if (FunctionList[i].passtoend == true)
                    continue;
                for (n = i; n < FunctionList.Count; n++)
                {
                    if (FunctionList[n].passtoend == true)
                        continue;
                    if (i == n)
                        continue;

                    needbreak = false;
                    for (x = 0; x < FunctionList[n].FunctionData.Length; x++)
                    {
                        if (FunctionList[n].FunctionData[x].IndexOf(FunctionList[i].FunctionName, StringComparison.Ordinal) >= 0)
                        {
                            Function tmpfunc = FunctionList[i];
                            tmpfunc.FunctionReferences = FunctionList[n].FunctionName;
                            FunctionList[i] = tmpfunc;
                            needbreak = true;
                            break;
                        }

                    }
                    if (needbreak)
                        break;
                }
                Console.Write("\r Search ..." + (int)(((float)i / (float)FunctionList.Count) * 100.0) + "% ( " + i + " ) ");
            }

            Console.WriteLine("\rEnd search references...       ");



            Console.WriteLine("Start function weaving...       ");


            List<string> FunctionNamesList = new List<string>();

            foreach (Function CurFunc in FunctionList)
            {
                FunctionNamesList.Add(CurFunc.FunctionName);
            }


            //

            //

            foreach (Function CurFunc in FunctionList)
            {
                foreach (Function CurFuncTwo in FunctionList)
                {
                    if (CurFunc.FunctionReferences == CurFuncTwo.FunctionName)
                    {
                        int FuncId1 = FunctionNamesList.IndexOf(CurFunc.FunctionName);
                        int FuncId2 = FunctionNamesList.IndexOf(CurFuncTwo.FunctionName);

                        string BackupCurFunc = FunctionNamesList[FuncId1];
                        FunctionNamesList.Insert(FuncId2, BackupCurFunc);
                        FunctionNamesList.RemoveAt(FuncId1);
                        break;
                    }
                }
            }



            Console.WriteLine("End function weaving...       ");

            foreach (string AddFunctionName in FunctionNamesList)
            {
                foreach (Function CurFunc in FunctionList)
                {
                    if (CurFunc.FunctionName == AddFunctionName)
                    {
                        outscriptdata.Add(CurFunc.FunctionNameWithArgs);
                        outscriptdata.AddRange(CurFunc.FunctionData);
                        break;
                    }
                }
            }


            Console.WriteLine("Start script saving...       ");



            File.WriteAllLines(ScriptOutFilePath, outscriptdata.ToArray());

            Console.WriteLine("End script saving...       ");





            Console.ReadLine();

        }
    }
}
