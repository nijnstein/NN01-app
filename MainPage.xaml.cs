
namespace NN01_app;

public partial class MainPage : ContentPage
{
    public const double PhysicsFPS = 60 * 5; 
    public const double TargetFPS = 60;
    private DateTime lastTick = DateTime.MinValue;
    private DateTime lastPhysicsTick = DateTime.MinValue;

    public Pong Game = new Pong();

	public MainPage()
	{
		InitializeComponent();
        Initialize(); 
	}

	private void OnResetClicked(object sender, EventArgs e)
	{
        Game.Reset(); 
	}

    public void Initialize()
    {
        GameView.Drawable = (IDrawable)Game;

        var ms = 1000.0 / PhysicsFPS;
        var ts = TimeSpan.FromMilliseconds(ms);

        Dispatcher.StartTimer(ts, TimerLoop);
        lastTick = DateTime.Now;

        Game.Reset();
    }

    private bool TimerLoop()
    {   
        // skip the first tick 
        if(lastPhysicsTick == DateTime.MinValue)
        {
            lastPhysicsTick = DateTime.Now;
            return true; 
        }

        // get deltaTime 
        DateTime now = DateTime.Now; 
        double dt = (DateTime.Now - lastPhysicsTick).TotalSeconds;

        // update physics 
        lastPhysicsTick = now;
        
        Game.DoUpdate(MathF.Max((float)dt, 1f / 60f));
        GenerationLabel.Text = $"Generation: {Game.Generation}, Step: {Game.Step.ToString().PadLeft(5, '0')}, Cost: {Game.Cost.ToString("0.000000")}";

        // update renderer as needed to reach 60fps
        if ((now - lastTick).TotalSeconds >= 1f / 60f)
        {
            lastTick = now;
            GameView.Invalidate();
        }
        return true;
    }



}

