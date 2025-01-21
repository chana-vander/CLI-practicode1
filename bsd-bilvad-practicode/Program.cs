using System.CommandLine;
using System.ComponentModel;
// dotnet publish -o publish
//bsddd bundle --output f.txt --l jkjk --n --sort ab --r --a ch
var bundleCommand = new Command("bundle", "Bundle files to a single file");
var createCommand = new Command("create-rsp", "enter the options values");
//output:
var outputOption = new Option<FileInfo>("--output", "file path and name");
outputOption.AddAlias("--o");
bundleCommand.AddOption(outputOption);

//language
var languageOption = new Option<string>("--language", "choose programing language to enter the bunde file ") { IsRequired = true };
languageOption.AddAlias("--l");
bundleCommand.AddOption(languageOption);
var languagesMap = new Dictionary<string, string>
        {
            { "C#", ".cs" },
            { "Java", ".java" },
            { "Python", ".py" },
            { "JavaScript", ".js" },
            { "C++", ".Cpp" },
            { "Html",".Html" },
            { "Css",".Css" }
        };

//
List<string> excludedDirectories = new List<string>
    {
        "Debug",
        "public",
        "node_modules",
        "Lib", ".idea",
        ".itynb_checkpoints",
        "bin",
        "obj",
        "publish",
        "Migrations",
        "test",
        ".git"
    };
//note
var noteOption = new Option<bool>("--note", "name file and path");
noteOption.AddAlias("--n");
bundleCommand.AddOption(noteOption);
//sort
var sortOption = new Option<string>("--sort", "the order of files ab-default or code")
{
    Arity = ArgumentArity.ZeroOrOne // מאפשר גם לא לעביר ערך
};
sortOption.SetDefaultValue("ab");
/*var sortOption = new Option<string>("--sort", "the order of files ab-default or code")
{
    Arity = ArgumentArity.ZeroOrOne,
    DefaultValue = "ab"
};*/
sortOption.AddAlias("--s");
bundleCommand.AddOption(sortOption);
//remove-empty-lines
var removeEmptyLinesOoption = new Option<bool>("--remove_empty_lines", "enter true to remove empty lines false not");
removeEmptyLinesOoption.AddAlias("--r");
bundleCommand.AddOption(removeEmptyLinesOoption);
//author
var authorOption = new Option<string>("--author", "enter name of author");
authorOption.AddAlias("--a");
bundleCommand.AddOption(authorOption);
//setHandler
bundleCommand.SetHandler((FileInfo output, string language, bool note, string sort, bool remove, string author) =>
{
    try
    {
        if (output == null || string.IsNullOrWhiteSpace(output.FullName))
        {
            File.Create(output.FullName).Close();
            Console.WriteLine("output command succeed");
        }
        else
            Console.WriteLine("output invalid");
        List<string> filesToBundle = new List<string>();
        List<string> sortfiles = new List<string>();
        //הכנסת ניתובי הקבצים הרצוייים לתוך ליסט
        if (!string.IsNullOrEmpty(language))
        {
            if (language == "all")
            {
                filesToBundle = Directory.GetFiles(Directory.GetParent(output.FullName).FullName, "*", SearchOption.AllDirectories)
                    .Where(file =>
                    {
                        string directoryName = Path.GetFileName(Path.GetDirectoryName(file) ?? string.Empty);
                        return !excludedDirectories.Any(excluded => directoryName.Equals(excluded, StringComparison.OrdinalIgnoreCase)) &&
                               languagesMap.Values.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
                    }).ToList();
                Console.WriteLine("all");
            }
            else
            {
                string[] lanArr = language.Split(',');
                Console.WriteLine("arr split: ");
                foreach (var item in lanArr)
                {
                    Console.WriteLine(item);
                }
                foreach (var lan in lanArr)
                {
                    foreach (var l in languagesMap)
                    {
                        if (lan.Equals(l.Key, StringComparison.OrdinalIgnoreCase))  // השוואה לא רגישה לאותיות
                        {
                            filesToBundle.AddRange(Directory.GetFiles(Directory.GetParent(output.FullName).FullName, "*" + l.Value, SearchOption.AllDirectories)
                                .Where(file =>
                                {
                                    string directoryName = Path.GetFileName(Path.GetDirectoryName(file) ?? string.Empty);
                                    return !excludedDirectories.Any(excluded => directoryName.Equals(excluded, StringComparison.OrdinalIgnoreCase)) &&
                                           file.EndsWith(l.Value, StringComparison.OrdinalIgnoreCase);
                                }).ToList());
                            Console.WriteLine("language command bsd work");
                        }
                    }
                }
                if (filesToBundle.Count == 0)
                    Console.WriteLine("ERROR: you want invalid language");
            }
            //sort
            sortfiles = sortfiles.Where(file => !file.Equals(output.FullName, StringComparison.OrdinalIgnoreCase)).ToList();
            //הכנסת שמות הקבצים ממוינים
            sortfiles = (sort.ToLower() == "ab")
                ? filesToBundle.Where(file => !file.Equals(output.FullName, StringComparison.OrdinalIgnoreCase)).OrderBy(path => Path.GetFileName(path)).ToList() // מיון לפי שם הקובץ
                : filesToBundle.Where(file => !file.Equals(output.FullName, StringComparison.OrdinalIgnoreCase)).OrderBy(path => Path.GetExtension(path)).ToList(); // מיון לפי סיומת הקובץ
            Console.WriteLine("sort files: ");
            foreach (var file in sortfiles)
            {
                Console.WriteLine(file);
            }
            using (StreamWriter writerBundle = new StreamWriter(output.FullName, append: true))
            {
                if (author != null)
                    writerBundle.WriteLine("# name author is: " + author);
                foreach (var filePath in sortfiles)
                {
                    string[] content = File.ReadAllLines(filePath);

                    if (note)
                        writerBundle.WriteLine("# " + filePath);

                    if (remove)
                        content = content.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();

                    foreach (var line in content)
                        writerBundle.WriteLine(line);
                }
            }
        }
        else
            Console.WriteLine("ERROR: you must chhose programing language");

    }
    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine("ERROR: path invalid");
    }
}, outputOption, languageOption, noteOption, sortOption, removeEmptyLinesOoption, authorOption);
createCommand.SetHandler(() =>
{
    var rspFile = new FileInfo("rspFile.txt");
    Console.WriteLine("Enter values for the bundle command: ");
    using (StreamWriter rspWriter = new StreamWriter(rspFile.FullName))
    {
        string res = "";
        Console.WriteLine("What is your file path?");
        string output;
        while (string.IsNullOrWhiteSpace(output = Console.ReadLine()))
        {
            Console.WriteLine("Path cannot be empty. Please enter a valid path.");
        }
        rspWriter.WriteLine("--output " + output);
        Console.WriteLine("Which programming languages do you want to combine to your file only- \n(C#, Java, Python, JavaScript, C++, Html, Css) 'all' for all files");
        string language;
        while (string.IsNullOrWhiteSpace(language = Console.ReadLine()))
        {
            Console.WriteLine("Language cannot be empty. Please enter a valid language.");
        }
        rspWriter.WriteLine(" --language " + language);

        Console.WriteLine("Do you want to write the path as a comment? (y/n)");
        string note = Console.ReadLine().Trim().ToLower() == "y" ? "--note true" : "--note false";
        rspWriter.WriteLine(" " + note);

        Console.WriteLine("Enter the order to sort! (ab/code)");
        string sort = Console.ReadLine();
        res += " --sort " + sort;

        Console.WriteLine("Do you want to remove empty lines? (y/n)");
        string remove = Console.ReadLine().Trim().ToLower() == "y" ? "--remove_empty_lines true" : "--remove_empty_lines false";
        rspWriter.WriteLine(" " + remove);

        Console.WriteLine("Who is your author?");
        string author = Console.ReadLine();
        rspWriter.WriteLine("--author " + author);

        rspWriter.WriteLine(res);
        Console.WriteLine("Response file created successfully: " + rspFile.FullName);

        Console.WriteLine($"To run the command, use: bsddd dundle @{rspFile}");
    }
});
var rootCommand = new RootCommand("root command for bundle CLI");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createCommand);
rootCommand.InvokeAsync(args);