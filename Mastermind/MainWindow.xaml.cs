using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Mastermind
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Colors: Red, Green, Blue, Yellow, Purple, Brown, White, Black
    /// </summary>
    public partial class MainWindow : Window
    {
        int[,] GuessedColors = new int[13, 5];
        int[] AnswerColors = new int[5];
        int[] SelectedColors = new int[5] { -1, -1, -1, -1, -1 };
        Color[] DefaultColors = { Colors.Black, Colors.White, Colors.Transparent, Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Purple, Colors.Orange, Colors.White, Colors.Black };
        string[] ColorNames = { "Black", "White", "Transparent", "Red", "Green", "Blue", "Yellow", "Purple", "Orange", "White", "Black" };
        bool won = false;
        bool prepared = false;
        bool running = false;
        BackgroundWorker timer = new BackgroundWorker();
        string[] args = Environment.GetCommandLineArgs();

        public MainWindow()
        {
            InitializeComponent();
            Render();
            InitialiseGame();
            NewGame();
            timer.WorkerReportsProgress = false;
            timer.WorkerSupportsCancellation = true;
            timer.DoWork += new DoWorkEventHandler(timer_DoWork);
        }
        private void Play()
        {
            running = true;
            timer.RunWorkerAsync();
            resetProgress.Text = "New";
        }
        private void Render()
        {

            Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
        }
        private void InitialiseGame()
        {
            SelectPoints();
        }
        private void NewGame()
        {
            EndGameHider.Visibility = Visibility.Hidden;
            newGameButton.Fill = new SolidColorBrush(Colors.Red);
            Render();
            decimal parsed = 0m;
            for (int i = 0; i < 13; i++)
            {
                for (int ii = 0; ii < 5; ii++)
                {
                    points[i, ii].Fill = new SolidColorBrush(Colors.Red);
                    points[i, ii].StrokeThickness = 1;
                    GuessedColors[i, ii] = -1;
                    parsed = AddProgress(parsed);
                    Thread.Sleep(20);
                }
                for (int ii = 0; ii < 5; ii++)
                {
                    results[i, ii].Fill = new SolidColorBrush(Colors.Red);
                    results[i, ii].StrokeThickness = 1;
                    GuessedColors[i, ii] = -1;
                    parsed = AddProgress(parsed);
                    Thread.Sleep(10);
                }
            }
            for (int i = 0; i < 13; i++)
            {
                for (int ii = 0; ii < 5; ii++)
                {
                    points[i, ii].Fill = new SolidColorBrush(Colors.Transparent);
                    points[i, ii].StrokeThickness = 1;
                    GuessedColors[i, ii] = -1;
                    parsed = AddProgress(parsed);
                    Thread.Sleep(10);
                }
                for (int ii = 0; ii < 5; ii++)
                {
                    results[i, ii].Fill = new SolidColorBrush(Colors.Transparent);
                    results[i, ii].StrokeThickness = 1;
                    GuessedColors[i, ii] = -1;
                    parsed = AddProgress(parsed);
                    Thread.Sleep(10);
                }
            }
            for (int i = 0; i < 5; i++)
            {
                AnswerColors[i] = -1;
                SelectedColors[i] = -1;
            }
            for (int i = 0; i < 5; i++)
            {
                Random random = new Random();
                bool done = false;
                while (!done)
                {
                    int index = random.Next(3, 10);
                    if (!AnswerColors.Contains(index))
                    {
                        done = true;
                        AnswerColors[i] = index;
                    }
                }
            }
            for (int i = 0; i < 5; i++)
            {
                answers[i].Fill = new SolidColorBrush(Colors.Red);
                colorSelectors[i].Fill = new SolidColorBrush(Colors.Red);
                parsed = AddProgress(parsed);
                Thread.Sleep(20);
            }
            for (int i = 0; i < 5; i++)
            {
                answers[i].Fill = new SolidColorBrush(Colors.Gray);
                colorSelectors[i].Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE2B163"));
                SelectedColors[i] = -1;
                parsed = AddProgress(parsed);
                Thread.Sleep(10);
            }
            newGameButton.Fill = new SolidColorBrush(Colors.Lime);
            if (resetProgress.Text == "100%")
            {
                resetProgress.Text = "Play";
            }
            if (args.Contains("/Dev") || args.Contains("-Dev"))
            {
                MessageBox.Show(string.Format("Current color code is: {0}, {1}, {2}, {3}, {4}", ColorNames[AnswerColors[0]], ColorNames[AnswerColors[1]], ColorNames[AnswerColors[2]], ColorNames[AnswerColors[3]], ColorNames[AnswerColors[4]]), "Development", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            if (timer.IsBusy) timer.CancelAsync();
            won = false;
            round = 0;
            Render();
            prepared = true;
            running = false;
            timerView.Text = "0:00";
            MainGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE2B163"));
            NewGameHider.Visibility = Visibility.Visible;
        }
        private void Guess()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int ii = 0; ii < 5; ii++)
                {
                    points[round, ii].Fill = new SolidColorBrush(DefaultColors[SelectedColors[ii]]);
                }

            }
            if (EvaluateFuzzy()) Won();
            round++;
            if (round == 12) Lost();
        }
        private void Won()
        {
            ShowCode();
            timer.CancelAsync();
            MainGrid.Background = new SolidColorBrush(Colors.LawnGreen);
            new System.Media.SoundPlayer(@"applause.wav").Play();
            //MessageBox.Show(string.Format("You won! It took you {0} moves in {1} minutes and {2} seconds.", round, timerView.Text.Split(':')[0], timerView.Text.Split(':')[1]), "Mastermind", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            resultsBox.Text = string.Format("You won the game in {0} moves and it took you {1} minutes and {2} seconds!", round + 1, timerView.Text.Split(':')[0], timerView.Text.Split(':')[1]);
            gameResult.Text = "WINNER!";
            won = true;
            timerView.Text = "0:00";
            EndGameHider.Visibility = Visibility.Visible;
        }
        private void Lost()
        {
            ShowCode();
            timer.CancelAsync();
            MainGrid.Background = new SolidColorBrush(Colors.Firebrick);
            new System.Media.SoundPlayer(@"boo.wav").Play();
            //MessageBox.Show(string.Format("You won! It took you {0} moves in {1} minutes and {2} seconds.", round, timerView.Text.Split(':')[0], timerView.Text.Split(':')[1]), "Mastermind", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            resultsBox.Text = string.Format("You lost the game in {0} moves and it took you {1} minutes and {2} seconds!", round + 1, timerView.Text.Split(':')[0], timerView.Text.Split(':')[1]);
            gameResult.Text = "LOSER!";
            won = true;
            timerView.Text = "0:00";
            EndGameHider.Visibility = Visibility.Visible;
        }
        private bool EvaluateFuzzy()
        {
            int[] AnswerCopy = (int[])AnswerColors.Clone();
            int black = 0;
            int white = 0;
            for (int i = 0; i < 5; i++)
            {
                if (SelectedColors[i] == AnswerCopy[i])
                {
                    AnswerCopy[i] = -1;
                    black++;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                for (int ii = 0; ii < 5; ii++)
                {
                    if (AnswerCopy[ii] == SelectedColors[i])
                    {
                        AnswerCopy[ii] = -1;
                        white++;
                    }
                }
            }
            for (int i = 0; i < white+black; i++)
            {
                int point = 0;
                for (int ii = 0; ii < black; ii++)
                {
                    results[round, point].Fill = new SolidColorBrush(Colors.Black);
                    point++;
                }
                for (int ii = 0; ii < white; ii++)
                {
                    results[round, point].Fill = new SolidColorBrush(Colors.White);
                    point++;
                }
            }
            if (black==5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void ShowCode()
        {
            for (int i = 0; i < 5; i++)
            {
                answers[i].Fill = new SolidColorBrush(DefaultColors[AnswerColors[i]]);
            }
        }
        private decimal AddProgress(decimal parsed)
        {
            parsed += (1m / ((13m * 5m * 4m) + 10m)) * 100m;
            resetProgress.Text = string.Format("{0:0}%", parsed);
            Render();
            return parsed;
        }
        private Ellipse[,] points;
        private Ellipse[,] results;
        private Rectangle[] answers;
        private Ellipse[] colorSelectors;
        private Ellipse[] defColors;
        private int round = 0;
        private void SelectPoints()
        {
            points = new Ellipse[13, 5] {
                { point1_1, point1_2, point1_3, point1_4, point1_5 },
                { point2_1, point2_2, point2_3, point2_4, point2_5 },
                { point3_1, point3_2, point3_3, point3_4, point3_5 },
                { point4_1, point4_2, point4_3, point4_4, point4_5 },
                { point5_1, point5_2, point5_3, point5_4, point5_5 },
                { point6_1, point6_2, point6_3, point6_4, point6_5 },
                { point7_1, point7_2, point7_3, point7_4, point7_5 },
                { point8_1, point8_2, point8_3, point8_4, point8_5 },
                { point9_1, point9_2, point9_3, point9_4, point9_5 },
                { point10_1, point10_2, point10_3, point10_4, point10_5 },
                { point11_1, point11_2, point11_3, point11_4, point11_5 },
                { point12_1, point12_2, point12_3, point12_4, point12_5 },
                { point13_1, point13_2, point13_3, point13_4, point13_5 }
            };
            results = new Ellipse[13, 5] {
                { result1_1, result1_2, result1_3, result1_4, result1_5 },
                { result2_1, result2_2, result2_3, result2_4, result2_5 },
                { result3_1, result3_2, result3_3, result3_4, result3_5 },
                { result4_1, result4_2, result4_3, result4_4, result4_5 },
                { result5_1, result5_2, result5_3, result5_4, result5_5 },
                { result6_1, result6_2, result6_3, result6_4, result6_5 },
                { result7_1, result7_2, result7_3, result7_4, result7_5 },
                { result8_1, result8_2, result8_3, result8_4, result8_5 },
                { result9_1, result9_2, result9_3, result9_4, result9_5 },
                { result10_1, result10_2, result10_3, result10_4, result10_5 },
                { result11_1, result11_2, result11_3, result11_4, result11_5 },
                { result12_1, result12_2, result12_3, result12_4, result12_5 },
                { result13_1, result13_2, result13_3, result13_4, result13_5 }
            };
            answers = new Rectangle[5] { answer1, answer2, answer3, answer4, answer5 };
            colorSelectors = new Ellipse[5] { colorSelector1, colorSelector2, colorSelector3, colorSelector4, colorSelector5 };
            defColors = new Ellipse[8] { defColors1, defColors2, defColors3, defColors4, defColors5, defColors6, defColors7, defColors8 };
            for (int i = 0; i < defColors.Length; i++)
            {
                defColors[i].Fill = new SolidColorBrush(DefaultColors[i+3]);
            }
        }

        private void newGameButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NewGameButtonWorker();
        }

        private void guess_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!won&&running&&CheckSelectedColorDuplicity()) Guess();
        }

        private bool CheckSelectedColorDuplicity()
        {
            for (int i = 0; i < SelectedColors.Length; i++)
            {
                for (int ii = 0; ii < SelectedColors.Length; ii++)
                {
                    if (SelectedColors[i] == SelectedColors[ii] && i != ii) return false;
                }
            }
            return !SelectedColors.Contains(-1);
        }

        private void timer_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            bool count = true;
            while (count)
            {
                if (worker.CancellationPending)
                {
                    count = false;
                }
                Thread.Sleep(1000);
                string timeView = "";
                string newTimeView = "";
                Dispatcher.Invoke(new Action(() => { timeView = timerView.Text; }));
                string[] time = timeView.Split(':');
                int length = 60 * int.Parse(time[0]) + int.Parse(time[1]) + 1;
                //TimeSpan length = new TimeSpan(0, int.Parse(time[0]), int.Parse(time[1]));
                //length.Add(TimeSpan.FromSeconds(1));
                TimeSpan newTime = TimeSpan.FromSeconds(length);
                newTimeView = string.Format("{0}:{1}", newTime.Minutes, newTime.Seconds);
                Dispatcher.Invoke(new Action(() => { timerView.Text = newTimeView; }));
            }
        }

        private void swapColors(int number, bool asend)
        {
            if (!running) return;
            number--;
            if (asend)
            {
                if (SelectedColors[number] == -1 || SelectedColors[number] == 10)
                {
                    SelectedColors[number] = 3;
                    colorSelectors[number].Fill = new SolidColorBrush(DefaultColors[SelectedColors[number]]);
                }
                else
                {
                    SelectedColors[number]++;
                    colorSelectors[number].Fill = new SolidColorBrush(DefaultColors[SelectedColors[number]]);
                }
            }
            else
            {
                if (SelectedColors[number] == -1 || SelectedColors[number] == 3)
                {
                    SelectedColors[number] = 10;
                    colorSelectors[number].Fill = new SolidColorBrush(DefaultColors[SelectedColors[number]]);
                }
                else
                {
                    SelectedColors[number]--;
                    colorSelectors[number].Fill = new SolidColorBrush(DefaultColors[SelectedColors[number]]);
                }
            }
        }

        private void colorSelector1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            swapColors(1, true);
        }

        private void colorSelector2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            swapColors(2, true);
        }

        private void colorSelector3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            swapColors(3, true);
        }

        private void colorSelector4_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            swapColors(4, true);
        }

        private void colorSelector5_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            swapColors(5, true);
        }

        private void colorSelector1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            swapColors(1, false);
        }

        private void colorSelector2_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            swapColors(2, false);
        }

        private void colorSelector3_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            swapColors(3, false);
        }

        private void colorSelector4_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            swapColors(4, false);
        }

        private void colorSelector5_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            swapColors(5, false);
        }

        private void newGameButton1_Click(object sender, RoutedEventArgs e)
        {
            NewGameButtonWorker();
        }
        private void NewGameButtonWorker()
        {
            if (!prepared)
            {
                NewGame();
            }
            else
            {
                if (running)
                {
                    timer.CancelAsync();
                    if (MessageBox.Show("Are you sure you want to start new game?", "Verify", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        NewGame();
                        return;
                    }
                    timerView.Text = TimeSpan.FromSeconds(TimeSpan.Parse(timerView.Text).TotalSeconds - 2).ToString();
                    timer.RunWorkerAsync();
                }
                else
                {
                    Play();
                    NewGameHider.Visibility = Visibility.Hidden;
                }
            }
        }
    }
}
