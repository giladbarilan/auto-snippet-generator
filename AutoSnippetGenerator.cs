using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace ASG
{
    public class ASGenerator
    {
        //the project file to search the snippets on.
        private string DIR_NAME;
        //where the project snippets located
        private string SNIPPET_LOCATION;
        private string SNIPPET_DATA_ = @"SNIPPET_DATA.txt"; //location where we save the data about the snippets.

        private static void Main(string[] args)
        {    

        }

        /// <summary>
        /// </summary>
        /// <param name="projectDirName">The project directory where to make the auto snippet search.</param>
        /// <param name="snippetLocation">The snippet directory where to locate the snippets.</param>
        public ASGenerator(string projectDirName, string snippetLocation)
        {
            this.DIR_NAME = projectDirName;
            this.SNIPPET_LOCATION = snippetLocation.EndsWith('\\') ? snippetLocation
                                    : snippetLocation + '\\';

            //We do that because we want to put the SNIPPET_DATA_ file into the project files.
            SNIPPET_DATA_ = projectDirName.EndsWith('\\') ? projectDirName + SNIPPET_DATA_ :
                            projectDirName + "\\" + SNIPPET_DATA_;
        }

        /// <summary>
        /// Generates the snippet code.
        /// Could raise an error.
        /// </summary>
        public void Generate()
        {
            var ctorNames = GetConstructors(DIR_NAME); //get the default constructor names of classes.

            if (!File.Exists(SNIPPET_DATA_))
            {
                //if it's the first time then write the snippet file.
                File.WriteAllLines(SNIPPET_DATA_, ctorNames);
            }
            else
            {
                //remove the no longer exsisted classes from the snippets list.
                RemoveUnneededSnippet(ctorNames);
            }

            foreach (string ct in ctorNames)
            {
                CreateNewSnippet(ct, ct, ct);
            }

            File.WriteAllLines(SNIPPET_DATA_, ctorNames); //updates the file.
        }

        /// <summary>
        /// Removes the all auto generated Snippets from project.
        /// Note: if you've created your own snippet (not auto-generated) it will not delete it.
        /// </summary>
        public void RemoveAllCustomSnipets()
        {
            //Auto Generated Snippets file names
            string[] ags_FileNames = File.ReadAllLines(SNIPPET_DATA_);

            foreach (var ags in ags_FileNames)
            {
                File.Delete(SNIPPET_LOCATION + ags + ".snippet");
            }

            File.WriteAllText(SNIPPET_DATA_, ""); //resets the file.
        }

        /// <summary>
        /// Removes the no longer exsisted snippets.
        /// </summary>
        /// <param name="newSnippets">The current snipepts in the code.</param>
        internal void RemoveUnneededSnippet(List<string> newSnippets)
        {
            //get the old snippets
            string[] oldClassNames = File.ReadAllLines(SNIPPET_DATA_);
            List<string> removedClasses = new List<string>(); //list of the no longer exsisted classes.

            foreach (string class_ in oldClassNames)
            {
                //The class is no longer exsisted.
                if (!newSnippets.Contains(class_))
                {
                    removedClasses.Add(class_); //we add the class to the removed classes.
                }
            }

            foreach (string removedClass in removedClasses)
            {
                //we delete the snippets of the no longer exsisted classes.
                File.Delete(SNIPPET_LOCATION + removedClass + ".snippet");
            }
        }

        /// <summary>
        /// Returns all the constructors from cs scripts on the specified directory.
        /// </summary>
        /// <param name="dirName">The directory where we look for constructors.</param>
        /// <returns>Constructor names in the directory.</returns>
        internal List<string> GetConstructors(string dirName)
        {
            DirectoryInfo dirName_ = new DirectoryInfo(dirName);

            //get the filenames that ends with .cs in the project directory.
            string[] fileNames = dirName_.GetFiles().Where(x => x.FullName.EndsWith(".cs")).Select(x => x.FullName).ToArray();
            List<string> ctorNames = new List<string>();

            //iterates over the files.
            foreach (string fileName in fileNames)
            {
                if (File.Exists(fileName))
                {
                    string text = File.ReadAllText(fileName); //the text of the file

                    Regex matchDefaultCtor = new Regex(@$"public[ ]+\S+([ ]|{Environment.NewLine}|\t)*\([ ]*\)"); //search for a default constructor.
                    var defaultConstructors = matchDefaultCtor.Matches(text);

                    Regex matchStructs = new Regex(@$"struct[ ]+\S+([ ]|{Environment.NewLine}|\t)*{{"); //search for structs.
                    var structMatches = matchStructs.Matches(text);

                    //adds the structs to the default constructors.
                    ctorNames.AddRange(structMatches.Select(x => x.Value.TrimStart().TrimEnd().Split(" ")[1].Trim().Replace("{","")));

                    //returns all the ctor names in the file.
                    ctorNames.AddRange(defaultConstructors.Select(
                                         x => x.Value.TrimStart().TrimEnd().Split(" ")[1].Split("(")[0]));

                }
            }

            return ctorNames; //returns the constructor names.
        }

        /// <summary>
        /// Creates a snippet.
        /// </summary>
        /// <param name="typeName">The name of the type to make the default create for.</param>
        /// <param name="shortcutName">The name of the shortcut.</param>
        /// <param name="title">The title of the snippet.</param>
        /// <param name="mySnippetDirectory">Where the project's snippets located.</param>
        /// <param name="author">THe name of the author.</param>
        /// <param name="description">The description of the snippet.</param>
        internal void CreateNewSnippet(string typeName,
                                       string shortcutName,
                                       string title,
                                       string author = "DefaultAuthor",
                                       string description = "DefaultDescription")
        {

#region SnippetStructureString
            string snippet_structure = @$"<?xml version=""1.0"" encoding=""utf-8""?>
  <CodeSnippets xmlns = ""http://schemas.microsoft.com/VisualStudio/2005/CodeSnippet"">  
     <CodeSnippet Format = ""1.0.0"">   
         <Header>    
            <Title>{title}</Title>    
            <Author>{author}</Author>
            <Description> {description}</Description>
            <Shortcut> {shortcutName} </Shortcut>   
         </Header>   
         <Snippet>  
           <Code Language = ""CSharp"">    
                 <![CDATA[{typeName} $varName$ = new {typeName}();]]>     
             </Code>     
             <Declarations>      
                <Literal>      
                    <ID> varName </ID>      
                    <ToolTip> variable name.</ToolTip>         
                       <Default > obj </Default>        
                   </Literal>       
                </Declarations>         
             </Snippet>        
           </CodeSnippet>
         </CodeSnippets>";
#endregion

            //write the snippet into the snippets directory.
            File.WriteAllText(SNIPPET_LOCATION + typeName + ".snippet", snippet_structure);


        }

    }
}