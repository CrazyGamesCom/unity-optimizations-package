# Unity WebGL optimizations

The purpose of this package is to help you optimize your game, by reducing the build size and increasing the performance. At the moment, it is targeted to WebGL games.

If you are using our CrazySDK, this package will come integrated with it.

## Export optimizations

![Export optimizations](Docs/export.png?raw=true "Export optimizations")

## Texture optimizations

![Texture optimizations](Docs/textures.png?raw=true "Texture optimizations")

The texture optimizations tool provides an overview of all the textures in your project, and also various tips about optimizing the size they occupy in the final build.

It finds textures in your project in these 2 ways:

1. By looking at the scenes included in the build (Build settings > Scenes in build), and finding recursively all the textures on which those scenes depend.
2. By fiding textures in `Resources` folders, or by recursively finding textures on which the assets from the Resources folders depend.

This means that the texture detection may miss more intricate textures that are not covered by the above cases.

## CrazyGames

We can also help your game become popular on the web. You can submit your game on CrazyGames [here](https://developer.crazygames.com/).
