using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DotNetDesignPatternDemos.Structural.Facade
{
  public class Generator
  {
    private static readonly Random random = new Random();

    public virtual List<int> Generate(int count)
    {
      return Enumerable.Range(0, count)
        .Select(_ => random.Next(1, 20))
        .ToList();
    }
  }

  public class UniqueGenerator : Generator
  {
    public override List<int> Generate(int count)
    {
      List<int> result;
      do
      {
        result = base.Generate(count);
      } while (result.Distinct().Count() != result.Count);

      return result;
    }
  }

  public class Splitter
  {
    public List<List<int>> Split(List<List<int>> array)
    {
      var result = new List<List<int>>();

      var rowCount = array.Count;
      var colCount = array[0].Count;

      // get the rows
      for (int r = 0; r < rowCount; ++r)
      {
        var theRow = new List<int>();
        for (int c = 0; c < colCount; ++c)
          theRow.Add(array[r][c]);
        result.Add(theRow);
      }

      // get the columns
      for (int c = 0; c < colCount; ++c)
      {
        var theCol = new List<int>();
        for (int r = 0; r < rowCount; ++r)
          theCol.Add(array[r][c]);
        result.Add(theCol);
      }

      // now the diagonals
      var diag1 = new List<int>();
      var diag2 = new List<int>();
      for (int c = 0; c < colCount; ++c)
      {
        for (int r = 0; r < rowCount; ++r)
        {
          if (c == r)
            diag1.Add(array[r][c]);
          var r2 = rowCount - r - 1;
          if (c == r2)
            diag2.Add(array[r][c]);
        }
      }

      result.Add(diag1);
      result.Add(diag2);

      return result;
    }
  }

  public class Verifier
  {
    public bool Verify(List<List<int>> array)
    {
      if (!array.Any()) return false;

      var expected = array.First().Sum();

      return array.All(t => t.Sum() == expected);
    }
  }

  public class MagicSquareGenerator
  {
    public List<List<int>> Generate
      <TGenerator, TSplitter, TVerifier>(int size)
      where TGenerator : Generator, new()
      where TSplitter : Splitter, new()
      where TVerifier : Verifier, new()
    {
      var g = new TGenerator();
      var s = new TSplitter();
      var v = new TVerifier();

      var square = new List<List<int>>();

      do
      {
        square = new List<List<int>>();
        var values = g.Generate(size * size);
        for (int i = 0; i < size; ++i)
          square.Add(new List<int>(
            values.Skip(i*size).Take(size)));
      } while (!v.Verify(s.Split(square)));

      return square;
    }

    public List<List<int>> Generate(int size)
    {
      return Generate<Generator, Splitter, Verifier>(size);
    }
  }

  public class MyVerifier
  {
    public bool Verify(List<List<int>> array)
    {
      if (!array.Any()) return false;

      var rowCount = array.Count;
      var colCount = array[0].Count;

      var expected = array.First().Sum();

      for (var row = 0; row < rowCount; ++row)
        if (array[row].Sum() != expected)
          return false;

      for (var col = 0; col < colCount; ++col)
        if (array.Select(a => a[col]).Sum() != expected)
          return false;

      var diag1 = new List<int>();
      var diag2 = new List<int>();
      for (var r = 0; r < rowCount; ++r)
      for (var c = 0; c < colCount; ++c)
      {
        if (r == c)
          diag1.Add(array[r][c]);
        var r2 = rowCount - r - 1;
        if (r2 == c)
          diag2.Add(array[r][c]);
      }

      return diag1.Sum() == expected && diag2.Sum() == expected;
    }
  }


  [TestFixture]
  public class TestSuite
  {
    private string SquareToString(List<List<int>> square)
    {
      var sb = new StringBuilder();
      foreach (var row in square)
      {
        sb.AppendLine(string.Join(" ",
          row.Select(x => x.ToString())));
      }

      return sb.ToString();
    }

    [Test]
    public void TestSizeThree()
    {
      var gen = new MagicSquareGenerator();
      var square = gen.Generate(3);

      Console.WriteLine(SquareToString(square));

      var v = new MyVerifier(); // prevents cheating :)
      Assert.IsTrue(v.Verify(square),
        "Verification failed: this is not a magic square");
    }
    
    
    [Test]
    public void TestSizeThreeUnique()
    {
      var gen = new MagicSquareGenerator();
      var square = gen
        .Generate<UniqueGenerator, Splitter, Verifier>(3);

      Console.WriteLine(SquareToString(square));

      var v = new MyVerifier(); // prevents cheating :)
      Assert.IsTrue(v.Verify(square),
        "Verification failed: this is not a magic square");
    }
  }
}