# Telethreat

This game was made by [Matthew Bedder](http://bedder.co.uk) ([bedder](http://github.com/bedder)), [Christian Guckelsberger](http://ccg.doc.gold.ac.uk/christianguckelsberger/) ([cguckelsberger](http://github.com/cguckelsberger)), and [Nick Sephton](http://www-users.cs.york.ac.uk/~nsephton/) ([tbigfish](http://github.com/tbigfish))for the first [IGGI](http://iggi.org.uk) Games Programming module at Goldsmiths, University of London.

The main chunk of code for this game was made over the course of a week, so is in a bit of a sorry state. Matthew and Nick worked on the general game mechanics and game AI. Matthew also got creative with the various 3d models, and ported the game to the recent Unity version. Christian came up with the game idea, and implemented the procedurally generated level design.

## Story
Telethreat is set in 2087. The first human scouts are sent into deep space to discover life on other planets and form interstellar alliances. As a soldier of the Eurasian army, you travel for fifteen years to reach X-23, a planet that once showed strong radio activity. On the first encounter, all well prepared routines to communicate fail and lead to a fatal misconception with alien intelligent life. Being on a peaceful mission, your ship quickly runs out of arms and pull-back is initiated. Nevertheless, the crew soon notices that it is trapped into a huge magnetic field encompassing the entire planet. In black despair, you and a few hand-picked comrades are sent to the dark and seemingly inanimate surface of the planet, using the last remaining landing vehicle.

Your mission is to discover the core reactor that supplies the magnetic shield. After a couple of hours in pure darkness, the sky starts to glow in a violet hue and suddenly, an overwhelming megastructure made of thousands of polygonal shaped blocks becomes apparent. Your mothership locates the source of energy on the top level of the structure, and you work up the courage and enter the structure through a narrow gap. The material seems neither mechanical nor biological, and the gap immediately closed after you entered. You notice that the space you're in has no other exit, but a strange apparatus on several walls. A sudden flash almost paralyses you, and all at once, alien enemies appear next to the walls and attack. You are able to defend yourself this time and understand that you're trapped in a huge synchronized structure, similar to a supercomputer, in which adjacent cells are connected via teleporters,  projecting the bodies next to them to the other side. Although you can trigger synchronization manually, the teleporters are activated at the same time in regular intervals. The only way to destroying the core reactor leads through these cells.

Your team consists of up to four experts with special abilities in their respective fields: 1) A soldier trained in heavy infantry: he is used to wear massive armour and weapons, but is limited in movement. 2) A light infantry, good at exploring with light weapons and armory, being quick and agile. 3) An engineer capable to study alien circuitry and structures, and able to trigger the teleport mechanism early. 4) A medic able to rescue soldiers on the battlefield.

Please note that the different character types and the cooperative component are still to be implemented.

## General Navigation and Controls
The player has to move through the different cells using the teleporting mechanism, to eventually find the portal to the next level. The partitioning of the floor indicates which teleporter a player whill use at each position in a cell. An arrow in the UI guides the player towards the arrow. Red flashing lights indicate that the teleporters will be activated soon, with the remaining time counting down at the top of the screen. A player can trigger the teleport early, but only if there are no enemies in a cell or all enemies have been eliminated. Some cells contain stations to recharge either health, ammunition or armor. Teleporters can be destroyed both by the player and enemies. If a cell loses all teleporters, entities within will be killed and the cell will be inaccessible, also for transit. Choose your way wisely!

Press W to move forward, S to move backward and use the mouse to rotate the player character. Shoot with backspace or LMB, and throw grenades using RMB or the G key.

## Play the Unity 5 Port

This game has been ported to Unity 5.1 by Matthew Bedder, with this version being found on the aptly-named "[Unity 5](https://github.com/bedder/Telethreat/tree/Unity5)" branch. This variation has a revamped UI, has replaced the explosions with particle systems, has added particles to the chargers, has has generally better lighting and materials, and is slightly rebalanced.

It also probably has a load of bugs.

Web builds of this branch can be played [here for a Unity Webplayer version](http://bedder.co.uk/games/telethreat/play), and [here for a WebGL version](http://bedder.co.uk/games/telethreat/play/WebGl). The WebGL one is a bit buggy, but is largely playable.

## Future work
-Removing all the bugs
-Improving balancing
-Potentially adding fog of war / a map
-Implementing different character classes and coop-mode
