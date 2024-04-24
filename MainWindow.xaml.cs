using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;



namespace WpfAppRocket
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		// Анімаційний об'єкт,
		// глобальний для кервуання його обертів
		// для розташування вздовж осі руху.
		private readonly AnimRocket animObject;
		// Тривалість анімації об'єкту
		private double _duration = 2;
		private readonly Random random = new();


		public MainWindow()
		{
			InitializeComponent();

			//AllowsTransparency = true;
			//WindowStyle = WindowStyle.None;
			//Background = Brushes.Transparent;

			animObject = new AnimRocket(mainGrid);

			// Визначення максимальних розмірів анімаційного об'єкту.
			Size _maxImgSize = animObject.maxImgSize;

			// Головне вікно буде квадратним,
			// оскільки об'єкт буде обертатися.
			Width = Height = Math.Max(_maxImgSize.Width, _maxImgSize.Height);

			// Запуск процесу анімації
			AnimationWindow();
		}


		/// <summary>
		/// Анімація пересування вікна
		/// </summary>
		private void AnimationWindow()
		{
			// Габарити пересування основного вікна
			double minX = 0;
			double maxX = SystemParameters.PrimaryScreenWidth;
			double minY = 0;
			double maxY = SystemParameters.PrimaryScreenHeight;

			// Отримання координат переміщення об'єкту 
			(double fromX, double toX, double fromY, double toY) = ComputeCoordinates(minX, maxX, minY, maxY);

			// Вираховуємо кут повороту об'єкту відносно горизонталі.
			Vector vectorBase = new(30, 0);
			Vector vectorDirection = new(toX - fromX, toY - fromY);
			double angle = Vector.AngleBetween(vectorBase, vectorDirection);
			animObject.Rotate(angle);


			// Координата Х розташування вікна на екрані.
			DoubleAnimation posLeft = new();
            // Координата Y розташування вікна на екрані.
            DoubleAnimation posTop = new();

			// Характеристики поведінки координат ідентичні.
			posLeft.Duration = posTop.Duration = new Duration(TimeSpan.FromSeconds(_duration));
			posLeft.FillBehavior = posTop.FillBehavior = FillBehavior.Stop;

			// Позиція вікна вираховується відносно його центру.
			posLeft.From = fromX - Width / 2;
			posLeft.To = toX - Width / 2;
			posTop.From = fromY - Height / 2;
			posTop.To = toY - Height / 2;


			// Анімація з'явлення вікна
			double opacityShow = 0.2;
			DoubleAnimation opacityAnimationShow = new()
			{
				From = 0.0,
				To = 1.0,
				Duration = new Duration(TimeSpan.FromSeconds(opacityShow)),
				FillBehavior = FillBehavior.Stop
			};


			// Анімація зникнення вікна.
			double opacityHide = 0.2;
			// Захист віт від'ємного часового проміжку.
			double beginTimeHide = _duration >= (opacityShow + opacityHide) ? (_duration - (opacityShow + opacityHide)) : 0.0;
			DoubleAnimation opacityAnimationHide = new()
			{
				From = 1.0,
				To = 0.0,
				Duration = new Duration(TimeSpan.FromSeconds(opacityHide)),
				BeginTime = TimeSpan.FromSeconds(beginTimeHide),
				FillBehavior = FillBehavior.Stop
			};

			// Подія закінчення переміщення
			posLeft.Completed += (sender, eArgs) =>
			{
				// Запуск нової анімациії
				DispatcherTimer timer = new() { Interval = TimeSpan.FromSeconds(2) };
				timer.Tick += (sender, args) =>
				{
					timer.Stop();

					AnimationWindow();
				};

				timer.Start();
			};

			// Подія з'явлення вікна
			opacityAnimationShow.Completed += (sender, eArgs) =>
			{
				this.Opacity = 1;

				// Повторна анімація прозорості запускається точно після закінчення першої,
				// інакше керування однією властивістю двома анімаціями створює артефакти.
				BeginAnimation(OpacityProperty, opacityAnimationHide);
			};

			// Закінчення зникнення.
			opacityAnimationHide.Completed += (sender, eArgs) =>
			{
				Opacity = 0;
			};



			// Запуск анимацій
			BeginAnimation(OpacityProperty, opacityAnimationShow);
			BeginAnimation(LeftProperty, posLeft);
			BeginAnimation(TopProperty, posTop);

		}


		/// <summary>
		/// Підготовка координат для переміщення анімації об'єкту
		/// </summary>
		/// <param name="minX"></param>
		/// <param name="maxX"></param>
		/// <param name="minY"></param>
		/// <param name="maxY"></param>
		/// <returns></returns>
		private (double fromX, double toX, double fromY, double toY) ComputeCoordinates(double minX, double maxX, double minY, double maxY)
		{
			Console.Beep(2000, 100);

			// Підготовка для вибору випадкових координат початку і кінця зміщення.
			List<int> randPos = new() { 0, 1, 2, 3 };
			randPos = randPos.OrderBy(a => random.Next()).ToList();

			// Отримання випадкової координати старту.
			Point start = GetPointMove(minX, maxX, minY, maxY, randPos[0]);
			double fromX = start.X;
			double fromY = start.Y;

			// Отримання координати фінішу.
			Point end = GetPointMove(minX, maxX, minY, maxY, randPos[1]);
			double toX = end.X;
			double toY = end.Y;

			// Получение довжини вектору зміщення
			Vector vecDirection = new()
			{
				X = toX - fromX,
				Y = toY - fromY,
			};

			// швидкість зміщення буде завжди однакова,
			// незалежно від відстані зміщення.
			_duration = vecDirection.Length / 1100;


			return (fromX, toX, fromY, toY);
		}


		/// <summary>
		/// Отримання одної координати зміщення 
		/// </summary>
		/// <param name="minX"></param>
		/// <param name="maxX"></param>
		/// <param name="minY"></param>
		/// <param name="maxY"></param>
		/// <param name="variant">вибір варіанту зміщення</param>
		/// <returns></returns>
		private Point GetPointMove(double minX, double maxX, double minY, double maxY, int variant)
		{
			// Кількість координатних точок на вибрану сторону екрану.
			int num = 10;

			// Список зберігання точок
			List<Point> points = new();


			// Варіанти отримання координатних точок.
			switch (variant)
			{
				case 0:
					// Верхня сторона екрану.
					for (int i = 0; i <= num; i++)
					{
						Point p = new() { X = maxX / num * i, Y = minY };
						points.Add(p);
					}
					break;

				case 1:
					// Нижня сторона екрану
					for (int i = 0; i <= num; i++)
					{
						Point p = new() { X = maxX / num * i, Y = maxY };
						points.Add(p);
					}
					break;

				case 2:
					// Ліва сторона екрану
					for (int i = 0; i <= num; i++)
					{
						Point p = new() { X = minX, Y = maxY / num * i };
						points.Add(p);
					}
					break;

				case 3:
					// Права сторона екрану
					for (int i = 0; i <= num; i++)
					{
						Point p = new() { X = maxX, Y = maxY / num * i };
						points.Add(p);
					}
					break;
			}

			// Випадковий вибір однієї точки для повернення.
			return points[random.Next(0, points.Count)];
		}


	}
}