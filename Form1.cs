using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Windows.Forms.VisualStyles;

namespace Hide_SeekGame
{

    public partial class Form1 : Form
    {
        private GameManager gameManager;

        private ObjectManager objectManager;

        private System.Windows.Forms.Timer mainTimer;

        private TimerTickRecoder tickOfMainTimer;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            #region 메인타이머 생성---------------------------------------
            mainTimer = new System.Windows.Forms.Timer();
            mainTimer.Enabled = true;
            mainTimer.Interval = 10;
            mainTimer.Tick += MainTimer_Tick;

            tickOfMainTimer = new TimerTickRecoder();

            #endregion

            #region 두 매니저 초기화------------------------------
            RandomChoose randQudrant = new RandomChoose(4);

            gameManager = new GameManager();
            objectManager = new ObjectManager(tickOfMainTimer);
            #endregion

            #region 테스트데이터-------------------------------------------
            PlayerInfo player1Info = new PlayerInfo()
            {
                IsPlaying = true,
                Keyboard = true,
                Controller = false,

                Qudrant = randQudrant.Choose(),
                Color = "Red",

                PlayerKeys = new PlayerKeys()
                {
                    Up = Keys.Up,
                    Down = Keys.Down,
                    Left = Keys.Left,
                    Right = Keys.Right,
                    Run = Keys.OemQuestion,
                    Aim = Keys.Oemcomma,
                    Attack = Keys.OemPeriod,
                },
            };

            PlayerInfo player2Info = new PlayerInfo()
            {
                IsPlaying = true,
                Keyboard = true,
                Controller = false,

                Qudrant = randQudrant.Choose(),
                Color = "Green",

                PlayerKeys = new PlayerKeys()
                {
                    Up = Keys.W,
                    Down = Keys.S,
                    Left = Keys.A,
                    Right = Keys.D,
                    Run = Keys.R,
                    Aim = Keys.Y,
                    Attack = Keys.T,
                },
            };

            PlayerInfo player3Info = new PlayerInfo()
            {
                IsPlaying = false,
                Keyboard = false,
                Controller = false,

                Qudrant = randQudrant.Choose(),
                Color = "Gray",

                PlayerKeys = KeyNone.None,
            };

            PlayerInfo player4Info = new PlayerInfo()
            {
                IsPlaying = false,
                Keyboard = false,
                Controller = false,

                Qudrant = randQudrant.Choose(),
                Color = "Gray",

                PlayerKeys = KeyNone.None,
            };
            #endregion

            #region 더미 추가--------------------------------------
            gameManager.DummyPatternSetting("0");
            gameManager.DummyCountSetting(40);

            for (int i = 0; i < gameManager.DummyCount; i++)
            {
                if(gameManager.DummyPattern == "Random")
                {
                    Random rand = new Random();
                    int tmp = rand.Next(0, 4);
                    objectManager.AddDummy(Controls, tmp);
                }
                else
                {
                    objectManager.AddDummy(Controls, Int32.Parse(gameManager.DummyPattern));
                }
            }

            #endregion

            #region 플레이어 추가 -----------------------------------------------
            gameManager.Player1Setting(player1Info);

            gameManager.Player2Setting(player2Info);

            gameManager.Player3Setting(player3Info);

            gameManager.Player4Setting(player4Info);

            objectManager.AddPlayer(Controls, gameManager.Player1Info);
            objectManager.AddPlayer(Controls, gameManager.Player2Info);
            objectManager.AddPlayer(Controls, gameManager.Player3Info);
            objectManager.AddPlayer(Controls, gameManager.Player4Info);
            #endregion


        }

        private void MainTimer_Tick(object? sender, EventArgs e)
        {
            tickOfMainTimer.IncreaseTotalTick();
            objectManager.PlayersMove();
            objectManager.DummiesMove();
        }
        

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            objectManager.PlayersKeyUpCheck(e);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            objectManager.PlayersKeyDownCheck(e);
        }
    }

    public struct PlayerInfo
    {
        public bool IsPlaying;
        public bool Keyboard;
        public bool Controller;

        public int Qudrant;
        public string Color;

        public PlayerKeys PlayerKeys;
        //public Gamepad Gamepad;

    }

    public struct PlayerKeys
    {
        public Keys Up;
        public Keys Down;
        public Keys Left;
        public Keys Right;
        public Keys Run;
        public Keys Aim;
        public Keys Attack;

    }

    public class KeyNone
    {
        public static PlayerKeys None = new PlayerKeys()
        {
            Up = Keys.None,
            Down = Keys.None,
            Left = Keys.None,
            Right = Keys.None,
            Run = Keys.None,
            Aim = Keys.None,
            Attack = Keys.None,
        };
    }

    public class Player
    {
        #region 필수 필드 ---------------------------------------
        protected bool isPlaying = false;

        public bool IsPlaying { get { return isPlaying; } }

        protected bool isAlive = true;

        public bool IsAlive { get { return isAlive; } }

        protected bool isGameStarted = false;

        protected int qudrant = 0;

        protected Color color = Color.Gray; //Player Color

        protected Color StrToColor(string str)
        {
            if (str == "Red")
            {
                return Color.Red;
            }
            else if (str == "Green")
            {
                return Color.Green;
            }
            else if (str == "Blue")
            {
                return Color.Blue;
            }
            else if (str == "Yellow")
            {
                return Color.Yellow;
            }
            else
            {
                return Color.Gray;
            }
        }


        protected Control.ControlCollection Controls;
        #endregion

        #region 플레이 관련 필드 --------------------------------

        private const int startCool = 5;
        private int nowStartCool = startCool;

        protected Point nowPosition;
        protected Point nowAimer1Position;
        protected Point nowAimer2Position;
        protected Point nowAimer3Position;
        protected Point nowAimer4Position;

        protected const int sizeOfAimer = 10;

        private const int moveSpeed = 1;

        protected bool isMovingL = false;
        protected bool isMovingR = false;
        protected bool isMovingU = false;
        protected bool isMovingD = false;
        protected bool isRunning = false;
        protected bool isAiming = false;


        private bool canAttack = true;
        private const int attackCooltime = 10; //sec
        private int attackCooldown = 0;
        private int attackCooltick = 0;

        private const int attackRange = 40;

        private bool canGunAttack = true;
        private bool bulletSizeBigger = true;

        #endregion

        #region 생성자 및 게임 시작 이벤트-----------------------

        protected void StartingTimer_Tick(object? sender, EventArgs e)
        {
            nowStartCool--;
            EditStartCoolLabel(nowStartCool);

            if (nowStartCool == -1)
            {
                EndStartingTimer();
            }
        }

        protected void EndStartingTimer()
        {
            this.isGameStarted = true;

            this.startingTimer.Enabled = false;
            this.startingTimer.Tick -= StartingTimer_Tick;

            RemoveControl(this.Controls, playerQudrant);
            RemoveControl(this.Controls, startCoolLabel);
        }
        #endregion

        #region 컨트롤 추가 및 제거 -----------------------------
        public void AddControl(Control.ControlCollection Controls, Control Obj)
        {
            Controls.Add(Obj);
        }

        public void RemoveControl(Control.ControlCollection Controls, Control Obj)
        {
            Obj.Dispose();
            Controls.Remove(Obj);
        }
        #endregion


        #region 타이머 및 컨트롤 --------------------------------
        protected System.Windows.Forms.Timer startingTimer = new System.Windows.Forms.Timer();
        protected System.Windows.Forms.Timer attackTimer = new System.Windows.Forms.Timer()
        {
            Enabled = true,
            Interval = 100,
        };
        protected System.Windows.Forms.Timer bulletTimer = new System.Windows.Forms.Timer()
        {
            Enabled = true,
            Interval = 5,
        };

        protected PictureBox playerObj = new PictureBox()
        {
            Size = new Size(20, 20),
            BackColor = Color.Orange,
            Visible = true,
        };

        protected PictureBox playerAttackRange = new PictureBox()
        {
            Size = new Size(attackRange, attackRange),
            BackColor = Color.Black,
            Visible = false,
        };

        protected PictureBox bullet = new PictureBox()
        {
            Size = new Size(10,10),
            BackColor = Color.Black,
            Visible = false,
        };

        protected PictureBox playerAimer1 = new PictureBox()
        {
            Size = new Size(sizeOfAimer, sizeOfAimer),
            Visible = false,
        };
        protected PictureBox playerAimer2 = new PictureBox()
        {
            Size = new Size(sizeOfAimer, sizeOfAimer),
            Visible = false,
        };
        protected PictureBox playerAimer3 = new PictureBox()
        {
            Size = new Size(sizeOfAimer, sizeOfAimer),
            Visible = false,
        };
        protected PictureBox playerAimer4 = new PictureBox()
        {
            Size = new Size(sizeOfAimer, sizeOfAimer),
            Visible = false,
        };

        protected PictureBox playerQudrant = new PictureBox()
        {
            Size = new Size(512, 300),
            Visible = true,
        };

        protected Label startCoolLabel = new Label()
        {
            Text = Convert.ToString(startCool),
            Visible = true,
            Font = new Font("맑은 고딕", 15F, FontStyle.Regular, GraphicsUnit.Point),
            Location = new Point(10, 10),
            Enabled = true,
            AutoSize = true,
        };
        #endregion


        #region 컨트롤 위치 설정 메서드--------------------------
        private void SetPosition(Control control, Point point)
        {
            control.Location = point;
        }
        protected void SetPlayerPosition(Point point)
        {
            SetPosition(this.playerObj, point);
            SetPosition(this.playerAttackRange, new Point(point.X - 10, point.Y - 10));

            SetPosition(this.playerAimer1, new Point(point.X + 40 - sizeOfAimer, point.Y - 20));
            SetPosition(this.playerAimer2, new Point(point.X - 20, point.Y - 20));
            SetPosition(this.playerAimer3, new Point(point.X - 20, point.Y + 40 - sizeOfAimer));
            SetPosition(this.playerAimer4, new Point(point.X + 40 - sizeOfAimer, point.Y + 40 - sizeOfAimer));
        }
        protected void SetQudrant()
        {
            this.playerQudrant.BackColor = this.color;
            if (this.qudrant == 1)
            {
                this.playerQudrant.Location = new Point(512, 0);
            }
            else if (this.qudrant == 2)
            {
                this.playerQudrant.Location = new Point(0, 0);
            }
            else if (this.qudrant == 3)
            {
                this.playerQudrant.Location = new Point(0, 300);
            }
            else
            {
                this.playerQudrant.Location = new Point(512, 300);
            }
        }

        protected Point RandomPoint()
        {
            int x, y;
            Random rand = new Random();

            if (this.qudrant == 1)
            {
                x = rand.Next(522, 994 + 1);
                y = rand.Next(10, 270 + 1);
            }
            else if (this.qudrant == 2)
            {
                x = rand.Next(10, 482 + 1);
                y = rand.Next(10, 270 + 1);
            }
            else if (this.qudrant == 3)
            {
                x = rand.Next(10, 482 + 1);
                y = rand.Next(310, 570 + 1);
            }
            else
            {
                x = rand.Next(522, 994 + 1);
                y = rand.Next(310, 570 + 1);
            }

            return new Point(x, y);
        }
        protected Point StartLabelPoint()
        {
            int x, y;

            if (this.qudrant == 1)
            {
                x = 522;
                y = 10;
            }
            else if (this.qudrant == 2)
            {
                x = 10;
                y = 10;
            }
            else if (this.qudrant == 3)
            {
                x = 10;
                y = 310;
            }
            else
            {
                x = 522;
                y = 310;
            }

            return new Point(x, y);
        }
        protected void SetStartLabelPosition(Point point)
        {
            this.startCoolLabel.Location = point;
        }

        #endregion


        #region 레이블 수정 -------------------------------------

        private void EditLable(Label label, int elem)
        {
            label.Text = Convert.ToString(elem);
        }

        private void EditStartCoolLabel(int elem)
        {
            EditLable(this.startCoolLabel, elem);
        }

        #endregion

        #region 이동 --------------------------------------------

        public void Move()
        {
            if (this.isPlaying == true && this.isAlive == true)
            {
                if (this.isAiming == false)
                {
                    if (this.isRunning == false)
                    {
                        if (isMovingU == true)
                        {
                            this.nowPosition.Y -= moveSpeed;
                        }
                        if (isMovingD == true)
                        {
                            this.nowPosition.Y += moveSpeed;
                        }
                        if (isMovingL == true)
                        {
                            this.nowPosition.X -= moveSpeed;
                        }
                        if (isMovingR == true)
                        {
                            this.nowPosition.X += moveSpeed;
                        }



                    }
                    else
                    {
                        if (isMovingU == true)
                        {
                            this.nowPosition.Y -= moveSpeed * 3;
                        }
                        if (isMovingD == true)
                        {
                            this.nowPosition.Y += moveSpeed * 3;
                        }
                        if (isMovingL == true)
                        {
                            this.nowPosition.X -= moveSpeed * 3;
                        }
                        if (isMovingR == true)
                        {
                            this.nowPosition.X += moveSpeed * 3;
                        }
                    }

                    if (this.nowPosition.X < 0)
                    {
                        this.nowPosition.X = 0;
                    }
                    else if (this.nowPosition.X > 1024 - 20)
                    {
                        this.nowPosition.X = 1024 - 20;
                    }

                    if (this.nowPosition.Y < 0)
                    {
                        this.nowPosition.Y = 0;
                    }
                    else if (this.nowPosition.Y > 600 - 20)
                    {
                        this.nowPosition.Y = 600 - 20;
                    }

                    this.nowAimer1Position = new Point(this.nowPosition.X + 40 - sizeOfAimer, this.nowPosition.Y - 20);
                    this.nowAimer2Position = new Point(this.nowPosition.X - 20, this.nowPosition.Y - 20);
                    this.nowAimer3Position = new Point(this.nowPosition.X - 20, this.nowPosition.Y + 40 - sizeOfAimer);
                    this.nowAimer4Position = new Point(this.nowPosition.X + 40 - sizeOfAimer, this.nowPosition.Y + 40 - sizeOfAimer);

                    SetPlayerPosition(this.nowPosition);
                }
                else
                {
                    if (isMovingU == true)
                    {
                        this.nowAimer1Position.Y -= moveSpeed * 4;
                        this.nowAimer2Position.Y -= moveSpeed * 4;
                        this.nowAimer3Position.Y -= moveSpeed * 4;
                        this.nowAimer4Position.Y -= moveSpeed * 4;

                    }
                    if (isMovingD == true)
                    {
                        this.nowAimer1Position.Y += moveSpeed * 4;
                        this.nowAimer2Position.Y += moveSpeed * 4;
                        this.nowAimer3Position.Y += moveSpeed * 4;
                        this.nowAimer4Position.Y += moveSpeed * 4;
                    }
                    if (isMovingL == true)
                    {
                        this.nowAimer1Position.X -= moveSpeed * 4;
                        this.nowAimer2Position.X -= moveSpeed * 4;
                        this.nowAimer3Position.X -= moveSpeed * 4;
                        this.nowAimer4Position.X -= moveSpeed * 4;
                    }
                    if (isMovingR == true)
                    {
                        this.nowAimer1Position.X += moveSpeed * 4;
                        this.nowAimer2Position.X += moveSpeed * 4;
                        this.nowAimer3Position.X += moveSpeed * 4;
                        this.nowAimer4Position.X += moveSpeed * 4;
                    }

                    SetPosition(this.playerAimer1, this.nowAimer1Position);
                    SetPosition(this.playerAimer2, this.nowAimer2Position);
                    SetPosition(this.playerAimer3, this.nowAimer3Position);
                    SetPosition(this.playerAimer4, this.nowAimer4Position);
                }
            }
        }

        #endregion

        #region 플레이어 죽음---------------------------------------
        public void PlayerDeath(Control.ControlCollection Controls)
        {
            this.isAlive = false;
            RemoveControl(Controls, playerObj);
            RemoveControl(Controls, playerAttackRange);
            RemoveControl(Controls, playerAimer1);
            RemoveControl(Controls, playerAimer2);
            RemoveControl(Controls, playerAimer3);
            RemoveControl(Controls, playerAimer4);
        }

        #endregion

        #region 공격 -----------------------------------------------

        public void Attack(Control.ControlCollection Controls, List<Player> players, List<Dummy> dummies)//
        {
            if(this.isPlaying == true && this.isAlive == true && this.isGameStarted == true && this.canAttack == true)
            {
                if(this.isAiming == false) //근거리 공격
                {

                    #region 공격 쿨타임 및 이펙트 -------------------------
                    this.playerAttackRange.Visible = true;

                    this.attackCooldown = attackCooltime;
                    this.canAttack = false;

                    this.attackTimer.Start();
                    this.attackTimer.Tick += AttackTimer_Tick;
                    #endregion
                    
                    #region 공격 효과 -------------------------

                    int thisPosX = this.playerAttackRange.Location.X;
                    int thisPosY = this.playerAttackRange.Location.Y;


                    foreach (Player player in players)
                    {

                        int thatPosX = player.playerObj.Location.X;
                        int thatPosY = player.playerObj.Location.Y;

                        if (thatPosX > thisPosX - 20 && thatPosX < thisPosX + attackRange &&
                            thatPosY > thisPosY - 20 && thatPosY < thisPosY + attackRange &&
                            player != this)
                        {
                            player.PlayerDeath(Controls);
                        }
                    }

                    foreach (Dummy dummy in dummies)
                    {

                        int thatPosX = dummy.dummyObj.Location.X;
                        int thatPosY = dummy.dummyObj.Location.Y;

                        if (thatPosX > thisPosX - 20 && thatPosX < thisPosX + attackRange &&
                            thatPosY > thisPosY - 20 && thatPosY < thisPosY + attackRange )
                        {
                            dummy.DummyDeath(Controls);
                        }
                    }



                    #endregion


                }
                else //원거리 공격
                {
                    if(this.canGunAttack == true)
                    {

                        #region 공격 이펙트-----------------------------------------
                        this.playerObj.BackColor = this.color;

                        this.canGunAttack = false;

                        this.bullet.Visible = true;
                        SetPosition(this.bullet, new Point(this.playerAimer2.Location.X + 25, this.playerAimer2.Location.Y + 25));
                        AddControl(Controls,this.bullet);

                        this.bulletTimer.Start();
                        this.bulletTimer.Tick += BulletTimer_Tick;
                        #endregion


                        #region 공격 효과-----------------------------------
                        int thisPosX = this.playerAimer2.Location.X;
                        int thisPosY = this.playerAimer2.Location.Y;

                        foreach (Player player in players)
                        {

                            int thatPosX = player.playerObj.Location.X;
                            int thatPosY = player.playerObj.Location.Y;

                            if (thatPosX > thisPosX - 20 && thatPosX < thisPosX + 60 &&
                                thatPosY > thisPosY - 20 && thatPosY < thisPosY + 60 &&
                                player != this)
                            {
                                player.PlayerDeath(Controls);
                            }
                        }

                        foreach (Dummy dummy in dummies)
                        {

                            int thatPosX = dummy.dummyObj.Location.X;
                            int thatPosY = dummy.dummyObj.Location.Y;

                            if (thatPosX > thisPosX - 20 && thatPosX < thisPosX + 60 &&
                                thatPosY > thisPosY - 20 && thatPosY < thisPosY + 60)
                            {
                                dummy.DummyDeath(Controls);
                            }
                        }
                        #endregion
                    }
                }

                // *필수* 게임오버체크
            }
        }

        private void BulletTimer_Tick(object? sender, EventArgs e)
        {
            if(this.bulletSizeBigger == true)
            {
                this.bullet.Size = new Size(this.bullet.Width + 2, this.bullet.Height + 2);
                this.bullet.Location = new Point(this.bullet.Location.X - 1, this.bullet.Location.Y - 1);

                if(this.bullet.Width == 60)
                {
                    this.bulletSizeBigger = false; 
                }
            }
            else
            {
                this.bullet.Size = new Size(this.bullet.Width - 2, this.bullet.Height - 2);
                this.bullet.Location = new Point(this.bullet.Location.X + 1, this.bullet.Location.Y + 1);

                if (this.bullet.Width == 10)
                {
                    this.playerObj.BackColor = Color.Orange;
                    RemoveControl(Controls, this.bullet);
                    
                    this.bulletTimer.Stop();
                    this.bulletTimer.Tick -= BulletTimer_Tick;
                }
            }
        }

        private void AttackTimer_Tick(object? sender, EventArgs e)
        {
            this.attackCooltick++;

            if(this.attackCooltick == 2)
            {
                this.playerAttackRange.Visible = false;
            }
            else if(this.attackCooltick % 10 == 0)
            {
                this.attackCooldown--;
                if(attackCooldown == 0)
                {
                    this.attackCooltick = 0;

                    this.canAttack = true;
                    this.attackTimer.Stop();
                    this.attackTimer.Tick -= AttackTimer_Tick;
                }
            }

        }

        #endregion
    }

    public class KeyboardPlayer : Player
    {
        private PlayerKeys playerKeys;

        #region 생성자 ---------------------------------------------
        public KeyboardPlayer(Control.ControlCollection Controls, bool isPlaying, int qudrant, string color, PlayerKeys playerKeys)
        {
            base.Controls = Controls;

            base.isPlaying = isPlaying;
            base.color = StrToColor(color);
            base.qudrant = qudrant;

            if (base.isPlaying == true)
            {
                this.playerKeys = playerKeys;

                base.playerAimer1.BackColor = base.color;
                base.playerAimer2.BackColor = base.color;
                base.playerAimer3.BackColor = base.color;
                base.playerAimer4.BackColor = base.color;

                base.nowPosition = RandomPoint();
                base.nowAimer1Position = new Point(base.nowPosition.X + 40 - sizeOfAimer, base.nowPosition.Y - 20);
                base.nowAimer2Position = new Point(base.nowPosition.X - 20, base.nowPosition.Y - 20);
                base.nowAimer3Position = new Point(base.nowPosition.X - 20, base.nowPosition.Y + 40 - sizeOfAimer);
                base.nowAimer4Position = new Point(base.nowPosition.X + 40 - sizeOfAimer, base.nowPosition.Y + 40 - sizeOfAimer);

                SetPlayerPosition(base.nowPosition);

                AddControl(Controls, base.playerObj);
                AddControl(Controls, base.playerAttackRange);
                AddControl(Controls, base.playerAimer1);
                AddControl(Controls, base.playerAimer2);
                AddControl(Controls, base.playerAimer3);
                AddControl(Controls, base.playerAimer4);


                SetStartLabelPosition(StartLabelPoint());
                AddControl(Controls, base.startCoolLabel);

                SetQudrant();
                AddControl(Controls, base.playerQudrant);

                base.startingTimer.Enabled = true;
                base.startingTimer.Interval = 1000;
                base.startingTimer.Tick += StartingTimer_Tick;


            }
        }
        #endregion


        #region 키보드 체크 ----------------------------------------
        public void KeyDownCheck(KeyEventArgs e, List<Player> players, List<Dummy> dummies)
        {
            if (e.KeyCode == this.playerKeys.Up)
            {
                base.isMovingU = true;
            }
            if (e.KeyCode == this.playerKeys.Down)
            {
                base.isMovingD = true;
            }
            if (e.KeyCode == this.playerKeys.Left)
            {
                base.isMovingL = true;
            }
            if (e.KeyCode == this.playerKeys.Right)
            {
                base.isMovingR = true;
            }
            if (e.KeyCode == this.playerKeys.Run)
            {
                base.isRunning = true;
            }
            if (e.KeyCode == this.playerKeys.Aim)
            {
                base.isAiming = true;
                base.playerAimer1.Visible = true;
                base.playerAimer2.Visible = true;
                base.playerAimer3.Visible = true;
                base.playerAimer4.Visible = true;
            }
            if (e.KeyCode == this.playerKeys.Attack)
            {
                base.Attack(base.Controls,players,dummies);
            }
        }
        public void KeyUpCheck(KeyEventArgs e)
        {
            if (e.KeyCode == this.playerKeys.Up)
            {
                base.isMovingU = false;
            }
            if (e.KeyCode == this.playerKeys.Down)
            {
                base.isMovingD = false;
            }
            if (e.KeyCode == this.playerKeys.Left)
            {
                base.isMovingL = false;
            }
            if (e.KeyCode == this.playerKeys.Right)
            {
                base.isMovingR = false;
            }
            if (e.KeyCode == this.playerKeys.Run)
            {
                base.isRunning = false;
            }
            if (e.KeyCode == this.playerKeys.Aim)
            {
                base.isAiming = false;
                base.playerAimer1.Visible = false;
                base.playerAimer2.Visible = false;
                base.playerAimer3.Visible = false;
                base.playerAimer4.Visible = false;
            }
        }
        #endregion
    }

    public class ControllerPlayer : Player
    {

    }

    public class Dummy
    {

        #region 필수 필드 -----------------------------------------------------
        private Control.ControlCollection Controls;
        private bool isAlive = true;

        public bool IsAlive { get { return isAlive; } } 

        #endregion

        #region 움직임 관련 필드-----------------------------------------------
        private int patternType = 0; // 0.always random, 1. random per 1sec, 2. random per 2sec, 3. only L/R/U/D per 2sec.

        private bool isMovingL = false;
        private bool isMovingR = false;
        private bool isMovingU = false;
        private bool isMovingD = false;

        private int direction = 0;

        /*
        4   3   2          
        5       1
        6   7   8
         */

        private Point nowPosition;

        private const int moveSpeed = 1;
        #endregion

        #region 생성자 --------------------------------------------------------

        public Dummy(Control.ControlCollection Controls, int patternType)
        {
            this.Controls = Controls;
            this.patternType = patternType;

            nowPosition = RandomPoint();

            SetPosition(dummyObj, nowPosition);
            AddControl(Controls, dummyObj);

            Random rand = new Random();

            this.direction = rand.Next(1,9);

        }

        #endregion

        #region 컨트롤 추가 및 제거 -------------------------------------------
        public void AddControl(Control.ControlCollection Controls, Control Obj)
        {
            Controls.Add(Obj);
        }

        public void RemoveControl(Control.ControlCollection Controls, Control Obj)
        {
            Obj.Dispose();
            Controls.Remove(Obj);
        }
        #endregion

        #region 타이머 및 컨트롤 ----------------------------------------------

        public PictureBox dummyObj = new PictureBox()
        {
            Size = new Size(20, 20),
            BackColor = Color.Orange,
            Visible = true,
        };

        #endregion

        #region 위치 설정 -----------------------------------------------------
        private void SetPosition(Control control, Point point)
        {
            control.Location = point;
        }

        private Point RandomPoint()
        {
            int x, y;
            Random rand = new Random();


            x = rand.Next(0, 1004 + 1);
            y = rand.Next(0, 580 + 1);


            return new Point(x, y);
        }

        #endregion

        #region 방향 조정(패턴에 따른)------------------------------------------
        private void directionToBool(int direction)
        {
            if(direction == 1)
            {
                this.isMovingL = false;
                this.isMovingR = true;
                this.isMovingU = false;
                this.isMovingD = false;
            }
            else if(direction == 2)
            {
                this.isMovingL = false;
                this.isMovingR = true;
                this.isMovingU = true;
                this.isMovingD = false;
            }
            else if (direction == 3)
            {
                this.isMovingL = false;
                this.isMovingR = false;
                this.isMovingU = true;
                this.isMovingD = false;
            }
            else if (direction == 4)
            {
                this.isMovingL = true;
                this.isMovingR = false;
                this.isMovingU = true;
                this.isMovingD = false;
            }
            else if (direction == 5)
            {
                this.isMovingL = true;
                this.isMovingR = false;
                this.isMovingU = false;
                this.isMovingD = false;
            }
            else if (direction == 6)
            {
                this.isMovingL = true;
                this.isMovingR = false;
                this.isMovingU = false;
                this.isMovingD = true;
            }
            else if (direction == 7)
            {
                this.isMovingL = false;
                this.isMovingR = false;
                this.isMovingU = false;
                this.isMovingD = true;
            }
            else
            {
                this.isMovingL = false;
                this.isMovingR = true;
                this.isMovingU = false;
                this.isMovingD = true;
            }
        }
        
        private void randomDirection(bool diagonal)
        {
            Random rand = new Random(); 
            


            if(diagonal == true)
            {
                int tmp = rand.Next(0, 3); //0 ; left, 1,save, 1.right

                if (tmp == 0)
                {
                    this.direction++;
                }
                else if(tmp == 2)
                {
                    this.direction--;
                }

                this.direction += 9;
                this.direction %= 9;
            }
            else
            {
                int tmp = rand.Next(0,4);

                this.direction = tmp * 2 + 1;
            }

        }
        
        private void chooseDirection(TimerTickRecoder maintick)
        {
            
            if(this.patternType == 0)
            {
                if(maintick.TotalTick %5 == 0)
                {
                    randomDirection(true);
                }
                
            }
            else if(this.patternType == 1)
            {
                if (maintick.TotalTick % 100 == 0)
                {
                    randomDirection(true);
                }
            }
            else if (this.patternType == 2)
            {
                if (maintick.TotalTick % 200 == 0)
                {
                    randomDirection(true);
                }
            }
            else
            {
                if (maintick.TotalTick % 200 == 0)
                {
                    randomDirection(false);
                }
            }
        }

        #endregion


        #region 이동 -----------------------------------------------------------

        public void Move(TimerTickRecoder maintick)
        {
            chooseDirection(maintick);

            directionToBool(this.direction);
            if (isMovingU == true)
            {
                this.nowPosition.Y -= moveSpeed;
            }
            if (isMovingD == true)
            {
                this.nowPosition.Y += moveSpeed;
            }
            if (isMovingL == true)
            {
                this.nowPosition.X -= moveSpeed;
            }
            if (isMovingR == true)
            {
                this.nowPosition.X += moveSpeed;
            }

            if (this.nowPosition.X < 0)
            {
                this.nowPosition.X = 0;
                this.direction = 1;
                directionToBool(this.direction);
            }
            else if (this.nowPosition.X > 1024 - 20)
            {
                this.nowPosition.X = 1024 - 20;
                this.direction = 5;
                directionToBool(this.direction);
            }

            if (this.nowPosition.Y < 0)
            {
                this.nowPosition.Y = 0;
                this.direction = 7;
                directionToBool(this.direction);
            }
            else if (this.nowPosition.Y > 600 - 20)
            {
                this.nowPosition.Y = 600 - 20;
                this.direction = 3;
                directionToBool(this.direction);
            }


            SetPosition(dummyObj, this.nowPosition);

            
        }

        #endregion

        #region 더미 죽음-------------------------------------------------------
        public void DummyDeath(Control.ControlCollection Controls)
        {
            this.isAlive = false;
            RemoveControl(Controls, this.dummyObj);
        }
        #endregion
    }
    public class GameManager
    {
        private PlayerInfo player1Info, player2Info, player3Info, player4Info;
        private string dummyPattern = "0"; // 0,1,2,3,Random
       
        private int dummyCount = 0;

        public PlayerInfo Player1Info { get { return player1Info; } }
        public PlayerInfo Player2Info { get { return player2Info; } }
        public PlayerInfo Player3Info { get { return player3Info; } }
        public PlayerInfo Player4Info { get { return player4Info; } }
        public string DummyPattern { get { return dummyPattern; } }
        public int DummyCount { get { return dummyCount; } }
        

        public GameManager()
        {
            player1Info = new PlayerInfo();
            player2Info = new PlayerInfo();
            player3Info = new PlayerInfo();
            player4Info = new PlayerInfo();
        }

        public void Player1Setting(PlayerInfo player1Setting)
        {
            player1Info = player1Setting;
        }

        public void Player2Setting(PlayerInfo player2Setting)
        {
            player2Info = player2Setting;
        }

        public void Player3Setting(PlayerInfo player3Setting)
        {
            player3Info = player3Setting;
        }

        public void Player4Setting(PlayerInfo player4Setting)
        {
            player4Info = player4Setting;
        }

        public void DummyPatternSetting(string dummyPattern)
        {
            this.dummyPattern = dummyPattern;
        }
        public void DummyCountSetting(int dummyCount)
        {
            this.dummyCount = dummyCount;
        }

    }
    public class ObjectManager
    {
        private TimerTickRecoder maintick;

        private List<Player> players = new List<Player>();

        private List<Dummy> dummies = new List<Dummy>();

        public ObjectManager(TimerTickRecoder maintick)
        {
            this.maintick = maintick;
        }

        public void AddPlayer(Control.ControlCollection Controls, PlayerInfo playerInfo)
        //플레이어 정보 >> 플레이어 인스턴스 >> 플레이어 리스트에 저장.
        {
            Player tmp;

            if (playerInfo.IsPlaying == true)
            {
                if (playerInfo.Keyboard == true)
                {
                    tmp = new KeyboardPlayer(Controls, true, playerInfo.Qudrant, playerInfo.Color, playerInfo.PlayerKeys);
                }
                else
                {
                    tmp = new ControllerPlayer();
                    //ADD ControllerPlayer
                }
                this.players.Add(tmp);
            }


        }
        public void AddDummy(Control.ControlCollection Controls,int patternType)//0~3
        {
            Dummy tmp = new Dummy(Controls, patternType);
            this.dummies.Add(tmp);
        }


        #region 플레이어 전체의 이벤트----------------------------------------------------
        public void PlayersMove()
        {
            foreach (Player player in this.players)
            {
                if (player.IsPlaying == true && player.IsAlive == true)
                {
                    player.Move();
                }
            }
        }

        public void PlayersKeyDownCheck(KeyEventArgs e) //only for keyboard player
        {
            foreach (Player player in this.players)
            {
                if (player.IsPlaying == true && player.IsAlive == true && player is KeyboardPlayer)
                {
                    ((KeyboardPlayer)player).KeyDownCheck(e,this.players,this.dummies);
                }
            }
        }

        public void PlayersKeyUpCheck(KeyEventArgs e) //only for keyboard player
        {
            foreach (Player player in this.players)
            {
                if (player.IsPlaying == true && player.IsAlive == true && player is KeyboardPlayer)
                {
                    ((KeyboardPlayer)player).KeyUpCheck(e);
                }
            }
        }
        #endregion

        #region 더미 전체의 이벤트----------------------------------------------------

        public void DummiesMove()
        {
            foreach(Dummy dummy in this.dummies)
            {
                if (dummy.IsAlive == true)
                    dummy.Move(this.maintick);
            }
        }

        #endregion

    }

    public class TimerTickRecoder
    {
        private int totalTick = 0;
        public int TotalTick { get { return totalTick; } }
        public void IncreaseTotalTick()
        {
            totalTick++;
        }

    }

    public class RandomChoose
    {
        private Random rand = new Random();

        private int N;

        private List<int> list = new List<int>();

        public RandomChoose(int N)
        {
            this.N = N;
            for (int i = 0; i < this.N; ++i)
            {
                list.Add(i);
            }
        }

        public int Choose()
        {
            int rst;
            int a = rand.Next(0, N);
            rst = list[a];

            list.RemoveAt(a);
            this.N--;
            return rst;
        }
    }
}