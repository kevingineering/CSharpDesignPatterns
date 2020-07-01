using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Factories
{
  #region Motivation for factory
  //imagine if you want this class to take both planar and polar coordinates - can't overload constructor with both planar and 
  public enum CoordinateSystem
  {
    Cartesian,
    Polar
  }

  //Must explain how this works with xml
  /// <summary>
  ///   Initializes point from either Cartesian or Polar
  /// </summary>
  /// <param name="a">x if Cartesian, rho if polar</param>
  /// <param name="b"></param>
  /// <param name="system"></param>
  public class Point
  {
    //don't want to name x and y, because could also be rho and theta
    private double x, y;

    public Point(double a, double b, CoordinateSystem system = CoordinateSystem.Cartesian)
    {
      switch (system)
      {
        case CoordinateSystem.Cartesian:
          x = a;
          y = b;
          break;
        case CoordinateSystem.Polar:
          x = a * Math.Cos(b);
          y = a * Math.Sin(b);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(system), system, null);
      }
    }
  }
  #endregion

  #region Factory Method

  public class FMPoint
  {
    private double x, y;

    //cartesian factory method
    public static FMPoint NewCartesianPoint(double x, double y)
    {
      return new FMPoint(x, y);
    }

    //polar factory method
    public static FMPoint NewPolarPoint(double rho, double theta)
    {
      return new FMPoint(rho * Math.Cos(theta), rho * Math.Sin(theta));
    }

    //notice constructor is private
    private FMPoint(double x, double y)
    {
      this.x = x;
      this.y = y;
    }

    public override string ToString()
    {
      return $"{nameof(x)}: {x}, {nameof(y)}: {y}";
    }
  }

  #endregion

  #region Async Factory Method

  public class Foo
  {
    private string name;
    //cannot await in constructor
    private Foo(string name)
    {
      this.name = name;
    }

    //fluent async method
    private async Task<Foo> InitAsync()
    {
      this.name = "Overriden";
      await Task.Delay(1000); //pretend delay is db call
      return this;
    }

    //factory method
    public static Task<Foo> CreateAsync(string name)
    {
      var result = new Foo(name);
      return result.InitAsync();
    }

    public override string ToString()
    {
      return $"{nameof(name)}: {name}";
    }
  }

  #endregion

  #region Factory 

  //external factory class - must use public constructor which is undesirable
  //   public static class FPointFactory
  //   {
  //     //cartesian factory method
  //     public static FPoint NewCartesianPoint(double x, double y)
  //     {
  //       return new FPoint(x, y);
  //     }

  //     //polar factory method
  //     public static FPoint NewPolarPoint(double rho, double theta)
  //     {
  //       return new FPoint(rho * Math.Cos(theta), rho * Math.Sin(theta));
  //     }
  //   }

  public class FPoint
  {
    private double x, y;

    //notice constructor cannot be private because it must be called by factory
    //internal works for external class if code is used by someone in different assembly
    private FPoint(double x, double y)
    {
      this.x = x;
      this.y = y;
    }

    public override string ToString()
    {
      return $"{nameof(x)}: {x}, {nameof(y)}: {y}";
    }

    public static class Factory
    {
      //cartesian factory method
      public static FPoint NewCartesianPoint(double x, double y)
      {
        return new FPoint(x, y);
      }

      //polar factory method
      public static FPoint NewPolarPoint(double rho, double theta)
      {
        return new FPoint(rho * Math.Cos(theta), rho * Math.Sin(theta));
      }
    }
  }
  #endregion

  #region Abstract Factory - both violating OCP and not
  //don't return type you are creating, you return abstract classes or interfaces
  public interface IHotDrink
  {
    void Consume();
  }

  internal class Tea : IHotDrink
  {
    public void Consume()
    {
      System.Console.WriteLine("This tea is good.");
    }
  }

  internal class Coffee : IHotDrink
  {
    public void Consume()
    {
      System.Console.WriteLine("This coffee is from Panera.");
    }
  }

  public interface IHotDrinkFactory
  {
    IHotDrink Prepare(int amount);
  }

  internal class TeaFactory : IHotDrinkFactory
  {
    public IHotDrink Prepare(int amount)
    {
      System.Console.WriteLine($"Make {amount} oz tea.");
      return new Tea();
    }
  }

  internal class CoffeeFactory : IHotDrinkFactory
  {
    public IHotDrink Prepare(int amount)
    {
      System.Console.WriteLine($"Make {amount} oz coffee");
      return new Coffee();
    }
  }

  //using reflection to avoid violating OCP
  public class HotDrinkMachine
  {
    private List<Tuple<string, IHotDrinkFactory>> factories = new List<Tuple<string, IHotDrinkFactory>>();

    public HotDrinkMachine()
    {
      foreach (var t in typeof(HotDrinkMachine).Assembly.GetTypes())
      {
        if (typeof(IHotDrinkFactory).IsAssignableFrom(t) && !t.IsInterface)
        {
          factories.Add(Tuple.Create(
            t.Name.Replace("Factory", string.Empty),
            (IHotDrinkFactory)Activator.CreateInstance(t)
          ));
        }
      }
    }

    public IHotDrink MakeDrink()
    {
      System.Console.WriteLine("Available drinks:");
      for (var index = 0; index < factories.Count; index++)
      {
        System.Console.WriteLine($"{index}: {factories[index].Item1}");
      }

      while (true)
      {
        string s;
        if ((s = Console.ReadLine()) != null && int.TryParse(s, out int i) && i >= 0 && i < factories.Count)
        {
          while (true)
          {
            System.Console.WriteLine($"Specify amount (in oz): ");
            s = Console.ReadLine();
            if (s != null && int.TryParse(s, out int amount) && amount > 0)
            {
              return factories[i].Item2.Prepare(amount);
            }

            Console.WriteLine("Incorrect input, please try again.");
          }
        }

        Console.WriteLine("Incorrect input, please try again.");
      }
    }
  }

  //notice enum violates OCP because if you want to add Milk, you have to enter code and change enum
  //   public class HotDrinkMachine
  //   {
  //     public enum AvailableDrink
  //     {
  //       Coffee, Tea
  //     }

  //     private Dictionary<AvailableDrink, IHotDrinkFactory> factories = new Dictionary<AvailableDrink, IHotDrinkFactory>();

  //     public HotDrinkMachine()
  //     {
  //       foreach (AvailableDrink drink in Enum.GetValues(typeof(AvailableDrink)))
  //       {
  //         var factory = (IHotDrinkFactory)Activator.CreateInstance(Type.GetType("Factories." + Enum.GetName(typeof(AvailableDrink), drink) + "Factory"));
  //         factories.Add(drink, factory);
  //       }
  //     }

  //     public IHotDrink MakeDrink(AvailableDrink drink, int amount)
  //     {
  //       return factories[drink].Prepare(amount);
  //     }
  //   }

  #endregion

  #region Coding Exercise

  //nonstatic PersonFactory
  //CreatePerson method that takes name
  //Id is set as 0 based index

  public class PersonFactory
  {
    private int count = -1;

    public Person CreatePerson(string name)
    {
      count++;
      return new Person(name, count);
    }
  }

  public class Person
  {
    public int Id { get; set; }
    public string Name { get; set; }

    public Person(string name, int count)
    {
      this.Id = count;
      this.Name = name;
    }

    public override string ToString()
    {
      return $"{Id}: {Name}";
    }
  }

  #endregion

  public class Demo
  {
    static void Main(string[] args)
    {
      //   RunFactoryMethod();
      //   await RunAsyncFactoryMethod();
      //   RunFactory();
      //   RunAbstractFactory();
      RunCodingExercise();
    }

    static void RunFactoryMethod()
    {
      var cartPoint = FMPoint.NewCartesianPoint(3, 4);
      var polPoint = FMPoint.NewPolarPoint(1, Math.PI / 2);
      System.Console.WriteLine("cartPoint: " + cartPoint);
      System.Console.WriteLine("polPoint: " + polPoint);
    }

    static async Task RunAsyncFactoryMethod()
    {
      //code without factory - coder may not call InitAsync, which is not good
      //  var foo = new Foo();
      //  await foo.InitAsync();

      //using factory method
      var foo = await Foo.CreateAsync("Kevin");
      System.Console.WriteLine(foo);
    }

    static void RunFactory()
    {
      //external class with public constructor
      //   var cartPoint = FPointFactory.NewCartesianPoint(3, 4);
      //   var polPoint = FPointFactory.NewPolarPoint(1, Math.PI / 2);

      //internal class with private constructor
      var cartPoint = FPoint.Factory.NewCartesianPoint(3, 4);
      var polPoint = FPoint.Factory.NewPolarPoint(1, Math.PI / 2);
      System.Console.WriteLine("cartPoint: " + cartPoint);
      System.Console.WriteLine("polPoint: " + polPoint);
    }

    static void RunAbstractFactory()
    {
      var machine = new HotDrinkMachine();
      machine.MakeDrink();
      //method that violates OCP
      //   var drink = machine.MakeDrink(HotDrinkMachine.AvailableDrink.Tea, 20);
      //   drink.Consume();

      //method using reflection
    }

    static void RunCodingExercise()
    {
      var pf = new PersonFactory();
      var a = pf.CreatePerson("a");
      var b = pf.CreatePerson("b");
      var c = pf.CreatePerson("c");
      var d = pf.CreatePerson("d");
      System.Console.WriteLine(a);
      System.Console.WriteLine(b);
      System.Console.WriteLine(c);
      System.Console.WriteLine(d);
    }
  }
}
