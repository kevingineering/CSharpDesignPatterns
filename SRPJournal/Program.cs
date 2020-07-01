using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Journal
{
  // Journal that keeps list of entries where every entry starts with its count 
  public class Journal
  {
    //Initially had too much responsibility - not only kept entries, also loaded them, wrote them, etc
    private readonly List<string> entries = new List<string>();

    private static int count = 0; //number of elements

    public int AddEntry(string text) //returns number
    {
      entries.Add($"{++count}: {text}");
      return count; //memento
    }

    public void RemoveEntry(int index)
    {
      entries.RemoveAt(index);
    }

    public override string ToString()
    {
      return string.Join(Environment.NewLine, entries);
    }


    //Instead of doing what is below, split into seperate Persistence class

    // public void Save(string filename)
    // {
    //   File.WriteAllText(filename, ToString());
    // }

    // public static Journal Load(string filename)
    // {

    // }

    // public void Load(Uri uri)
    // {

    // }
  }

  public class Persistence
  {
    public void SaveToFile(Journal journal, string filename, bool overwrite = false)
    {
      if (overwrite || !File.Exists(filename))
      {
        File.WriteAllText(filename, journal.ToString());
      }
    }
  }

  public class Demo
  {
    static void Main(string[] args)
    {
      var j = new Journal();
      j.AddEntry("I started a new course today.");
      j.AddEntry("Tomorrow is Paul's bachelor party.");

      Console.WriteLine(j);

      var p = new Persistence();
      var filename = @"./";
      p.SaveToFile(j, filename, true);

      Process.Start(filename);
    }
  }
}
