using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Yeetify
{
    class Program
    {
        private static Random rng = new Random();
        public static bool inBlockComment = false;
        static void Main(string[] args)
        {
            string fileIn = @"C:\Users\walak\Desktop\History.c";
            string fileOut = @"C:\Users\walak\Desktop\YeetHistory.txt";
            yeetify(fileIn, fileOut);
        }

        static string unSubLine(Dictionary<string, string> sub, string line)
        {
            List<char> validBorder = new List<char> { ';', '{', '}', '(', ')', ' ', };

            // ignore if # statement
            if (line.Length > 0 && line[0] == '#')
            {
                return line;
            }


            string final = "";
            int i = 0;
            while (i < line.Length)
            {
                string stringToSub = null;
                string yeetId = null;

                int maxLength = -1;

                foreach (string key in sub.Keys)
                {
                    string value = null;
                    sub.TryGetValue(key, out value);
                    if ( allIndexesOf(line, key).Contains(i)  
                        && ( (i == 0) || validBorder.Contains(line[i-1]) ) 
                        && ( ( (i + key.Length) >= line.Length) || validBorder.Contains(line[i+key.Length]) ) )
                    {
                        // Take longest possible subsitution
                        if (key.Length > maxLength)
                        {
                            stringToSub = key;
                            yeetId = value;
                            maxLength = key.Length;
                        }
                    }
                }

                if (stringToSub != null)
                {
                    Console.WriteLine($"Found match: {stringToSub}");
                    final += $"{yeetId} ";
                    i += stringToSub.Length;
                }
                else
                {
                    // If comment, ignore the rest
                    if ((i + 1 < line.Length) && (line[i] == '/') && (line[i + 1] == '/'))
                    {
                        final += line.Substring(i, line.Length - i);
                        return final;
                    }
                    else
                    {
                        final += line[i];
                        i++;
                    }
                }
            }

            return final;
        }

        static Dictionary<string, string> getOriginalDefines(string fileIn)
        {
            Console.WriteLine("Getting original defines");
            Dictionary<string, string> result = new Dictionary<string, string>();
            string line;
            System.IO.StreamReader reader = new System.IO.StreamReader(fileIn);
            while ((line = reader.ReadLine()) != null)
            {
                if(line.IndexOf("#define") == 0)
                {

                    List<int> locations = allIndexesOf(line, " ");
                    if(locations.Count < 2)
                    {
                        Console.WriteLine($"Continue with line: {line}");
                        continue;
                    }
                    locations.Sort();

                    string key = line.Substring(locations[0]+1, locations[1]- locations[0] - 1);
                    string value = line.Substring(locations[1] + 1, line.Length - locations[1] - 1);
                    Console.WriteLine($"Key: {key}; Value: {value}");
                    result.Add(key, value);
                }
            }
            reader.Close();
            return result;
        }

        public static List<string> shuffle(List<string> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                string value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        static void undoProc(string fileIn, string fileOut)
        {
            // Undo current preprocessing
            Dictionary<string, string> originalDefines = getOriginalDefines(fileIn);

            string line;
            string final = "";
            System.IO.StreamReader reader = new System.IO.StreamReader(fileIn);
            while ((line = reader.ReadLine()) != null)
            {
                //Console.WriteLine($"Subsituting line: {line}");
                if((line.IndexOf("#define") != 0) || (allIndexesOf(line, " ").Count < 2) )
                {
                    string result = unSubLine(originalDefines, line);
                    //Console.WriteLine($"Result: {result}");
                    final += result;
                    final += "\r\n";
                }
            }
            reader.Close();

            File.WriteAllText(fileOut, final);
        }

        static void yeetify(string fileIn, string fileOut)
        {
            // Undo original preprocessing
            undoProc(fileIn, fileOut);

            // Get subsitutions
            List<string> sub = getSubsitutions(fileOut);
            inBlockComment = false;
            Console.WriteLine("----------------------FULL LIST---------------------------");
            foreach (string s in sub)
            {
                Console.WriteLine(s+"|");
            }

            // Convert string list to dictionary
            sub = shuffle(sub);
            Dictionary<string, string> subDict = new Dictionary<string, string>();
            for(int i = 0; i < sub.Count; i++)
            {
                subDict.Add(getYeetId(i), sub[i]);
            }


            Console.WriteLine("----------------------MAKING SUBSITUTIONS---------------------------");
            // Make subsitutions
            string line;
            string final = getDefines(sub);
            System.IO.StreamReader reader = new System.IO.StreamReader(fileOut);
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine($"Subsituting line: {line}");
                string result = subsituteLine(subDict, line);
                Console.WriteLine($"Result: {result}");
                final += result;
                final += "\r\n";
            }
            reader.Close();
            inBlockComment = false;
            File.WriteAllText(fileOut, final);
        }

        public static string getDefines(List<string> sub)
        {
            string final = "";

            for(int i = 0; i < sub.Count; i++)
            {
                final += "#define ";
                final += getYeetId(i);
                final += " " + sub[i];
                final += "\r\n";
            }

            return final;
        }

        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        static string getYeetId(int id)
        {
            string binary = Convert.ToString(id, 2);
            string final = "";

            while(binary.Length < 4)
            {
                binary = "0" + binary;
            }

            binary = Reverse(binary);

            for(int i = 0; i < binary.Length; i++)
            {
                if(i == 0)
                {
                    if(binary[i] == '1')
                    {
                        final = "T";
                    }
                    else
                    {
                        final = "t";
                    }
                }else if(i == binary.Length - 1)
                {
                    if (binary[i] == '1')
                    {
                        final = "Y" + final;
                    }
                    else
                    {
                        final = "y" + final;
                    }
                }
                else
                {
                    if (binary[i] == '1')
                    {
                        final = "E" + final;
                    }
                    else
                    {
                        final = "e" + final;
                    }
                }
            }
            return final;
        }

        public static List<int> allIndexesOf(string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }

        static string subsituteLine(Dictionary<string, string> sub, string line)
        {
            // ignore if # statement
            if(line.Length > 0 && line[0] == '#')
            {
                return line;
            }


            string final = "";
            int i = 0;
            while (i < line.Length)
            {
                if (inBlockComment)
                {
                    if ((i + 1 < line.Length) && line[i] == '*' && line[i + 1] == '/')
                    {
                        inBlockComment = false;
                        i += 2;
                    }
                    else
                    {
                        i++;
                    }
                }

                else if ((i + 1 < line.Length) && line[i] == '/' && line[i + 1] == '*')
                {
                    inBlockComment = true;
                    i += 2;
                }

                else if ((i + 1 < line.Length) && line[i] == '/' && line[i + 1] == '/')
                {
                    return final;
                }

                else
                {


                    string stringToSub = null;
                    string yeetId = null;

                    int maxLength = -1;

                    foreach (string key in sub.Keys)
                    {
                        string value = null;
                        sub.TryGetValue(key, out value);
                        if (allIndexesOf(line, value).Contains(i))
                        {
                            // Take longest possible subsitution
                            if (value.Length > maxLength)
                            {
                                stringToSub = value;
                                yeetId = key;
                                maxLength = value.Length;
                            }
                        }
                    }

                    if (stringToSub != null)
                    {
                        Console.WriteLine($"Found match: {stringToSub}");
                        final += $"{yeetId} ";
                        i += stringToSub.Length;
                    }
                    else
                    {
                        // If comment, ignore the rest
                        if ((i + 1 < line.Length) && (line[i] == '/') && (line[i + 1] == '/'))
                        {
                            final += line.Substring(i, line.Length - i);
                            return final;
                        }
                        else
                        {
                            final += line[i];
                            i++;
                        }
                    }
                }
            }
            return final;
        }

        static List<string> getSubsitutions(string filename)
        {
            List<string> sub = new List<string>();

            // Add automatic subsitutions
            sub.Add(";");
            sub.Add("(");
            sub.Add(")");
            sub.Add("{");
            sub.Add("}");
            sub.Add(",");

            string line;
            System.IO.StreamReader reader = new System.IO.StreamReader(filename);
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine($"Analyzing line: {line}");
                sub = parseLine(sub, line);
            }
            reader.Close();
            return sub;
        }

        static List<string> parseLine(List<string> current, string line)
        {
            List<char> ignore = new List<char> { ' ', '\t', '\n', ';', '{', '}', '(', ')', ',' };
            List<char> stoppingCharacters = new List<char> { ' ', '\t', '\n', ';', '{', '}', '(', ')', ',', '"', '/', '\'' };
            int i = 0;
            while (i < line.Length)
            {
                if (inBlockComment)
                {
                    if(line[i] == '*' && line[i+1] == '/')
                    {
                        inBlockComment = false;
                        i += 2;
                    }
                    else
                    {
                        i++;
                    }
                }
                else
                {
                    // Test for start of block comment
                    if ((i + 1 < line.Length) && (line[i] == '/') && (line[i + 1] == '*'))
                    {
                        inBlockComment = true;
                        i++;
                    }

                    // Test for characters to ignore
                    else if (ignore.Contains(line[i]))
                    {
                        i++;
                    }

                    // Ignore other preproccessor/include statements
                    else if (line[i] == '#')
                    {
                        // Ignore the rest of the line
                        Console.WriteLine($"Found #, ignoring line");
                        return current;
                    }

                    // Test for line comment
                    else if ((i + 1 < line.Length) && (line[i] == '/') && (line[i + 1] == '/'))
                    {
                        // Ignore the rest of the line
                        Console.WriteLine($"Found line comment, ignoring line");
                        return current;
                    }

                    // Test for quotes (Assumes there is a closing quote)
                    else if ((line[i] == '"') || (line[i] == '\''))
                    {
                        bool isQuote = (line[i] == '"');



                        int end = i + 1;
                        while (end < line.Length)
                        {

                            if(isQuote)
                            {
                                // Stopping requirements for quote
                                if (line[end] == '"' && line[end - 1] != '\\')
                                {
                                    end++; // Include last quote
                                    break;
                                }
                                end++;
                            }else{
                                // Stopping requirements for char
                                if (line[end] == '\'' && line[end - 1] != '\\')
                                {
                                    end++; // Include last quote
                                    break;
                                }
                                end++;
                            }
                            
                        }
                        string quoted = line.Substring(i, end - i);
                        Console.WriteLine($"Adding quoted string {quoted}");
                        addToList(current, quoted);
                        i = end + 1;
                    }
                    else
                    {
                        // Make string to next stopping character
                        int end = i + 1;
                        while (end < line.Length && !stoppingCharacters.Contains(line[end]))
                        {
                            end++;
                        }
                        string resultString = line.Substring(i, end - i);
                        Console.WriteLine($"Adding string {resultString}");
                        addToList(current, resultString);
                        i = end + 1;
                    }
                }
            }
            return current;
        }

        public static void addToList(List<string> list, string toAdd){
            if (list.Contains(toAdd))
            {
                return;
            }
            else
            {
                list.Add(toAdd);
            }
        }
    }
}
