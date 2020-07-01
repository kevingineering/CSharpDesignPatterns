using System;

namespace LSPRectangles
{
  public class Rectangle
  {
    //virtual allows us to 
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }

    public Rectangle()
    {

    }

    public Rectangle(int width, int height)
    {
      Width = width;
      Height = height;
    }

    public override string ToString()
    {
      return $"{nameof(Width)}: {Width}, {nameof(Height)}: {Height}";
    }
  }

  public class Square : Rectangle
  {
    public override int Width
    {
      set { base.Width = base.Height = value; }
    }

    public override int Height
    {
      set { base.Width = base.Height = value; }
    }
  }

  public class Demo
  {
    static public int Area(Rectangle r) => r.Width * r.Height;

    static void Main(string[] args)
    {
      Rectangle rect1 = new Rectangle(4, 7);
      Console.WriteLine(rect1 + " has area " + Area(rect1));

      Rectangle rect2 = new Rectangle();
      Console.WriteLine(rect2);

      //notice that this violates the Liskov substitution principle 
      Rectangle sq1 = new Square();
      sq1.Width = 4;
      Console.WriteLine(sq1 + " has area " + Area(sq1));
    }
  }
}
