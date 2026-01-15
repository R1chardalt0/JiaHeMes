// LightSignal.xaml.cs
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ChargePadLine.Client.CustomerControl
{
    /// <summary>
    /// LightSignal.xaml 的交互逻辑
    /// </summary>
    public partial class LightSignal : UserControl
    {
        private DropShadowEffect _shadowEffect;

        public LightSignal()
        {
            InitializeComponent();

            // 监听属性变化
            var activeColorDescriptor = TypeDescriptor.GetProperties(this)[nameof(ActiveColor)];
            var isActiveDescriptor = TypeDescriptor.GetProperties(this)[nameof(IsActive)];

            activeColorDescriptor?.AddValueChanged(this, OnActiveColorPropertyChanged);
            isActiveDescriptor?.AddValueChanged(this, OnIsActivePropertyChanged);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeLightEffect();
            UpdateLightState();
        }

        private void InitializeLightEffect()
        {
            if (Light.Effect is DropShadowEffect existing)
            {
                _shadowEffect = existing;
            }
            else
            {
                _shadowEffect = new DropShadowEffect
                {
                    BlurRadius = 15,
                    ShadowDepth = 0,
                    Color = GetColorFromBrush(ActiveColor)
                };
                Light.Effect = _shadowEffect;
            }
        }

        private void UpdateLightState()
        {
            // 防止 Light 或 _shadowEffect 为 null
            if (Light == null || _shadowEffect == null)
                return;

            if (IsActive)
            {
                Light.Fill = ActiveColor;
                _shadowEffect.Color = GetColorFromBrush(ActiveColor);
            }
            else
            {
                Light.Fill = Brushes.Gray;
                _shadowEffect.Color = Colors.Transparent;
            }
        }

        private void OnIsActivePropertyChanged(object sender, EventArgs e)
        {
            UpdateLightState();
        }

        private void OnActiveColorPropertyChanged(object sender, EventArgs e)
        {
            if (IsActive && _shadowEffect != null)
            {
                _shadowEffect.Color = GetColorFromBrush(ActiveColor);
            }
        }

        private Color GetColorFromBrush(Brush brush)
        {
            return brush switch
            {
                SolidColorBrush solid => solid.Color,
                GradientBrush gradient when gradient.GradientStops.Count > 0 => gradient.GradientStops[0].Color,
                _ => Colors.Transparent
            };
        }

        #region Dependency Properties

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
                nameof(IsActive),
                typeof(bool),
                typeof(LightSignal),
                new PropertyMetadata(false));

        public Brush ActiveColor
        {
            get => (Brush)GetValue(ActiveColorProperty);
            set => SetValue(ActiveColorProperty, value);
        }

        public static readonly DependencyProperty ActiveColorProperty =
            DependencyProperty.Register(
                nameof(ActiveColor),
                typeof(Brush),
                typeof(LightSignal),
                new PropertyMetadata(Brushes.Red));

        #endregion
    }
}