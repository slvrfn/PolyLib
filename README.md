
  
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
Show what each parameter does

Seed G&S
Frequency G&S
BleedY G&S
BleedX G&S
CellSize G&S
Variance G&S
HideLines G&S
GradientShader G&S
Points G
TriangulatedPoints G
PointToTriangleDic G

## What makes this library different?
Explain that this does more than create triangulations, but allows you to put animations on top of the triangulations. Even allows for creation of custom animations

## Libraries and tools used

- [S-hull][sHullRef]
Responsible for creating the triangulations. Chosen for its proven speed
- [Auburns/FastNoise_CSharp][fastNoiseRef]
Responsible for generating noise used in creating source points for triangulation
- [capesean/ColorBru][colorBruRef]
Binding to the ColorBrewer library developed by Cynthia Brewer

[sHullRef]: http://www.s-hull.org/
[fastNoiseRef]: https://github.com/Auburns/FastNoise_CSharp
[colorBruRef]: https://github.com/capesean/ColorBru

### Alternate Usages
Describe how to use TriangulationView and AnimationUpdateView seperately with screenshots

## Future plans
 - Support dynamically resizing the view
 - Support more platforms
###### <small>does anyone want to pick up the ball here?

## Compatibility
iOS and Android versions supported

#### Find where to place in this doc:
framerate