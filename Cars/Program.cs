using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cars
{
  class Program
  {
    static void Main(string[] args)
    {
      #region LINQ
      //var cars = ProcessFile("fuel.csv");
      var cars = ProcessCars("fuel.csv");
      var manufacturers = ProcessManufacturers("manufacturers.csv");

      //foreach (var car in cars)
      //{
      //    Console.WriteLine(car.Name);
      //}
      //foreach (var manufacturer in manufacturers)
      //{
      //  Console.WriteLine(manufacturer.Name);
      //}

      #region Join
      var query2 = from car in cars
                   join manufacturer in manufacturers
                     on new { car.Manufacturer, car.Year } equals new { Manufacturer = manufacturer.Name, manufacturer.Year }
                   orderby car.Combined descending, car.Name
                   select new
                   {
                     manufacturer.Name,
                     manufacturer.Headquarters,
                     car.Year
                   };

      var query = cars.Join(manufacturers, c => new { c.Manufacturer, c.Year }, m => new { Manufacturer = m.Name, m.Year }, (c, m) => new
      {
        Manufacturer = c.Manufacturer,
        Headquarters = m.Headquarters,
        Combined = c.Combined
      }).OrderByDescending(c => c.Combined).ThenBy(c => c.Manufacturer);

      //foreach (var c in query.Take(10))
      //    {
      //      Console.WriteLine($"Manufacturer :{c.Manufacturer} City : {c.Headquarters} Mileage: {c.Combined}");

      //    }
      #endregion

      #region grouping
      //Query for grouping example using query syntax
      var queryGroup = from car in cars
                       group car by car.Manufacturer.ToUpper()
          into manu
                       orderby manu.Key
                       select manu;

      var queryGroup2 = cars.GroupBy(c => c.Manufacturer)
                              .OrderBy(g => g.Key);

      //foreach (var group in queryGroup2)
      //{
      //  Console.WriteLine(group.Key);
      //  foreach (var car in group.OrderByDescending(c => c.Combined).Take(2))
      //  {
      //    Console.WriteLine($"\t {car.Name} : {car.Combined}");
      //  }

      //}

      #endregion

      #region GroupJoin

      //join and grouping cannot be used together in query syntax rather reverse joining is in a way GROUP JOIN.

      var queryGJ = from manufacturer in manufacturers
                    join car in cars
                    on manufacturer.Name equals car.Manufacturer into carGroup
                    orderby manufacturer.Name
                    select new
                    {
                      Manufacturer = manufacturer,
                      Cars = carGroup
                    };

      var queryGJ2 = manufacturers.GroupJoin(cars, m => m.Name, c => c.Manufacturer,
          (m, c) => new { Manufacturer = m, Cars = c });


      //foreach (var group in queryGJ2)
      //{
      //    Console.WriteLine($"Manufacturer name : {group.Manufacturer.Name} , Manufactured Year : {group.Manufacturer.Year}");
      //    foreach (var car in group.Cars.OrderByDescending(c => c.Combined).Take(2))
      //    {
      //        Console.WriteLine($"\t {car.Name} : {car.Combined}");
      //    }

      //}

      //Top 3 fuel efficient cars by country--exercise

      var exQuery = from manufacturer in manufacturers
                    join car in cars
                    on manufacturer.Name equals car.Manufacturer into carGroup
                    orderby manufacturer.Name
                    select new
                    {
                      Manufacturer = manufacturer,
                      Cars = carGroup
                    }
          into result
                    group result by result.Manufacturer.Headquarters;

      //var exqueryyyy = from q in exQuery
      //    group q by q.Manufacturer.Headquarters;

      var exQuery2 =
          manufacturers.GroupJoin(cars, m => m.Name, c => c.Manufacturer,
              (m, groupCars) => new { Manufacturer = m, Cars = groupCars }).GroupBy(m => m.Manufacturer.Headquarters);

      //foreach (var group in exQuery2)
      //{
      //  Console.WriteLine(group.Key);

      //  //flattening the result using select many since group will have no insight/projection to the cars object
      //  //since cars group is only available.query has to go one step deeper for getting cars detils.
      //  //so using select many instead of select
      //  foreach (var car in group.SelectMany(g => g.Cars).OrderByDescending(c => c.Combined).Take(3))
      //  {
      //    Console.WriteLine($"\t {car.Name} : {car.Combined}");
      //  }

      //}

      #endregion

      #region aggregation

      var aggQuery = from car in cars
                     group car by car.Manufacturer
        into carGroup
                     select new
                     {
                       Name = carGroup.Key,
                       Max = carGroup.Max(c => c.Combined),
                       Min = carGroup.Min(c => c.Combined),
                       Avg = carGroup.Average(c => c.Combined)

                     }
        into result
                     orderby result.Max descending
                     select result;

      //using query syntac executes cargroup to find max,min,avg three times(i.e every car is processed thrice / once per agregate)
      //while extension method syntax uses accumulator(CarStatistics) which executes these values only once per car
      var aggQuery2 = cars.GroupBy(c => c.Manufacturer)
        .Select(g =>
        {
          var results = g.Aggregate(new CarStatistics(),
            (acc, c) => acc.Accumulate(c), acc => acc.Compute());

          return new
          {
            Name = g.Key,
            Avg = results.Average,
            Min = results.Min,
            Max = results.Max

          };
        }
        ).OrderByDescending(r => r.Max);


      //foreach (var result in aggQuery2)
      //{
      //  Console.WriteLine($"{result.Name}");
      //  Console.WriteLine($"\tMax : {result.Max}");
      //  Console.WriteLine($"\tMin : {result.Min}");
      //  Console.WriteLine($"\tAvg : {result.Avg}");

      //}



      #endregion
      #endregion

      #region LinqToXml

      //code to write into xml
      var records = ProcessCars("fuel.csv");

      var document=new XDocument();

      var cars1=new XElement("Cars",
        
        from record in records
        select new XElement("Car",new XAttribute("Name",record.Name),
        new XAttribute("Combined",record.Combined),
        new XAttribute("Manufacturer",record.Manufacturer)));
      document.Add(cars1);
      document.Save("fuel.xml");



      //to read xml

      var readDocument = XDocument.Load("fuel.xml");

      var queryDoc = from element in readDocument.Element("Cars")?.Elements("Car")
        where element.Attribute("Manufacturer")?.Value == "BMW"
        select element.Attribute("Name")?.Value;

      var queryDoc2 = readDocument.Element("Cars")?
        .Elements("Car")
        .Where(e => e.Attribute("Manufacturer")?.Value == "BMW")
        .Select(ele => ele.Attribute("Name")?.Value);

      foreach (var carName in queryDoc2)
      {
        Console.WriteLine(carName);
        
      }

      #endregion
    }

    /// <summary>
    /// Takes file path,reads all the lines,sjip unwanted lines and converts it to list of cars using ToCar extension method
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static List<Car> ProcessCars(string path)
    {
      return File.ReadAllLines(path)
        .Skip(1)
        .Where(line => line.Length > 1)
        .ToCar().ToList();
    }

    private static List<Manufacturer> ProcessManufacturers(string path)
    {
      return File.ReadAllLines(path)
        .Where(line => line.Length > 1)
        .Select(l =>
        {
          var columns = l.Split(',');
          return new Manufacturer
          {
            Name = columns[0],
            Headquarters = columns[1],
            Year = int.Parse(columns[2])
          };

        }).ToList();
    }

    //private static List<Car> ProcessFile(string path)
    //    {
    //         return 
    //        File.ReadAllLines(path)
    //            .Skip(1)
    //            .Where(line => line.Length > 1)
    //            .Select(Car.ParseFromCsv).ToList();
    //    }
  }

  public static class CarExtensions
  {
    /// <summary>
    /// Converts the Ienumerable of string from processcars to Ienumerable of cars
    /// </summary>
    /// <param name="lines"></param>
    /// <returns></returns>
    public static IEnumerable<Car> ToCar(this IEnumerable<string> lines)
    {
      foreach (var line in lines)
      {
        var column = line.Split(',');
        yield return new Car
        {
          Year = int.Parse(column[0]),
          Manufacturer = column[1],
          Name = column[2],
          Displacement = double.Parse(column[3]),
          Cylinders = int.Parse(column[4]),
          City = int.Parse(column[5]),
          Highway = int.Parse(column[6]),
          Combined = int.Parse(column[7])
        };
      }
    }
  }
  public class CarStatistics
  {
    public CarStatistics()
    {
      Min = Int32.MaxValue;
      Max = Int32.MinValue;

    }

    public int Min { get; set; }
    public int Max { get; set; }
    public double Average { get; set; }
    public int Count { get; set; }
    public int Total { get; set; }

    public CarStatistics Accumulate(Car c)
    {
      Total += c.Combined;
      Count += 1;
      Max = Math.Max(Max, c.Combined);
      Min = Math.Min(Min, c.Combined);
      return this;
    }

    public CarStatistics Compute()
    {
      Average = Total / Count;
      return this;
    }
  }

}
