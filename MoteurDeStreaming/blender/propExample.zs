# Ceci est un commentaire
# b <xMin> <xMax> <zMin> <zMax>
# vertice :v <index> <X> <Y> <Z>
# textureCoord : vt <index> <X> <Y> <Z>
# veticeNormal : vn <index> <X> <Y> <Z>
# face : f <vX/vtX/vnX> <vY/vtY/vnY> <vZ/vtZ/vnZ>
# z <index> <name> # <nbEntry> # pour accelerer la recherche dans le fichier?
# o <centerX> <centerY> <centerZ> # A voir si c'est utile, pas sur. #

b -2 8 -8 10
z 0 zone1
o 1.545 0 -2.4
v 0 -1.5485 1 -2.455
v 1 -2.5485 1 -2.455
v 2 -1.5485 1 -2.455
v 3 -0.5485 1 -2.48
vt 0 -1.5485 1 -2.455
vt 2 -2.5485 1 -2.455
vt 3 -1.5485 1 -2.455
vt 1 -0.5485 1 -2.48
vn 8 -1.5485 1 -2.455
vn 7 -2.5485 1 -2.455
vn 6 -1.5485 1 -2.455
vn 0 -0.5485 1 -2.48
f 0/0/0 1/2/1 0/0/0

z 1 zone2
o
v 0 -1.5485 1 -2.455
v 5 -2.5485 1 -2.455
v 6 -1.5485 1 -2.455
v 3 -0.5485 1 -2.48
vt 0 -1.5485 1 -2.455
vt 2 -2.5485 1 -2.455
vt 3 -1.5485 1 -2.455
vt 1 -0.5485 1 -2.48
vn 8 -1.5485 1 -2.455
vn 7 -2.5485 1 -2.455
vn 6 -1.5485 1 -2.455
vn 0 -0.5485 1 -2.48
f 0/0/0 1/2/1 0/0/0