namespace DiscreteFrechetDistance;

public class Point
{
    /** The dimensions of this point */
    public readonly int[] dimensions;

    public Point(params int[] dimensions)
        => this.dimensions = dimensions;
}
public class DiscreteFrechetDistance
{
    /** Scanner instance */
    private TextReader console = Console.In;
    /** Dimensions of the time series */
    private static int dim;
    /** Dynamic programming memory array */
    private static double[,] mem;
    /** First time series */
    private static List<Point> timeSeriesP;
    /** Second time series */
    private static List<Point> timeSeriesQ;

    public static void Main(string[] args)
    {
        var console = Console.In;
        Console.WriteLine("--------------------");
        Console.WriteLine("Discrete Frechet Distance Calculator");
        Console.WriteLine("--------------------\n");

        Console.WriteLine("Please enter the first time series: ");
        var xSeries = console.ReadLine();
        Console.WriteLine("Please enter the second time series: ");
        var ySeries = console.ReadLine();

        if (!xSeries.Equals("") && !ySeries.Equals(""))
        {
            long startTime = DateTime.Now.Microsecond;

            timeSeriesP = ParseInput(xSeries);
            timeSeriesQ = ParseInput(ySeries);

            double res = ComputeDiscreteFrechet(timeSeriesP, timeSeriesQ);

            // how long it took to process
            long runTime = DateTime.Now.Microsecond - startTime;
            Console.WriteLine("The Discrete Frechet Distance between these two time series is " + res + ". (" + runTime + " ms)");
        }
        else
        {
            Console.WriteLine("ERROR: Invalid format! Please enter time series data like the following:");
            Console.WriteLine("64,25;42,55;37,21;34,76;77,2;98,0;9,20;20,10;12,27;32,61;88,49;60,90;99,37;85,53;7,87;67,33;20,62;4,88");
            Console.WriteLine("76,92;71,59;65,73;29,52;19,13;81,6;89,36;76,10;38,93;60,44;5,26;58,84;46,16;0,55;56,71");

            Console.WriteLine("\nEach sequence is given as pairs of values separated by semicolons and the values for each pair are separated by a comma.");
        }
    }

    /**
	 * Wrapper that makes a call to computeDFD. Initializes mem array with all
	 * -1 values.
	 * 
	 * @param P - the first time series
	 * @param Q - the second time series
	 * 
	 * @return The length of the shortest distance that can traverse both time
	 *         series.
	 */
    private static double ComputeDiscreteFrechet(List<Point> P, List<Point> Q)
    {
        mem = new double[P.Count,Q.Count];

        // initialize all values to -1
        for (int i = 0; i < mem.GetLength(0); i++)
        {
            for (int j = 0; j < mem.GetLength(1); j++)
            {
                mem[i,j] = -1.0;
            }
        }

        return ComputeDFD(P.Count - 1, Q.Count - 1);
    }

    /**
	 * Compute the Discrete Frechet Distance (DFD) given the index locations of
	 * i and j. In this case, the bottom right hand corner of the mem two-d
	 * array. This method uses dynamic programming to improve performance.
	 * 
	 * Pseudocode of computing DFD from page 5 of
	 * http://www.kr.tuwien.ac.at/staff/eiter/et-archive/cdtr9464.pdf
	 * 
	 * @param i - the row
	 * @param j - the column
	 * 
	 * @return The length of the shortest distance that can traverse both time
	 *         series.
	 */
    private static double ComputeDFD(int i, int j)
    {
        // if the value has already been solved
        if (mem[i,j] > -1)
            return mem[i,j];
        // if top left column, just compute the distance
        else if (i == 0 && j == 0)
            mem[i,j] = EuclideanDistance(timeSeriesP[i], timeSeriesQ[j]);
        // can either be the actual distance or distance pulled from above
        else if (i > 0 && j == 0)
            mem[i,j] = Max(ComputeDFD(i - 1, 0), EuclideanDistance(timeSeriesP[i], timeSeriesQ[j]));
        // can either be the distance pulled from the left or the actual
        // distance
        else if (i == 0 && j > 0)
            mem[i,j] = Max(ComputeDFD(0, j - 1), EuclideanDistance(timeSeriesP[i], timeSeriesQ[j]));
        // can be the actual distance, or distance from above or from the left
        else if (i > 0 && j > 0)
        {
            mem[i,j] = Max(Min(ComputeDFD(i - 1, j), ComputeDFD(i - 1, j - 1), ComputeDFD(i, j - 1)), EuclideanDistance(timeSeriesP[i], timeSeriesQ[j]));
        }
        // infinite
        else
            mem[i,j] = int.MaxValue;

        // printMemory();
        // return the DFD
        return mem[i, j];
    }

    /**
	 * Get the max value of all the values.
	 * 
	 * @param values - the values being compared
	 * 
	 * @return The max value of all the values.
	 */
    private static double Max(params double[] values)
    {
        double max = int.MinValue;
        foreach (double i in values)
        {
            if (i >= max)
                max = i;
        }
        return max;
    }

    /**
	 * Get the minimum value of all the values.
	 * 
	 * @param values - the values being compared
	 * 
	 * @return The minimum value of all the values.
	 */
    private static double Min(params double[] values)
    {
        double min = int.MaxValue;
        foreach (double i in values)
        {
            if (i <= min)
                min = i;
        }
        return min;
    }

    /**
	 * Given two points, calculate the Euclidean distance between them, where
	 * the Euclidean distance: sum from 1 to n dimensions of ((x - y)^2)^1/2
	 * 
	 * @param i - the first point
	 * @param j - the second point
	 * 
	 * @return The total Euclidean distance between two points.
	 */
    private static double EuclideanDistance(Point i, Point j)
    {

        double distance = Math.Sqrt(
            Math.Pow(i.dimensions[0] - j.dimensions[0], 2) 
            + Math.Pow((i.dimensions[1] - j.dimensions[1]), 2));

        return distance;
    }

    /**
	 * Parses console input in order to construct a list of points.
	 * 
	 * @param input - the string input from the console
	 * @return A list of Points that can be evaluated into a polygonal curve.
	 */
    private static List<Point> ParseInput(string input)
    {
        List<Point> points = new ();

        // split the input string up by semi-colon
        var tuples = input.Split(";");
        if (tuples != null && tuples.Length > 0)
        {
            // for each tuple pair
            foreach (var tup in tuples)
            {
                // get the dimension of each
                var dims = tup.Split(",");

                // if valid split
                if (dims != null && dims.Length > 0)
                {
                    // construct new array of dims.length dimensions
                    var dimensions = new int[dims.Length];

                    // set the global dimensional value
                    if (dim != dims.Length)
                        dim = dims.Length;

                    // for each dimension
                    for (int i = 0; i < dims.Length; i++)
                    {
                        dimensions[i] = int.TryParse(dims[i], out var val) ? val : 0;
                    }

                    // add the point to list of points
                    points.Add(new (dimensions));
                    //Arrays.toString(p.dimensions);
                }
            }
        }

        return points;
    }

    /**
	 * Test method that prints the 2D dynamic programming array.
	 */
    private static void PrintMemory()
    {
        Console.WriteLine("\n\n");
        for (int row = 0; row < mem.GetLength(0); row++)
        {
            for (int col = 0; col < mem.GetLength(1); col++)
            {
                Console.WriteLine(mem[row,col] + "\t");
            }
            Console.WriteLine();
        }
    }
}

