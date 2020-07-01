using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Builders
{
  #region OOP Fluent Builder but not inheritable

  public class HtmlElement
  {
    public string Name, Text;
    public List<HtmlElement> Elements = new List<HtmlElement>();
    private const int indentSize = 2;

    public HtmlElement()
    {

    }

    public HtmlElement(string name, string text)
    {
      Name = name ?? throw new ArgumentNullException(paramName: nameof(name));
      Text = text ?? throw new ArgumentNullException(paramName: nameof(text));
    }

    private string ToStringImpl(int indent)
    {
      var sb = new StringBuilder();
      var i = new string(' ', indentSize * indent);
      sb.AppendLine($"{i}<{Name}>");

      if (!string.IsNullOrWhiteSpace(Text))
      {
        sb.Append(new string(' ', indentSize * (indent + 1)));
        sb.AppendLine(Text);
      }

      foreach (var e in Elements)
      {
        sb.Append(e.ToStringImpl(indent + 1)); //recursive
      }
      sb.AppendLine($"{i}</{Name}>");

      return sb.ToString();
    }

    public override string ToString()
    {
      return ToStringImpl(0);
    }
  }

  public class HtmlBuilder
  {
    private readonly string rootName;
    HtmlElement root = new HtmlElement();

    public HtmlBuilder(string rootName)
    {
      this.rootName = rootName;
      root.Name = rootName;
    }

    public HtmlBuilder AddChild(string childName, string childText)
    {
      var e = new HtmlElement(childName, childText);
      root.Elements.Add(e);
      return this; //makes fluent
    }

    public override string ToString()
    {
      return root.ToString();
    }

    public void Clear()
    {
      root = new HtmlElement { Name = rootName };
    }
  }

  #endregion

  #region OOP Fluent Builder with inheritance via recursive generics 
  //Person Builder - fluent interface, uses recursive generics to support inheritance 
  public class Person
  {
    public string Name;
    public string Position;

    public class Builder : PersonJobBuilder<Builder>
    {

    }

    public static Builder New => new Builder();

    public override string ToString()
    {
      return $"{nameof(Name)}: {Name}, {nameof(Position)}: {Position}";
    }
  }

  public abstract class PersonBuilder
  {
    protected Person person = new Person();

    public Person Build()
    {
      return person;
    }
  }

  //restrict SELF - class Foo : Bar<Foo>
  public class PersonInfoBuilder<SELF> : PersonBuilder where SELF : PersonInfoBuilder<SELF>
  {

    public SELF Called(string name)
    {
      person.Name = name;
      return (SELF)this;
    }
  }

  public class PersonJobBuilder<SELF> : PersonInfoBuilder<SELF> where SELF : PersonJobBuilder<SELF>
  {
    public SELF WorksAsA(string position)
    {
      person.Position = position;
      return (SELF)this;
    }
  }

  #endregion

  #region Functional Fluent Builder with extensibility (rather than inheritance)

  //TSubject is type of subject we are building
  //TSelf is concrete implementation of builder that inherits from this class
  public abstract class FunctionalBuilder<TSubject, TSelf>
    where TSelf : FunctionalBuilder<TSubject, TSelf>
    where TSubject : new()
  {
    private readonly List<Func<TSubject, TSubject>> actions = new List<Func<TSubject, TSubject>>();

    public TSelf Do(Action<TSubject> action)
    {
      return AddAction(action);
    }

    public TSelf AddAction(Action<TSubject> action)
    {
      actions.Add(p =>
      {
        action(p);
        return p;
      });
      return (TSelf)this;
    }

    public TSubject Build()
    {
      //compact list into single application
      return actions.Aggregate(new TSubject(), (p, f) => f(p));
    }
  }

  public class FPerson
  {
    public string Name, Position;

    public override string ToString()
    {
      return $"{nameof(Name)}: {Name}, {nameof(Position)}: {Position}";
    }
  }

  //sealed means it cannot be inherited - must use extension methods
  //extension methods work because we provide list of mutating functions
  public sealed class FPersonBuilder : FunctionalBuilder<FPerson, FPersonBuilder>
  {
    public FPersonBuilder Called(string name)
    {
      return Do(p => p.Name = name);
    }
  }

  //following OCP, this is how we extend FPersonBuilder
  public static class FPersonBuilderExtensions
  {
    public static FPersonBuilder WorksAsA(this FPersonBuilder builder, string position)
    {
      return builder.Do(p => p.Position = position);
    }
  }

  #region Original implementation
  // //sealed means it cannot be inherited - must use extension methods
  // //extension methods work because we provide list of mutating functions
  // public sealed class FPersonBuilder
  // {
  //   private readonly List<Func<FPerson, FPerson>> actions = new List<Func<FPerson, FPerson>>();

  //   public FPersonBuilder Called(string name)
  //   {
  //     return Do(p => p.Name = name);
  //   }

  //   public FPersonBuilder Do(Action<FPerson> action)
  //   {
  //     return AddAction(action);
  //   }

  //   public FPerson Build()
  //   {
  //     //compact list into single application
  //     return actions.Aggregate(new FPerson(), (p, f) => f(p));
  //   }

  //   private FPersonBuilder AddAction(Action<FPerson> action)
  //   {
  //     actions.Add(p =>
  //     {
  //       action(p);
  //       return p;
  //     });
  //     return this; //makes fluent
  //   }
  // }

  // //following OCP, this is how we extend FPersonBuilder
  // public static class FPersonBuilderExtensions
  // {
  //   public static FPersonBuilder WorksAsA(this FPersonBuilder builder, string position)
  //   {
  //     return builder.Do(p => p.Position = position);
  //   }
  // }

  #endregion

  #endregion

  #region Faceted Builder

  public class Employee
  {
    //address
    public string StreetAddress, Postcode, City;

    //employment
    public string CompanyName, Position;
    public int AnnualIncome;

    public override string ToString()
    {
      return $"{nameof(StreetAddress)}: {StreetAddress}, {nameof(Postcode)}: {Postcode}, {nameof(City)}: {City}, {nameof(CompanyName)}: {CompanyName}, {nameof(Position)}: {Position}, {nameof(AnnualIncome)}: {AnnualIncome}, ";
    }
  }

  //facade - keeps reference to object being built and allows access to subbuilders
  public class EmployeeBuilder
  {
    //reference
    protected Employee employee = new Employee();

    public EmployeeJobBuilder Works => new EmployeeJobBuilder(employee);
    public EmployeeAddressBuilder Lives => new EmployeeAddressBuilder(employee);

    //implicit casts the returned element to the type required 
    public static implicit operator Employee(EmployeeBuilder eb)
    {
      return eb.employee;
    }
  }

  public class EmployeeJobBuilder : EmployeeBuilder
  {
    public EmployeeJobBuilder(Employee employee)
    {
      this.employee = employee;
    }

    public EmployeeJobBuilder At(string companyname)
    {
      employee.CompanyName = companyname;
      return this;
    }

    public EmployeeJobBuilder AsA(string position)
    {
      employee.Position = position;
      return this;
    }

    public EmployeeJobBuilder Earning(int annualIncome)
    {
      employee.AnnualIncome = annualIncome;
      return this;
    }
  }

  public class EmployeeAddressBuilder : EmployeeBuilder
  {
    public EmployeeAddressBuilder(Employee employee)
    {
      this.employee = employee;
    }

    public EmployeeAddressBuilder At(string streetAddress)
    {
      employee.StreetAddress = streetAddress;
      return this;
    }

    public EmployeeAddressBuilder WithPostcode(string postcode)
    {
      employee.Postcode = postcode;
      return this;
    }

    public EmployeeAddressBuilder In(string city)
    {
      employee.City = city;
      return this;
    }
  }

  #endregion

  #region Coding Exercise 

  public class CodeElement
  {
    public string ClassName, FieldName, FieldType;
    public List<CodeElement> Fields = new List<CodeElement>();
    private const int indentSize = 2;

    public CodeElement()
    {

    }

    public CodeElement(string fieldName, string fieldType)
    {
      FieldName = fieldName ?? throw new ArgumentNullException(paramName: nameof(fieldName));
      FieldType = fieldType ?? throw new ArgumentNullException(paramName: nameof(fieldType));
    }

    private string ToStringElement(int indent)
    {
      var sb = new StringBuilder();
      var i = new string(' ', indentSize * indent);
      sb.AppendLine($"{i}public {FieldType} {FieldName};");
      return sb.ToString();
    }

    private string ToStringImpl(int indent)
    {
      var sb = new StringBuilder();
      var i = new string(' ', indentSize * indent);
      sb.AppendLine($"{i}public class {ClassName}");
      sb.AppendLine($"{i}{"{"}");

      foreach (var f in Fields)
      {
        sb.Append(f.ToStringElement(indent + 1));
      }

      sb.AppendLine($"{i}{"}"}");

      return sb.ToString();
    }

    public override string ToString()
    {
      return ToStringImpl(0);
    }
  }

  public class CodeBuilder
  {
    private readonly string rootName;
    CodeElement root = new CodeElement();

    public CodeBuilder(string rootName)
    {
      this.rootName = rootName;
      root.ClassName = rootName;
    }

    public CodeBuilder AddField(string fieldName, string fieldType)
    {
      var e = new CodeElement(fieldName, fieldType);
      root.Fields.Add(e);
      return this;
    }

    public override string ToString()
    {
      return root.ToString();
    }
  }

  // public class 

  #endregion

  public class Demo
  {
    static void RunCodingExercise()
    {
      var cb = new CodeBuilder("Person").AddField("Name", "string").AddField("Age", "int");
      Console.WriteLine(cb);

      //returns
      // public class Person
      // {
      //   public string Name;
      //   public int Age;
      // }
    }

    static void Main(string[] args)
    {
      //RunSimpleStringBuilder();
      //RunHtmlBuilder();
      //RunPersonBuilder();
      //RunFunctionalBuilder();
      //RunFacetedBuilder();
      RunCodingExercise();
    }

    #region Methods

    static void RunSimpleStringBuilder()
    {
      var hello = "hello";
      var sb = new StringBuilder();
      sb.Append("<p>");
      sb.Append(hello);
      sb.Append("</p>");
      Console.WriteLine(sb);

      var words = new[] { "hello", "world" };
      sb.Clear();
      sb.Append("<ul>");
      foreach (var word in words)
      {
        sb.AppendFormat("<li>{0}</li>", word);
      }
      sb.Append("</ul>");
      Console.WriteLine(sb);
    }

    static void RunHtmlBuilder()
    {
      //html builder
      var builder = new HtmlBuilder("ul");
      builder.AddChild("li", "hello");
      builder.AddChild("li", "world");
      System.Console.WriteLine(builder.ToString());

      //fluent html builder (added return to add child method)
      builder.Clear();
      builder.AddChild("li", "hello").AddChild("li", "world");
      System.Console.WriteLine(builder.ToString());
    }

    static void RunPersonBuilder()
    {
      //.Called returns PersonJobBuilder<Person.Builder>
      var kevin = Person.New.Called("kevin").WorksAsA("student").Build();
      System.Console.WriteLine(kevin);
    }

    static void RunFunctionalBuilder()
    {
      var person = new FPersonBuilder().Called("Kevin").WorksAsA("Student").Build();
      System.Console.WriteLine(person);
    }

    static void RunFacetedBuilder()
    {
      var eb = new EmployeeBuilder();
      Employee employee = eb
        .Works.AsA("Student").At("Panera").Earning(2)
        .Lives.In("La Vista").At("7728 S 71st St").WithPostcode("68128");
      System.Console.WriteLine(employee);
    }

    #endregion
  }
}
