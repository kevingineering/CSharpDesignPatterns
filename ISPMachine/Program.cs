using System;

namespace ISPMachine
{
  public class Document
  {

  }

  //one big interface - provides everything, and eventually violates interface segration princple
  public interface IMachine
  {
    void Print(Document d);
    void Scan(Document d);
    void Fax(Document d);
  }

  public interface IPrinter
  {
    void Print(Document d);
  }

  public interface IScanner
  {
    void Scan(Document d);
  }

  public interface IFaxer
  {
    void Fax(Document d);
  }

  //can do everything
  public class MultiFunctionPrinter : IMachine
  {
    public void Print(Document d)
    {

    }

    public void Scan(Document d)
    {

    }

    public void Fax(Document d)
    {

    }
  }

  //can only print - IMachine doesn't work
  public class OldFashionedPrinter : IPrinter  //IMachine - provides more than is necessary
  {
    public void Print(Document d)
    {

    }

    public void Scan(Document d)
    {
      throw new NotImplementedException();
    }

    public void Fax(Document d)
    {
      throw new NotImplementedException();
    }
  }

  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Hello World!");
    }
  }
}
