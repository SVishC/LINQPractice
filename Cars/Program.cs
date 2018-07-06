using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cars
{
    class Program
    {
        static void Main(string[] args)
        {
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
          var query2 = from car in cars
            join manufacturer in manufacturers
              on new {car.Manufacturer, car.Year} equals new {Manufacturer = manufacturer.Name, manufacturer.Year}
            orderby car.Combined descending, car.Name
            select new
            {
              manufacturer.Name,
              manufacturer.Headquarters,
              car.Year
            };

     var query = cars.Join(manufacturers, c => new {c.Manufacturer,c.Year}, m => new {Manufacturer=m.Name,m.Year}, (c, m) => new
          {
            Manufacturer = c.Manufacturer,
            Headquarters = m.Headquarters,
            Combined = c.Combined
          }).OrderByDescending(c => c.Combined).ThenBy(c => c.Manufacturer);

      foreach (var c in query.Take(10))
          {
            Console.WriteLine($"Manufacturer :{c.Manufacturer} City : {c.Headquarters} Mileage: {c.Combined}");
            
          }
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
          City = double.Parse(column[5]),
          Highway = double.Parse(column[6]),
          Combined = double.Parse(column[7])
        };
      }
    }
  }
}
