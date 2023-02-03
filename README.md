
# Magician

Magician is in extremely early stages and is a general-purpose mathematical engine for generating and visualizing data, built with a simple API in mind.


## Features/Design

Magician is built around two core abstractions, Multis and IMaps.
### Multi
A Multi is recursively-defined geometry. Anything drawn to the screen is a Multi, or a Texture, which is always a member of a Multi

Each Multi stores its position relative to its parent, but not its absolute position. A Multi contains flags that determine how it is drawn, either as a plot, or a filled/outlined geometrical shape.
### IMap
An IMap represents a one-dimensional mathematical function f(x). They can be defined in multiple ways, such as with a lambda, or with built-in features like a sequence of polynomial terms. IMaps cam be used to define dynamic behaviour or to render plots in the form of a Multi.
#### Multimap
A Multimap is an IMap with an arbitrary number of inputs and outputs. This is useful for defining parametric movement/plots.

### Interactivity
Magician currently offers limited user interactivity, defineable through IMaps.

## Usage/Examples
Currently, Magician has basic functionality, but is not ready for general use. It is best used by writing code in spell/Spell.cs. It consists of the Preloop, which runs once and the Loop, which runs every frame. The following examples are written in the Preloop.
#### These examples reflect usage as of Magician Alpha 0.0, and may break at any time

```c#
// Create an equilateral triangle with radius 60 in the upper-right quadrant
Origin["myTriangle"] = RegularPolygon(100, 100, 3, 60);
```
![triangle](https://i.imgur.com/gv5u7XG.png)

```c#
// Create a pentagon with radius 100, with a random easy-to-see colour
Origin["myPentagon"] = RegularPolygon(5, 100).Colored(
    HSLA.Random(saturation: 1, lightness: 1, alpha: 120)
);
```
![pentagon](https://i.imgur.com/OR7v1Wb.png)
```c#
...
// Let that pentagon track the mouse cursor
Origin["myPentagon"].DrivenXY(
    x => Events.MouseX,
    y => Events.MouseY
);
```
![pentagon following cursor](https://i.imgur.com/Od9e0Ha.gif)
```c#
...
// Now let that pentagon revolve about the origin, while still tracking the mouse
Origin["myPentagon"].DrivenPM(
    p => Data.Env.Time,
    m => m
);
```
![revolving pentagon with radial mouse tracking](https://i.imgur.com/EyCAnaf.gif)
```c#
...
// Let that pentagon rotate about its axis
Origin["myPentagon"].Sub(m => m
    .DrivenPM(
        p => p + 0.1,
        m => m
    )
);
```
![revolving and rotating pentagon with mouse tracking](https://i.imgur.com/PkV7Uap.gif)

```c#
Origin["parametric"] = new Multimap(1,
    x => 180 * Math.Cos(x / 3),
    y => 180 * Math.Sin(y / 7)
).TextAlong(-49, 49, 0.3, "Here's an example of a Multimap with 1 input and two outputs being used to draw text parametrically",
new RGBA(0x00ff9080));
```

![](https://i.imgur.com/gha7jej.png)

## Acknowledgements

 Thanks to these people for sharing their knowledge.
 - [Atul Narkhede, Dinesh Manocha](http://gamma.cs.unc.edu/SEIDEL/)
 - [Yuvraj Nigade, Pushkar Newaskar](http://www.polygontriangulation.com/)
 - [Lazyfoo](https://lazyfoo.net/)


## Gallery
Some cool screenshots
![](https://i.imgur.com/7rM5V6s.png)
![](https://i.imgur.com/v1pMOMp.png)
![](https://i.imgur.com/2my6cn7.png)

