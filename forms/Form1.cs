using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lab1_playingcardLibrary;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace forms
{
    public enum CardLocation
    {
        Deck,
        DiscardPile,
        StandardPile,
        Hand1,
        Hand2,
        temporary
    }
    public partial class Form1 : Form
    {
        Random random = new Random();
        private Deck _deck;
        private PlayingHand _hand1;
        private PlaySpace _table;
        private Transform _cardTrasnfomrBlueprint;
        private Label _attemptsBox;
        private Label _successesBox;
        public float LerpSmoothness;
        private ListBox _listBox;

        private List<Card> _cards;
        public Card FindCard(PlayingCard card) => _cards.Find(s => s.PlayingCard == card);

        private Point _deckPostion;
        public Point DeckPosition { get { return _deckPostion; } set { MoveDeck(value, 200); } }



        public Form1()
        {
            InitializeComponent();
            this.Shown += Form1_Shown;
            this.FormClosed += Form1_FormClosed;
        }

        string leaderBoardPath = "./leaderderboard.data";

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            List<User> users = new List<User>();
            foreach (object o in _listBox.Items) users.Add(o as User);
            JsonFactory.Save(leaderBoardPath, users.ToArray());
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Width = Screen.FromControl(this).Bounds.Width;
            Height = Screen.FromControl(this).Bounds.Height;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.Green;
            LerpSmoothness = 1f / 60f;
            DoubleBuffered = true;
            SetupGame();
            Shown += PaintAllCards;
        }

        private void PaintAllCards(object sender, EventArgs e)
        {
            RefreshAllCards();
        }
        private void SetupGame()
        {
            _hand1 = new PlayingHand((Width / 2), Height / 8 * 7);
            _table = new PlaySpace(Width / 2, Height / 2, Width / 8 * 7, Height / 4 * 3, 1f);
            _table.Ypos = _table.OffsetY + Width / 16;
            CardRank[] temp = new CardRank[]
            {
                CardRank.Ace,
                CardRank.King,
                CardRank.Queen,
                CardRank.Jack,
                CardRank.Two,
                CardRank.Three,
                CardRank.Four,
                CardRank.Five
            };
            _deck = new Deck(1, temp);
            _cards = new List<Card>();
            _cardTrasnfomrBlueprint = new Transform(0, 0, 75, 107, 1);

            _attemptsBox = CreateLable(_hand1.PositionCards.X + 100, _hand1.PositionCards.Y - 20, 100, 30, "Guesses: 0", UpdateLable);
            _successesBox = CreateLable(_hand1.PositionCards.X + 100, _hand1.PositionCards.Y + 20, 100, 30, "Correct: 0", UpdateLable);

            Controls.Add(new Label() { Location = new Point(_hand1.PositionCards.X - 350, _hand1.PositionCards.Y - 300), Text = "High Scores" });

            _listBox = new ListBox();
            User[] users = JsonFactory.Read<User[]>(leaderBoardPath);
            if (users != null) _listBox.Items.AddRange(users);
            _listBox.Location = new Point(_hand1.PositionCards.X - 500, _hand1.PositionCards.Y - 250);
            _listBox.Name = "HighScoresBox";
            _listBox.Size = new Size(300, 450);
            _listBox.Enabled = false;
            _listBox.BackColor = Color.White;
            _listBox.ForeColor = Color.Black;
            
            Controls.Add(_listBox);

            TextBox textBoxName = GenerateTextBox(_hand1.PositionCards.X + 300, _hand1.PositionCards.Y - 20, 200, 30, "Enter Name");
            GenerateButton(_hand1.PositionCards.X + 300, _hand1.PositionCards.Y + 20, 200, 50, "upload To leaderboard", ButtonPressed, textBoxName);

            foreach (PlayingCard card in _deck.Cards) { _cards.Add(new Card(card,_cardTrasnfomrBlueprint,this, _table)); }
            SetUpCards();
            SortCardLayers2Deck();
            SpreadCards();
            RefreshAllCards();
        }



        private TextBox GenerateTextBox(int x, int y, int w, int h, string text)
        {
            TextBox textBox = new TextBox();
            textBox.Text = text;
            textBox.Location = new Point(x, y);
            textBox.Width = w;
            textBox.Height = h;
            textBox.Tag = false;
            textBox.TextChanged += (sender, args) => (sender as TextBox).Tag = true;
            Controls.Add(textBox);
            return textBox;
        }

        private Button GenerateButton(int x, int y, int w, int h,string text, EventHandler _event, TextBox temp)
        {
            Button btn = new Button();
            btn.Location = new Point(x, y);
            btn.Size = new Size(w, h);
            btn.Text = text;
            btn.Click += _event;
            btn.Tag = temp;
            Controls.Add(btn);
            return btn;
        }

        private void ButtonPressed(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            TextBox box = btn.Tag as TextBox;
            if (box == null) return;
            if ((bool)box.Tag) AddUser(_listBox, new User(box.Text, ((int)_successesBox.Tag), ((int)_attemptsBox.Tag)));
        }

        private void AddUser(ListBox listBox, User user)
        {
            bool flag = false;
            List<User> users = new List<User>();
            for (int i = 0; i < listBox.Items.Count; i++)
            {
                User temp = (User)listBox.Items[i];
                if(temp.UserName == user.UserName)
                {
                    if (temp.CompareTo(user) < 0)
                    {
                        flag = true;
                        break;
                    }
                    continue;
                }
                users.Add((User)listBox.Items[i]);

            }
            if (flag) return;

            users.Add(user);
            Sort(listBox, users);
            
        }
        private void Sort(ListBox listBox)
        {
            List<User> users = new List<User>();
            for (int i = 0; i < listBox.Items.Count; i++) users.Add((User)listBox.Items[i]);
            Sort(listBox, users);
        }
        private void Sort(ListBox listBox, List<User> users)
        {
            users.Sort();
            listBox.Items.Clear();
            listBox.Items.AddRange(users.ToArray());
            listBox.Refresh();
        }

        private void UpdateLable(object sender, EventArgs e)
        {
            Label temp = sender as Label;
            if (temp == null) return;
            temp.Refresh();
        }
        private void IncreaseLabel(Label label)
        {
            string[] items = label.Text.Split(' ');
            int current = int.Parse(items[1]);
            label.Text = $"{items[0]} {current + 1}";
            label.Tag = current + 1;

        }

        private Label CreateLable(int x, int y, int width, int height, string text, EventHandler eventHandler){
            Label temp = new Label();
            temp.Text = text;
            temp.Size = new Size(width,height);
            temp.Location = new Point(x,y);
            temp.Tag = 0;
            temp.TextChanged += eventHandler;
            Controls.Add(temp);
            return temp;
        }
        int cardMargin = 3;
        private void SpreadCards()
        {
            int columns = (int)(_table.Width/((_cardTrasnfomrBlueprint.Width + cardMargin) * _table.ScaleMultiplier));
            int column = 0;
            int row = 0;
            
            for (int i = 0; i < _cards.Count; i++)
            {
                column = i % columns;
                row = i / columns;
                Transform transform = _cards[i].transform;
                transform.Xpos = 5 + transform.OffsetX + (transform.Width + cardMargin) * column;
                transform.Ypos = 5 + transform.OffsetY + (transform.Height + cardMargin) * row;

                _cards[i].PaintFast();
            }


        }

        private void SortCardLayers2Deck()//sorts layers based of _deck card order. will follow the shuffle of the deck.
        {
            _cards.Sort((a, b) => {
                return _deck.Cards.IndexOf(a.PlayingCard).CompareTo(_deck.Cards.IndexOf(b.PlayingCard));
            
            
            });
            foreach (Card card in _cards)
            {
                card._Image.BringToFront();
            }
        }

        public void ShuffleDeck()//shuffles deck
        {
            _deck.Shuffle();
            _deck.Shuffle();
        }

        private void SetUpCards()//first time card setup
        {
            foreach (Card card in _cards)
            {
                card.transform.PlaySpace = _table;
                card.UpdateImage();
                card.Paint();
                card._Image.MouseClick += MouseClickCard;
                card._Image.MouseEnter += MouseHandHover;
                card._Image.MouseLeave += MouseHandLeave;
            }
            ShuffleDeck();
        }


        private void RefreshAllCards()
        {
            foreach(Card card in _cards)
            {
                //if(card.PlayingCard._suit != CardSuit.Heart) card.transform.ScaleMultiplier = 0.2f; 
                card.PaintFast();
            }
        }


        private async Task MoveCard(Card card, Point destination, int timeMS)
        {
            await CardMovement(card, destination, timeMS);
            return;
        }
        private async Task CardMovement(Card card, Point destination, int timeMS)
        {
            if (timeMS == 0) throw new Exception("timeMS cant be 0. Math error");
            Point StartPosition = new Point(card.transform.Xpos, card.transform.Ypos);
            float time = 0f;

            while(time < timeMS)
            {
                card.transform.Xpos = Lerp(StartPosition.X, destination.X, Math.Min(1f, time / timeMS));
                card.transform.Ypos = Lerp(StartPosition.Y, destination.Y, Math.Min(1f, time / timeMS));
                time += LerpSmoothness * 1000f;
                card.PaintFast();
                await Task.Delay((int)(LerpSmoothness * 1000));
            }
            card.transform.Xpos = destination.X;
            card.transform.Ypos= destination.Y;
            card.PaintFast();

            return;
        }
        private int Lerp(float a, float b, float f)
        {
            return (int) (a + f * (b - a));
        }
        private async void MoveDeck(Point destination, int TimeMS)
        {
            float time = 0f;
            float jumps = TimeMS / _cards.Count;
            int jumpsI = (int)jumps;
            int i = 0;
            while(time < TimeMS && i < _cards.Count)
            {
                //destination = new Point((int)(Width * random.NextDouble()), (int)(Height * random.NextDouble()));
                MoveCard(_cards[i], destination, TimeMS);
                time += jumps;
                i++;
                await Task.Delay(jumpsI);
            }
        }

        private async void DealCard(int speed)//gives a card to player 1
        {
            PlayingCard pcard = _deck.DealTopCard();//gets playingcard from deck. gets removed from _deck
            Card card = FindCard(pcard);//gets Card with the containing Playingcard.

            _hand1.AddCard(pcard, card);// add playingCard and Card to _hand 1

            await MoveCard(card, _hand1.PositionCards, speed);//does animation, waits for it to complete

            card.Location = CardLocation.Hand1;// sets cards location to hand1
            card.transform.ScaleMultiplier = 1.5f;//increases card scale
            card.PlayingCard.SetFaceUp();// makes face up
            card.PaintFast();//paints card again.
            _hand1.UpdateHand();// redraws all cards and positions.

            
        }



        private bool canClick = true;
        private Card _pastCard;
        private bool firstCard = true;

        private async void CheckCard(Card card)
        {
            int unflipSpeed = 70;
            int flipSpeed = 200;
            int removeCardSpeed = 300;
            int CardRevealWaitSpeed = 400;

            canClick = false;
            card.Location = CardLocation.temporary;
            await FlipAnimation(card, flipSpeed);
            //do the animation
            if (!firstCard)
            {
                IncreaseLabel(_attemptsBox);
                await Task.Delay(CardRevealWaitSpeed);
                if (CheckMatch(card)) {
                    IncreaseLabel(_successesBox);
                    moveBothCards(card, removeCardSpeed);
                    card.Location = CardLocation.Hand2;
                    _pastCard.Location = card.Location;
                }
                else
                {
                    List<Task> tsks = new List<Task>();
                    tsks.Add(FlipAnimation(card, unflipSpeed));
                    tsks.Add(FlipAnimation(_pastCard, unflipSpeed));

                    await Task.WhenAll(tsks);
                    
                    card.Location = CardLocation.Deck;
                    _pastCard.Location = CardLocation.Deck;

                }
            }
            _pastCard = card;
            firstCard = !firstCard;
            canClick = true;
        }
        private async Task moveBothCards(Card card, int speed)
        {
            _pastCard._Image.BringToFront();
            card._Image.BringToFront();
            MoveCard(card, new Point(_table.WidthRaw/2, _table.Height), speed);
            MoveCard(_pastCard, new Point(_table.WidthRaw / 2, _table.Height), speed);
            await Task.Delay(speed);
            RefreshAllCards();
            return;
        }

        private async Task FlipAnimation(Card card, int speed)
        {
            if (speed == 0) throw new Exception("Speed cant be 0. Math devide error");
            int timer = 0;
            int originalWidth = card.transform.WidthRaw;
            bool flipped = false;
            while (timer < speed)
            {
                if(timer > speed / 2 && !flipped) { flipped = true; card.PlayingCard.Flip(); }
                card.transform.Width = Lerp(originalWidth, -originalWidth, (float)timer / (float)speed);
                card.PaintFast();
                await Task.Delay((int)(LerpSmoothness * 1000));
                timer += (int)(LerpSmoothness * 1000);
            }
            card.transform.Width = originalWidth;
            card.PaintFast();
        }

        private bool CheckMatch(Card card)
        {
            if (_pastCard == null) return false;
            return _pastCard.PlayingCard._rank.CompareTo(card.PlayingCard._rank) == 0;
        }




        // EVENTS ----------------------------------------------------------------------------------------------


        private void MouseHandLeave(object sender, EventArgs e)//mouse hover exit playing card
        {
            Card card = _cards.Find(c => c._Image == (PictureBox)sender);
            if (card.Location == CardLocation.Hand1)
            {
                card.transform.Ypos = _hand1.PositionCards.Y;
                card.PaintFast();
            }
        }

        private void MouseHandHover(object sender, EventArgs e)// mouse hover enters playing card 
        {
            Card card = _cards.Find(c => c._Image == (PictureBox)sender);
            if(card.Location == CardLocation.Hand1)
            {
                card.transform.Ypos = _hand1.PositionCards.Y - 30;
                card.PaintFast();
            }
        }

        private void button1_MouseClick(object sender, MouseEventArgs e)//reset button
        {
            foreach(Card card in _cards)
            {
                card.transform.ScaleMultiplier = 1f;
                card.Location = CardLocation.Deck;
                card.PlayingCard.SetFaceDown();
            }
            int count = _hand1._cards.Count;
            for(int i = 0; i < count; i++)
            {
                _deck.AddCard(_hand1.DealTopCard());
            }
            _deck.Shuffle();
            DeckPosition = new Point(70, 140);
        }

        
        private void MouseClickCard(object sender, MouseEventArgs e)//mouse clicks any card
        {
            Card card = _cards.Find(c => c._Image == (PictureBox)sender);
            if (card.Location == CardLocation.Deck && canClick) CheckCard(card);


        }


    }
}
