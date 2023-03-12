using GeneticSharp;
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
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;

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

            if(filegood)
            {

                // First, test if the user's query exists and if it is formatted correctly
                string query = querybox.Text;

                if(query.Split(',').Length != leng - 1) 
                {
                    MessageBox.Show("Invalid Query Format", "Invalid Query", MessageBoxButton.OK);
                    return;
                }

                // set up a new MLContext
                MLContext mlContext = new MLContext();

                // load the data into an IDataView
                IDataView dataView = mlContext.Data.LoadFromTextFile<SentimentData>(
                    filename, separatorChar: ',', hasHeader: true);

                // split the data into training and testing sets
                var trainTestSplit = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
                IDataView trainData = trainTestSplit.TrainSet;
                IDataView testData = trainTestSplit.TestSet;

                // define the pipeline
                var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", "placeholder")
                    .Append(mlContext.Transforms.Text.FeaturizeText("Features", new TextFeaturizingEstimator.Options
                    {
                        OutputTokensColumnName = "Words",
                        CharFeatureExtractor = new WordBagEstimator.Options
                        {
                            NgramLength = 2,
                            UseAllLengths = true
                        }
                    }))
                    .Append(mlContext.Transforms.NormalizeLpNorm("Features"))
                    .Append(mlContext.Transforms.Conversion.MapKeyToValue("Sentiment"));

                // train the model
                var trainedModel = pipeline.Fit(trainData);

                // evaluate the model
                var predictions = trainedModel.Transform(testData);
                var metrics = mlContext.MulticlassClassification.Evaluate(predictions);

                // make a prediction
                var predictionEngine = mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(trainedModel);
                var prediction = predictionEngine.Predict(new SentimentData
                {
                    Features = new string[] { query }
                });
                resultbox.Text = prediction.PredictedSentiment;
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

    }

    public class SentimentData
    {
        int len;

        [LoadColumn(0)]
        public string Sentiment { get; set; }

        [LoadColumn(1, 4)]
        public string[] Features { get; set; }
    }

    public class SentimentPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedSentiment;
    }


}
