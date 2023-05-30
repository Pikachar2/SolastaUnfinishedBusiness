#
# AUTHOR: magicskysword
#
# DESCRIPTION: This script is used to synchronize content from official translation to unofficial translation
#
# REQUIRES:
#   - Python 3.9.x

import os
import codecs

def unpack_record(record):
    term = ""
    text = ""
    try:
        (term, text) = record.split("=", 1)
        text = text.strip()
    except:
        term = record

    return term, text

def readRecord(filename):
    # read file and split with "=" to dict
    dic = {}
    try:
        line_count = 0
        with open(filename, "rt", encoding="utf-8") as f:
            record = "\n"
            while record:
                record = f.readline()
                # remove BOM
                if line_count == 0 and record.startswith(codecs.BOM_UTF8.decode("utf-8")):
                    record = record[1:]
                line_count += 1
                if record:
                    term, text = unpack_record(record)
                    dic[term] = text
    except FileNotFoundError:
        print("ERROR")

    return dic

def split_dict(dict):
    # split dict by /& in key
    dict_group = {}
    for key, value in dict.items():
        key_group = "Other"
        if "/&" in key:
            key_group = key.split("/&")[0]
        if key_group not in dict_group:
            dict_group[key_group] = {}
        dict_group[key_group][key] = value
    return dict_group

def sync_file(offcial_dict, file_record, file_full_name):
    print(f"sync {file_full_name}")

    # sync file with offcial dict
    unused_keys = []
    for key, value in file_record.items():
        if key not in offcial_dict:
            print(f"unused {file_full_name} {key} {value}")
            unused_keys.append(key)

    for key in unused_keys:
        del file_record[key]

    for key, value in offcial_dict.items():
        if key not in file_record or file_record[key] == "EMPTY":
            print(f"Add {file_full_name} {key} {value}")
            file_record[key] = offcial_dict[key]

    # write file
    with open(file_full_name, "wt", encoding="utf-8") as f:
        for key, value in file_record.items():
            f.write(f"{key}={value}\n")

    # sort file
    with open(file_full_name, "rt", encoding="utf-8") as f:
        data = f.readlines()
        data[0] = data[0].replace('﻿', '')
        data.sort()
    with open(file_full_name, "wt", encoding="utf-8") as f:
        f.writelines(data)

def sync_folder(dict_group, unofficial_file_code):
    unoffcial_folder_name = f"SolastaUnfinishedBusiness\\UnofficialTranslations\\{unofficial_file_code}"
    for group_name in dict_group.keys():
        file_full_name = os.path.join(unoffcial_folder_name, f"{group_name}-{unofficial_file_code}.txt")
        if os.path.exists(file_full_name):
            file_record = readRecord(file_full_name)
            print(f"read {file_full_name}")
        else:
            file_record = {}
            print(f"create {file_full_name}")
        sync_file(dict_group[group_name], file_record, file_full_name)

    for file_name in os.listdir(unoffcial_folder_name):
        file_full_name = os.path.join(unoffcial_folder_name, file_name)
        group_name = file_name.split("-")[0]
        if group_name not in dict_group and file_name.endswith(".txt"):
            print(f"unuse file {file_full_name}")

def sync_translation(offcial_file_code, unofficial_file_code):
    # read offcial translation file
    offcial_file_name = f"Diagnostics\\OfficialTranslations-{offcial_file_code}.txt"
    offcial_dict = readRecord(offcial_file_name)
    dict_group = split_dict(offcial_dict)
    # read unofficial translation group
    sync_folder(dict_group, unofficial_file_code)

def main():
    # run this script in root folder
    # sync cn language
    sync_translation("cn-ZN", "zh-CN-Unofficial")


if __name__ == "__main__":
    main()