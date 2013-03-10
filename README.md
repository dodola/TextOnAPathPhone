TextOnAPathPhone
================

Text on A Path for Windows Phone

The code come from a CodeProject article, only silverlight and WPF version ,I fix some bug and run on Windows Phone 

Article: [Text on A Path for Silverlight](http://www.codeproject.com/Articles/30478/Text-on-A-Path-for-Silverlight?msg=3649940)

###Document
####The Control Property

1.Text (string): the string to be displayed. If the Text string is longer than the geometry path, then text will be truncated.

2.TextPath (Geometry): the geometry for the text to follow.

3.DrawPath (Boolean) (only in WPF version): if true, draws the geometry below the text string.

4.DrawLinePath (Boolean): if true, draws the flattened path geometry below the text string.

5.ScaleTextPath (Boolean): if true, the geometry will be scaled to fit the size of the control.

6.GeoPathColor (Brush): the geometry color


####Example
``` xml
 <Grid.Resources>
                <core:String x:Key="MyPath">M0,0 C120,361 230.5,276.5 230.5,276.5 L308.5,237.50001 C308.5,237.50001 419.5,179.5002 367.5,265.49993 315.5,351.49966 238.50028,399.49924 238.50028,399.49924 L61.500017,420.49911</core:String>
                <PathConverter:StringToPathGeometryConverter x:Key="MyPathConverter" />
            </Grid.Resources>

            <TextOnAPath:TextOnAPath TextPath="{Binding Source={StaticResource MyPath}, Converter={StaticResource MyPathConverter}}"
                                     FontSize="25"
                                     DrawLinePath="True"
                                     Text="The quick brown fox jumped over the lazy dog." />
```
####Know Issue

before using you need add TextOnAPath style on app.xaml

``` xml
 <Style TargetType="local:TextOnAPath">
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="local:TextOnAPath">
                        <Border Background="{StaticResource TransparentBrush}"
                                BorderBrush="{StaticResource PhoneAccentBrush}"
                                BorderThickness="{TemplateBinding BorderThickness}">
                            <Canvas x:Name="LayoutPanel">
                                <!-- all items are added to this panel -->
                            </Canvas>
                        </Border>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
```


####截图:
![1](https://github.com/dodola/TextOnAPathPhone/blob/master/1.png?raw=true)
![2](https://github.com/dodola/TextOnAPathPhone/blob/master/2.png?raw=true)
![3](https://github.com/dodola/TextOnAPathPhone/blob/master/3.png?raw=true)
![4](https://github.com/dodola/TextOnAPathPhone/blob/master/4.png?raw=true)



