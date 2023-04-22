using GeneticSharp;
using Microsoft.VisualBasic.FileIO;
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
using Accord.Statistics.Filters;
using Accord.Statistics.Models.Regression;
using Accord.Statistics.Models.Regression.Fitting;
using Accord.Statistics.Analysis;
using Accord.Math;
using Accord.MachineLearning.Bayes;
using Accord.MachineLearning;




namespace AMLA
{
    /// <summary>
    /// Interaction logic for Page4.xaml
    /// </summary>
    public partial class Page4 : Page
    {

        bool filegood;
        string filename;
        int leng;

        public Page4()
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

        private void CalcButton_Clicked(object sender, RoutedEventArgs e)
        {

            if (filegood)
            {

                // First, test if the user's query exists and if it is formatted correctly
                string query = querybox.Text;
                string[] queries = query.Split(',');

                if (queries.Length != leng - 1)
                {
                    MessageBox.Show("Invalid Query Format", "Invalid Query", MessageBoxButton.OK);
                    return;
                }

                //read data from file into table
                DataTable data = GetDataSourceFromFile(filename);

                //Also, get new Codification using headers
                List<string> categories = new List<string>();

                foreach(DataColumn column in data.Columns)
                {
                    categories.Add(column.ToString());
                }


                Codification code = new Codification(data, categories.ToArray());

                //consider all before last as inputs and last as output
                DataTable symbols = code.Apply(data);
                string last = categories.Last();
                int[] outputs = symbols.ToArray<int>(last);
                categories.RemoveAt(categories.Count - 1);
                int[][] inputs = symbols.ToArray<int>(categories.ToArray());

                // Create a new Naive Bayes learning
                var learner = new NaiveBayesLearning();

                // Learn a Naive Bayes model from the examples
                NaiveBayes nb = learner.Learn(inputs, outputs);

                //Translate using codebook
                int[] request = code.Translate(queries);

                int c = nb.Decide(request);

                string result = code.Translate(last, c);

                resultbox.Text = "Your query is estimated to be: " + result;

            }


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
                        leng = length;

                        if (leng > 7 || leng < 4)
                        {
                            uploadfile.Content = "Invalid File";
                            MessageBox.Show("Please check file for correct format. Rows must have 3 to 6 variables followed by 1 result", "Invalid File", MessageBoxButton.OK);
                            filegood = false;
                            return;
                        }

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

        public static int[][] ConvertToArrayOfArrays(int[,] matrix)
        {
            int numRows = matrix.GetLength(0);
            int numCols = matrix.GetLength(1);

            int[][] arrayOfArrays = new int[numRows][];
            for (int i = 0; i < numRows; i++)
            {
                arrayOfArrays[i] = new int[numCols];
                for (int j = 0; j < numCols; j++)
                {
                    arrayOfArrays[i][j] = matrix[i, j];
                }
            }

            return arrayOfArrays;
        }

        public DataTable GetDataSourceFromFile(string fileName)
        {
            DataTable dt = new DataTable();
            string[] columns = null;

            var lines = File.ReadAllLines(fileName);

            // assuming the first row contains the columns information
            if (lines.Count() > 0)
            {
                columns = lines[0].Split(new char[] { ',' });

                foreach (var column in columns)
                    dt.Columns.Add(column);
            }

            // reading rest of the data
            for (int i = 1; i < lines.Count(); i++)
            {
                DataRow dr = dt.NewRow();
                string[] values = lines[i].Split(new char[] { ',' });

                for (int j = 0; j < values.Count() && j < columns.Count(); j++)
                    dr[j] = values[j];

                dt.Rows.Add(dr);
            }
            return dt;
        }


    }
}