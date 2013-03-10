using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using PathConverter;

namespace TextOnAPathTest
{
    public partial class MainPage : PhoneApplicationPage
    {
        // 构造函数
        public MainPage()
        {
            InitializeComponent();
            drawGeometry.IsChecked = false;
            scaleGeometry.IsChecked = false;

            TextOnAPath.ScaleTextPath = false;

            TextOnAPath.Text = textBox.Text;

            var items = new List<string> { "Path Geometry", "Ellipse", "Rectangle", "Line" };

            ListPickerSgo.SelectionChanged += OnSelectionChanged;
            ListPickerSgo.ItemsSource = items;

            ListPickerSgo.SelectedIndex = 0;

            // 用于本地化 ApplicationBar 的示例代码
            //BuildLocalizedApplicationBar();
        }


        // 用于生成本地化 ApplicationBar 的示例代码
        //private void BuildLocalizedApplicationBar()
        //{
        //    // 将页面的 ApplicationBar 设置为 ApplicationBar 的新实例。
        //    ApplicationBar = new ApplicationBar();

        //    // 创建新按钮并将文本值设置为 AppResources 中的本地化字符串。
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // 使用 AppResources 中的本地化字符串创建新菜单项。
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Geometry geo = null;
            switch (ListPickerSgo.SelectedIndex)
            {
                default:
                case 0:
                    var pathGeo = Resources["PathGeometry"] as String;
                    var converter = new StringToPathGeometryConverter();
                    geo = converter.Convert(pathGeo);
                    break;
                case 1:
                    geo = Resources["EllipseGeometry"] as EllipseGeometry;
                    break;

                case 2:
                    geo = Resources["RectangleGeometry"] as RectangleGeometry;
                    break;

                case 3:
                    geo = Resources["LineGeometry"] as LineGeometry;
                    break;
            }

            if (geo == null)
                return;

            TextOnAPath.TextPath = geo;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextOnAPath.Text = textBox.Text;
        }

        private void drawGeometry_Checked(object sender, RoutedEventArgs e)
        {
            TextOnAPath.DrawLinePath = true;
        }

        private void drawGeometry_Unchecked(object sender, RoutedEventArgs e)
        {
            TextOnAPath.DrawLinePath = false;
        }

        private void scaleGeometry_Checked(object sender, RoutedEventArgs e)
        {
            TextOnAPath.ScaleTextPath = true;
        }

        private void scaleGeometry_Unchecked(object sender, RoutedEventArgs e)
        {
            TextOnAPath.ScaleTextPath = false;
        }
    }
}