# <center>Game Documentation</center>


Este proyecto utiliza la topología Cliente-Servidor de modo que un cliente es el servidor a la vez, e implementa el hospedaje de servidor que admite Photon Fusion.

La documentación explica la jugabilidad y las características técnicas del código del juego tal como se implementa con Photon Fusion únicamente.

***EL proyecto está diseñado para PC con entrada de teclado y mouse.***

Se presenta patrones e implementaciones para las siguientes características:

- Múltiples modos de juego configurables
- Spells usando un buffer de datos de proyectiles
- Gestión de intereses en la red
- Procesamiento de entrada
- Sistema de salud y daños
- Gestión de jugadores: unirse/salir, recuperar datos del jugador después de volver a conectarse, unirse tarde
- Pickup de cartas: salud, damage, speed.
- Areas de da;o: explosivas;
- Powerups
- Sistema de buffs y energia
- Menú con emparejamiento y opciones.

## Estructura del proyecto
Assets/

- Photon/	... librerias de fuision y addons.
- Code/	... Codigo del proyecto.
- MysticCatsAssets/	... algunos assets para la demostracion.
- Prefabs/	... Prefabs para el proyecto.
- Resources/ ... Configuraciones del proyecto.

Scenes/

- Menu/ ... Menu e inicio del juego
- LoadingScene/ ... Pantalla de carga entre el menu y el juego
- Game/ ... escena para la prueba de las funcionalidades.
- GameplayUI ... Escena con interfaz de usuario de juego independiente, cargada de forma aditiva 

## Nucleo del juego(Core)
La siguiente imagen muestra el bucle del juego:

![descripcion](/ImgDoc/img1.png)

El siguiente diagrama muestra la vida util de los scripts principales del juego:

![descripcion](/ImgDoc/img2.png)

En cada espacio de la aplicacion se puede agregar clases para cualquier funcionalidad de acuerdo a la jerarquia de datos que se tiene

## Base y contexto
### Base
maneja funciones y servicios específicos de la escena ( BaseService), como interfaz de usuario y otros. Los servicios de escena se actualizan manualmente desde el Scene, de modo que se pueden inicializar, activar, desactivar y actualizar en cualquier momento.

### BaseContext 
proporciona acceso seguro a servicios comunes u otra información necesaria en todo el código base sin utilizar estática. El contexto de la escena se pasa automáticamente a los servicios de escena y se asigna a objetos en red en Gameplay y NetworkObjectPool. La herencia de ContextBehaviour y ContextSimulationBehaviouren lugar de NetworkBehaviour y SimulationBehaviour es necesaria para acceder a SceneContext.

## Red del Juego
NetworkGame maneja la entrada y salida de jugadores conectados y genera una prefab para cada cliente. Los jugadores desconectados se guardan, por lo que sus datos se pueden recuperar en caso de una reconexión. NetworkGame genera GameplayMode durante el proceso de inicialización.

## Modo de juego
Script principal que controla el juego y evalúa las condiciones de victoria. Parte de esto GameplayMode es el procesamiento de muertes/muertes de jugadores y la escritura en las estadísticas del jugador.


## Player

Player( Player script, Player prefab) representa un jugador conectado en el juego y no tiene representacion visual en el juego, solo de datos. El jugador proporciona acceso a metadatos comunes: ID de usuario, nickname, personaje seleccionado y otros datos que deberían sobrevivir a la reaparición del Agent(representación visual generada en el mundo del juego).

Agent( Agent script, AgentBase prefab y sus variantes) representa un personaje del juego controlado por el jugador. Es generado por GameplayMode y tiene Health, Spells y otros componentes. El personaje aparece y desaparece según sea necesario.

El siguiente diagrama muestra la jerarquía de componentes como una cascada de ejecución:
![descripcion](/ImgDoc/img3.png)