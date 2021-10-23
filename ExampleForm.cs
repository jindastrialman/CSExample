using System;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

public abstract class Sprite
{
	protected float _x_pos;
	protected float _y_pos;
	
	private float _x_size;
	private float _y_size;

	protected Pen _pen;
	
	public Sprite(float x_pos, float y_pos, float x_size, float y_size, float pen_width = 1f)
	{
		_x_pos = x_pos;
		_y_pos = y_pos;
		_x_size = x_size;
		_y_size = y_size;
		_pen = new Pen(Color.Black, pen_width);		
	}
	
	public virtual void Draw(object sender, PaintEventArgs e)
	{
		e.Graphics.DrawEllipse(_pen, _x_pos/100f * ((PictureBox)sender).Size.Width, _y_pos/100f * ((PictureBox)sender).Size.Height, _x_size/100f * ((PictureBox)sender).Size.Width, _y_size/100 * ((PictureBox)sender).Size.Height);
	}	
}

public class Plane : Sprite
{
	private float _x_speed;
	private float _x_target;

	private int _turbulence;
	
	private bool _isTargetReached = false;
	
	public delegate void Delegate();
	public event Delegate TargetReached;
	public event Delegate EdgeReached;
	
	public Plane(float x_pos, float y_pos, float x_size, float y_size, float x_speed, float x_target, float pen_width = 10f)
	 : base(x_pos, y_pos, x_size, y_size, pen_width)
	{
		_x_speed = x_speed;
		_x_target = x_target;
		_turbulence = -1;	
	}
	
	private void Change()
	{
		_x_pos += _x_speed;
		_y_pos += _turbulence;
		_turbulence *= -1
	}
	
	public override void Draw(object sender, PaintEventArgs e)
	{
		if(!_isTargetReached && _x_pos >= _x_target)
		{
			this.TargetReached();
			_isTargetReached = true; 
		}
		if(_y_pos > 105f)
		{
			this.EdgeReached();
		}
		e.Graphics.DrawEllipse(_pen, _x_pos/100f * ((PictureBox)sender).Size.Width, _y_pos/100f * ((PictureBox)sender).Size.Height, 10, 10);
		Change();
	}
}

public class Bomb : Sprite 
{
	
}

public class Target : Sprite
{
	public void Hit()
	(
		_pen = Color.Red;
	)
	public override void Draw(object sender, PaintEventArgs e)
	{
		e.Graphics.DrawRectangle(_pen, _x_pos/100f * ((PictureBox)sender).Size.Width, _y_pos/100f * ((PictureBox)sender).Size.Height, _x_size/100f * ((PictureBox)sender).Size.Width, _y_size/100 * ((PictureBox)sender).Size.Height);
	}
}

public class MyForm : Form
{
	protected Button _leftScore_btn;// свойства
	protected Button _rightScore_btn;
	protected Graphics _main_graphics;
	protected PictureBox _main_pb;
	
	private Ball _ball;
	private Basket left_basket;
	private Basket right_basket;
	private Thread _screen_update_thread;
	private bool _ball_is_tossed;

	public MyForm()
	{
		_leftScore_btn = new Button(); // инициализация
		_rightScore_btn = new Button();
		_main_pb = new PictureBox();
		
		_leftScore_btn.Location = new Point((this.ClientSize.Width)/2 - _leftScore_btn.Width, 0);// распределение по окну
		_rightScore_btn.Location = new Point((this.ClientSize.Width)/2, 0);
		_main_pb.Location = new Point(0, 25);
		_main_pb.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 25);

		_leftScore_btn.Anchor = AnchorStyles.Top;// привязка к отдельным сторонам окна
		_rightScore_btn.Anchor = AnchorStyles.Top;
		_main_pb.Anchor = (AnchorStyles.Top | AnchorStyles.Left);

		_leftScore_btn.Text = "0";// задача начальных значений
		_rightScore_btn.Text = "0";
		_main_graphics = _main_pb.CreateGraphics();
		_ball_is_tossed = false;

		_leftScore_btn.Click += MakeToss; // подписка на события
		_rightScore_btn.Click += MakeToss;
		this.Resize += ResizePictureBox;
		this.Closing += Finisher;
		
		this.Controls.Add(_leftScore_btn); // добавление компонентов
		this.Controls.Add(_main_pb);
		this.Controls.Add(_rightScore_btn);
		
		_screen_update_thread = new Thread(new ThreadStart(Tick));
		
		_screen_update_thread.Start();
		
		Basket left_basket = new Basket(90f, 60f);
		Basket right_basket = new Basket(0f, 60f);
		
		
		_main_pb.Paint += left_basket.Draw;
		_main_pb.Paint += right_basket.Draw;
	}

	private void ResizePictureBox(object sender, EventArgs e)
	{
		_main_pb.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 25); //изменение размера места рисования 
	}
	
	private void Tick()
	{
		for(;;)
		{
			_main_pb.Invalidate();
			Thread.Sleep(5);
		}	
	}
	private void Finisher(object sender, EventArgs e)
	{
		_screen_update_thread.Abort();
	}
	private void MakeToss(object sender, EventArgs e)
	{
		if(!_ball_is_tossed) // кидается ли мяч?
		{
			_ball_is_tossed = true; // если нет, то кинуть
			if((Button)sender == _rightScore_btn)
				_ball = new Ball(0f, 38f, 0, 0, 0.8f, -1.2f, 60f);
			else
				_ball = new Ball(100f, 38f, 0, 0, -0.8f, -1.2f, 60f);
				
			_ball.IncBtn = (Button)sender;
			_main_pb.Paint += _ball.Draw;
			_ball.BasketReached += BasketReached;
			_ball.EdgeReached += EndOfScreenReached;
		}
	}
	private void BasketReached()
	{
		_ball.IncBtn.Text = (Int32.Parse(_ball.IncBtn.Text) + 1).ToString();
	}
	private void EndOfScreenReached()
	{
		_main_pb.Paint -= _ball.Draw;
		_ball_is_tossed = false;// закончили кидать	
	}
}

public class Program
{
    public static void Main()
    {
        var f = new MyForm();
        f.Text = "Hello World";
        Application.Run(f);
    }

}
