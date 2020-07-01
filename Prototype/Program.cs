using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace Prototype
{
  #region IClonable - not good - typically shallow copy, must cast to object

  public class Person : ICloneable
  {
    public string[] Names;
    public Address Address;

    public Person(string[] names, Address address)
    {
      Names = names ?? throw new ArgumentNullException(paramName: nameof(names));
      Address = address ?? throw new ArgumentNullException(paramName: nameof(address));
    }

    public object Clone()
    {
      return new Person(Names, (Address)Address.Clone());
    }

    public override string ToString()
    {
      return $"{nameof(Names)}: {string.Join(" ", Names)}, {Address}";
    }
  }

  public class Address : ICloneable
  {
    public string StreetName;
    public int HouseNumber;

    public Address(string streetName, int houseNumber)
    {
      StreetName = streetName ?? throw new ArgumentNullException(paramName: nameof(streetName));
      HouseNumber = houseNumber;
    }

    public override string ToString()
    {
      return $"{nameof(StreetName)}: {StreetName}, {nameof(HouseNumber)}: {HouseNumber}";
    }

    public object Clone()
    {
      return new Address(StreetName, HouseNumber);
    }
  }

  #endregion

  #region Copy Constructors - common in C++, not C#

  public class CCPerson
  {
    public string[] Names;
    public CCAddress CCAddress;

    public CCPerson(string[] names, CCAddress address)
    {
      Names = names ?? throw new ArgumentNullException(paramName: nameof(names));
      CCAddress = address ?? throw new ArgumentNullException(paramName: nameof(address));
    }

    public CCPerson(CCPerson other)
    {
      Names = new string[other.Names.Length];
      other.Names.CopyTo(Names, 0);
      CCAddress = new CCAddress(other.CCAddress);
    }

    public override string ToString()
    {
      return $"{nameof(Names)}: {string.Join(" ", Names)}, {CCAddress}";
    }
  }

  public class CCAddress
  {
    public string StreetName;
    public int HouseNumber;

    public CCAddress(string streetName, int houseNumber)
    {
      StreetName = streetName ?? throw new ArgumentNullException(paramName: nameof(streetName));
      HouseNumber = houseNumber;
    }

    public CCAddress(CCAddress other)
    {
      StreetName = other.StreetName;
      HouseNumber = other.HouseNumber;
    }

    public override string ToString()
    {
      return $"{nameof(StreetName)}: {StreetName}, {nameof(HouseNumber)}: {HouseNumber}";
    }
  }

  #endregion

  #region Deep Copy Interface - requires every copied item to have interface

  public interface IPrototype<T>
  {
    T DeepCopy();
  }

  public class DCPerson : IPrototype<DCPerson>
  {
    public string[] Names;
    public DCAddress DCAddress;

    public DCPerson(string[] names, DCAddress address)
    {
      Names = names ?? throw new ArgumentNullException(paramName: nameof(names));
      DCAddress = address ?? throw new ArgumentNullException(paramName: nameof(address));
    }

    public DCPerson DeepCopy()
    {
      var names = new string[Names.Length];
      Names.CopyTo(names, 0);
      return new DCPerson(names, DCAddress.DeepCopy());
    }

    public override string ToString()
    {
      return $"{nameof(Names)}: {string.Join(" ", Names)}, {DCAddress}";
    }
  }

  public class DCAddress : IPrototype<DCAddress>
  {
    public string StreetName;
    public int HouseNumber;

    public DCAddress(string streetName, int houseNumber)
    {
      StreetName = streetName ?? throw new ArgumentNullException(paramName: nameof(streetName));
      HouseNumber = houseNumber;
    }

    public DCAddress DeepCopy()
    {
      return new DCAddress(StreetName, HouseNumber);
    }

    public override string ToString()
    {
      return $"{nameof(StreetName)}: {StreetName}, {nameof(HouseNumber)}: {HouseNumber}";
    }
  }

  #endregion

  #region Serializer - best option

  public static class ExtensionMethods
  {
    //serialize information to binary data, then deserialize it in new variable - process gives deep copy of all elements - binary formatter requires [Serializable] tag over elements
    public static T DeepCopy<T>(this T self)
    {
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, self);

      //reset location to 0 - was at end after serializing, must reset to deserialize
      stream.Seek(0, SeekOrigin.Begin);
      object copy = formatter.Deserialize(stream);
      stream.Close();
      return (T)copy;
    }

    //similar to above - no need for [Serializable] tag, but need for parameterless constructor - most serializers need something like this or have some kind of limits
    public static T DeepCopyXml<T>(this T self)
    {
      //using closes stream automatically on dispose
      using (var ms = new MemoryStream())
      {
        var s = new XmlSerializer(typeof(T));
        s.Serialize(ms, self);
        ms.Position = 0;
        return (T)s.Deserialize(ms);
      }
    }
  }

  // [Serializable]
  public class SPerson
  {
    public string[] Names;
    public SAddress SAddress;

    public SPerson()
    {

    }

    public SPerson(string[] names, SAddress address)
    {
      Names = names ?? throw new ArgumentNullException(paramName: nameof(names));
      SAddress = address ?? throw new ArgumentNullException(paramName: nameof(address));
    }

    public override string ToString()
    {
      return $"{nameof(Names)}: {string.Join(" ", Names)}, {SAddress}";
    }
  }

  // [Serializable]
  public class SAddress
  {
    public string StreetName;
    public int HouseNumber;

    public SAddress()
    {

    }

    public SAddress(string streetName, int houseNumber)
    {
      StreetName = streetName ?? throw new ArgumentNullException(paramName: nameof(streetName));
      HouseNumber = houseNumber;
    }

    public override string ToString()
    {
      return $"{nameof(StreetName)}: {StreetName}, {nameof(HouseNumber)}: {HouseNumber}";
    }
  }

  #endregion

  #region Coding Exercise

  public class Point
  {
    public int X, Y;

    public Point(int x, int y)
    {
      X = x;
      Y = y;
    }

    public Point() { }
  }

  public class Line
  {
    public Point Start, End;

    public Line(Point start, Point end)
    {
      Start = start;
      End = end;
    }

    public Line() { }

    public Line DeepCopy()
    {
      using (var ms = new MemoryStream())
      {
        var s = new System.Xml.Serialization.XmlSerializer(typeof(Line));
        s.Serialize(ms, this);
        ms.Position = 0;
        return (Line)s.Deserialize(ms);
      }
    }

    public override string ToString()
    {
      return $"{nameof(Start)} 1: ({Start.X}, {Start.Y}), {nameof(End)} 1: ({End.X}, {End.Y})";
    }
  }

  #endregion

  class Program
  {
    static void Main(string[] args)
    {
      // RunIClonable();
      // RunCopyConstructor();
      // RunDeepCopy();
      RunSerializer();
      // RunCodingExercise();
    }

    static void RunIClonable()
    {
      var john = new Person(new[] { "John", "Smith" },
      new Address("1st St", 123));

      var jane = (Person)john.Clone();
      jane.Names[0] = "Jane";
      jane.Address.HouseNumber = 321;

      System.Console.WriteLine(john);
      System.Console.WriteLine(jane);
    }

    static void RunCopyConstructor()
    {
      var john = new CCPerson(new[] { "John", "Smith" },
      new CCAddress("1st St", 123));

      var jane = new CCPerson(john);
      jane.Names[0] = "Jane";
      jane.CCAddress.HouseNumber = 321;

      System.Console.WriteLine(john);
      System.Console.WriteLine(jane);
    }

    static void RunDeepCopy()
    {
      var john = new DCPerson(new[] { "John", "Smith" }, new DCAddress("1st St", 123));

      var jane = john.DeepCopy();
      jane.Names[0] = "Jane";
      jane.DCAddress.HouseNumber = 321;

      System.Console.WriteLine(john);
      System.Console.WriteLine(jane);
    }

    static void RunSerializer()
    {
      var john = new SPerson(new[] { "John", "Smith" }, new SAddress("1st St", 123));

      var jane = john.DeepCopyXml();
      jane.Names[0] = "Jane";
      jane.SAddress.HouseNumber = 321;

      System.Console.WriteLine(john);
      System.Console.WriteLine(jane);
    }

    static void RunCodingExercise()
    {
      var pointA = new Point(0, 1);
      var pointB = new Point(2, 3);
      var line = new Line(pointA, pointB);

      var line2 = line.DeepCopy();
      line2.Start.X = 3;

      System.Console.WriteLine(line);
      System.Console.WriteLine(line2);
    }
  }
}
