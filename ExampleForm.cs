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
	
	public Sprite(float x_pos, float y_pos, float x_size, float y_size, Color color = Black, float pen_width = 1f)
	{
		_x_pos = x_pos;
		_y_pos = y_pos;
		_x_size = x_size;
		_y_size = y_size;
		_pen = new Pen(color, pen_width)		
	}
	
	public virtual void Draw(object sender, PaintEventArgs e)
	{
		var g = e.Graphics;
		g.DrawEllipse(_pen, _x_pos/100f * g.DpiX, _y_pos/100f * g.DpiY, _x_size/100f * g.DpiX, _y_size/100 * g.DpiY);
	}	
}

public class Ball : Sprite
{
	private float _x_speed;
	private float _y_speed;
	private float _y_target;
	
	private bool IsBasketReached = false;
	
	private const float _d_x = 0;
	private const float _d_y = 0.02f;
	private const float _d_air = 0.98f;
	
	public delegate void Delegate;
	public event Delegate BasketReached;
	public event Delegate EdgeReached;
	
	public Ball(float x_pos, float y_pos, float x_size, float y_size, float x_speed, float y_speed, float y_target, Color color = Black, float pen_width = 10f)
	 : base(x_pos, y_pos, x_size, y_size, color, pen_width)
	{
		_x_speed = x_speed;
		_y_speed = y_speed;
		_y_target = y_target;	
	}
	
	private void Change()
	{
		_x_pos += _x_speed;
		_y_pos += _y_speed;
		
		_x_speed += _d_x;
		_y_speed += _d_y;
		
		_x_speed *= _d_air;
		_y_speed *= _d_air;
	}
	
	public override void Draw(object sender, PaintEventArgs e)
	{
		var g = e.Graphics;
		if(!IsBasketReached && _y_pos >= _y_target)
		{
			this.BasketReached(); 
		}
		if(_y_pos > 105f)
		{
			this.EdgeReached();
		}
		g.FillEllipse(_pen, _x_pos/100f * g.DpiX, _y_pos/100f * g.DpiY, 10, 10);
		Change();
	}
}

public class Basket : Sprite
{
	public Basket(float x_pos, float y_pos, float x_size = 10f, float y_size = 5f, Color color = Black, float pen_width = 1f)
	 : base(x_pos, y_pos, x_size, y_size, color, pen_width){}
}

public class MyForm : Form
{
	protected Button _leftScore_btn;// свойства
	protected Button _rightScore_btn;
	protected Graphics _main_graphics;
	protected PictureBox _main_pb;

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
		
		this.Controls.Add(_leftScore_btn); // добавление компонентов
		this.Controls.Add(_main_pb);
		this.Controls.Add(_rightScore_btn);
		
		Basket left
		
	}

	private void ResizePictureBox(object sender, EventArgs e)
	{
		_main_pb.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 25); //изменение размера места рисования 
	}
	
	private void Tick()
	{
		while(true)
		{
			_main_pb.Invalidate();
			Thread.Sleep(50);
		}	
	}
	private async void MakeToss(object sender, EventArgs e)
	{
		if(!_ball_is_tossed) // кидается ли мяч?
		{
			_ball_is_tossed = true; // если нет, то кинуть
			await Task.Run(() => // ожидание, пока произойдёт бросок
			{
				float x_distance = 0.0f;
				float y_distance = 38.0f; // позиция мяча в пространстве в процентах
				
				float x_speed = 0.8f;
				float y_speed = -1.2f; // скорость мяча в процентах в отрисовку

				bool IsIncremented = false; // был ли увеличен счёт?

				while(y_distance < 100f && x_distance < 100f) //отрисовка пока мяч не вышел за пределы экрана
				{
					_main_pb.Invalidate();//_main_graphics.Clear(this.BackColor); // отчистка фона
					_main_graphics.DrawEllipse(new Pen(Color.Black, 1f), 0.9f * _main_pb.Size.Width, 0.6f * _main_pb.Size.Height, 0.1f * _main_pb.Size.Width, 0.05f * _main_pb.Size.Height);// отрисовка кольца	
					_main_graphics.DrawEllipse(new Pen(Color.Black, 10f), x_distance/100f * _main_pb.Size.Width, y_distance/100f * _main_pb.Size.Height, 10, 10);// отрисовка мяча

					if(x_distance >= 95f && !IsIncremented)// если расстояние до корзины пройдено и если счёт не увеличивался
					{
						((Button)sender).Text = (Int32.Parse(((Button)sender).Text) + 1).ToString(); // увеличение счёта
						IsIncremented = true; // счёт был увеличен
					}

					x_distance += x_speed; // мяч передвинут
					y_distance += y_speed;
					Thread.Sleep(5);// поток спит, межну отрисовками
					y_speed += 0.02f;// сила притяжения

					y_speed *= 0.998f;// сопротивление воздуха
					x_speed *= 0.998f;
				}
				_main_pb.Invalidate();
				_main_graphics.DrawEllipse(new Pen(Color.Black, 1f), 0.9f * _main_pb.Size.Width, 0.6f * _main_pb.Size.Height, 0.1f * _main_pb.Size.Width, 0.05f * _main_pb.Size.Height);//финальная отрисовка кольца
			});
			_ball_is_tossed = false;// закончили кидать
		}
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
