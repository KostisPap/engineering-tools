# -*- coding: utf-8 -*-
"""
Created on Tue Sep 29 12:46:02 2020

"""


import os
import openpyxl
import numpy as np


# NOTE: define the path to the folder that contains the spreadsheet template
# (set the BOLTED_FLANGE_TEMPLATE_DIR environment variable, or edit the line below).
template_dir = os.environ.get('BOLTED_FLANGE_TEMPLATE_DIR', os.getcwd())
template_filename = r'\BoltedFlangeDesign - v3 - no img.xlsx'
storage_dir = template_dir + '\database'

def populate_spreadsheet(open_dir, filename, dw, wf, nobolts, dwasher,
                         dbolt, snorm, tnorm, save_dir):
    
    book = openpyxl.load_workbook(open_dir + filename)
    sheet = book.active

    sheet['B35'] = dw # Outer diameter
    sheet['B36'] = wf # Flange radius
    sheet['B39'] = nobolts # Number of bolts
    sheet['B41'] = dwasher # Washer diameter
    sheet['B42'] = dbolt # Bolt diameter
    sheet['B48'] = snorm # Wall thickness
    sheet['B55'] = tnorm # Flange thickness
    
    book.save(save_dir + '\BF_' + str(dw) + '_' + str(wf)
              + '_' + str(nobolts) + '_' + str(dwasher)
              + '_' + str(dbolt) + '_' + str(snorm)
              + '_' + str(tnorm) + '.xlsx')

total = 0
dw_list = np.arange(7500, 8501, 500).tolist()
total += len(dw_list)
wf_list = np.arange(330, 421, 10).tolist()
total *= len(wf_list)
nobolts_list = np.arange(188, 206, 1).tolist()
total *= len(nobolts_list)
dwasher_list = np.arange(125, 126, 2).tolist()
total *= len(dwasher_list)
dbolt_list = np.arange(72, 73, 2).tolist()
total *= len(dbolt_list)
snorm_list = np.arange(85, 121, 5).tolist()
total *= len(snorm_list)
tnorm_list = np.arange(140, 181, 5).tolist()
total *= len(tnorm_list)


count = 0
for i in dw_list:
    for j in wf_list:
        for k in nobolts_list:
            for l in dwasher_list:
                for m in dbolt_list:
                    for n in snorm_list:
                        for o in tnorm_list:
                            populate_spreadsheet(template_dir, template_filename,
                                                 i, j, k, l, m, n, o, storage_dir)
                            count += 1
                            if count % 5 == 0:
                                print ('Progress: ', count, ' out of ', total)
                
