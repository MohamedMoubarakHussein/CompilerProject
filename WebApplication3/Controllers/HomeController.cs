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
        //Global List Data

        List<Tuple<int, Tuple<string, string>>> tokens = new List<Tuple<int, Tuple<string, string>>> { };

        // First Time Enter Website 
        public IActionResult Index( )
        {    
            return View(tokens);
        }


        // Handel The TextArea
        [HttpPost]
        public IActionResult Index2( )
        {
            string path = null , data;
            data = Request.Form["text"];

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
            // no Data Found in text area
            if(path == null)
            {
                return View();

            }
            // Found path Go and Get the data
            MyMain(path);
            return View(tokens);
        }




        // To handel The Browse Button
        [HttpPost]
        public IActionResult Index3(IFormFile file)
        {
            
           // no file selected
            if (file ==null||file.Length == 0)
            {
                return View(tokens);
            }

            //Get the path
            string path = Path.Combine(Directory.GetCurrentDirectory(), file.FileName);
            MyMain(path);

            return View(tokens);
        }



       
  

       //Main entry of my project
        public void MyMain(string path)
        {
            // Read The Content of file and convert it to <LineNumber , LineData>
            List<Tuple<int, string>> Myfile = ReadFile(path);
              
            // Store the token of every line in  temp 
            List<Tuple<int, Tuple<string, string>>> temp = new List<Tuple<int, Tuple<string, string>>> { };
            
            for (int i = 0; i < Myfile.Count; i++)
            {
                
                temp =ProcessLine(Myfile[i].Item2 , Myfile[i].Item1);
               
                for (int j =0; j < temp.Count; j++)
                {
                    tokens.Add(Tuple.Create(temp[j].Item1, Tuple.Create(temp[j].Item2.Item1 , temp[j].Item2.Item2)));
                }
            }


        }

        // Read file from sys
        public List<Tuple<int, string>> ReadFile(string path)
        {
            int counter = 1;
            // store <LineNumber , LineData>
            List<Tuple<int, string>> File = new List<Tuple<int, string>> { };

            using (StreamReader reader = new StreamReader(path))
            {
                string line = reader.ReadLine();

                while (line != null)
                {
                    File.Add(Tuple.Create(counter, line));
                    line = reader.ReadLine();
                    counter++;
                }
            }

            return File;
        }





        // Get token for every Line 
        int ErrorCounter = 0;
        public List<Tuple<int, Tuple<string, string>>> ProcessLine(string line , int lineNumber)
        {
            
            int state = 0, pointer = 0;
            
            // Store all token of the line  in  <LineNumber , <OriginalToken , ReturnKeyWord> > 
            List<Tuple<int, Tuple<string, string>>> retur = new List<Tuple<int, Tuple<string, string>>> { };
          
                // To handel all token in the line and make sure we in the line
            while (pointer < line.Length)
            {
                // Get the token
                string ss = Next_token(line, state, ref pointer);
                if(ss == "Using" && pointer == 5)
                {
                    string g = "";
                    for(int i = 5; i < line.Length; i++)
                    {
                        g += line[i];
                    }
                    MyMain(g.Trim());
                    break;
                }
                if (ss != "Skip"  && ss != "OutOfLine")
                {
                    if (ss == null)
                    {
                        ErrorCounter++;
                    }
                    else  if (ss[ss.Length - 1] == '^')
                    {

                        string sss = "";
                        for (int i = 0; i < ss.Length - 1; i++)
                            sss += ss[i];
                        retur.Add(Tuple.Create(lineNumber, Tuple.Create(sss, "Constant")));
                    } else
                    {
                        string ss_new = KeyWord(ss);
                        retur.Add(Tuple.Create(lineNumber, Tuple.Create(ss, ss_new)));
                    }
                }
                
               
            }
           
            return retur;

        }



      
     

        // Get the nextToken 
        public string Next_token(string line , int state ,ref int pointer)
        {
           // temp to store and return the originalData if it's accepted token
            string temp = "";
            char LastAcceptedSymbol = ' ';

            while (true)
            {
               
               
                switch (state)
                {
                  
                    case 0:
                        // check if pointer is in vaild location 
                        if(pointer >= line.Length)
                        {
                            return "OutOfLine";
                        }

                        char x = line[pointer++];
                        LastAcceptedSymbol = x;
                       
                        if (x == ' ')
                        {
                            state = 26;
                        }
                        else if (((int)x >= 65 && (int) x <= 90) || ((int)x >= 97 && (int)x <= 122) || x == '_')
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
                            temp += x;
                            state = 4;
                        }
                        else if (x == '|')
                        {
                            temp += x;
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
                        else if ((int)x-'0' >= 0 && (int)x - '0' <= 9)
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
                        else if (x == '*')
                        {
                            state = 30;
                        }
                        else if (x == ';')
                        {
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
                        if (((int)x >= 65 && (int)x <= 90) || ((int)x >= 97 && (int)x <= 122) || x == '_' 
                                                                                   || (int)x - '0' >= 0 && (int)x - '0' <= 9)
                        {
                            temp += x;
                            state = 1;
                        }
                        else
                        {
                            pointer--;
                            state = 2;

                        }
                        break;
          
                    
                    case 2:
                        return temp;

                       
                    case 3:
                        
                        return LastAcceptedSymbol.ToString();
                        

                    case 4:
                        if (pointer >= line.Length)
                        {
                            return "OutOfLine";
                        }
                        x = line[pointer++];
                        if(x == '&')
                        {
                            temp += x;
                            state = 5;
                        }
                        else
                        {
                            pointer--;
                            state = 999;
                        }
                        break;


                    case 5:
                        return temp;
                       

                    case 6:
                        
                        if (pointer >= line.Length)
                        {
                            return "OutOfLine";
                        }
                        x = line[pointer++];
                        if(x == '|')
                        {
                            temp += x;
                            state = 7;
                        }
                        else
                        {
                            pointer--;
                            state = 999;
                        }
                        break;

                    case 7:
                        return temp; 
                       

                    case 8:
                        return LastAcceptedSymbol.ToString();  
                        

                    case 9:
                        if (pointer >= line.Length)
                        {
                            return "OutOfLine";
                        }
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
                       

                    case 11:
                        return LastAcceptedSymbol.ToString();
                        

                    case  12:
                        if (pointer >= line.Length)
                        {
                            return "OutOfLine";
                        }
                        x = line[pointer++];
                        if (x == '=')
                        {
                            state = 13;
                        }
                        else if (x == '-') {
                           
                            state = 14;
                        }else
                        {
                            pointer--;
                            state = 15;
                        }
                        break;

                    case 13:
                        return "<=";
                        

                    case 14:
                        
                        return "<-";
                       

                    case 15:
                        return LastAcceptedSymbol.ToString();


                    case 16:
                        if (pointer >= line.Length)
                        {
                            return "OutOfLine";
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
                        

                    case  18:
                        return LastAcceptedSymbol.ToString();
                      
                    
                    case 19:
                        if (pointer >= line.Length)
                        {
                            return "OutOfLine";
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
                      
                    case 21:
                        return LastAcceptedSymbol.ToString();
                      

                    case 22:
                        return LastAcceptedSymbol.ToString();
                       

                    case 23:
                        if (pointer >= line.Length)
                        {
                            return temp;
                        }
                        x = line[pointer++];
                        if ((int)x - '0' >= 0 && (int)x - '0' <= 9)
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
                       

                    case 24:
                        temp += '^';
                        return temp;
                  

                    case 25:
                        return LastAcceptedSymbol.ToString();
                     

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
                        
                    case 28:
                        if (pointer >= line.Length)
                        {
                            return "OutOfLine";
                        }
                        x = line[pointer++];
                        if (x == '-')
                        {
                            state = 29;
                        }
                        else if (x == '>') {
                            state = 35;
                        }
                        else
                        {
                            pointer--;
                            state = 999;
                        }
                        break;


                    case 29:
                        return "--";
                    case 35:
                        return "--";

                    case 30:
                        if (pointer >= line.Length)
                        {
                            return "OutOfLine";
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
                        return null;
                     
                    default:
                        return null;
                          
                }
            }
        }



        public string KeyWord(string s)
        {
            string[] Key = { "Category", "Derive", "If", "Else", "Ilap", "Silap", "Clop", "Series", "Ilapf", "Silapf", "None",
                "Logical", "terminatethis","Rotatewhen", "Continuewhen" , "Replywith", "Seop", "Check", "situationof" , "Program",
                 "End" , "Using" , "+" , "-" , "*" , "/" , "&&" , "||" , "~" , "==" , "<" , ">" , "<=" , ">=" , "!=" ,"=" , ".","}","{"
            ,"[","]","(",")" , "\'" , "\"",";" , "->" ,"--" , "<-"};
            string[] value = { "Class", "Inheritance" , "Condition" , "Condition" , "Integer" , "SInteger", "Character", "String"
                            , "Float" , "SFloat" , "Void" , "Boolean" , "Terminite " , "Loop" ,"Loop", "Return" , "Struct" , "Switch" ,
                            "Switch" , "Stat Statement" , "End Statement" , "Inclusion","Arithmetic Operation","Arithmetic Operation",
            "Arithmetic Operation","Arithmetic Operation","Logic operators","Logic operators","Logic operators" ,"relationaloperators",
            "relationaloperators","relationaloperators","relationaloperators","relationaloperators","relationaloperators","Assignment operator",
            "Access Operator" , "Braces" , "Braces", "Braces", "Braces", "Braces", "Braces" ,"Quotation Mark" , "QuotationMark"
            ,"EndOFstatement" , "Comment" , "Comment" ,  "Comment" };
               
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
