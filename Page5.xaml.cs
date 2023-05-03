using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Accord.Statistics.Filters;
using Accord.Statistics.Models.Regression;
using Accord.Statistics.Models.Regression.Linear;
using Accord.Statistics.Models.Regression.Fitting;
using Accord.Statistics.Analysis;
using Accord.Math;
using System.Data;
using OfficeOpenXml;
using System.Globalization;
using Accord.Math.Optimization.Losses;
using OxyPlot;
using OxyPlot.Wpf;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace AMLA
{
    /// <summary>
    /// Interaction logic for Page5.xaml
    /// </summary>
    public partial class Page5 : Page
    {
        bool filegood;
        string filename;
        public Page5()
        {
            InitializeComponent();
            filegood = false;
        }

        private void ToLinearRegressionExplanation_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);

            var frame = parentWindow.FindName("MainFrame") as Frame;

            LinearRegressionExplanation home = new LinearRegressionExplanation();

            frame.NavigationService.Navigate(home);
        }

        private void ToHome_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);

            var frame = parentWindow.FindName("MainFrame") as Frame;

            Page1 home = new Page1();

            frame.NavigationService.Navigate(home);

        }

        private void FileButton_Clicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "csv (*.csv)|*.csv|xlsx (*.xlsx)|*.xlsx";

            if (openFileDialog.ShowDialog() == true)
            {
                string file = openFileDialog.FileName;

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                FileInfo fileInfo = new FileInfo(file); // gather file info and create new excel package
                using (var excelPackage = new ExcelPackage(fileInfo))
                {
                    var worksheet = excelPackage.Workbook.Worksheets[0]; // select sheet 0
                    int rowCount = worksheet.Dimension.Rows; // gather row dimensions
                    int columnCount = worksheet.Dimension.Columns; // gather column dimensions
                    int[] valid = new int[columnCount]; // array to hold column len for each column
                    int numRows = 0;
                    object valTest = 0;

                    for (int col = 1; col <= columnCount; col++)
                    {
                        for (int row = 1; row <= rowCount; row++)
                        {
                            valTest = worksheet.Cells[row, col].Value;
                            if (!string.IsNullOrEmpty(worksheet.Cells[row, col].Text) && (valTest is double || valTest is int)) // iterate through each columns rows and check for valid row value
                            {
                                numRows++;
                                valid[col - 1] = numRows;
                            }
                            else
                            {
                                uploadfile.Content = "Invalid File";
                                MessageBox.Show("Please check file for correct format. All values must be of decimal type", "Invalid File", MessageBoxButton.OK);
                                filegood = false;
                                return;
                            }

                        }
                        numRows = 0;
                    }
                    if (valid[0] != valid[1]) // test if both columns have the same length, if not throw error.
                    {
                        uploadfile.Content = "Invalid File";
                        MessageBox.Show("Please check file for correct format. All columns must be of the same length", "Invalid File", MessageBoxButton.OK);
                        filegood = false;
                        return;

                    }

                }
                uploadfile.Content = file.Split('\\')[file.Split('\\').Length - 1];
                filegood = true;
                filename = file;
            }
        }

        private void CalcButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (filegood)
            {
                double[,] data = GetDataSourceFromFile(filename);
                double[] inputs = new double[data.GetLength(1)];
                double[] outputs = new double[data.GetLength(1)];

                for (int i = 0; i < data.GetLength(1); i++)
                {
                    inputs[i] = data[0, i];
                    outputs[i] = data[1, i];
                }

                // Transform inputs to logarithms
                double[] logx = Accord.Math.Matrix.Log(inputs);

                // Use Ordinary Least Squares to learn the regression
                OrdinaryLeastSquares ols = new OrdinaryLeastSquares();

                // Use OLS to learn the simple linear regression
                SimpleLinearRegression lr = ols.Learn(logx, outputs);

                // Compute predicted values for inputs
                double[] predicted = lr.Transform(logx);

                // Get an expression representing the learned regression model
                // We just have to remember that 'x' will actually mean 'log(x)'
                string result = lr.ToString("N4", CultureInfo.InvariantCulture);
                double slope = lr.Slope;
                double intercept = lr.Intercept;

                // The mean squared error between the expected and the predicted is
                double error = new SquareLoss(outputs).Loss(predicted);
                resultbox.Text = "Learned Regression Model: " + result + "\nSlope: " + slope + "\nIntercept: " + intercept + "\nMean Squared Error: " + error.ToString();

                PlotView plotView = new PlotView();

                // Create a new plot model
                PlotModel plotModel = new PlotModel();
                plotModel.Title = "My Plot";
                plotModel.PlotType = PlotType.XY;

                ScatterSeries inputs_plot = new ScatterSeries();
                //inputs_plot.Title = "Inputs ";
                inputs_plot.MarkerType = MarkerType.Circle;
                inputs_plot.MarkerSize = 5;

                for (int i = 0; i < outputs.Length; i++)
                {
                    inputs_plot.Points.Add(new ScatterPoint(inputs[i], predicted[i]));
                }
                plotModel.Series.Add(inputs_plot);

                // Customize the appearance of the plot
                plotModel.Axes.Add(new LinearAxis { Title = "Inputs", Position = AxisPosition.Bottom });
                plotModel.Axes.Add(new LinearAxis { Title = "Predicted Ouputs", Position = AxisPosition.Left });

                plotView.Model = plotModel;
            }

        }

        public double[,] GetDataSourceFromFile(string fileName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Open the Excel file
            FileInfo fileInfo = new FileInfo(fileName);
            double[,] columnData = null;

            using (ExcelPackage excelPackage = new ExcelPackage(fileInfo))
            {
                // Get the first worksheet in the Excel file
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[0];

                // Get the number of columns in the worksheet
                int numColumns = worksheet.Dimension.Columns;
                int numRows = worksheet.Dimension.Rows;

                columnData = new double[numColumns, numRows];

                // Loop through the columns in the row and print the values
                for (int column = 0; column < numColumns; column++)
                {
                    for (int row = 0; row < numRows; row++)
                    {
                        double cellValue;
                        if (double.TryParse(worksheet.Cells[(row + 1), (column + 1)].Value.ToString(), out cellValue))
                        {
                            columnData[column, (row)] = cellValue;
                        }
                    }
                }
                excelPackage.Dispose();
            }
            return columnData;
        }
    }
}
