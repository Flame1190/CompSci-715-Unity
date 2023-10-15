import math
import numpy as np
import pandas as pd
from scipy.stats import mannwhitneyu
from collections import Counter
from scipy.stats import chi2_contingency

###########################################################################

#region Helper Functions

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

            # Introduce special case to handle "Recording start bounds" inconsistencies
            if not portals and room_name == "Start" and z > 2.8:
                return "Dinosaur"
            
            return room_name
        
    print("ERROR: Player not in any room bounds at x=" + str(x) + ", z=" + str(z))
    return ""

def angle(q1, q2, degrees = True):

    # Inspired code from:
    # https://forum.unity.com/threads/quaternion-angle-implementation.572632/

    quaternion1 = np.array([q1[3], q1[0], q1[1], q1[2]])
    quaternion2 = np.array([q2[3], q2[0], q2[1], q2[2]])

    dot_product = min(np.dot(quaternion1, quaternion2), 1)

    angle_radians = np.arccos(dot_product) * 2
    angle_degrees = np.degrees(angle_radians)

    return angle_degrees if degrees else angle_radians

def round_sig(x, sig=3):
    return round(x, sig-int(math.floor(math.log10(abs(x))))-1)

#endregion

###########################################################################

#region Extracting Data

class UserInfo:
    def __init__(self):
        self.data = {}

        self.data["prestudy"] = {}

        self.data["path"] = {}
        
        self.data["poststudy"] = {}

        self.data["spatial_test"] = {}

    def infer_path_info(self, raw_data, rooms_info, portals):
        self.data["path"]["visited"] = {}

        raw_lines = raw_data.split("\n")

        last_time = 0
        last_position = ()
        last_rotation = ()
        last_room = ""
        room_visits = 0

        total_distance = 0
        total_turn = 0

        for i in range(len(raw_lines) - 1):
            line_info = raw_lines[i].split(",")

            time = float(line_info[0])
            position = (float(line_info[1]), float(line_info[2]), float(line_info[3]))
            rotation = (float(line_info[4]), float(line_info[5]), float(line_info[6]), float(line_info[7]))

            current_room = in_room(position[0], position[2], rooms_info, portals)

            if current_room not in self.data["path"]["visited"]:
                self.data["path"]["visited"][current_room] = {}
                self.data["path"]["visited"][current_room]["order"] = len(self.data["path"]["visited"]) - 1
                self.data["path"]["visited"][current_room]["sequence"] = []
                self.data["path"]["visited"][current_room]["total_time"] = 0
                self.data["path"]["visited"][current_room]["total_distance"] = 0
                self.data["path"]["visited"][current_room]["total_turn"] = 0

            self.data["path"]["visited"][current_room]

            if current_room != last_room:
                self.data["path"]["visited"][current_room]["sequence"].append(room_visits)
                room_visits += 1
            elif i > 0:
                distance = math.dist(position, last_position)
                turn = angle(rotation, last_rotation)

                self.data["path"]["visited"][current_room]["total_time"] += time - last_time
                self.data["path"]["visited"][current_room]["total_distance"] += distance
                self.data["path"]["visited"][current_room]["total_turn"] += turn

                total_distance += distance
                total_turn += turn

            last_time = time
            last_position = position
            last_rotation = rotation
            last_room = current_room

        self.data["path"]["total_room_visits"] = room_visits
        self.data["path"]["total_time"] = last_time
        self.data["path"]["distance_per_time"] = total_distance / last_time
        self.data["path"]["turn_per_time"] = total_turn / last_time


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

        portals = (i % 2) == 1
        user_infos[i].infer_path_info(raw_data, rooms_info, portals)

        # Extract spatial test info from ?image? data (images need to be converted to readable graphs first)
        # ...

    # Extract prestudy info from qualtrics data (.csv)
        
    prestudy_file = 'prestudy.csv'
    prestudy_df = pd.read_csv(prestudy_file)

    prestudy_id_values = prestudy_df.iloc[:, 0]
    baseline_memory_values = prestudy_df.iloc[:, 2]

    for index, value in prestudy_id_values.items():
        if index < 2:
            continue

        id = int(value)

        user_infos[id].data["prestudy"]["baseline"] = int(baseline_memory_values[index])

    # Extract poststudy info from qualtrics data (.csv)
    
    poststudy_file = 'poststudy.csv'
    poststudy_df = pd.read_csv(poststudy_file)

    poststudy_id_values = poststudy_df.iloc[:, 0]
    gender_values = poststudy_df.iloc[:, 1]
    age_values = poststudy_df.iloc[:, 3]
    xr_xp_values = poststudy_df.iloc[:, 6]
    joystick_xp_values = poststudy_df.iloc[:, 7]
    enjoy_walk_values = poststudy_df.iloc[:, 8]
    enjoy_museum_values = poststudy_df.iloc[:, 9]
    play_games_values = poststudy_df.iloc[:, 10]

    objects_values_list = [poststudy_df.iloc[:, 17 + (i * 2)] for i in range(6)]
    object_surety_values_list = [poststudy_df.iloc[:, 18 + (i * 2)] for i in range(6)]
    distractors_values_list = [poststudy_df.iloc[:, 29 + (i * 2)] for i in range(12)]
    distractor_surety_values_list = [poststudy_df.iloc[:, 30 + (i * 2)] for i in range(12)]
    other_objects_values = poststudy_df.iloc[:, 53]

    easy_navigate_values = poststudy_df.iloc[:, 54]
    easy_object_test_values = poststudy_df.iloc[:, 55]
    easy_spatial_test_values = poststudy_df.iloc[:, 56]

    for index, value in poststudy_id_values.items():
        if index < 2:
            continue

        id = int(value)

        user_infos[id].data["poststudy"]["gender"] = gender_values[index]
        user_infos[id].data["poststudy"]["age"] = int(age_values[index])
        user_infos[id].data["poststudy"]["xr_xp"] = xr_xp_values[index]
        user_infos[id].data["poststudy"]["joystick_xp"] = joystick_xp_values[index]
        user_infos[id].data["poststudy"]["enjoy_walk"] = -int(enjoy_walk_values[index])
        user_infos[id].data["poststudy"]["enjoy_museum"] = -int(enjoy_museum_values[index])
        user_infos[id].data["poststudy"]["play_games"] = play_games_values[index]

        objects_seen = 0
        objects_seen_level = 0
        objects_confidence = 0
        for i in range(6):
            if objects_values_list[i][index] == "Yes":
                objects_seen += 1
                objects_seen_level += -int(object_surety_values_list[i][index]) - 1
            else:
                objects_seen_level += int(object_surety_values_list[i][index])
            objects_confidence += -int(object_surety_values_list[i][index])
        distractors_seen = 0
        distractors_seen_level = 0
        distractors_confidence = 0
        for i in range(12):
            if distractors_values_list[i][index] == "Yes":
                distractors_seen += 1
                distractors_seen_level += -int(distractor_surety_values_list[i][index]) - 1
            else:
                distractors_seen_level += int(distractor_surety_values_list[i][index])
            distractors_confidence += -int(distractor_surety_values_list[i][index])

        user_infos[id].data["poststudy"]["objects_seen"] = objects_seen
        user_infos[id].data["poststudy"]["objects_seen_level"] = objects_seen_level
        user_infos[id].data["poststudy"]["objects_confidence"] = objects_confidence
        user_infos[id].data["poststudy"]["distractors_seen"] = distractors_seen
        user_infos[id].data["poststudy"]["distractors_seen_level"] = distractors_seen_level
        user_infos[id].data["poststudy"]["distractors_confidence"] = distractors_confidence
        user_infos[id].data["poststudy"]["other_objects_length"] = len(other_objects_values[index])

        user_infos[id].data["poststudy"]["easy_navigate"] = -int(easy_navigate_values[index])
        user_infos[id].data["poststudy"]["easy_object_test"] = -int(easy_object_test_values[index])
        user_infos[id].data["poststudy"]["easy_spatial_test"] = -int(easy_spatial_test_values[index])

    return user_infos
    
#endregion

###########################################################################

def main():
    rooms_info = get_rooms_info()
    user_infos = get_user_infos(32, rooms_info)

    print() #

    print(user_infos[0].data)

    print() #

    compare_conditions(["prestudy","baseline"], user_infos)

    compare_conditions(["path","total_time"], user_infos)
    compare_conditions(["path","visited"], user_infos)
    compare_conditions(["path","total_room_visits"], user_infos)
    compare_conditions(["path","distance_per_time"], user_infos)
    compare_conditions(["path","turn_per_time"], user_infos)

    compare_conditions(["poststudy","gender"], user_infos)
    compare_conditions(["poststudy","age"], user_infos)
    compare_conditions(["poststudy","xr_xp"], user_infos)
    compare_conditions(["poststudy","joystick_xp"], user_infos)
    compare_conditions(["poststudy","enjoy_walk"], user_infos)
    compare_conditions(["poststudy","enjoy_museum"], user_infos)
    compare_conditions(["poststudy","play_games"], user_infos)

    compare_conditions(["poststudy","objects_seen"], user_infos)
    compare_conditions(["poststudy","objects_seen_level"], user_infos)
    compare_conditions(["poststudy","objects_confidence"], user_infos)
    compare_conditions(["poststudy","distractors_seen"], user_infos)
    compare_conditions(["poststudy","distractors_seen_level"], user_infos)
    compare_conditions(["poststudy","distractors_confidence"], user_infos)
    compare_conditions(["poststudy","other_objects_length"], user_infos)

    compare_conditions(["poststudy","easy_navigate"], user_infos)
    compare_conditions(["poststudy","easy_object_test"], user_infos)
    compare_conditions(["poststudy","easy_spatial_test"], user_infos)

    print() #
    print("Done")

###########################################################################

#region Analysing Data

def compare_conditions(variable, user_infos):

    list0 = []
    list1 = []

    ordered = True

    for id in sorted(list(user_infos.keys())):
        data = user_infos[id].data
        target_list = list0 if (id % 2) == 0 else list1

        target_value = data
        for value_type in variable:
            target_value = target_value[value_type]

        if ordered and isinstance(target_value, str):
            ordered = False

        if isinstance(target_value, list) or isinstance(target_value, dict):
            target_value = len(target_value)

        target_list.append(target_value)

    print("#")

    if ordered:
        print(variable, "(Ordered):")
        
        mean0 = np.mean(list0)
        mean1 = np.mean(list1)

        # Note: Currently only using wilcoxon-test, this can be changed to t-test for parametric data
        statistic, wilcoxon_p_value = mannwhitneyu(list0, list1)

        print("C Mean =", round_sig(mean0))
        print("E Mean =", round_sig(mean1))
        print("P-value =", round(wilcoxon_p_value, 3))
        if wilcoxon_p_value < 0.05:
            print("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ^ Significant ^ !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!")
    else:
        print(variable, "(Unordered):")
        
        count_list0 = dict(Counter(list0))
        count_list1 = dict(Counter(list1))
        sorted_count_list0 = dict(sorted(count_list0.items()))
        sorted_count_list1 = dict(sorted(count_list1.items()))

        observed = []
        for category in set(list0 + list1):
            observed.append([list0.count(category), list1.count(category)])

        chi2, chi_p_value, _, _ = chi2_contingency(observed)

        print("C =", sorted_count_list0)
        print("E =", sorted_count_list1)
        print("P-value =", chi_p_value)

        if chi_p_value < 0.05:
            print("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ^ Significant ^ !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!")

#endregion

###########################################################################

main()