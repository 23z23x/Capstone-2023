using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.MachineLearning.DecisionTrees;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Filters;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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
using Accord.Math;
using Accord;

namespace AMLA
{
    /// <summary>
    /// Interaction logic for Page7.xaml
    /// </summary>
    public partial class Page7 : Page
    {
        public bool filegood;
        public string filename;

        public Page7()
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

        private void ToCARTExplanation_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);

            var frame = parentWindow.FindName("MainFrame") as Frame;

            CARTExplanation home = new CARTExplanation();

            frame.NavigationService.Navigate(home);
        }

        private void FileButton_Clicked(object sender, RoutedEventArgs e)
        {
            //Use the file dialog to get file. Read through to get needed info, and tell the user if the file is not formatted correctly
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "csv files (*.csv)|*.csv";

            if (openFileDialog.ShowDialog() == true)
            {
                string file = openFileDialog.FileName;

                //get number of lines in file

                int numlines = File.ReadLines(file).Count();


                using (StreamReader reader = new StreamReader(file))
                {
                    string line;
                    int length = 0;

                    //read first line to know how many elements should be in each line
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        string[] parts = line.Split(',');
                        length = parts.Length;


                    }
                    else
                    {
                        uploadfile.Content = "Invalid File";
                        MessageBox.Show("Please check file for correct format. All rows must be of the same length", "Invalid File", MessageBoxButton.OK);
                        filegood = false;
                        return;
                    }

                    reader.BaseStream.Seek(0, SeekOrigin.Begin);

                    int totallines = File.ReadAllLines(file).Length;

                    if (totallines < 10)
                    {
                        uploadfile.Content = "Invalid File";
                        MessageBox.Show("Not enough training sets to work with (Must be at least 10)", "Invalid File", MessageBoxButton.OK);
                        filegood = false;
                        return;
                    }

                    foreach (string lines in File.ReadAllLines(file))
                    {
                        string[] parts = lines.Split(",");
                        if (parts.Length != length)
                        {
                            uploadfile.Content = "Invalid File";
                            MessageBox.Show("Please check file for correct format. All rows must be of the same length", "Invalid File", MessageBoxButton.OK);
                            filegood = false;
                            return;
                        }
                    }

                }
                uploadfile.Content = file.Split('\\')[file.Split('\\').Length - 1];
                filegood = true;
                filename = file;
            }

        }

        private void EvaluateButton_Clicked(Object sender, RoutedEventArgs e)
        {
            // Filepath variable
            string filepath = filename;
            // Global variables used to parse and satisfy various data structures
            string[] columnArray;
            string resultColumn;
            List<List<string>> rows = new List<List<string>>();

            // Create DataTable object data
            DataTable data = new DataTable("DataTable");
            // Create string list of column names from first row of file
            List<string> columns = new List<string>();
            // Read file by lines, delimit by comma for csv
            using (StreamReader reader = new StreamReader(filepath))
            {
                // Read first line for headers list
                string line = reader.ReadLine() ?? string.Empty;
                // Check file / first line is not empty
                if (line == string.Empty)
                {
                    Console.WriteLine("Error: ReadLine returned null or an empty line. Check file");
                }
                // Set columns list to strings in first row
                columns = new List<string>(line.Split(','));
                // Add columns to data object
                data.Columns.Add(columns.ToArray());
                // Initialize columnArray for DecisionTree inputs
                columnArray = columns.ToArray();
                // Initialize resultColumn for DecisionTree outputs to last elem in first row of file
                resultColumn = columnArray[columnArray.Length - 1];
                // Remove outputs string from inputs array
                Array.Resize(ref columnArray, columnArray.Length - 1);
                // Read subsequent lines to create subsequent lists
                while ((line = reader.ReadLine() ?? string.Empty) != string.Empty)
                {
                    string[] row = line.Split(',');
                    // Add rows to data object
                    data.Rows.Add(row);
                    // Add row to rows list
                    rows.Add(row.ToList());
                }
            }

            // Create a new codification codebook to convert strings into integer symbols
            var codebook = new Codification(data);
            // Translate our training data into integer symbols using codebook
            DataTable symbols = codebook.Apply(data);
            // Create input and output data structures for decisionTree
            int[][] inputs = symbols.ToArray<int>(columnArray);
            int[] outputs = symbols.ToArray<int>(resultColumn);


            // Count number of unique strings in each column
            // int list of number of unique strings per column
            List<int> uniqueStringsPerColumn = new List<int>();
            // Loop through columns in rows 2d list
            for (int col = 0; col < rows[0].Count; col++)
            {
                // Use HashSet to count unique strings
                HashSet<string> uniqueStrings = new HashSet<string>();
                // Loop through each row in current column
                for (int row = 0; row < rows.Count; row++)
                {
                    // Add the string to the HashSet
                    uniqueStrings.Add(rows[row][col]);
                }
                // Add the count of unique strings to the list
                uniqueStringsPerColumn.Add(uniqueStrings.Count);
            }
            // Remove the last element in the uniqueStringsPerColumn list (the result column (always 2))
            uniqueStringsPerColumn.RemoveAt(uniqueStringsPerColumn.Count - 1);

            // Add a new DecisionVariable for each column
            int numOfDecisionAttributes = columnArray.Length;
            // List of DecisionVariables
            List<DecisionVariable> attributes = new List<DecisionVariable>();
            // Initialize list / DecisionVariables
            for (int i = 0; i < numOfDecisionAttributes; i++)
            {
                attributes.Add(new DecisionVariable(columnArray[i], uniqueStringsPerColumn[i]));
            }


            // Create ID3Learning object with array of DecisionVariables
            var id3learning = new ID3Learning(attributes.ToArray());
            // Train DecisionTree
            DecisionTree tree = id3learning.Learn(inputs, outputs);
            // Compute the training error when predicting training instances
            double error = new ZeroOneLoss(outputs).Loss(tree.Decide(inputs));

            // String input from user / ui
            //3 possible values(Sunny, overcast, rain)
            // 3 possible values (Hot, mild, cool)  
            // 2 possible values (High, normal)    
            // 2 possible values (Weak, strong)
            string userInputQuery = querybox.Text; // Will = Yes
                                                              //string userInputQuery = "Sunny,Hot,High,Strong"; // Will = No

            // Make empty int list to fill with query
            List<int> queryAsList = new List<int>();
            // Split user query on commas into string array
            string[] columnValues = userInputQuery.Split(',');
            // Loop through array, convert string to its int representation with codebook, add to array
            for (int i = 0; i < columnValues.Length; i++)
            {
                try
                {
                    queryAsList.Add(codebook.Transform(new[,] { { columnArray[i], columnValues[i] } })[0]); // Dont ask about this line
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The Query does not match the format of the file or a value in your query does not exist in the file.", "Invalid Query", MessageBoxButton.OK);
                    return;
                }
            }
            // Convert query List to Array
            int[] query = queryAsList.ToArray();
            // Run query on array
            int prediction = tree.Decide(query);

            // Translate query to string for ui output
            result.Text = codebook.Revert(resultColumn, prediction);
        }


    }
}
