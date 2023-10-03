###########################################################################

#region Extracting Data

def get_rooms_info():
    rooms = {}

    rooms["Start"           ] =  ( 0,  0)
    rooms["Dinosaur"        ] =  ( 0,  1)
    rooms["Egypt"           ] =  ( 0,  2)
    rooms["Cherry Blossom"  ] =  ( 1,  2)
    rooms["Clocks"          ] =  (-1,  3)
    rooms["Signs"           ] =  ( 0,  3)
    rooms["Paintings"       ] =  ( 1,  3)
    rooms["Volcano"         ] =  (-1,  4)
    rooms["Aquarium"        ] =  ( 1,  4)
    rooms["Giant Ladybug"   ] =  (-1,  5)
    rooms["Sundial"         ] =  ( 0,  5)
    rooms["Statues"         ] =  ( 1,  5)
    rooms["Ice Cube"        ] =  (-1,  6)
    rooms["Mammoth"         ] =  ( 0,  6)
    rooms["Pond"            ] =  ( 1,  6)
    rooms["Pots"            ] =  (-1,  7)
    rooms["Bird Nest"       ] =  ( 0,  7)
    rooms["Beach"           ] =  (-1,  8)
    rooms["Face"            ] =  ( 0,  8)
    rooms["End"             ] =  ( 0,  9)

    return rooms

def in_room(x, z, rooms_info, portals = False):
    for room_name in list(rooms_info.keys()):
        room = rooms_info[room_name]
        room_x = room[0] * 3
        room_z = room[1] * 6

        portal_mutlipler = 100
        if portals:
            room_x *= portal_mutlipler
            room_z *= portal_mutlipler

        if abs(x - room_x) < 3/2 and abs(z - room_z) < 6/2:
            return room_name
    print("ERROR: Player not in any room bounds!")
    return ""

class UserInfo:
    def __init__(self):
        self.data = {}

        self.data["prestudy"] = {}

        self.data["path"] = {}

    def infer_path_info(self, raw_data, rooms_info):
        self.data["path"]["visited"] = {}
        self.data["path"]["unvisited"] = {}

        raw_lines = raw_data.split("\n")

        last_time = 0
        last_room = ""
        room_visits = 0

        for line in raw_lines:
            line_info = line.split(",")

            time = float(line_info[0])
            position = (float(line_info[1]), float(line_info[2]), float(line_info[3]))
            rotation = (float(line_info[4]), float(line_info[5]), float(line_info[6]), float(line_info[7]))

            current_room = in_room(position[0], position[2], rooms_info)

            if current_room not in self.data["path"]["visited"]:
                self.data["path"]["visited"][current_room] = {}
                self.data["path"]["visited"][current_room]["order"] = len(self.data["path"]["visited"]) - 1
                self.data["path"]["visited"][current_room]["sequence"] = []

            self.data["path"]["visited"][current_room]

            if current_room != last_room:
                self.data["path"]["visited"][current_room]["sequence"].append(room_visits)
                room_visits += 1

            last_room = current_room
            last_time = time

        self.data["path"]["total_time"] = last_time


def get_user_infos(count, rooms_info):

    user_infos = {}
    for i in range(0, count):
        user_infos[i] = UserInfo()

        # Infer path info from raw game data (.txt) with rooms info
        raw_data = ""
        path_data_filename = str(i) + '.txt'
        try:
            with open(path_data_filename, 'r') as file:
                raw_data = file.read()
        except FileNotFoundError:
            print(f"File '{path_data_filename}' not found.")
        except Exception as e:
            print(f"An error occurred: {str(e)}")

        user_infos[i].infer_path_info(raw_data, rooms_info)

        # Extract prestudy info from qualtrics data (.csv)
        # ...

        # Extract poststudy info from qualtrics data (.csv)
        # ...

    return user_infos
    
#endregion

###########################################################################

def main():
    rooms_info = get_rooms_info()
    user_infos = get_user_infos(1, rooms_info)

    print("Done")

###########################################################################

main()