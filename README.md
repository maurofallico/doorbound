# La Mansión

## Sobre el juego

**La Mansión** es un survival horror de fantasía oscura en tercera persona. Para resolver el misterio de la mansión, el jugador debe explorar sus habitaciones, encontrar pistas y completar acertijos sin permitir que una criatura lo alcance.

El personaje no tiene capacidades de combate. Su ventaja consiste en leer el entorno, decidir cuándo caminar o correr y aprovechar el espacio para evitar al perseguidor.

La experiencia alterna momentos de investigación y tensión: cada avance revela una nueva pista, pero también expone al jugador a nuevas rutas de patrulla y persecución.

## Estado actual

El proyecto cuenta con:

- Movimiento del jugador en tercera persona.
- Sistema de interacción.
- Llaves y puertas bloqueadas.
- Puertas capaces de instanciar objetos o enemigos.
- Enemigo con patrulla y persecución.
- Detección del jugador por campo de visión.
- Navegación mediante NavMesh.
- Condiciones de derrota y victoria.

## Controles


- Movimiento: `W`, `A`, `S`, `D`.
- Interactuar: `E`.
- Correr: `Shift`.


## Organización del código

Los scripts principales se encuentran en `Assets/Scripts`.


### Pendientes

- [ ] Anotar cómo se resuelve cada acertijo.
- [ ] Registrar las escenas disponibles.
- [ ] Documentar sonidos, animaciones y efectos visuales.

### Problemas conocidos

- 

## Herramientas

- Unity `6000.3.5f2`.
- Universal Render Pipeline.
- AI Navigation.
- Cinemachine.
