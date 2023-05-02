using Accord.Math;
using Cotur.DataMining.Association;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AMLA
{
    /// <summary>
    /// Interaction logic for Page6.xaml
    /// </summary>
    /// 
    public partial class Page6 : Page
    {

        public string filename;
        bool filegood;
        public string query;

        public Page6()
        {
            InitializeComponent();
            filegood = false;
        }

        private void ToHome_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);

            var frame = parentWindow.FindName("MainFrame") as Frame;

            Page1 home = new Page1();

            frame.NavigationService.Navigate(home);
        }

        private void ToAprioriExplanation_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);

            var frame = parentWindow.FindName("MainFrame") as Frame;

            AprioriExplanation home = new AprioriExplanation();

            frame.NavigationService.Navigate(home);
        }

        private void FileButton_Clicked(object sender, RoutedEventArgs e)
        {
            //File has header that indicates max elements, so scan through and make sure everything is within that range (0 to maxelement-1)
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "csv files (*.csv)|*.csv";

            if (openFileDialog.ShowDialog() == true)
            {
                using (StreamReader reader = new StreamReader(openFileDialog.FileName))
                {
                    string line = reader.ReadLine();
                    //get number of elements in first line
                    int elems = line.Split(',').GetNumberOfElements();
                    while (!reader.EndOfStream)
                    { 
                        //now read every other line, testing each comma seperated element to see if it is within bounds
                        line = reader.ReadLine();
                        string[] linesep = line.Split(',');
                        foreach (string s in linesep)
                        {
                            int test = -1;
                            try
                            {
                                test = int.Parse(s);
                            }
                            catch (FormatException)
                            {
                                uploadfile.Content = "Invalid File";
                                MessageBox.Show("Please check file for correct format. At least one input is not an integer.", "Invalid File", MessageBoxButton.OK);
                                filegood = false;
                                return;
                            }
                            if (test < 0 || test > elems - 1)
                            {
                                uploadfile.Content = "Invalid File";
                                MessageBox.Show("Please check file for correct format. At least one input is outside of the range.", "Invalid File", MessageBoxButton.OK);
                                filegood = false;
                                return;
                            }
                        }
                    }
                    //Otherwise, file should be ok
                    filegood = true;
                    uploadfile.Content = openFileDialog.FileName.Split('\\')[openFileDialog.FileName.Split('\\').Length - 1];
                    filename = openFileDialog.FileName;
                }
            }

        }

        private void EvaluateButton_Clicked(Object sender, RoutedEventArgs e)
        {
            query = querybox.Text;
            if(filegood)
            {
                // Create DataFields object for apriori object
                DataFields? data = readDataFieldsFromFile(filename);
                if (data == null)
                {
                    Console.WriteLine("Error: readDataFieldsFromFile returned null");
                }
                // Create Apriori object with Datafield data for apriori algorithm
                Apriori apriori = new Apriori(data);
                // Run / print output of the apriori algorithm with 20% min support and 50% min confidence
                result.Text = WriteApriori(apriori, .20f, .50f, query);

            }


        }

        // Reads file at filepath and returns DataField object
        public static DataFields? readDataFieldsFromFile(string filepath)
        {
            // Create string list for first row of file (column/item names)
            List<string> headers = new List<string>();
            // Create list of int lists of each subsequent row
            List<List<int>> rows = new List<List<int>>();

            // Read file by lines, delimit by comma for csv
            using (StreamReader reader = new StreamReader(filepath))
            {
                // Read first line for headers list
                string line = reader.ReadLine() ?? string.Empty;
                // Check file / first line is not empty
                if (line == string.Empty)
                {
                    Console.WriteLine("Error: ReadLine returned null or an empty line. Check file");
                    return null;
                }
                // Set headers list to strings in first row
                headers = new List<string>(line.Split(','));

                // Read subsequent lines to create list of int lists
                while ((line = reader.ReadLine() ?? string.Empty) != string.Empty)
                {
                    string[] fields = line.Split(',');
                    List<int> row = new List<int>();
                    // Parse strings to ints
                    for (int i = 0; i < fields.Length; i++)
                    {
                        int value;
                        if (int.TryParse(fields[i], out value))
                        {
                            row.Add(value);
                        }
                        else
                        {
                            Console.WriteLine($"Error: Field '{fields[i]}' cannot be parsed as int");
                            return null;
                        }
                    }
                    // Add each rows list to list of int lists
                    rows.Add(row);
                }
            }

            // (Optional) Print first row of file (column / item names)
            Console.WriteLine("Headers:");
            foreach (string header in headers)
            {
                Console.Write($"{header} ");
            }
            Console.WriteLine();
            // (Optional) Print subsequent rows of file (boolean int)
            Console.WriteLine("Rows:");
            foreach (List<int> row in rows)
            {
                foreach (int value in row)
                {
                    Console.Write($"{value} ");
                }
                Console.WriteLine();
            }

            // Create datafield object data to be returned
            DataFields data = new DataFields(headers.Count, rows, headers);
            return data;
        }

        // Runs apriori algorithm and prints output tables and rules
        public static string WriteApriori(Apriori apriori, float minimumSupport, float minimumConfidence, string query)
        {
            // Run algorithm on apriori object with minimumSupport minimum support
            apriori.CalculateCNodes(minimumSupport);
            int table = 1;
            // (Optional) Print tables produced by algorithm
            foreach (var Levels in apriori.EachLevelOfNodes)
            {
                Console.WriteLine("\n-- Table{0} --", table++);
                foreach (var node in Levels)
                {
                    Console.WriteLine(node.ToDetailedString(apriori.Data));
                }
            }
            // Prints rules
            Console.WriteLine("\n-- Rules --");
            //foreach(var rules in apriori.Rules){
            foreach (var rules in apriori.Rules.Where(rule => rule.Confidence >= minimumConfidence))
            {
                string[] delims = { "||", "=>" };


                string test = rules.ToDetailedString(apriori.Data);

                //Parse string for right hand item that matches query, and return string with left hand items
                string[] result = test.Split(delims, StringSplitOptions.None);
                string a = result[1].Replace(" ", string.Empty);

                if(a == query)
                {
                    return result[0];
                }
            }
            //if we reach end without finding anything, send error and return
            MessageBox.Show("Query not found in database", "Invalid Query", MessageBoxButton.OK);
            return string.Empty;
        }

    }
}
