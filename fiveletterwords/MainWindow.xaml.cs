using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace fiveletterwords
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string _currentPath = "";
        private double _runValue = 0;
        public double RunValue { get { return _runValue; } set { _runValue = value; } }

        public MainWindow()
        {
            InitializeComponent();
            txtNumLetter.Text = _numlettersValue.ToString();
            txtNumWords.Text = _numWordsValue.ToString();
        }

        private void SelectAddPlus_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                List<string> paths = new List<string>();
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "Text files (*.txt)|*.txt";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (string filename in openFileDialog.FileNames)
                        paths.Add(filename);
                }
                foreach (var item in paths)
                {
                    int n = StackpanelFileList.Children.Count;
                    Grid grid = new Grid() { Height = 22, Name = "f"+n };
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    grid.ColumnDefinitions.Add(new ColumnDefinition() );
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    StackpanelFileList.Children.Add(grid);
                    RadioButton radioButton = new RadioButton() { Name = "r" + n, GroupName = "FileGroup", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(2), IsChecked = false };
                    //radioButton.Checked += new System.EventHandler(this.Checked_Click);
                    if (item == paths.Last())
                    {
                        radioButton.IsChecked = true;
                        _currentPath = item;
                    } 
                    Grid.SetColumn(radioButton, 0);
                    grid.Children.Add(radioButton);
                    Separator separator1 = new Separator() { Margin = new Thickness(2) };
                    Grid.SetColumn(separator1, 1);
                    grid.Children.Add(separator1);
                    TextBlock textBlock = new TextBlock() { Name = "t"+n, FontSize = 16, Text = item };
                    Grid.SetColumn(textBlock, 2);
                    grid.Children.Add(textBlock);
                    Separator separator2 = new Separator() { Margin = new Thickness(2) };
                    Grid.SetColumn(separator2, 3);
                    grid.Children.Add(separator2);
                    //Image image = new Image() { Source = new BitmapImage(new Uri(@"\resources\downarrow.png")) , Height  = 14, Margin = new Thickness(0,0,4,0), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    //Grid.SetColumn(image, 4);
                    Separator separator3 = new Separator() { Margin = new Thickness(2) };
                    StackpanelFileList.Children.Add(separator3);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Checked_Click(object? sender, RoutedEventArgs e)
        {
            _currentPath = "t"+this.Name.Substring(1, this.Name.Length - 1);
        }

        // all logic for the letter configuration 
        private int _numlettersValue = 5;
        public int NumlettersValue
        {
            get { return _numlettersValue; }
            set
            {
                _numlettersValue = value;
                txtNumLetter.Text = value.ToString();
            }
        }
        private void cmdUpNumLetter_Click(object sender, RoutedEventArgs e)
        {
            if (NumWordsValue * (NumlettersValue+1) < 27)
                NumlettersValue++;
        }
        private void cmdDownNumLetter_Click(object sender, RoutedEventArgs e)
        {
            NumlettersValue--;
        }
        private void txtNum_TextLetterChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNumLetter == null)
            {
                return;
            }

            if (!int.TryParse(txtNumLetter.Text, out _numlettersValue))
                txtNumLetter.Text = _numlettersValue.ToString();
        }


        // All the logic for the word length configurations 
        private int _numWordsValue = 5;
        public int NumWordsValue
        {
            get { return _numWordsValue; }
            set
            {
                _numWordsValue = value;
                txtNumWords.Text = value.ToString();
            }
        }
        private void cmdUpNumWords_Click(object sender, RoutedEventArgs e)
        {
            if((NumWordsValue+1) * NumlettersValue < 27)
                NumWordsValue++;
        }
        private void cmdDownNumWords_Click(object sender, RoutedEventArgs e)
        {
            NumWordsValue--;
        }
        private void txtNum_TextWordsChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNumWords == null)
            {
                return;
            }

            if (!int.TryParse(txtNumWords.Text, out _numWordsValue))
                txtNumWords.Text = _numWordsValue.ToString();
        }

        static List<string> wordCombos = new List<string>();
        double seconds = 0;
        int fileSize = 0;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //TextBlock myTextBlock = (TextBlock)this.FindName(_currentPath);
                if (true/*myTextBlock != null*/)
                {
                    fileSize = 0;
                    wordCombos.Clear();
                    seconds = 0;
                    Stopwatch stopwatch = new();
                    stopwatch.Start();
                    try
                    {
                        string[] lines = File.ReadAllLines(_currentPath);
                        fileSize = lines.Length;
                        Array.Sort(lines);
                        lines = lines.Where(x => x.Length == NumlettersValue && x.Distinct().Count() == NumlettersValue).ToArray();
                        long[] numbers = Array.Empty<long>();
                        if (NoAnagrams.IsChecked == true)
                        {
                            (lines, numbers) = RemoveAnagrams(lines);
                        }
                        else
                        {
                            numbers = WordToNumber(lines);
                        }
                        Parallel.For(0, numbers.Length, i =>
                        {
                            long[] result = new long[NumWordsValue];
                            result[0] = numbers[i];
                            AndingLoop(ShortArray(numbers, i), numbers[i], lines, 0, result, numbers);
                        });
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        stopwatch.Stop();
                    }


                }
                Popup popup = new Popup() { IsOpen = true };
            }
            catch (Exception)
            {

                throw;
            }
        }
        static long[] WordToNumber(string[] arr)
        {
            List<long> longs = new List<long>();
            foreach (var item in arr)
            {
                long Bit = 0;
                foreach (char i in item)
                {
                    Bit += 1 << i - 'a';
                }
                longs.Add(Bit);
            }
            return longs.ToArray();
        }
        static (string[], long[]) RemoveAnagrams(string[] arr)
        {
            List<long> longs = new();
            List<string> newArr = arr.ToList();
            foreach (var item in arr)
            {
                long Bit = 0;
                foreach (char i in item)
                {
                    Bit += 1 << i - 'a';
                }
                if (longs.Contains(Bit))
                {
                    newArr.Remove(item);
                    continue;
                }
                longs.Add(Bit);
            }
            return (newArr.ToArray(), longs.ToArray());
        }
        static void AndingLoop(long[] arr, long BitSum, string[] Lines, int SuccessCount, long[] Result, long[] Numbers)
        {
            try
            {
                long[] newArr = arr.Where(x => (x & BitSum) == 0).ToArray();

                if (SuccessCount == Result.Length)
                {
                    wordCombos.Add($"{Lines[Array.IndexOf(Numbers, Result[0])]}, {Lines[Array.IndexOf(Numbers, Result[1])]}, {Lines[Array.IndexOf(Numbers, Result[2])]}, {Lines[Array.IndexOf(Numbers, Result[3])]}, {Lines[Array.IndexOf(Numbers, Result[4])]}");
                    return;
                }
                for (int i = 0; i < newArr.Length; i++)
                {
                    SuccessCount++;
                    if ((newArr[i] & BitSum) == 0)
                    {
                        Result[SuccessCount] = newArr[i];
                        AndingLoop(ShortArray(newArr, i), newArr[i] + BitSum, Lines, SuccessCount, Result, Numbers);
                    }
                    SuccessCount--;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Shorty of Array, remov old used bits n' bobs
        static long[] ShortArray(long[] arr, int StartPos)
        {
            long[] newArr = new long[arr.Length - StartPos];
            for (int i = 0; i < newArr.Length; i++)
            {
                newArr[i] = arr[i + StartPos];
            }
            return newArr;
        }
    }
}
