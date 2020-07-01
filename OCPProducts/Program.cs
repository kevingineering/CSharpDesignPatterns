using System;
using System.Collections.Generic;

namespace OCPProducts
{
  public enum Color
  {
    Red, Green, Blue
  }

  public enum Size
  {
    Small, Medium, Large, XLarge
  }

  public class Product
  {
    public string Name;
    public Color Color;
    public Size Size;

    public Product(string name, Color color, Size size)
    {
      if (name == null)
      {
        throw new ArgumentNullException(paramName: nameof(name));
      }
      Name = name;
      Color = color;
      Size = size;
    }
  }

  //original code violated open closed principle - implementation does not allow for extension
  //instead to add another filter (e.g. color and size) you must modify original code
  //   public class ProductFilter
  //   {
  //     public IEnumerable<Product> FilterBySize(IEnumerable<Product> products, Size size)
  //     {
  //       foreach (var p in products)
  //       {
  //         if (p.Size == size)
  //         {
  //           yield return p;
  //         }
  //       }
  //     }

  //     public IEnumerable<Product> FilterByColor(IEnumerable<Product> products, Color color)
  //     {
  //       foreach (var p in products)
  //       {
  //         if (p.Color == color)
  //         {
  //           yield return p;
  //         }
  //       }
  //     }
  //   }

  #region SPECIFICATION PATTERN

  //Allow people to make specifications and check whether particular item of type T satisfites some criteria 
  public interface ISpecification<T>
  {
    bool IsSatisfied(T t);
  }

  //take type T items and filter them according to specification
  public interface IFilter<T>
  {
    IEnumerable<T> Filter(IEnumerable<T> items, ISpecification<T> spec);
  }

  public class ColorSpecification : ISpecification<Product>
  {
    private Color color;

    public ColorSpecification(Color color)
    {
      this.color = color;
    }

    public bool IsSatisfied(Product t)
    {
      return t.Color == color;
    }
  }

  public class SizeSpecification : ISpecification<Product>
  {
    private Size size;

    public SizeSpecification(Size size)
    {
      this.size = size;
    }

    public bool IsSatisfied(Product t)
    {
      return t.Size == size;
    }
  }

  //combinator
  public class AndSpecification<T> : ISpecification<T>
  {
    private ISpecification<T>[] specs;

    public AndSpecification(ISpecification<T>[] specs)
    {
      this.specs = specs;
    }

    public bool IsSatisfied(T t)
    {
      bool isSatisfied = true;
      foreach (var spec in specs)
      {
        if (!spec.IsSatisfied(t))
        {
          isSatisfied = false;
        }
      }
      return isSatisfied;
    }
  }

  public class ProductFilter : IFilter<Product>
  {
    public IEnumerable<Product> Filter(IEnumerable<Product> items, ISpecification<Product> spec)
    {
      foreach (var i in items)
      {
        if (spec.IsSatisfied(i))
        {
          yield return i;
        }
      }
    }
  }

  #endregion



  public class Demo
  {
    static void Main(string[] args)
    {
      var apple = new Product("Apple", Color.Green, Size.Small);
      var tree = new Product("Tree", Color.Green, Size.Large);
      var house = new Product("House", Color.Blue, Size.Large);

      Product[] products = { apple, tree, house };

      var pf = new ProductFilter();

      Console.WriteLine("Green products: ");
      foreach (var p in pf.Filter(products, new ColorSpecification(Color.Green)))
      {
        Console.WriteLine(p.Name);
      }

      Console.WriteLine("Large products: ");
      foreach (var p in pf.Filter(products, new SizeSpecification(Size.Large)))
      {
        Console.WriteLine(p.Name);
      }

      Console.WriteLine("Large green products: ");
      var specs = new ISpecification<Product>[] {
        new SizeSpecification(Size.Large),
        new ColorSpecification(Color.Green)
      };

      foreach (var p in pf.Filter(products, new AndSpecification<Product>(specs)))
      {
        Console.WriteLine(p.Name);
      }

      //original implementation
      //   var pf = new ProductFilter();
      //   Console.WriteLine("Green products: ");
      //   foreach (var p in pf.FilterByColor(products, Color.Green))
      //   {
      //     Console.WriteLine(p.Name);
      //   }

    }
  }
}
