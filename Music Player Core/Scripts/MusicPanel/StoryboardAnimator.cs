using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;


namespace Music_Player.MusicPanel
{
    // Pseudo-static
    public class StoryboardAnimator
    {
        // Constants
        private const int BEGIN_START_ANIMATION_TIME = 3;
        private const int BEGIN_BACK_ANIMATION_TIME = 3;
        private const string THICKNESS_ANIMATION_NOT_SET_ERROR = "{0} kontrolka dziedzicząca od storyboard musi być typu ThicknessAnimation\n" +
            "object type {1}\n" +
            "object name {2}";

        // References
        public static StoryboardAnimator Instance { get; set; }
        private Storyboard Storyboard { get; set; }
        private TextBlock MusicTextBlock { get; }
        private ColumnDefinition GridMusicPlayerColumnWidth { get; }

        // Fields
        private readonly ThicknessAnimation startAnimation;
        private readonly ThicknessAnimation backAnimation;

        // Properties
        private double pixelsPerSecond = 40d;
        /// <summary>
        /// Indicates the speed of animation
        /// </summary>
        public double PixelsPerSecond
        {
            get => pixelsPerSecond;
            set
            {
                pixelsPerSecond = value;
                RestartAnimation();
            }
        }

        private int beginStartAnimationTime;
        /// <summary>
        /// Begin Time in seconds for Starting Animation (Text expanding to right)
        /// </summary>
        public int BeginStartAnimationTime
        {
            get => beginStartAnimationTime;
            set
            {
                beginStartAnimationTime = value;
                startAnimation.BeginTime = new TimeSpan(beginStartAnimationTime * TimeSpan.TicksPerSecond);
                RestartAnimation();
            }
        }

        private int beginBackAnimationTime;
        /// <summary>
        /// Begin Time in seconds for Backing Animation (Text withdrawing)
        /// </summary>
        public int BeginBackAnimationTime
        {
            get => beginBackAnimationTime;
            set
            {
                beginBackAnimationTime = value;
                RestartAnimation();
            }
        }

        /// <summary>
        /// Text of TextBlock Music Name
        /// </summary>
        public string MusicText
        {
            get => MusicTextBlock.Text;
            set
            {
                MainWindow.Instance.PreviewMusicTimer_Slider.ToolTip = MusicTextBlock.Text = value;
                RestartAnimation();
            }
        }


        // Constructor
        public StoryboardAnimator(MainWindow _mainWindow, TextBlock _musicTextBlock,
            ColumnDefinition _gridMusicPlayerColumnWidth)
        {
            if (Instance != null)
                throw new Exception(message: "StoryboardAnimator as pseudo-static class has been duplicated");

            Instance = this;
            MusicTextBlock = _musicTextBlock;
            MainWindow.Instance.MusicTimer_Slider.ToolTip = _musicTextBlock.Text;
            GridMusicPlayerColumnWidth = _gridMusicPlayerColumnWidth;
            Storyboard = MusicTextBlock.Resources["Storyboard"] as Storyboard;

            startAnimation = Storyboard.Children[0] is ThicknessAnimation ? Storyboard.Children[0] as ThicknessAnimation
                : throw new Exception(string.Format(THICKNESS_ANIMATION_NOT_SET_ERROR, 1, Storyboard.Children[0].GetType(), Storyboard.Children[0].Name));

            backAnimation = Storyboard.Children[1] is ThicknessAnimation ? Storyboard.Children[1] as ThicknessAnimation
                : throw new Exception(string.Format(THICKNESS_ANIMATION_NOT_SET_ERROR, 2, Storyboard.Children[1].GetType(), Storyboard.Children[1].Name));

            BeginStartAnimationTime = BEGIN_START_ANIMATION_TIME;
            BeginBackAnimationTime = BEGIN_BACK_ANIMATION_TIME;

            StartAnimation();
        }

        // Methods
        /// <summary>
        /// Startuje animację - dla całkowitego restartu użyj RestartAnimation
        /// </summary>
        public void StartAnimation()
        {
            if (SetAnimationProperties())
                Storyboard.Begin(MainWindow.Instance, true);
            else
                StopAnimation();
        }

        public void StopAnimation() => Storyboard.Stop(MainWindow.Instance);

        /// <summary>
        /// Restartuje animację, resetując przesunięcie TextBlock na początek
        /// </summary>
        public async void RestartAnimation()
        {
            StopAnimation();

            await Task.Delay(300);

            StartAnimation();
        }

        /// <summary>
        /// Checks if animation is needed for longer music names, if not then animation won't start <br/>
        /// Sets properties for animation to have specific animation time based on a length of a music name
        /// </summary>
        private bool SetAnimationProperties()
        {
            const double WHITE_SPACE_WIDTH = 20d;

            double animationRange = -(MusicTextBlock.ActualWidth
                - GridMusicPlayerColumnWidth.Width.Value + WHITE_SPACE_WIDTH);

            if (-animationRange < 20)
                return false;
            else
            {
                startAnimation.To = backAnimation.From = new Thickness(animationRange, 0, 0, 0);

                startAnimation.Duration = backAnimation.Duration =
                    new TimeSpan(ticks: Convert.ToInt64((-animationRange) / PixelsPerSecond * TimeSpan.TicksPerSecond));

                backAnimation.BeginTime = new TimeSpan(backAnimation.Duration.TimeSpan.Ticks +
                    startAnimation.BeginTime.Value.Ticks + BeginBackAnimationTime * TimeSpan.TicksPerSecond);

                return true;
            }
        }
    }
}