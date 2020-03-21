using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace mayınTarlası
{
    public partial class Form1 : Form
    {
        
        Color buttonBackColor;
        int buttonSize;
        int buttonNumberX;
        int buttonNumberY;
        int mineNumber;
        Game game;
        Timer timer;
        ElapsedTime elapsedTime;
        Button[,] allButtons;
        Dictionary<Button, Squares> squaresInButtons;
        Dictionary<GameStatus, string> gameResultText;
        Dictionary<GameStatus, Color> gameResultColor;

        public Form1()
        {
            buttonSize = 35;
            buttonNumberX = 38;
            buttonNumberY = 17;
            mineNumber = (buttonNumberX * buttonNumberY) / 9 ;
            buttonBackColor = Color.FromArgb(160, 90, 250);

            gameResultText = new Dictionary<GameStatus, string>
            {
                { GameStatus.Won, "- - - - - WON - - - - - -" },
                { GameStatus.Lost, "- - - - - LOST - - - - - -" }
            };

            gameResultColor = new Dictionary<GameStatus, Color>
            {
                { GameStatus.Won, Color.Green },
                { GameStatus.Lost, Color.Red }
            };

            InitializeComponent();           
        }
       
        private void Form1_Load(object sender, EventArgs e)
        {           
            int panelWidth = buttonNumberX * buttonSize;
            int panelHeight = buttonNumberY * buttonSize;
            this.Width = panelWidth + 50;
            this.Height = panelHeight + 100;

            pnlMayınlar.Size = new Size(panelWidth, panelHeight);
            pnlMayınlar.Left = 20;
            pnlMayınlar.Top = 85;
            pnlMayınlar.BackColor = Color.Black;

            InitializeGame();

            int lblTop = 40;
            label2.Top = lblTop;           
            lblTimeShower.Top = lblTop;
            
            label1.Text = "Remaining Square : " + game.NumberOfNotOpenedSafetySquare().ToString();
            label1.Location = new Point(panelWidth - label1.Width, lblTop);
            pnlMayınlar.Show();
        }

       
        void InitializeGame()
        {
            squaresInButtons = new Dictionary<Button, Squares>();
            game = new Game(buttonNumberX, buttonNumberY, mineNumber);
            allButtons = new Button[buttonNumberY, buttonNumberX];
            pnlMayınlar.Enabled = true;

            for (int i = 0; i < game.Squares.GetLength(0); i++)
            {
                for (int j = 0; j < game.Squares.GetLength(1); j++)
                {
                    Button button = CreateButton(j, i);
                    squaresInButtons.Add(button, game.Square(j, i));
                    pnlMayınlar.Controls.Add(button);
                }
            }

            label2.Hide();
            label1.Show();
            SetLabelText(game.NumberOfNotOpenedSafetySquare());
            elapsedTime = new ElapsedTime();
            timer = new Timer
            {
                Interval = 1000,

            };
            timer.Tick += DrawElapsedTime;
            timer.Start();
        }

        Button CreateButton(int x, int y)
        {
                    Button button = new Button()
                    {
                        Size = new Size(buttonSize, buttonSize),
                        Top = y * buttonSize,
                        Left = x * buttonSize,
                        BackColor = buttonBackColor,
                        BackgroundImageLayout = ImageLayout.Stretch
                    };
                    button.MouseDown += ClickingOnSquares;
                    allButtons[y, x] = button;

                    return button;
        }

        private void yeniOyunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pnlMayınlar.Controls.Clear();
            InitializeGame();
        }


        void ClickingOnSquares(object sender, MouseEventArgs e)
        {
            Button clicked = sender as Button;
            Squares square = squaresInButtons[clicked];

            if (e.Button == MouseButtons.Right)
            {

                Actions actions = game.ClickSquare(Clicks.RightClick, square);

                if (actions == Actions.DoNothing)
                {
                    return;
                }

                if (actions == Actions.PutFlag)
                {
                    clicked.BackgroundImage = Properties.Resources.flagIcon;
                }
                else if(actions == Actions.RemoveFlag)
                {
                    clicked.BackgroundImage = null;
                    clicked.BackColor = buttonBackColor;
                }                
            }

            if (e.Button == MouseButtons.Left)
            {
                Actions actions = game.ClickSquare(Clicks.LeftClick, square);

                if (actions == Actions.DoNothing)
                {
                    return;
                }
                // open left clicked square that has at least one neighborhood mine
                else if (actions == Actions.OpenSquare)
                {
                    OpenMineFreeSquare(square);
                }
                // open square that has no mine neighborhood and its neighborhoods at once
                else if (actions == Actions.OpenSquaresRecursively)
                {
                    IEnumerable<Squares> squareList = game.SquaresWillBeOpened(square);
                    foreach (Squares item in squareList)
                    {
                        OpenMineFreeSquare(item);
                    }
                }
                else if (actions == Actions.ExplodeAllMines)
                {
                    // show where all mines are after any mine is clicked
                    IEnumerable<Squares> allMines = game.MinesToShow();
                    ShowMines(allMines);
                    Thread.Sleep(1000);
                    // put exploded mine image on every mine 
                    //in order to their distance first clicked mine
                    IEnumerable<Squares> inLineMines = game.MinesToExplode(square);
                    ExplodeAllMines(inLineMines);
                }


                SetLabelText(game.NumberOfNotOpenedSafetySquare());

                // getting game situation for checking if there is a win or lose
                GameStatus gameState = game.GameSituation();

                // if game should be continue then leave method 
                // else check there is a win or lose and do necessary things
                if (gameState == GameStatus.NotFinished | gameState == GameStatus.Default)
                {
                    return;
                }
                else
                {
                    // stop counting time and write resulting text above map
                    timer.Stop();
                    label1.Hide();

                    label2.Show();
                    label2.ForeColor = gameResultColor[gameState];
                    label2.Text = gameResultText[gameState];
                    label2.Left = (this.Width - label2.Width) / 2;

                    if (gameState == GameStatus.Won)
                    {
                        IEnumerable<Squares> notDetonetedMines = game.MinesToShow();
                        ShowMines(notDetonetedMines);
                    }
                    else
                    {
                        // opening all not opened non-mine squares after all mines exploded
                        IEnumerable<Squares> NotOpenedSquares = game.NotOpenedSquare();
                        foreach (Squares item in NotOpenedSquares)
                        {
                            OpenMineFreeSquare(item);
                            Thread.Sleep(10);
                        }
                    }

                    pnlMayınlar.Enabled = false;
                }    
            }  
            
        }

        // when a no-mine square is clicked, number of neighborhood mine is wrote 
        // on it and colored depending on that number
        void OpenMineFreeSquare(Squares square)
        {
            Button clicked = allButtons[square.Location.Y, square.Location.X];
            if (square.NumberOfAdjacentMines > 0)
            {
                clicked.Text = square.NumberOfAdjacentMines.ToString();
            }           
            clicked.BackColor = SquareTextColor(square.NumberOfAdjacentMines);
            clicked.Enabled = false;
        }

        // put a detoneted mine image on squares after any mine is clicked
        void ExplodeAllMines(IEnumerable<Squares> inLineMines)
        {
            foreach (Squares item in inLineMines)
            {
                Button willBeDetoneted = allButtons[item.Location.Y, item.Location.X];
                willBeDetoneted.BackgroundImage = Properties.Resources.detonetedmine;
                willBeDetoneted.Enabled = false;
                willBeDetoneted.Update();
                Thread.Sleep(50);
            }  
        }

        // put a not-detoneted mine image on squares before detoneted mine image is put
        // for making exploding animation
        void ShowMines(IEnumerable<Squares> inLineMines)
        {
            foreach (Squares item in inLineMines)
            {
                Button willBeDetoneted = allButtons[item.Location.Y, item.Location.X];
                willBeDetoneted.BackgroundImage = Properties.Resources.notDetonetedMine;
                willBeDetoneted.Enabled = false;
            }
        }

        // start when map is loaded and showing elapsed time at the left upper corner 
        void DrawElapsedTime(object source, EventArgs e)
        {
            lblTimeShower.Text = elapsedTime.TimeInHourFormat();
        }

        // write number of how many more square must be clicked for to win
        void SetLabelText(int score)
        {
            label1.Text = "Remaining Square : " + score.ToString();
        }

        // color list for squares those have neighborhood mine at least one
        Color SquareTextColor(int mineNumber)
        {
            Color[] colors = {
                 Color.FromArgb(180, 180, 180) ,
                 Color.FromArgb(20, 110, 250) ,
                 Color.FromArgb(10, 220, 20),
                 Color.FromArgb(250, 20, 20),
                 Color.FromArgb(150, 20, 60),
                 Color.FromArgb(180, 40, 170),
                 Color.FromArgb(90, 20, 20),
                 Color.FromArgb(80, 30, 60),
                 Color.FromArgb(50, 10, 40)
            };

            return colors[mineNumber];
        }
    }
}
