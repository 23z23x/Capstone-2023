using GeneticSharp;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Xml.Schema;

namespace AMLA
{
    /// <summary>
    /// Interaction logic for Page3.xaml
    /// </summary>
    public partial class Page3 : Page
    {
        //Variables
        //Decide which fitness function
        bool greatest;
        bool closeT;
        bool smallest;

        bool filegood;

        //Algorithm input
        OurPop pops;

        public Page3()
        {
            InitializeComponent();

            greatest = false;
            closeT = false;
            smallest = false;

            filegood = false;
        }

        private void ToHome_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);

            var frame = parentWindow.FindName("MainFrame") as Frame;

            Page1 home = new Page1();

            frame.NavigationService.Navigate(home);
        }

        private void ToGeneticExplanation_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);

            var frame = parentWindow.FindName("MainFrame") as Frame;

            GeneticExplanation about = new GeneticExplanation();

            frame.NavigationService.Navigate(about);
        }

        private void Radio_Checked(object sender, RoutedEventArgs e)
        {
            var button = sender as RadioButton;

            if (String.Equals(button.Content, "Closest sum to value t"))
            {
                Tinput.Visibility = Visibility.Visible;
                Tlabel.Visibility = Visibility.Visible;
                closeT = true;
                greatest = false;
                smallest = false;
            }
            else
            {
                Tinput.Visibility = Visibility.Hidden;
                Tlabel.Visibility = Visibility.Hidden;
            }

            if (String.Equals(button.Content, "Largest Sum"))
            {
                closeT = false;
                greatest = true;
                smallest = false;
            }

            if (String.Equals(button.Content, "Smallest Sum"))
            {
                closeT = false;
                greatest = false;
                smallest = true;
            }

        }

        private void FileButton_Clicked(object sender, RoutedEventArgs e)
        {
            //Use the file dialog to get file. Read through to get needed info, and tell the user if the file is not formatted correctly
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "csv files (*.csv)|*.csv";

            if(openFileDialog.ShowDialog() == true) 
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

                    //read everything and add it to population

                    string[] fileLines = File.ReadAllLines(file);

                    int totallines = fileLines.Length;

                    if(totallines < 2)
                    {
                        uploadfile.Content = "Invalid File";
                        MessageBox.Show("Not enough starter genes to work with (Must be at least 2)", "Invalid File", MessageBoxButton.OK);
                        filegood = false;
                        return;
                    }

                    var chromosomes = new List<IChromosome>();

                    foreach (string lines in fileLines)
                    {
                        var chrom = new IntegerChromosome(length);
                        string[] parts = lines.Split(",");
                        if(parts.Length != length)
                        {
                            uploadfile.Content = "Invalid File";
                            MessageBox.Show("Please check file for correct format. All rows must be of the same length", "Invalid File", MessageBoxButton.OK);
                            filegood = false;
                            return;
                        }

                        for (int i = 0; i < length; i++)
                        {
                            int val = int.Parse(parts[i]);
                            chrom.ReplaceGene(i, new Gene(val));
                        }
                        //Now add chromosome to list
                        chromosomes.Add(chrom);
                    }
                    //put chromosomes in population
                    pops = new OurPop(100, 100, new IntegerChromosome(length));
                    pops.chromosomes = chromosomes;
                    
                }
                uploadfile.Content = file.Split('\\')[file.Split('\\').Length - 1];
                filegood = true;
            }

        }

        private void EvaluateButton_Clicked(object sender, RoutedEventArgs e)
        {
            int generations;
            int t = 0; ;
            //We need to check firstly that everything has valid input. If not, button doesn't do anything.
            if(filegood == true && ((closeT == true && int.TryParse(Tinput.Text, out t)) || greatest == true || smallest == true) && (int.TryParse(ginput.Text, out generations) && generations > 0)) 
            {
                //Use fitness function based on input
                if(greatest == true)
                {
                    var fitness = new GreatestSum();

                    var selection = new EliteSelection();
                    var crossover = new UniformCrossover();
                    var mutation = new UniformMutation();
                    var ga = new GeneticAlgorithm(pops, fitness, selection, crossover, mutation);

                    ga.Termination = new GenerationNumberTermination(generations);
                    ga.Start();

                    //Return best chromosome
                    var bestChrom = ga.BestChromosome;
                    result.Text = string.Join(",", bestChrom.GetGenes());
                }
                if(smallest == true)
                {
                    var fitness = new LowestSum();

                    var selection = new EliteSelection();
                    var crossover = new UniformCrossover();
                    var mutation = new UniformMutation();
                    var ga = new GeneticAlgorithm(pops, fitness, selection, crossover, mutation);

                    ga.Termination = new GenerationNumberTermination(generations);
                    ga.Start();

                    //Return best chromosome
                    var bestChrom = ga.BestChromosome;
                    result.Text = string.Join(",", bestChrom.GetGenes());
                }

                if(closeT == true)
                {
                    var fitness = new CloseT();
                    fitness.t = t;

                    var selection = new EliteSelection();
                    var crossover = new UniformCrossover();
                    var mutation = new UniformMutation();
                    var ga = new GeneticAlgorithm(pops, fitness, selection, crossover, mutation);

                    ga.Termination = new GenerationNumberTermination(generations);
                    ga.Start();

                    //Return best chromosome
                    var bestChrom = ga.BestChromosome;
                    result.Text = string.Join(",", bestChrom.GetGenes());
                }
            }
        }

    }

    public class IntegerChromosome : ChromosomeBase
    {
        int length;
        public IntegerChromosome(int length) : base(length) 
        {
            this.length = length;
        }

        public override IChromosome CreateNew()
        {
            return new IntegerChromosome(this.length);
        }

        public override Gene GenerateGene(int value)
        {
            return new Gene(value);
        }

        public int[] returnArray()
        {
            int[] returnArray = new int[length];
            for(int i = 0; i < length; i++)
            {
                returnArray[i] = (int)this.GetGene(i).Value;
            }
            return returnArray;
        }
    }

    public class GreatestSum : IFitness
    {
        public double Evaluate(IChromosome chromosome)
        {
            int[] genes = ((IntegerChromosome)chromosome).returnArray();

            int fitness = 0;
            for (int i = 0; i < genes.Length; i++)
            {
                fitness += genes[i];
            }
            return fitness;
        }
    }

    public class LowestSum : IFitness
    {
        public double Evaluate(IChromosome chromosome)
        {
            int[] genes = ((IntegerChromosome) chromosome).returnArray();

            int fitness = 0;
            for(int i = 0; i <genes.Length; i++)
            {
                fitness -= genes[i];
            }

            return fitness;
        }
    }

    public class CloseT : IFitness
    {
        public int t { get; set; }

        public double Evaluate(IChromosome chromosome)
        {
            int[] genes = ((IntegerChromosome)chromosome).returnArray();

            int fitness = 0;
            //For this one, we want to sum the genes, then subtract t, then do the inverse so that close results return higher values.
            for (int i = 0; i < genes.Length; i++)
            {
                fitness += genes[i];
            }
            return 1.0 / (1.0 + Math.Abs(fitness - t));


        }
    }

    //We need to implement population interface to create first generation the way we want to
    //To this end, I have coppied their implementation so I can change a few things
    public class OurPop : IPopulation
    {
        /// <summary>
        /// Occurs when best chromosome changed.
        /// </summary>
        public event EventHandler BestChromosomeChanged;

        //simply assign this before running
        public List<IChromosome> chromosomes;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneticSharp.Population"/> class.
        /// </summary>
        /// <param name="minSize">The minimum size (chromosomes).</param>
        /// <param name="maxSize">The maximum size (chromosomes).</param>
        /// <param name="adamChromosome">The original chromosome of all population ;).</param>
        public OurPop(int minSize, int maxSize, IChromosome adamChromosome)
        {
            if (minSize < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(minSize), "The minimum size for a population is 2 chromosomes.");
            }

            if (maxSize < minSize)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSize), "The maximum size for a population should be equal or greater than minimum size.");
            }

            ExceptionHelper.ThrowIfNull(nameof(adamChromosome), adamChromosome);

            CreationDate = DateTime.Now;
            MinSize = minSize;
            MaxSize = maxSize;
            AdamChromosome = adamChromosome;
            Generations = new List<Generation>();
            GenerationStrategy = new PerformanceGenerationStrategy(10);

                
        }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; protected set; }

        /// <summary>
        /// Gets or sets the generations.
        /// <remarks>
        /// The information of Generations can vary depending of the IGenerationStrategy used.
        /// </remarks>
        /// </summary>
        /// <value>The generations.</value>
        public IList<Generation> Generations { get; protected set; }

        /// <summary>
        /// Gets or sets the current generation.
        /// </summary>
        /// <value>The current generation.</value>
        public Generation CurrentGeneration { get; protected set; }

        /// <summary>
        /// Gets or sets the total number of generations executed.
        /// <remarks>
        /// Use this information to know how many generations have been executed, because Generations.Count can vary depending of the IGenerationStrategy used.
        /// </remarks>
        /// </summary>
        public int GenerationsNumber { get; protected set; }

        /// <summary>
        /// Gets or sets the minimum size.
        /// </summary>
        /// <value>The minimum size.</value>
        public int MinSize { get; set; }

        /// <summary>
        /// Gets or sets the size of the max.
        /// </summary>
        /// <value>The size of the max.</value>
        public int MaxSize { get; set; }

        /// <summary>
        /// Gets or sets the best chromosome.
        /// </summary>
        /// <value>The best chromosome.</value>
        public IChromosome BestChromosome { get; protected set; }

        /// <summary>
        /// Gets or sets the generation strategy.
        /// </summary>
        public IGenerationStrategy GenerationStrategy { get; set; }

        /// <summary>
        /// Gets or sets the original chromosome of all population.
        /// </summary>
        /// <value>The adam chromosome.</value>
        protected IChromosome AdamChromosome { get; set; }

        /// <summary>
        /// Creates the initial generation.
        /// </summary>
        public virtual void CreateInitialGeneration()
        {
            Generations = new List<Generation>();
            GenerationsNumber = 0;

            CreateNewGeneration(chromosomes);
        }

        /// <summary>
        /// Creates a new generation.
        /// </summary>
        /// <param name="chromosomes">The chromosomes for new generation.</param>
        public virtual void CreateNewGeneration(IList<IChromosome> chromosomes)
        {
            ExceptionHelper.ThrowIfNull("chromosomes", chromosomes);
            chromosomes.ValidateGenes();

            CurrentGeneration = new Generation(++GenerationsNumber, chromosomes);
            Generations.Add(CurrentGeneration);
            GenerationStrategy.RegisterNewGeneration(this);
        }

        /// <summary>
        /// Ends the current generation.
        /// </summary>        
        public virtual void EndCurrentGeneration()
        {
            CurrentGeneration.End(MaxSize);

            if (BestChromosome == null || BestChromosome.CompareTo(CurrentGeneration.BestChromosome) != 0)
            {
                BestChromosome = CurrentGeneration.BestChromosome;

                OnBestChromosomeChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the best chromosome changed event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnBestChromosomeChanged(EventArgs args)
        {
            BestChromosomeChanged?.Invoke(this, args);
        }
    }

}
