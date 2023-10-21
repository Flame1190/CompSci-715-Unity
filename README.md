# CompSci 715
 
This contains the Unity project for CompSci 715 of Group 5 in 2023. It also contains all the data collected from user studies, the images used in memory tests and "pdf" versions of the questionnaires (prestudy and poststudy).

Link to GitHub: https://github.com/Flame1190/CompSci-715-Unity

# Data

The "_User_Study_Data" folder contains the data gathered from user studies, as well as its own "_Explanation.md" file for further explanations.

# Memory Testing

The "_Images" folder contains images used for the Object Memory Test on Qualtrics, and the "_Images/Rooms" folder contains images we printed onto magnets used for the Spatial Memory Test.

# Unity Project

This project was developed with Unity version "2022.3.#f#" where the "#" varied from researcher to researcher. When you attempt to open this project via github for the first time, a pop-up screen may appear saying that the project is dated before Unity versions 5.0. This is simply because our .gitignore excludes the unity version file. Simply open the project, and then revert any changes that are made by Unity (as Unity will add extra files assuming the project needed upgrading from a pre-Unity 5.0 version).

The project was targeted towards the Meta Quest Pro. One build containing solely the "_Control" scene was put onto one headset (the control condition), while another headset contained a build with solely the "_Portals" scene (this was the experimental condition).

Note that this project behaved differently from computer to computer (when using github), causing various issues. On some computers, opening specific scenes (like "DomTest") would crash Unity. On other computers, input from the Meta Quest Pro Controllers would not work on the andriod build for the headset. The computer which allowed for builds to work correctly included the following details:
- Unity version 2022.3.5f1
- The settings shown in the images found in the "_Settings" folder.

Baked lighting files were too large for gihub, therefore they are not included on github. However, these can be regenerated per new install.

# User Studies

When save files are created for a user study, they are found in the "Application.persistentDataPath" folder. For computers, this is within subfolders of our LocalLaw folder. On a Quest Pro headset, this is found in subfolders of the sdcard/Android/data folder (accessable via SideQuest).

When you open either of the Control or Experimental scenes/builds, you need to use the controllers to set the Participant ID before traversing the museum. On the Meta Quest Pro controllers, the "A" button decreases the ID, and the "B" button increases the ID. The "X" button toggles the ID/Recording text, and the "Y" button will force the recording to stop and save (if recording has started, used incase participants never reach the end). Recording will start when participants enter the dinosaur room, and end/save when they cross the finish line in the end room. Recording with negative IDs will fail (Note: Untested, but values below -1 may work). A horn will play when the game is first loaded. A sound indicating the recording has successfully started will play, being the same horn sound. A different sound (an Among Us sound) will play if the start-recording process failed (due to a negative ID value). The horn sound will play when the player crosses the finish line, regardless of whether the recording was successful. Toggle the Recording text at anytime (with "Y") to check the status of recording.

Participants experiencing the control condition will need the controllers to move. The left joystick moves the player, and the right joystick snap-turns the player. Participants in the experimental condition do not need the controllers, however the researchers need them initally to set the ID value. The researchers can also use the controllers in the experimental condition to rotate the player (to ensure the physical playspace matches the dimensions of the room). These are controlled using the left and right trigger and grip buttons. In the experimental condition, simply place the controllers in a random corner of the playspace to avoid the chance participants kick them, but to also still within the Oculus Guardian so that the Guardian isn't constantly appearing to the user. Viewing what the participant sees using Oculus Casting is recommended.

# Acknowledgments of used assets

"PortalsVR" https://github.com/daniellochner/portals-vr

"Low Poly Mammoth" (https://skfb.ly/6UUMM) by rkuhlf is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).

https://blendswap.com/blend/9811

"Among us Low Poly Free" (https://skfb.ly/o7Ayv) by Cartoon Props is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).

"Coccinellidae" (https://skfb.ly/oKvsK) by tunosemi is licensed under CC Attribution-NonCommercial-ShareAlike (http://creativecommons.org/licenses/by-nc-sa/4.0/).

"Low-Poly Wolf Head" (https://skfb.ly/6VQI7) by TlVl8 is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).

"Birdhouse" (https://skfb.ly/o8EQZ) by Jana Smirinova is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).

"Isometric low-poly shop building" (https://skfb.ly/JXsO) by daviddickball is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).

"Moa nunui / South Island giant moa skeleton" (https://skfb.ly/oAB6y) by Auckland Museum is licensed under Creative Commons Attribution-ShareAlike (http://creativecommons.org/licenses/by-sa/4.0/).

"Sumatran Tiger" (https://skfb.ly/KF8y) by Jérémie Louvetz is licensed under Creative Commons Attribution-NonCommercial (http://creativecommons.org/licenses/by-nc/4.0/).

"Street Light" (https://skfb.ly/oLCLo) by naderlabbad309 is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).

"mask" (https://skfb.ly/oouvz) by ストレンジ is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).

"Low Poly Chicken" (https://skfb.ly/6UqLu) by Tiberiu Uncu is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).