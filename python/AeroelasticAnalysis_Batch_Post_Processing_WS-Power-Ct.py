
import os
import linecache
#import pandas as pd
#import xlsxwriter
from datetime import datetime
#import numpy as np
import fnmatch
import csv

startTime = datetime.now()


# NOTE: the script processes the folder it is located in. Copy it into the folder
# of your batch results (the path must contain a folder called "Batch runs",
# which is used to name the output .csv file).
cwd = os.path.dirname(os.path.realpath(__file__))

storage_dir = cwd + "//"
csv_filename = cwd.split("Batch runs\\")[1].split("\\")[0] + ".csv"

#filename = cwd + r"\tdists.out"
filename = "Rotor.txt"

load_case_results = {}
value_type = ["min", "min_t", "max", "max_t", "mean", "st_dev"]

for subdir, dirs, files in os.walk(cwd):
	for file in files:
		if fnmatch.fnmatch(file, filename):
			filepath = os.path.join(subdir, file)
								
			#filepath = os.path.dirname(file)
			filedata = open(filepath,'r')
			filelines = filedata.readlines()
			
			case_id = filepath.split("Load case set")[1].split("\\")[1].split(" ")[2]
			
			parameters_list = [x.split(" [")[0] for x in filelines[7].split("\t")]
			parameters_unit_list = [x.split("[")[-1] for x in filelines[7].split("]")]
			
			param_min_value = [x.split(" [")[0] for x in filelines[9].split("\t")[1:]]
			param_min_time = [x.split(" [")[1][:-1] for x in filelines[9].split("\t")[1:]]
			param_max_value = [x.split(" [")[0] for x in filelines[10].split("\t")[1:]]
			param_max_time = [x.split(" [")[1][:-1] for x in filelines[10].split("\t")[1:]]
			
			param_mean = [x for x in filelines[11].split("\t")[1:]]
			param_st_dev = [x for x in filelines[12].split("\t")[1:]]

			results_type = [x for x in [param_min_value, param_min_time, param_max_value, param_max_time, param_mean, param_st_dev]]
			
			load_case_results[case_id] = {parameters_list[i+1]: {value_type[j]:results_type[j][i] for j in range(len(results_type))} for i in range(len(parameters_list) - 1)}
			
			#single_param_results = {value_type[j]:results_type[j] for j in range(len(results_type))}

			#pof_float = [float(i) for i in pof_list]
			
			#PoF[case] = {}
			#PoF[case] = pof_float
			

#for i in range(len(parameters_list)):
#	print(f"{i}: {parameters_list[i]}")

##print(f"File identifier: {case_id}")
#for i in range(len(load_case_results)):
#	print(f"Wind speed: {load_case_results[str(i+1)][parameters_list[14]][value_type[4]]}: Ct = {load_case_results[str(i+1)][parameters_list[8]][value_type[4]]}")


"""  USE THIS TABLE TO REQUEST DATA TO BE EXPORTED TO THE CSV
_______________________________________________________________________________
   *** parameters_list contents ***	  |      *** value_type contents ***	  |
0: "Time"							  |   0: "min"							  |
1: "Power (aero)"					  |   1: "min_t"						  |
2: "Torque (aero)"					  |   2: "max",							  |
3: "Thrust (aero)"					  |   3: "max_t"						  |
4: "RPM"							  |   4: "mean"							  |
5: "TSR"							  |   5: "st_dev"						  |
6: "Demanded collective pitch angle"  |
7: "Power coef. (CP)"				  |
8: "Thrust coef. (CT)"				  |
9: "Tip speed"						  |
10: "1P (one revolution)"			  |
11: "nP (blade passing)"			  |
12: "Azimuth angle"					  |
13: "Rotation per timestep"			  |
14: "Wind speed at hub, magnitude"	  |
15: "Wind speed at hub.x"			  |
16: "Wind speed at hub.y"			  |
17: "Wind speed at hub.z"			  |
18: "Yaw angle"						  |
"""

wind_list = ["Wind speed at hub, magnitude"] + [load_case_results[str(i+1)]["Wind speed at hub, magnitude"]["mean"] for i in range(len(load_case_results))]
cp_list = ["Power (aero)"] + [float(load_case_results[str(i+1)]["Power (aero)"]["mean"]) / 1000 for i in range(len(load_case_results))]
ct_list = ["Thrust coef. (CT)"] + [float(load_case_results[str(i+1)]["Thrust coef. (CT)"]["mean"]) / 100 for i in range(len(load_case_results))]

rows = zip(wind_list, cp_list, ct_list)

#for i in range(len(wind_list)):
#	print(f"{wind_list[i]}: {ct_list[i]}")

with open(storage_dir + csv_filename, 'w', newline='') as myfile:
     wr = csv.writer(myfile, quoting=csv.QUOTE_ALL)
     wr.writerows(rows)
