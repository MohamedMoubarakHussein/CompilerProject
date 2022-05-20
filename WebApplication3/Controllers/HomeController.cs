using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebApplication3.Models;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index( )
        {
           
           List< Tuple<int, Tuple<string, string>> > ss = new List< Tuple<int, Tuple<string, string>> >{ };
               
            return View(ss);
        }
        [HttpPost]
        public IActionResult Index2( )
        {
            string path = null;
            string data = Request.Form["text"];
            if (data != null)
            {
                string[] lines = data.Split('\n');
                using (StreamWriter s = new StreamWriter("FileOFText2.txt"))
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        s.WriteLine(lines[i]);
                    }
                }
                 path = "FileOFText2.txt";
            }
            if(path == null)
            {
                return View();

            }
            MyMain(path);

            return View(tokens);
        }

        [HttpPost]
        public IActionResult Index3(IFormFile file)
        {
            string path="";
            List<Tuple<int, Tuple<string, string>>> ss = new List<Tuple<int, Tuple<string, string>>> { };
            if (file ==null||file.Length == 0)
            {
                 
                System.Diagnostics.Debug.WriteLine("The pathzz   " + path);
                return View(ss);
            }
             path = Path.Combine(Directory.GetCurrentDirectory(), file.FileName);
            System.Diagnostics.Debug.WriteLine("The path   "+path);
            MyMain(path);

            return View(tokens);
        }



        public IActionResult Privacy()
        {
           

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
  

        public List<Tuple<int ,string>> ReadFile(string path)
        {
            int counter = 1;
            List<Tuple<int, string>> File = new List<Tuple<int, string>> { };
            using (StreamReader reader = new StreamReader(path)) { 
            string line = reader.ReadLine();
           
            while(line != null)
            {
                File.Add(Tuple.Create(counter, line));
                line = reader.ReadLine();
                counter++;
            }
            }
            return File;
        }
        List<Tuple<int, Tuple<string, string>>> tokens = new List<Tuple<int, Tuple<string, string>>> { };
        public void MyMain(string path)
        {
            
           
   
          
            List<Tuple<int, string>> Myfile = ReadFile(path);
     
            List<Tuple<int, Tuple<string, string>>> temp = new List<Tuple<int, Tuple<string, string>>> { };
            
            for (int i = 0; i < Myfile.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine("Phase 2 "+i);
                temp =ProcessLine(Myfile[i].Item2 , Myfile[i].Item1);
                for (int j = 0; j < temp.Count; j++)
                {
                    System.Diagnostics.Debug.WriteLine(temp[j].Item1 + " " +temp[j].Item2);
                }
                for (int j =0; j < temp.Count; j++)
                {
                    tokens.Add(Tuple.Create(temp[j].Item1, Tuple.Create(temp[j].Item2.Item1 , temp[j].Item2.Item2)));
                }
            }

        }
        int LineIFsemi = 0;
        List<string> token = new List<string> { };
        public List<Tuple<int, Tuple<string, string>>> ProcessLine(string line , int lineNumber)
        {
            int state = 0, pointer = 0;
            List<Tuple<int, string>> tokens = new List<Tuple<int, string>> { };
            List<Tuple<int, Tuple<string, string>>> retur = new List<Tuple<int, Tuple<string, string>>> { };
          
                int newLine = lineNumber + LineIFsemi;
            while (pointer < line.Length)
            {
                //    System.Diagnostics.Debug.WriteLine("Before " + pointer);
                string ss = Next_token(line, state, ref pointer);
                System.Diagnostics.Debug.WriteLine("Beforez " + ss);
                if (ss != "Skip")
                {
                    if (ss == null)
                    {
                    }
                    else if (ss == "") { }
                    else
                   if (ss[ss.Length - 1] == '^')
                    {

                        string sss = "";
                        for (int i = 0; i < ss.Length - 1; i++)
                            sss += ss[i];
                        retur.Add(Tuple.Create(newLine, Tuple.Create(sss, "Constant")));
                    }
                    else
                   if (KeyWord(ss) != ss)
                    {
                        string ss_new = KeyWord(ss);
                        retur.Add(Tuple.Create(newLine, Tuple.Create(ss, ss_new)));
                    }
                    else
                    {
                        string ss_new = KeyWord(ss);
                        retur.Add(Tuple.Create(newLine, Tuple.Create(ss, ss_new)));
                    }
                }
                
               // System.Diagnostics.Debug.WriteLine("After " + pointer);
            }
                return retur;

        }
        Regex patternFirstLetter =new Regex( @"[_a-zA-Z]");
        Regex patternLetter =new Regex( @"[_a-zA-Z0-9]");
        Regex patternDigit = new Regex(@"[0-9]");
     
        public string Next_token(string line , int state ,ref int pointer)
        {
           
            string temp = "";
            int last_enter_error = 0;
            char xx = ' ';
            while (true)
            {
                // System.Diagnostics.Debug.WriteLine(temp);
               
                switch (state)
                {
                   
                    case 0:
                        if(pointer >= line.Length)
                        {
                            return "";
                        }
                        char x = line[pointer++];
                        xx = x;
                       
                        if (x == ' ')
                        {
                            state = 26;
                        }
                        else if (patternFirstLetter.IsMatch(x.ToString()))
                        {
                            temp += x;
                            state = 1;
                        }
                        else if (x == '+' || x == '-' || x == '*' || x == '/')
                        {
                        
                            state = 3;
                           
                        }
                        else if (x == '&')
                        {
                            state = 4;
                        }
                        else if (x == '|')
                        {
                            state = 6;
                        }
                        else if (x == '~')
                        {
                            state = 8;
                        }
                        else if (x == '=')
                        {
                            state = 9;
                        }
                        else if (x == '<')
                        {
                            state = 12;
                        }
                        else if (x == '>')
                        {
                            state = 16;
                        }
                        else if (x == '!')
                        {
                            state = 19;
                        }
                        else if (x == '.')
                        {
                            state = 21;
                        }
                        else if (x == '(' || x == ')' || x == '{' || x == '}' || x == '[' || x == ']')
                        {
                            state = 22;
                        }
                        else if (patternDigit.IsMatch(x.ToString()))
                        {
                            temp += x;
                            state = 23;
                        }
                        else if (x == '\'' || x == '\"')
                        {
                            state = 25;
                        }
                        else if (x == '-')
                        {
                            state = 28;
                        }
                        else if (last_enter_error == 1) {
                        
                            pointer++;
                            if (pointer >= line.Length)
                            {
                                return "";
                            }
                            last_enter_error = 0;
                            state = 0;
                        }
                        else if (x == '*')
                        {
                            state = 30;
                        }
                        else if (x == ';')
                        {
                            LineIFsemi++;
                            state = 33;
                        }
                        else
                        {
                            state = 999;
                        }

                        break;
                    case 1:
                        if (pointer >= line.Length)
                        {
                            return temp;
                        }
                        x = line[pointer++];
                        if (patternLetter.IsMatch(x.ToString()))
                        {
                            temp += x;
                            state = 1;
                        }
                        else
                        {
                          //  System.Diagnostics.Debug.WriteLine("I'm here");
                            pointer--;
                            state = 2;

                        }
                        break;
                    case 2:
                        // check if it ident or key and return

                        return temp;

                        break;
                    case 3:
                        
                        return xx.ToString();
                        break;

                    case 4:
                        if (pointer >= line.Length)
                        {
                            return "";
                        }
                        x = line[pointer++];
                        if(x == '&')
                        {
                            state = 5;
                        }
                        else
                        {
                            pointer--;
                            state = 999;
                        }
                        break;

                    case 5:
                        return "&&";
                        break;

                    case 6:
                        if (pointer >= line.Length)
                        {
                            return "";
                        }
                        x = line[pointer++];
                        if(x == '|')
                        {
                            state = 7;
                        }
                        else
                        {
                            pointer--;
                            state = 999;
                        }
                        break;

                    case 7:
                        return "||"; 
                        break;

                    case 8:
                        return xx.ToString();  
                        break;

                    case 9:
                        x = line[pointer++];
                        if (x == '=')
                        {
                            state = 10;
                        }
                        else
                        {
                            pointer--;
                            state = 11;
                        }
                        break;

                    case 10:
                        return "==";
                        break;

                    case 11:
                        return xx.ToString();
                        break;

                    case  12:
                        if (pointer >= line.Length)
                        {
                            return "";
                        }
                        x = line[pointer++];
                        if (x == '=')
                        {
                            state = 13;
                        }
                        else if (x == '*') {
                            state = 14;
                        }
                        {
                            pointer--;
                            state = 15;
                        }
                        break;

                    case 13:
                        return "<=";
                        break;

                    case 14:
                        return "comment ";
                        break;

                    case 15:
                        return xx.ToString();
                        break;

                    case 16:
                        if (pointer >= line.Length)
                        {
                            return "";
                        }
                        x = line[pointer++];
                        if (x == '=')
                        {
                            state = 17;
                        }
                        else
                        {
                            pointer--;
                            state = 18;
                        }

                        break;

                    case 17:
                        return ">=";
                        break;

                    case  18:
                        return xx.ToString();
                        break;
                    case 19:
                        if (pointer >= line.Length)
                        {
                            return "";
                        }
                        x = line[pointer++];
                        if (x == '=')
                        {
                            state = 20;
                        }
                        else
                        {
                            pointer--;
                            state = 999;
                        }
                        break;
                    case 20:
                        return "!=";
                        break;
                    case 21:
                        return xx.ToString();
                        break;
                    case 22:
                        return xx.ToString();
                        break;
                    case 23:
                        if (pointer >= line.Length)
                        {
                            return "";
                        }
                        x = line[pointer++];
                        if (patternDigit.IsMatch(x.ToString()))
                        {
                            temp += x;
                            state = 23;
                        }
                        else
                        {
                            pointer--;
                            state = 24;

                        }
                        break;
                        break;
                    case 24:
                        temp += '^';
                        return temp;
                        break;
                    case 25:
                        return xx.ToString();
                        break;
                    case 26:
                        if (pointer >= line.Length)
                        {
                            state= 27;
                            break;
                        }
                        x = line[pointer++];
                        if (x == ' ')
                        {
                            state = 26;
                        }
                        else
                        {
                            pointer--;
                            state = 27;
                        }
                        break;
                    case 27:
                        return "Skip";
                        break;
                    case 28:
                        if (pointer >= line.Length)
                        {
                            return "";
                        }
                        x = line[pointer++];
                        if (x == '-')
                        {
                            state = 29;
                        }
                        else
                        {
                            pointer--;
                            state = 999;
                        }
                        break;
                    case 29:
                        return "comment";
                        break;
                    case 30:
                        if (pointer >= line.Length)
                        {
                            return "";
                        }
                        x = line[pointer++];
                        if (x == '>')
                        {
                            state = 31;
                        }
                        else
                        {
                            pointer--;
                            state = 999;
                        }
                        break;
                    case 31:
                        return "comment";
                    case 33:
                        return "semiColon";
                    case 999:
                        last_enter_error = 1;
                        return null;
                        break;
                    default:
                        last_enter_error = 1;
                        return null;
                        break;   



                }
            }
        }

        public string KeyWord(string s)
        {
            string[] Key = { "Category", "Derive", "If", "Else", "Ilap", "Silap", "Clop", "Series", "Ilapf", "Silapf", "None",
                "Logical", "terminatethis","Rotatewhen", "Continuewhen" , "Replywith", "Seop", "Check", "situationof" , "Program",
                 "End" , "Using" , "+" , "-" , "*" , "/" , "&&" , "||" , "~" , "==" , "<" , ">" , "<=" , ">=" , "!=" ,"=" , ".","}","{"
            ,"[","]","(",")" , "\'" , "\"",";"};
            string[] value = { "Class", "Inheritance" , "Condition" , "Condition" , "Integer" , "SInteger", "Character", "String"
                            , "Float" , "SFloat" , "Void" , "Boolean" , "Terminite " , "Loop" ,"Loop", "Return" , "Struct" , "Switch" ,
                            "Switch" , "Stat Statement" , "End Statement" , "Inclusion","Arithmetic Operation","Arithmetic Operation",
            "Arithmetic Operation","Arithmetic Operation","Logic operators","Logic operators","Logic operators" ,"relationaloperators",
            "relationaloperators","relationaloperators","relationaloperators","relationaloperators","relationaloperators","Assignment operator",
            "Access Operator" , "Braces" , "Braces", "Braces", "Braces", "Braces", "Braces" ,"Quotation Mark" , "QuotationMark"
            ,"EndOFstatement"};
               
            for(int i =  0; i < Key.Length; i++)
            {
                if(s == Key[i])
                {
                    return value[i];
                }
            }
            return "IDENTIFIER";
        } 
    }
}
