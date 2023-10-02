Data Explanation:

Each of the .dat and .txt files is a data file relating to a particular participant's user study. The number in these files refer to a participant's research ID.
The .dat file type is an encoded save file that the SaveAndLoad script in this Unity project can directly read and write to as a custom class (LogData class).
The .txt file type displays information from the LogData class in a readable text format. This .txt file is automatically generated when a LogData class is saved in Unity via the SaveAndLoad script.

The .txt files contain data on various lines. The following explains the comma-seperated information stored per line:
- Time index in seconds since the start of recording (as float),
- X-axis information of the head's position at the given time index (as float, originated from Vector3),
- Y-axis information of the head's position at the given time index (as float, originated from Vector3),
- Z-axis information of the head's position at the given time index (as float, originated from Vector3),
- X-axis information of the head's rotation at the given time index (as float, originated from Quaternion),
- Y-axis information of the head's rotation at the given time index (as float, originated from Quaternion),
- Z-axis information of the head's rotation at the given time index (as float, originated from Quaternion),
- W-axis information of the head's rotation at the given time index (as float, originated from Quaternion)
The time index from line to line should always increase. Note that for a Unity Vector3, 1 unit is 1 meter.

Large distances in head position between subsequent time indexes indicate that a user traveled through a portal. Keep this in mind if you plan to infer certain results, such as total distance traveled.
Each room in the virtual museum is 3 meters wide in the x-axis and 6 meters wide in the z-axis.
The following list contains the room-grid-position of the center of each room:
- "Start"           ,   0,  0
- "Dinosaur"        ,   0,  1
- "Egypt"           ,   0,  2
- "Cherry Blossom"  ,   1,  2
- "Clocks"          ,  -1,  3
- "Signs"           ,   0,  3
- "Paintings"       ,   1,  3
- "Volcano"         ,  -1,  4 
- "Aquarium"        ,   1,  4
- "Giant Ladybug"   ,  -1,  5
- "Sundial"         ,   0,  5
- "Statues"         ,   1,  5
- "Ice Cube"        ,  -1,  6
- "Mammoth"         ,   0,  6
- "Pond"            ,   1,  6
- "Pots"            ,  -1,  7
- "Bird Nest"       ,   0,  7
- "Beach"           ,  -1,  8
- "Face"            ,   0,  8
- "End"             ,   0,  9
To get the real position in meters of each room*, multiply the first axis of the room-grid-position by 3, and the second axis by 6. 
*Note that in the Portals condition, the x and z positions of all rooms were multiplied by 100 (for various reasons). Therefore, we either need to keep track of which condition each participant experienced in some document, or we simply detect the Portals conditions per log data via large jumps in position between some subsequent lines.
With a participant's log data, as well as the bounds and position of each room, we can infer new data, such as how many rooms a participant visited and for how long they visited in total.

Example: (Infering what room a participant is in)

Line from logdata: 27.53, 1.05, 1.65, 11.72, 0.0, 0.0, 0.0, 1.0
We can infer that the user was at position (1.05, 1.65, 11.72) 27.53 seconds into the experiment.

Room-Grid-Position: Egypt = 0, 2
Real-Position of room (in the control non-portals condition): Egypt = 0 * 3, 2 * 6 = Vector3(0 * 3, 0, 2 * 6) = Vector3(0, 0, 12)
Room size: 3m, 6m
Room bounds: ±3, ±6
User-position-x check: 1.05 is within ±3 of 0
User-position-z check: 11.72 is within ±6 of 12
We can infer that the user was in the Egypt room 27.53 seconds into the experiment.

Note that while rotation (Quaternion) data is available, it may be complicated to manipulate for meaningful inferences. 