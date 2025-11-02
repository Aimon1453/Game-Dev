# Individual Assignment

## Part 1

Our game is a near-future cyberpunk title with a target audience of players who enjoy science fiction, dystopian themes, and casual games. This report will detail how the game, through its core concept of the "CALM" emotion suppression system, constructs a story about technological ethics, informed consent, and the value of personal authenticity.

The core ethical conflict of the game is rooted in a meticulously designed "cozy dystopia," where a mega-corporation named Nexus promotes the "CALM" technology to society with the promise of eliminating mental anguish such as PTSD and anxiety. However, the story's truth reveals a significant ethical pitfall behind this promise: the so-called "voluntary" installations are, in reality, the result of citizens being subjected to long-term, secret cognitive pre-conditioning through environmental factors like the water supply and air. Through this setup, the game poses a critical societal question to the players: when a technology offers immense convenience and comfort, would we give up the right to know the true cost behind it?

To explore this theme in depth, we have crafted a series of characters representing different perspectives and dilemmas. For instance, the PTSD patient Victor Kolva faces the core conflict of "meaningful suffering versus unbearable pain." His eventual choice to install CALM represents individuals who, when faced with immense trauma, are forced to abandon a part of their humanity simply to survive. Meanwhile, the low-level worker Zhang Meiling embodies the dilemma of economic coercion—facing unemployment if she refuses CALM, her "free will" is effectively held hostage by the realities of survival. Her memories are eventually even altered, highlighting technology's violation of personal identity. The presence of these characters ensures the game avoids a simple binary opposition, instead offering a multi-faceted presentation of a complex societal issue.

Ultimately, as the game is set in a fictional world without direct mapping to real-world corporations or nations, it focuses less on addressing specific societal problems and more on exploring the possibility of a world controlled by a mega-corporation. The game does not provide easy answers, leaving players with an open-ended conclusion that compels them to reflect: is a society without pain and anxiety, but also without genuine emotion and free will, a cure or a castration?

If we could go back to the beginning of the project, we would perhaps attempt to streamline the game's narrative length. The time saved could then be dedicated to further polish and optimization.

## Part 2

In this project, I served as the primary developer. My responsibilities included the vast majority of the programming and UI implementation, a portion of the art assets, and the complete development of the Night Mini-Game, in addition to various other supporting tasks.

Specifically, my contributions can be broken down into three main areas:

First is the game's core architecture. This includes the implementation of in-game interfaces, the singleton pattern for global systems, and the development of most manager classes (e.g., GameManager, UIManager, ObjectPool, StoryManager, DialogueManager and so on). This architectural layer was crucial for connecting different scenes and for integrating and managing the smooth operation of the entire game.

Second is the Dialogue/Story System. As our game is dialogue-driven, this system formed its most critical backbone. I was responsible for architecting and programming the entire system, as well as creating its UI entities. This provided the project with an extensible dialogue system robust enough to support our entire game flow. The system was also designed to provide an interface for the Day Mini-Game (which was not developed by me). After building the system, I also authored half of the dialogue content (stored as ScriptableObject data files) and was responsible for the system's ongoing updates and maintenance. Finally, I also created the art assets for this system, including the dialogue and name boxes.
![对话框](image.png)

Finally, I developed the Night Mini-Game. I handled the game design, programming, and art for this section entirely on my own. Specifically, this included: the enemy and cell spawning systems, the player's movement and attack logic, the logic for item pickups and pickable objects, and finally, the scoring system and parts of its UI.

![virus](image-1.png)
![激光纹理](image-2.png)
![玩家和细胞](image-3.png)

Regarding the report writing, I was responsible for authoring several sections within "3. Development Process," specifically: 3.2 Dialogue System, 3.4 Night Game, 3.5 Technical Specifications, and 3.6.3 Night Game Design.

Furthermore, I was responsible for the project management of the entire repository. This involved creating the GitHub repository using the FLS template and managing the merging of different branches.

I also participated in one playtesting session, where I acted as both an observer and a demonstrator for our game, though this was for a brief period. During this session, I also played games from other groups and provided them with constructive feedback.

Additionally, I regularly attended group meetings where I contributed my ideas and opinions.
