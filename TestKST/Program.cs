using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestKST
{
    class Models
    {
        public class SortingNameModel
        {
            public String FileName { get; set; }
            public String FullPath { get; set; }
            public List<String> ListName { get; set; }
        }

        public class ResultModel
        {
            public bool Status { get; set; } 
            public String Msg { get; set; }
            public SortingNameModel Data { get; set; }
        }
    }

    public interface IshowResultinScreen
    {
        void ViewDatainScreen(List<string> ListName);
    }

    public interface ImakeFileTXT : IshowResultinScreen
    {
        void MakeTXT(string fullPathName, List<String> ListName);
    }

    class StartUp
    {
        public static void Main(string[] args)
        {
            Program Run = new Program();
            Run.MainProgram();
        }
    }

    class Program
    {
        private ProgramInfo _info = new ProgramInfo();
        private ProgramSortingName _execCmd = new ProgramSortingName();

        public virtual void MainProgram()
        {
            _info.ListTheNameinTXT();

            Console.Write("-> ");
            String input = Console.ReadLine();

            //process the input
            var resExec = _execCmd.CommandforSortingNameOrigin(input);

            if (resExec.Status)
            {
                ResultsProcess results = new ResultsProcess();
                results.ViewDatainScreen(resExec.Data.ListName);
                results.MakeTXT(resExec.Data.FullPath, resExec.Data.ListName);
                Console.WriteLine("\n" + "Completed, " + resExec.Msg);
            }
            else
                Console.WriteLine("Failed, " + resExec.Msg);


            Console.ReadLine();
        }
    }

    class ExecCommand
    {

        private CommandValidate _validate = new CommandValidate();
        private ModelBuilder _builder = new ModelBuilder();

        // normally sorting ASC without some condition
        public virtual Models.ResultModel CommandforSortingNameOrigin(string command)
        {
            // validate 
            var resValidate = _validate.ValidateSortingName(command);

            // build a model
            Models.ResultModel resBuilder = new Models.ResultModel();
            if (resValidate.Status)
            {
                resBuilder = _builder.BuilderSortingName(resValidate);
            }
            else
            {
                resBuilder = resValidate;
            }

            return resBuilder;
        }

        public virtual Models.ResultModel CommandforSortingNameDESC(string command)
        {
            var resOrigin = CommandforSortingNameOrigin(command);

            Models.ResultModel resBuilder = new Models.ResultModel();
            if (resOrigin.Status)
            {
                resBuilder = _builder.BuilderSortingNameDESC(resOrigin);
            }
            else
            {
                resBuilder = resOrigin;
            }

            return resBuilder;
        }
    }

    class CommandValidate
    {

        public virtual Models.ResultModel ValidateSortingName(string command)
        {
            // process the command 
            string isCommand = command.Split(' ').FirstOrDefault().ToString().Trim();
            string isPath = command.Split(' ').LastOrDefault().ToString().Trim();

            // validation the command 
            var s = false;
            var FailedModel = new Models.ResultModel
            {
                Status = s,
                Msg = "Please check the Command again!"
            };

            if (string.IsNullOrWhiteSpace(isCommand) || string.IsNullOrWhiteSpace(isPath))
                return FailedModel;
            if (isCommand != "name-sorter")
                return FailedModel;
            if (File.Exists(isPath))
            {
                return new Models.ResultModel { Status = !s, Msg = "",
                    Data = new Models.SortingNameModel
                    {
                        FullPath = isPath,
                        FileName = "file exist",
                    }
                };
            }
            else
            {
                return new Models.ResultModel
                {
                    Status = s,
                    Msg = "Path Not Found, " + FailedModel.Msg
                };
            }

        }
    }

    class ModelBuilder
    {
        public virtual Models.ResultModel BuilderSortingName(Models.ResultModel model)
        {
            // if validate are success
            Models.SortingNameModel ConvertModel = new Models.SortingNameModel();
            foreach (var file in new DirectoryInfo(Path.GetDirectoryName(model.Data.FullPath)).GetFiles())
            {
                if (file.Name.ToLower().Contains("unsorted-"))
                {
                    var getExtension = file.Extension;
                    if (getExtension == ".txt")
                    {
                        if (file.Name.ToLower() == Path.GetFileName(model.Data.FullPath))
                        {
                            using (var stream = new StreamReader(file.FullName))
                            {
                                var content = stream.ReadToEnd();
                                if (!String.IsNullOrWhiteSpace(content))
                                {
                                    string[] stringSeparators = new string[] { "\r\n" };
                                    var resList = content.Split(stringSeparators, StringSplitOptions.None);

                                    ConvertModel.FileName = file.Name;
                                    ConvertModel.FullPath = file.FullName;
                                    ConvertModel.ListName = resList.ToList();
                                }
                            }
                        }
                    }
                }
            }

            return new Models.ResultModel
            {
                Status = ConvertModel.ListName == null ? !model.Status : model.Status,
                Msg = "testorg",
                Data = ConvertModel
            };
        }

        public virtual Models.ResultModel BuilderSortingNameDESC(Models.ResultModel model)
        {
            return new Models.ResultModel
            {
                Status = model.Status,
                Msg = "testdesc",
                Data = new Models.SortingNameModel
                {
                    FileName = model.Data.FileName,
                    ListName = model.Data.ListName.OrderByDescending(x => x).ToList()
                }
            };
        }
    }


    class ProgramSortingName : ExecCommand
    {
        public override Models.ResultModel CommandforSortingNameOrigin(string command)
        {
            var baseResult = base.CommandforSortingNameOrigin(command);

            if (baseResult.Status)
            {
                // several condition in sorting -> sorting by last third of name or by family name
                var _newListNameTemp = baseResult
                                    .Data
                                    .ListName
                                    .Select((Val, Idx) => new {
                                        FullName = Val,
                                        LastName = Val.Split(' ').Count() > 3 ? Val.Split(' ').ElementAt(2) + Val : Val.Split(' ').Last() + Val,
                                        Index = Idx
                                    })
                                    .OrderBy(l => l.LastName)
                                    .ToList();
                var newListName = _newListNameTemp.Select(x => x.FullName).ToList();

                var newRes = new Models.ResultModel
                {
                    Status = baseResult.Status,
                    Msg = "Success Sorting Name",
                    Data = new Models.SortingNameModel
                    {
                        FileName = "",
                        FullPath = baseResult.Data.FullPath,
                        ListName = newListName
                    }
                };


                return newRes;
            }
            else
            {
                return baseResult;
            }
        }
    }


    class ProgramInfo
    {
        public void ListTheNameinTXT()
        {
            Console.WriteLine(" ============================== \n");
            Console.WriteLine(" Welcome to this Application ");
            Console.WriteLine(" ______________________________ \n");
            Console.WriteLine(" + This Apllication used to Sorting List of Name from TXT file with Several Conditions ");
            Console.WriteLine(" + How to use this Application : ");
            Console.WriteLine("    - Type the Command with Format -> 'Convert FullPath + FileName of TXT file'");
            Console.WriteLine("    - Example - > 'name-sorter " + @"D:\Publish\testKST\unsorted-test.txt'");
            Console.WriteLine(" ============================== ");
        }
    }

    class ResultsProcess : IshowResultinScreen, ImakeFileTXT
    {


        public virtual void ViewDatainScreen(List<string> ListName)
        {
            Console.WriteLine("the Result...\n");
            foreach (var i in ListName)
            {
                Console.WriteLine(i);
            }

        }

        public virtual void MakeTXT(string fullPathName, List<String> ListName)
        {
            string newfullPathName = fullPathName.Replace("unsorted", "sorted");
            using (StreamWriter sw = System.IO.File.CreateText(newfullPathName))
            {
                foreach (var names in ListName)
                {
                    sw.WriteLine(names);
                }
            }
        }
    }

    
}
