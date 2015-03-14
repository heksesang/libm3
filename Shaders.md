# Introduction #

The aim of this Wiki page is to contain all shaders equations that compute the M3 models rendering.

Effects are computed with deferred rendering, I advise you to read this very interesting article that could be find easily on google:
  * the diapo version : S2008-Filion-McNaughton-StarCraftII.pdf
  * or the full article version : Chapter05-Filion-StarCraftII.pdf

I will base this research on the barracks unit.


---


# Barracks: General presentation #

Here are some parameters of the Barracks:


## Diffuse texture ##

![http://members.gamedev.net/richard-geslot/sharedimg/barracks_diffuse.png](http://members.gamedev.net/richard-geslot/sharedimg/barracks_diffuse.png)


## Specular texture ##

![http://members.gamedev.net/richard-geslot/sharedimg/barracks_specular.png](http://members.gamedev.net/richard-geslot/sharedimg/barracks_specular.png)


## Emissive texture ##

![http://members.gamedev.net/richard-geslot/sharedimg/barracks_emissive.png](http://members.gamedev.net/richard-geslot/sharedimg/barracks_emissive.png)

the RGB composants are null except on the lights

## Normal texture ##

![http://members.gamedev.net/richard-geslot/sharedimg/barracks_normal.png](http://members.gamedev.net/richard-geslot/sharedimg/barracks_normal.png)


default parameters of the game:

```
specularType = RGB
specularity = 20
hDRSpecularMultiplier = 1.7
hDREmissiveMultiplier = 3
```


---


# Barracks: Different shaders steps #

## wireframe ##
![http://members.gamedev.net/richard-geslot/sharedimg/wireframe.png](http://members.gamedev.net/richard-geslot/sharedimg/wireframe.png)

The wireframe isn’t the more interesting, but it’s important to view it.


## full bright diffuse only ##
![http://members.gamedev.net/richard-geslot/sharedimg/full_bright_diffuse_only.png](http://members.gamedev.net/richard-geslot/sharedimg/full_bright_diffuse_only.png)

it seems to be the exact diffuse texture.


## diffuse only ##
![http://members.gamedev.net/richard-geslot/sharedimg/diffuse_only.png](http://members.gamedev.net/richard-geslot/sharedimg/diffuse_only.png)


## diffuse lighting only ##
![http://members.gamedev.net/richard-geslot/sharedimg/diffuse_lighting_only.png](http://members.gamedev.net/richard-geslot/sharedimg/diffuse_lighting_only.png)



## emissive only ##
![http://members.gamedev.net/richard-geslot/sharedimg/emissive_only.png](http://members.gamedev.net/richard-geslot/sharedimg/emissive_only.png)

I think this effect is computed from the emissive texture. It's not exactly the same because, for example, the texture doesn't contain the halo information.



## lighting only ##
![http://members.gamedev.net/richard-geslot/sharedimg/lighting_only.png](http://members.gamedev.net/richard-geslot/sharedimg/lighting_only.png)



## normal only ##
![http://members.gamedev.net/richard-geslot/sharedimg/normal_only.png](http://members.gamedev.net/richard-geslot/sharedimg/normal_only.png)

it's normals of the vertices. remember that, in Starcraft 2, Z (blue color) is the up vector


## normal map only ##
![http://members.gamedev.net/richard-geslot/sharedimg/normal_map_only.png](http://members.gamedev.net/richard-geslot/sharedimg/normal_map_only.png)

It's the normal texture.


## problem visualization ##
![http://members.gamedev.net/richard-geslot/sharedimg/problem_visualization.png](http://members.gamedev.net/richard-geslot/sharedimg/problem_visualization.png)

It's beautifull... but I have no idea what it is...


## shadows only ##
![http://members.gamedev.net/richard-geslot/sharedimg/shadows_only.png](http://members.gamedev.net/richard-geslot/sharedimg/shadows_only.png)

## specular lighting only ##
![http://members.gamedev.net/richard-geslot/sharedimg/specular_lighting_only.png](http://members.gamedev.net/richard-geslot/sharedimg/specular_lighting_only.png)


## specular only ##
![http://members.gamedev.net/richard-geslot/sharedimg/specular_only.png](http://members.gamedev.net/richard-geslot/sharedimg/specular_only.png)

doesn't really look like the specular texture...


## UV mapping ##
![http://members.gamedev.net/richard-geslot/sharedimg/UV_mapping.png](http://members.gamedev.net/richard-geslot/sharedimg/UV_mapping.png)


## Final ##
![http://members.gamedev.net/richard-geslot/sharedimg/standard.png](http://members.gamedev.net/richard-geslot/sharedimg/standard.png)

Final combination of all effects!


---


# Equations #

Now the hard stuff...

## notation ##

In order that everyone in this wiki uses the same notations :
x and y are the screen coordinates.

```
fullBrightDiffuse(x,y)
diffuseLighting(x,y)
diffuse(x,y)
emissive(x,y)
lighting(x,y)
normal(x,y)
normalMap(x,y)
problemVizualization(x,y)
shadows(x,y)
specularLighting(x,y)
specular(x,y)
uvMapping(x,y)
final(x,y) 
depth(x,y)
```

## equations ##

Stacraft 2 uses deferred rendering, we suppose that we have the G-buffer computed
so we know :

  * emissive(x,y)
  * normal(x,y)
  * depth(x,y)
  * diffuse(x,y)
  * specular(x,y)

the aim is to get final(x,y) with these 5 known.


> --- To be continued... ---


If you have suggestions, ideas... Don't hesitate to comment.



