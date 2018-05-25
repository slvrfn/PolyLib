

# PolyLib

Sweep | Touch | Grow
--- | --- | ---
<img src="https://github.com/cameronwhite08/PolyLib/blob/master/gifs/sweep.gif?raw=true" alt="Demo of the Sweep Animation" title="Demo of the Sweep Animation"> | <img src="https://github.com/cameronwhite08/PolyLib/blob/master/gifs/touch.gif?raw=true" alt="Demo of the random Touch animation" title="Demo of the random Touch animation"> | <img src="https://github.com/cameronwhite08/PolyLib/blob/master/gifs/grow.gif?raw=true" alt="Demo of the Grow Animation" title="Demo of the Grow Animation">

## Usage
#### Create the view
>Android
```xml
<LowPolyLibrary.Views.Android.LowPolyView
	android:layout_width="match_parent"		<!-- Any size is possible -->
	android:layout_height="match_parent"	<!-- Same here -->
	android:id="@+id/triangulationView" />
```
>iOS

Add a `View` instance in your .storyboard or .xib (named polyView here), set `LowPolyView` class

#### Get reference to the view

>Android
```c#
LowPolyView polyView = FindViewById<LowPolyView> (Resource.Id.triangulationView);
```

>iOS
>
Connect `Outlet` in *ViewController.designer.cs*
```c#
partial class ViewController{
    [Outlet]
    LowPolyLibrary.Views.iOS.LowPolyView polyView { get; set; }
}
```
#### Get Reference to a Triangulation
Create standard `Triangulation`
```
var tri = new Triangulation(
	boundsWidth:1080,
	boundsHeight:720
	);
```
Create `Triangulation` with custom colors, provided to a random `SKShader`
```
var tri = new Triangulation(
	boundsWidth: 100,
	boundsHeight: 100,
	gradientColors: new SKColor[]
	);
```
Create `Triangulation` with a provided [`SKShader`](https://developer.xamarin.com/api/type/SkiaSharp.SKShader/)
```
var tri = new Triangulation(
	boundsWidth: 100,
	boundsHeight: 100,
	gradientShader: SKShader.Create...(...)
	);
```
__OR__

Get the `Triangulation` hosted by the current `LowPolyView`
```
var tri = polyView.CurrentTriangulation;
```


#### Create animations
>Grow Animation
```c#
var growAnim = new Grow(
	triangulation: _polyView.CurrentTriangulation,
	numFrames: _numAnimFrames
	);
polyView.AddAnimation(growAnim);
```
> Sweep Animation
```C#
var sweepAnim = new Sweep(
	triangulation: _polyView.CurrentTriangulation,
	numFrames: _numAnimFrames
	);
polyView.AddAnimation(sweepAnim);
```
> Touch Animation
```c#
// new Touch animation at touch location, radius=250, 6 frames in the animation
var touchAnimation = new RandomTouch(
	triangulation: _polyView.CurrentTriangulation,
	numFrames: 12,
	x: touch.X,
	y: touch.Y,
	radius: 150
	);
polyView.AddAnimation(touchAnimation);
```

## API

For `LowPolyLibrary.Triangulation`

>Note: Points are initially generated in a grid pattern, then offset by noise.

- `public float Seed { get; set; }`
	- Seed value used in determining noise for point generation
- `public float Frequency { get; set; }`
	- Frequency value used in determining noise for point generation
- `public float BleedY { get; set; }`
	- Y direction of how far points can be generated outside bounding rectangle defined by (Left, Top, Right, Bottom) : (0, 0, BoundsWidth, BoundsHeight)
- `public float BleedX { get; set; }`
	- X direction of how far points can be generated outside bounding rectangle defined by (Left, Top, Right, Bottom) : (0, 0, BoundsWidth, BoundsHeight)
- `public float CellSize { get; set; }`
	- How far apart points are initially generated in the grid
	- A grid cell has the dimensions: `CellSize`x`CellSize`
- `public float Variance { get; set; }`
	- How far a point can vary (X/Y directions independent) from its initial location in the grid.
	- Designed to be in (0,1]
		- Any number can be provided (0, infinity), although behavior is undefined outside of  (0,1]
	- Used internally as `CellSize * Variance`
- `public bool HideLines { get; set; }`
	- Whether or not to draw outer lines of triangles
- `public SKShader GradientShader { get; set; }`
	- A [`SKShader`](https://developer.xamarin.com/api/type/SkiaSharp.SKShader/) used for the gradient
- `public List<Vertex> Points { get; }`
	- Copy of internal points used in a `Triangulation`
- `public List<Triad> TriangulatedPoints { get; }`
	- Copy of `Triad`(s) used in a `Triangulation`
	- A `Triad` represents the 3 indices of a triangle in a `Triangulation`
- `public Dictionary<Vertex, HashSet<Triad>> PointToTriangleDic { get; }`
	- Copy of Dictionary which maps a `Vertex` to the `Triads` it is associated with

## What makes this library different?
Explain that this does more than create triangulations, but allows you to put animations on top of the triangulations. Even allows for creation of custom animations

## Libraries used

- [S-hull][sHullRef]
Responsible for creating the triangulations. Chosen for its proven speed
- [Auburns/FastNoise_CSharp][fastNoiseRef]
Responsible for generating noise used in creating source points for triangulation
- [capesean/ColorBru][colorBruRef]
Binding to the ColorBrewer library developed by Cynthia Brewer

[sHullRef]: http://www.s-hull.org/
[fastNoiseRef]: https://github.com/Auburns/FastNoise_CSharp
[colorBruRef]: https://github.com/capesean/ColorBru

## Alternate Usages
Describe how to use TriangulationView and AnimationUpdateView seperately with screenshots

## Future plans
 - Support dynamically resizing the view
 - Support more platforms
 - OpenGL support
###### <small>Does anyone want to pick up the ball here? </small>

## Compatibility
Built with
- `.Net Standard` 2.0
- `Xamarin.Android` 8
- `Xamarin.iOS` 11

#### Find where to place in this doc:
framerate
