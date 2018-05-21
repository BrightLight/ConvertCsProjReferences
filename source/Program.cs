﻿
namespace ConvertCsProjReferences
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Xml;

  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length < 1)
      {
        Console.WriteLine("Usage:");
        Console.WriteLine("???");
        Environment.Exit(-1);
      }

      var fileOrFolder = args[0];

      foreach (var csprojFile in GetFileNames(fileOrFolder))
      {
        var csProjContent = new XmlDocument();
        csProjContent.Load(csprojFile);
        var csProjUri = new Uri(csprojFile);

        foreach (XmlNode referenceNode in csProjContent.SelectNodes(@"//HintPath"))
        {
          var fileReference = referenceNode.Value;
          var newFileReference = Environment.ExpandEnvironmentVariables(fileReference);
          var newFileReferenceUri = new Uri(newFileReference);
          var relativeFileReference = newFileReferenceUri.MakeRelativeUri(csProjUri);
          referenceNode.Value = relativeFileReference.ToString();
        }

        var xmlWriterSettings = new XmlWriterSettings();
        xmlWriterSettings.Indent = true;
        xmlWriterSettings.IndentChars = "  ";
        var xmlWriter = XmlWriter.Create(csprojFile, xmlWriterSettings);
        csProjContent.WriteTo(xmlWriter);
        xmlWriter.Flush();
      }
    }

    private static IEnumerable<string> GetFileNames(string fileOrFolder)
    {
      if (File.Exists(fileOrFolder))
      {
        yield return fileOrFolder;
      }
      else
      {
        if (Directory.Exists(fileOrFolder))
        {
          foreach (var file in Directory.EnumerateFiles(fileOrFolder, "*.csproj"))
          {
            yield return file;
          }
        }
        else
        {
          Console.WriteLine($"{fileOrFolder} not found");
          Environment.Exit(-2);
        }
      }
    }
  } 
}
